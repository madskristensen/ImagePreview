using System.Collections.Generic;
using System.IO;
using ImagePreview.Resolvers;
using Microsoft.VisualStudio.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImagePreview
{
    internal static class ExtensionMethods
    {
        private static readonly string[] _sizeSuffixes = new[] { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        private static readonly List<IImageResolver> _resolvers = new()
        {
            new Base64Resolver(),
            new PackResolver(),
            new HttpImageResolver(),
            new FileImageResolver(),
        };

        /// <summary>
        /// Finds image references in the given text under the specified trigger point.
        /// </summary>
        /// <param name="triggerPoint">The point in the text where image references should be searched for.</param>
        /// <returns>The first found image reference.</returns>
        public static async Task<ImageReference> FindImageReferencesAsync(this ITrackingPoint triggerPoint)
        {
            int cursorPosition = triggerPoint.GetPosition(triggerPoint.TextBuffer.CurrentSnapshot);
            ITextSnapshotLine line = triggerPoint.TextBuffer.CurrentSnapshot.GetLineFromPosition(cursorPosition);
            string lineText = line.GetText();

            foreach (IImageResolver resolver in _resolvers)
            {
                try
                {
                    if (!resolver.TryGetMatches(lineText, out MatchCollection matches))
                    {
                        continue;
                    }

                    foreach (Match match in matches)
                    {
                        Span span = new(line.Start + match.Index, match.Length);

                        // Perf: Break the loop if image refs are located after the cursor position
                        if (span.Start > cursorPosition)
                        {
                            break;
                        }

                        if (span.Contains(cursorPosition))
                        {
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            return new ImageReference(resolver, span, match, triggerPoint.TextBuffer.GetFileName());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
            }

            return null;
        }

        public static string GetImageFormat(this Match match)
        {
            return match.Groups["ext"]?.Value?.TrimStart('.').ToUpperInvariant();
        }

        // From https://stackoverflow.com/a/14488941
        public static string ToFileSize(this long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + ToFileSize(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return value < 1024
                ? string.Format("{0:n0} {1}", adjustedSize, _sizeSuffixes[mag])
                : string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, _sizeSuffixes[mag]);
        }

        /// <summary>
        /// Retrieves the root folder of the specified project.
        /// </summary>
        /// <param name="project">The project to retrieve the root folder for.</param>
        /// <returns>The root folder path of the project.</returns>
        public static string GetRootFolder(this EnvDTE.Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (project == null)
            {
                return null;
            }

            if (project.IsKind("{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")) // solution folder
            {
                return Path.GetDirectoryName(project.DTE.Solution.FullName);
            }

            if (string.IsNullOrEmpty(project.FullName))
            {
                return null;
            }

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    // MFC projects don't have FullPath, and there seems to be no way to query existence
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    // Installer projects have a ProjectPath.
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            return string.IsNullOrEmpty(fullPath)
                ? File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null
                : Directory.Exists(fullPath) ? fullPath : File.Exists(fullPath) ? Path.GetDirectoryName(fullPath) : null;
        }

        /// <summary>
        /// Determines if project is of a specific kind, based on specified list of Guids.
        /// </summary>
        /// <param name="project">The EnvDTE.Project object</param>
        /// <param name="kindGuids">List of Guids to check if project is a match</param>
        /// <returns>True if project's kind matches any of the provided Guids, otherwise false</returns>
        public static bool IsKind(this EnvDTE.Project project, params string[] kindGuids)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (string guid in kindGuids)
            {
                if (project.Kind.Equals(guid, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
