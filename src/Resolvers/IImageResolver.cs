using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    /// <summary>
    ///  A base interface for all image resolvers to implement.
    /// </summary>
    internal interface IImageResolver
    {
        /// <summary>
        /// Tries to get regular expression matches from a given string.
        /// </summary>
        /// <param name="lineText">The string to retrieve matches from.</param>
        /// <param name="matches">Outputs the matched collection.</param>
        /// <returns>Returns true if matches are found, else false.</returns>
        bool TryGetMatches(string lineText, out MatchCollection matches);

        /// <summary>
        /// Asynchronously retrieves the image file and its metadata from a given file path.
        /// </summary>
        /// <param name="span">The character span that contains the image path reference.</param>
        /// <param name="value">The file path reference extracted from the matched string.</param>
        /// <param name="filePat">The file path of the source.</param>
        /// <returns>Returns the metadata information of the image file picked up.</returns>
        Task<ImageReference> GetImageReferenceAsync(Span span, string value, string filePat);

        /// <summary>
        /// Asynchronously decodes the image metadata into a <see cref="BitmapSource"/> object.
        /// </summary>
        /// <param name="result">The image metadata as represented by an <see cref="ImageReference"/> object.</param>
        /// <returns>Returns a <see cref="BitmapSource"/> object that can be used to display the image.</returns>
        Task<BitmapSource> GetBitmapAsync(ImageReference result);
    }
}
