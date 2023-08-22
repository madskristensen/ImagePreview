using System.IO;

namespace ImagePreview
{
    internal static class ExtensionMethods
    {
        private static readonly string[] _sizeSuffixes = new[] { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

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

            if (value < 1024)
            {
                return string.Format("{0:n0} {1}", adjustedSize, _sizeSuffixes[mag]);
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, _sizeSuffixes[mag]);
        }

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

            if (string.IsNullOrEmpty(fullPath))
            {
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null;
            }

            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }

            return File.Exists(fullPath) ? Path.GetDirectoryName(fullPath) : null;
        }

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
