using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Svg;

namespace ImagePreview.Helpers
{
    internal class SvgHelper
    {
        /// <summary>
        /// Converts an Svg file to a BitmapImage.
        /// </summary>
        /// <param name="filePath">The path of the Svg file to convert.</param>
        /// <returns>The BitmapImage representing the Svg file.</returns>
        public static BitmapImage GetBitmapFromSvgFile(string filePath)
        {
            return GetBitmapFromSvgFile(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// Converts an Svg file to a BitmapImage.
        /// </summary>
        /// <param name="buffer">The byte array representing the svg file.</param>
        /// <returns>The BitmapImage representing the Svg file.</returns>
        public static BitmapImage GetBitmapFromSvgFile(byte[] buffer)
        {
            using (MemoryStream byteStream = new(buffer))
            {
                SvgDocument svg = SvgDocument.Open<SvgDocument>(byteStream);

                if (svg == null)
                {
                    return null;
                }

                Size size = CalculateDimensions(new Size(svg.Width.Value, svg.Height.Value));

                using (System.Drawing.Bitmap bmp = svg.Draw((int)size.Width, (int)size.Height))
                using (MemoryStream ms = new())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;

                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    return bitmap;
                }
            }
        }

        /// <summary>
        /// Calculates the dimensions of the destination image when converting from an Svg file.
        /// </summary>
        /// <param name="currentSize">The dimensions of the original Svg file.</param>
        /// <returns>The dimensions of the destination image.</returns>
        private static Size CalculateDimensions(Size currentSize)
        {
            double sourceWidth = currentSize.Width;
            double sourceHeight = currentSize.Height;

            double widthPercent = 500 / sourceWidth;
            double heightPercent = 500 / sourceHeight;

            double percent = Math.Min(heightPercent, widthPercent);

            int destinationWidth = (int)(sourceWidth * percent);
            int destinationHeight = (int)(sourceHeight * percent);

            return new Size(destinationWidth, destinationHeight);
        }
    }
}
