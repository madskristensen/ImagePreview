using System.IO;
using System.Net.Cache;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImagePreview.Helpers;

namespace ImagePreview.Resolvers
{
    internal class HttpImageResolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"(?<image>(https?:|ftp:)?//[\w/\-?=%.\\]+\.(?<ext>png|gif|jpg|jpeg|ico|svg|tif|tiff|bmp|wmp))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string DisplayName => "HTTP";

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

        public Task<string> GetResolvableUriAsync(ImageReference reference)
        {
            if (string.IsNullOrEmpty(reference?.RawImageString))
            {
                return Task.FromResult<string>(null);
            }

            string rawFilePath = reference.RawImageString.Trim('\'', '"', '~');

            if (rawFilePath.StartsWith("//", StringComparison.Ordinal))
            {
                rawFilePath = "http:" + rawFilePath;
            }

            return Uri.TryCreate(rawFilePath, UriKind.Absolute, out Uri result) ? Task.FromResult(result.OriginalString) : Task.FromResult<string>(null);
        }

        public async Task<BitmapSource> GetBitmapAsync(ImageReference result)
        {
            try
            {
                using (HttpClient client = new())
                {
                    byte[] imageBytes = await client.GetByteArrayAsync(await GetResolvableUriAsync(result));
                    result.SetFileSize(imageBytes.Length);

                    if (result.Format == ImageFormat.SVG)
                    {
                        return SvgHelper.GetBitmapFromSvgFile(imageBytes);
                    }
                    else
                    {
                        using (MemoryStream ms = new(imageBytes, 0, imageBytes.Length))
                        {
                            BitmapImage bitmap = new();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                            bitmap.Freeze();

                            return bitmap;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
                return null;
            }
        }
    }
}
