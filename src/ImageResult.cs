using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text;

namespace ImagePreview
{
    internal class ImageResult(Span span, string rawImageString)
    {
        public Span Span { get; } = span;
        public string RawImageString { get; } = rawImageString;
    }
}
