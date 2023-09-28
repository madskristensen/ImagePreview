using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using EnvDTE;
using ImagePreview.Helpers;
using WpfApplication1.Classes;

namespace ImagePreview.Resolvers
{
    internal class FileImageResolver : IImageResolver
    {
        private static readonly string _pattern = @$"(?:^|[\s""'\<\>\(\)]|)(?<image>([a-z]:[\\./]+)?([\w\.\\\-/]+)(\.(?<ext>{BitmapImageCheck.Instance.AllSupportedExtensionsString})))\b";
        private static readonly Regex _regex = new(_pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string DisplayName => "File";

        public bool TryGetMatches(string lineText, out MatchCollection matches)
        {
            matches = _regex.Matches(lineText);
            return matches.Count > 0;
        }

        public async Task<string> GetResolvableUriAsync(ImageReference reference)
        {
            if (string.IsNullOrEmpty(reference?.RawImageString))
            {
                return null;
            }

            string rawFilePath = reference.RawImageString.Trim('\'', '"', '~');
            rawFilePath = Uri.UnescapeDataString(rawFilePath);
            bool isAbsolute = reference.RawImageString.Contains(":");
            string absolute;

            if (!isAbsolute)
            {
                // Find relative to source file
                string sourceDir = Path.GetDirectoryName(reference.SourceFilePath);
                absolute = Path.GetFullPath(Path.Combine(sourceDir, rawFilePath.TrimStart('/')));

                if (File.Exists(absolute))
                {
                    return absolute;
                }

                // Search all folders in the project
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                DTE dte = await VS.GetRequiredServiceAsync<DTE, DTE>();
                ProjectItem item = dte.Solution.FindProjectItem(reference.SourceFilePath);

                string projectRoot = item.ContainingProject?.GetRootFolder();

                if (string.IsNullOrEmpty(projectRoot))
                {
                    return null;
                }

                absolute = Path.GetFullPath(Path.Combine(projectRoot, rawFilePath.TrimStart('/')));

                if (!File.Exists(absolute))
                {
                    string fileName = rawFilePath.TrimStart('.', '/', '\\').Replace("/", "\\");
                    TryFindFile(fileName, projectRoot, out absolute);
                }
            }
            else
            {
                absolute = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(reference.SourceFilePath), rawFilePath));
            }

            return absolute;
        }

        internal bool TryFindFile(string searchPattern, string startDirectory, out string result)
        {
            string fileName = Path.GetFileName(searchPattern);
            result = Directory.EnumerateFiles(startDirectory, fileName, SearchOption.TopDirectoryOnly)
                              .FirstOrDefault(f => f.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase) > -1);

            if (!string.IsNullOrEmpty(result))
            {
                return true;
            }

            foreach (string directory in Directory.EnumerateDirectories(startDirectory))
            {
                if (directory.Contains("node_modules"))
                {
                    continue;
                }

                if (TryFindFile(searchPattern, directory, out result))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<BitmapSource> GetBitmapAsync(ImageReference result)
        {
            string absoluteFilePath = await GetResolvableUriAsync(result);

            if (string.IsNullOrEmpty(absoluteFilePath) || !File.Exists(absoluteFilePath))
            {
                return null;
            }

            result.SetFileSize(new FileInfo(absoluteFilePath).Length);

            if (result.ImageFileType == "SVG")
            {
                return SvgHelper.GetBitmapFromSvgFile(absoluteFilePath);
            }
            else
            {
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                bitmap.UriSource = new Uri(absoluteFilePath);
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
        }
    }
}