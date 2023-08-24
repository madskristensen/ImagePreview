using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using EnvDTE;
using ImagePreview.Helpers;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    internal class FileImageResolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"(?:^|[\s""'\<\>\(\)]|)(?<image>([a-z]:[\\./]+)?([\w\.\\\-/]+)(\.(?<ext>png|gif|jpg|jpeg|ico|svg)))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool TryGetMatches(string lineText, out MatchCollection matches)
        {
            matches = null;

            if (lineText.IndexOf(".png", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".gif", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".ico", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".svg", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".jpeg", StringComparison.OrdinalIgnoreCase) > -1)
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

            rawFilePath = rawFilePath.Trim('\'', '"', '~');
            rawFilePath = Uri.UnescapeDataString(rawFilePath);
            string absolute;

            if (rawFilePath.StartsWith("/"))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                DTE dte = await VS.GetRequiredServiceAsync<DTE, DTE>();
                ProjectItem item = dte.Solution.FindProjectItem(absoluteSourceFile);

                string projectRoot = item.ContainingProject?.GetRootFolder();
                absolute = Path.GetFullPath(Path.Combine(projectRoot, rawFilePath.TrimStart('/')));

                // Check the wwwroot sub folder which is used in ASP.NET Core projects
                if (!File.Exists(absolute))
                {
                    absolute = Path.GetFullPath(Path.Combine(projectRoot, "wwwroot", rawFilePath.TrimStart('/')));
                }
            }
            else
            {
                absolute = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(absoluteSourceFile), rawFilePath));
            }

            return absolute;
        }

        public Task<BitmapSource> GetBitmapAsync(ImageReference result)
        {
            if (string.IsNullOrEmpty(result.RawImageString) || !File.Exists(result.RawImageString))
            {
                return Task.FromResult<BitmapSource>(null);
            }

            TaskCompletionSource<BitmapSource> tcs = new();

            result.SetFileSize(new FileInfo(result.RawImageString).Length);

            if (result.RawImageString.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                tcs.SetResult(SvgHelper.GetBitmapFromSvgFile(result.RawImageString));
            }
            else
            {
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
            }

            return tcs.Task;
        }
    }
}
