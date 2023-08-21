using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text;

namespace ImagePreview
{
    internal class ImageResult
    {
        public ImageResult(Span span, string rawImageString)
        {
            Span = span;
            RawImageString = rawImageString;
        }

        public Span Span { get; }
        public string RawImageString { get; }
    }
}
