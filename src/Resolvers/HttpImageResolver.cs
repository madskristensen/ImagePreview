using System.IO;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    internal class HttpImageResolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"(https?:|ftp:)?//[\w/\-?=%.\\ ]+\.(png|gif|jpg|jpeg|ico)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool HasPotentialMatch(string lineText)
        {
            return lineText.IndexOf(".png", StringComparison.OrdinalIgnoreCase) > -1 ||
                   lineText.IndexOf(".gif", StringComparison.OrdinalIgnoreCase) > -1 ||
                   lineText.IndexOf(".ico", StringComparison.OrdinalIgnoreCase) > -1 ||
                   lineText.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) > -1 ||
                   lineText.IndexOf(".jpeg", StringComparison.OrdinalIgnoreCase) > -1;
        }

        public Task<ImageResult> GetImageAsync(int cursorPosition, string lineText, string filePath)
        {
            MatchCollection matches = _regex.Matches(lineText);

            foreach (Match match in matches)
            {
                Span span = new(match.Index, match.Length);

                if (span.Contains(cursorPosition))
                {
                    string absoluteUrl = GetFullUrl(match.Value);
                    return Task.FromResult(new ImageResult(span, absoluteUrl));
                }
            }

            return Task.FromResult<ImageResult>(null);
        }

        public static string GetFullUrl(string rawFilePath)
        {
            if (string.IsNullOrEmpty(rawFilePath))
            {
                return null;
            }

            rawFilePath = rawFilePath.Trim(['\'', '"', '~']);

            if (rawFilePath.StartsWith("//", StringComparison.Ordinal))
            {
                rawFilePath = "http:" + rawFilePath;
            }

            return Uri.TryCreate(rawFilePath, UriKind.Absolute, out Uri result) ? result.OriginalString : null;
        }

        public BitmapImage GetBitmap(ImageResult result)
        {
            if (string.IsNullOrEmpty(result.RawImageString))
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
