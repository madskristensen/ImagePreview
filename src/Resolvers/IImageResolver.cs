using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImagePreview.Resolvers
{
    internal interface IImageResolver
    {
        bool HasPotentialMatch(string lineText);

        Task<ImageResult> GetImageAsync(int cursorPosition, string lineText, string filePath);

        Task<BitmapSource> GetBitmapAsync(ImageResult result);
    }
}
