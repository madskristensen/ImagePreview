using System.IO;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    internal class Base64Resolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"data:image/[^;]+;base64,[^\s=]+==?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool HasPotentialMatch(string lineText)
        {
            return lineText.IndexOf("data:image/", StringComparison.OrdinalIgnoreCase) > -1;
        }

        public Task<ImageResult> GetImageAsync(int cursorPosition, string lineText, string filePath)
        {
            MatchCollection matches = _regex.Matches(lineText);

            foreach (Match match in matches)
            {
                Span span = new(match.Index, match.Length);

                if (span.Contains(cursorPosition))
                {
                    return Task.FromResult(new ImageResult(span, match.Value));
                }
            }

            return Task.FromResult<ImageResult>(null);
        }

        public BitmapImage GetBitmap(ImageResult result)
        {
            if (string.IsNullOrEmpty(result.RawImageString))
            {
                return null;
            }

            int index = result.RawImageString.IndexOf("base64,", StringComparison.Ordinal) + 7;
            byte[] imageBytes = Convert.FromBase64String(result.RawImageString.Substring(index));

            using (MemoryStream ms = new(imageBytes, 0, imageBytes.Length))
            {
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                bitmap.EndInit();

                return bitmap;
            }
        }
    }
}
