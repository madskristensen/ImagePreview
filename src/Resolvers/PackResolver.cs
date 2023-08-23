using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using EnvDTE;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    internal class PackResolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"(pack://application:[^/]+)?/[\w]+;component/(?<image>[^""]+\.(png|gif|jpg|jpeg|ico))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

        public async Task<ImageReference> GetImageReferenceAsync(Span span, string value, string filePath)
        {
            string absoluteUrl = await GetFullUrlAsync(value, filePath);
            return new ImageReference(span, absoluteUrl);
        }

        public static async Task<string> GetFullUrlAsync(string rawFilePath, string absoluteSourceFile)
        {
            if (string.IsNullOrEmpty(rawFilePath))
            {
                return null;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = await VS.GetRequiredServiceAsync<DTE, DTE>();
            ProjectItem item = dte.Solution.FindProjectItem(absoluteSourceFile);

            string projectRoot = item.ContainingProject?.GetRootFolder();
            return Path.GetFullPath(Path.Combine(projectRoot, rawFilePath.TrimStart('/')));
        }

        public Task<BitmapSource> GetBitmapAsync(ImageReference result)
        {
            if (string.IsNullOrEmpty(result.RawImageString) || !File.Exists(result.RawImageString))
            {
                return Task.FromResult<BitmapSource>(null);
            }

            result.SetFileSize(new FileInfo(result.RawImageString).Length);

            TaskCompletionSource<BitmapSource> tcs = new();
            BitmapImage bitmap = new(new Uri(result.RawImageString));

            if (bitmap.IsDownloading)
            {
                bitmap.DownloadCompleted += (s, e) => tcs.SetResult(bitmap);
                bitmap.DownloadFailed += (s, e) => tcs.SetException(e.ErrorException);
            }
            else
            {
                tcs.SetResult(bitmap);
            }

            return tcs.Task;
        }
    }
}
