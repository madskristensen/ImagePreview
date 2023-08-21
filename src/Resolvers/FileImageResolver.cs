using System.IO;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using EnvDTE;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    internal class FileImageResolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"([a-z]:|[\./]+)?([\\\w\.\/\-]+)(\.(png|gif|jpg|jpeg|ico))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool HasPotentialMatch(string lineText)
        {
            return lineText.IndexOf(".png", StringComparison.OrdinalIgnoreCase) > -1 ||
                   lineText.IndexOf(".gif", StringComparison.OrdinalIgnoreCase) > -1 ||
                   lineText.IndexOf(".ico", StringComparison.OrdinalIgnoreCase) > -1 ||
                   lineText.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) > -1 ||
                   lineText.IndexOf(".jpeg", StringComparison.OrdinalIgnoreCase) > -1;
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

            rawFilePath = rawFilePath.Trim('\'', '"', '~');
            rawFilePath = Uri.UnescapeDataString(rawFilePath);
            string absolute;

            if (rawFilePath.StartsWith("/"))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                DTE dte = await VS.GetRequiredServiceAsync<DTE, DTE>();
                ProjectItem item = dte.Solution.FindProjectItem(absoluteSourceFile);
                //Project project = await VS.Solutions.GetActiveProjectAsync();
                //string projectRoot = Path.GetDirectoryName(project.FullPath);
                string projectRoot = Path.GetDirectoryName(item.ContainingProject?.FileName);
                absolute = Path.GetFullPath(Path.Combine(projectRoot, rawFilePath.TrimStart('/')));

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

        public BitmapImage GetBitmap(ImageResult result)
        {
            if (string.IsNullOrEmpty(result.RawImageString) || !File.Exists(result.RawImageString))
            {
                return null;
            }

            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(result.RawImageString, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
            bitmap.EndInit();

            return bitmap;
        }
    }
}
