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
        private static readonly Regex _regex = new(@";component/[^""]+\.(png|gif|jpg|jpeg|ico)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool HasPotentialMatch(string lineText)
        {
            return lineText.IndexOf(";component/", StringComparison.OrdinalIgnoreCase) > -1;
        }

        public async Task<ImageResult> GetImageAsync(int cursorPosition, string lineText, string filePath)
        {
            MatchCollection matches = _regex.Matches(lineText);

            foreach (Match match in matches)
            {
                Span span = new(match.Index, match.Length);

                if (span.Contains(cursorPosition))
                {
                    string absoluteUrl = await GetFullUrlAsync(match.Value, filePath);
                    return new ImageResult(span, absoluteUrl);
                }
            }

            return null;
        }

        public static async Task<string> GetFullUrlAsync(string rawFilePath, string absoluteSourceFile)
        {
            if (string.IsNullOrEmpty(rawFilePath))
            {
                return null;
            }

            rawFilePath = rawFilePath.Substring(10);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = await VS.GetRequiredServiceAsync<DTE, DTE>();
            ProjectItem item = dte.Solution.FindProjectItem(absoluteSourceFile);

            string projectRoot = Path.GetDirectoryName(item.ContainingProject?.FileName);
            return Path.GetFullPath(Path.Combine(projectRoot, rawFilePath.TrimStart('/')));
        }

        public Task<BitmapSource> GetBitmapAsync(ImageResult result)
        {
            if (string.IsNullOrEmpty(result.RawImageString) || !File.Exists(result.RawImageString))
            {
                return Task.FromResult<BitmapSource>(null);
            }

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
