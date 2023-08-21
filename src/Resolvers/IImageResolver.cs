using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    internal interface IImageResolver
    {
        bool TryGetMatches(string lineText, out MatchCollection matches);

        Task<ImageResult> GetImageAsync(Span span, string value, string filePat);

        Task<BitmapSource> GetBitmapAsync(ImageResult result);
    }
}
