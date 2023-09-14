using System.IO;
using System.Net.Cache;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImagePreview.Helpers;
using WpfApplication1.Classes;

namespace ImagePreview.Resolvers
{
    internal class HttpImageResolver : IImageResolver
    {
        private static readonly string _pattern = $@"(?<image>(https?:|ftp:)?//[\w/\-?=%.\\]+\.(?<ext>{BitmapImageCheck.Instance.AllSupportedExtensionsString}))\b";
        private static readonly Regex _regex = new(_pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string DisplayName => "HTTP";

        public bool TryGetMatches(string lineText, out MatchCollection matches)
        {
            matches = _regex.Matches(lineText);
            return matches.Count > 0;
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

                    if (result.ImageFileType == "SVG")
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
