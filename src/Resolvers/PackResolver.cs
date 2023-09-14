using System.IO;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using EnvDTE;
using WpfApplication1.Classes;

namespace ImagePreview.Resolvers
{
    internal class PackResolver : IImageResolver
    {
        private static readonly string _pattern = $@"(pack://application:[^/]+)?/[\w]+;component/(?<image>[^""]+\.(?<ext>{BitmapImageCheck.Instance.AllSupportedExtensionsString}))\b";
        private static readonly Regex _regex = new(_pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string DisplayName => "Pack URI";

        public bool TryGetMatches(string lineText, out MatchCollection matches)
        {
            matches = null;

            if (lineText.IndexOf(";component/", StringComparison.OrdinalIgnoreCase) > -1)
            {
                matches = _regex.Matches(lineText);
                return true;
            }

            return false;
        }

        public async Task<string> GetResolvableUriAsync(ImageReference reference)
        {
            if (string.IsNullOrEmpty(reference?.RawImageString))
            {
                return null;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = await VS.GetRequiredServiceAsync<DTE, DTE>();
            ProjectItem item = dte.Solution.FindProjectItem(reference.SourceFilePath);

            string projectRoot = item.ContainingProject?.GetRootFolder();
            return Path.GetFullPath(Path.Combine(projectRoot, reference.RawImageString.TrimStart('/')));
        }

        public async Task<BitmapSource> GetBitmapAsync(ImageReference reference)
        {
            string absoluteFilePath = await GetResolvableUriAsync(reference);

            if (string.IsNullOrEmpty(absoluteFilePath) || !File.Exists(absoluteFilePath))
            {
                return null;
            }

            reference.SetFileSize(new FileInfo(absoluteFilePath).Length);

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
