using Microsoft.VisualStudio.Text;

namespace ImagePreview
{
    internal record class ImageResult(Span Span, string RawImageString);
}
