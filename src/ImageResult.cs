using Microsoft.VisualStudio.Text;

namespace ImagePreview
{
    /// <summary>
    /// Represents an image reference.
    /// </summary>
    internal class ImageReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageReference"/> class.
        /// </summary>
        /// <param name="span">The position of the image in the source text.</param>
        /// <param name="rawImageString">The image URI, file path, or other raw representation of the image or its reference.</param>
        public ImageReference(Span span, string rawImageString)
        {
            Span = span;
            RawImageString = rawImageString;
        }

        /// <summary>
        /// Gets or sets the position of the image in the source text.
        /// </summary>
        public Span Span { get; }

        /// <summary>
        /// Gets or sets the image string.
        /// </summary>
        public string RawImageString { get; }

        /// <summary>
        /// Gets the size of the image file.
        /// </summary>
        public long FileSize { get; private set; }

        /// <summary>
        /// Sets the size of the image file.
        /// </summary>
        /// <param name="fileSize">The size of the image file.</param>
        public void SetFileSize(long fileSize)
        {
            FileSize = fileSize;
        }
    }
}
