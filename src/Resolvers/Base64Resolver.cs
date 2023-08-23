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
        private static readonly Regex _regex = new(@"data:image/[^;]+;base64,(?<image>([^\s=]+)=*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool TryGetMatches(string lineText, out MatchCollection matches)
        {
            matches = null;

            if (lineText.IndexOf("data:image/", StringComparison.OrdinalIgnoreCase) > -1)
            {
                matches = _regex.Matches(lineText);
                return true;
            }

            return false;
        }

        public Task<ImageReference> GetImageAsync(Span span, string value, string filePath)
        {
            return Task.FromResult(new ImageReference(span, value));
        }

        public Task<BitmapSource> GetBitmapAsync(ImageReference result)
        {
            if (string.IsNullOrEmpty(result.RawImageString))
            {
                return Task.FromResult<BitmapSource>(null);
            }

            byte[] imageBytes = Convert.FromBase64String(result.RawImageString);
            result.SetFileSize(imageBytes.Length);

            using (MemoryStream ms = new(imageBytes, 0, imageBytes.Length))
            {
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                return Task.FromResult<BitmapSource>(bitmap);
            }
        }
    }
}
