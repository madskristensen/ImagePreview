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
        public long FileSize { get; private set; }

        public void SetFileSize(long fileSize)
        {
            FileSize = fileSize;
        }
    }
}
