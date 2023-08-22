using System.IO;
using System.Net.Cache;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    internal class HttpImageResolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"(?<image>(https?:|ftp:)?//[\w/\-?=%.\\]+\.(png|gif|jpg|jpeg|ico))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool TryGetMatches(string lineText, out MatchCollection matches)
        {
            matches = null;

            if (lineText.IndexOf(".png", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".gif", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".ico", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".jpeg", StringComparison.OrdinalIgnoreCase) > -1)
            {
                matches = _regex.Matches(lineText);
                return true;
            }

            return false;
        }

        public Task<ImageReference> GetImageAsync(Span span, string value, string filePat)
        {
            string absoluteUrl = GetFullUrl(value);
            return Task.FromResult(new ImageReference(span, absoluteUrl));
        }

        public static string GetFullUrl(string rawFilePath)
        {
            if (string.IsNullOrEmpty(rawFilePath))
            {
                return null;
            }

            rawFilePath = rawFilePath.Trim('\'', '"', '~');

            if (rawFilePath.StartsWith("//", StringComparison.Ordinal))
            {
                rawFilePath = "http:" + rawFilePath;
            }

            return Uri.TryCreate(rawFilePath, UriKind.Absolute, out Uri result) ? result.OriginalString : null;
        }

        public async Task<BitmapSource> GetBitmapAsync(ImageReference result)
        {
            using (HttpClient client = new())
            {
                byte[] imageBytes = await client.GetByteArrayAsync(result.RawImageString);
                result.SetFileSize(imageBytes.Length);

                using (MemoryStream ms = new(imageBytes, 0, imageBytes.Length))
                {
                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();

                    return bitmap;
                }
            }
        }
    }
}
