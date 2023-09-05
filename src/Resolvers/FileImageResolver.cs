using System.IO;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using EnvDTE;
using ImagePreview.Helpers;

namespace ImagePreview.Resolvers
{
    internal class FileImageResolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"(?:^|[\s""'\<\>\(\)]|)(?<image>([a-z]:[\\./]+)?([\w\.\\\-/]+)(\.(?<ext>png|gif|jpg|jpeg|ico|svg|tif|tiff|bmp|wmp)))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string DisplayName => "File";

        public bool TryGetMatches(string lineText, out MatchCollection matches)
        {
            matches = null;

            if (lineText.IndexOf(".png", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".gif", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".ico", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".svg", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".tif", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".tiff", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".bmp", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".wmp", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".jpeg", StringComparison.OrdinalIgnoreCase) > -1)
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

            string rawFilePath = reference.RawImageString.Trim('\'', '"', '~');
            rawFilePath = Uri.UnescapeDataString(rawFilePath);
            string absolute;

            if (rawFilePath.StartsWith("/"))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                DTE dte = await VS.GetRequiredServiceAsync<DTE, DTE>();
                ProjectItem item = dte.Solution.FindProjectItem(reference.SourceFilePath);

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
                absolute = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(reference.SourceFilePath), rawFilePath));
            }

            return absolute;
        }

        public async Task<BitmapImage> GetBitmapAsync(ImageReference result)
        {
            string absoluteFilePath = await GetResolvableUriAsync(result);

            if (string.IsNullOrEmpty(absoluteFilePath) || !File.Exists(absoluteFilePath))
            {
                return null;
            }

            result.SetFileSize(new FileInfo(absoluteFilePath).Length);

            if (result.RawImageString.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
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