using System.Text.RegularExpressions;

namespace ImagePreview
{
    public static class FileSupport
    {
        public static ImageFormat GetImageFormat(this Match match)
        {
            return match.Groups["ext"]?.Value?.TrimStart('.').ToLowerInvariant() switch
            {
                "gif" => ImageFormat.GIF,
                "png" => ImageFormat.PNG,
                "jpg" or "jpeg" => ImageFormat.JPG,
                "ico" or "icon" => ImageFormat.ICO,
                "svg" => ImageFormat.SVG,
                _ => ImageFormat.Unknown
            };
        }
    }

    public enum ImageFormat
    {
        GIF,
        PNG,
        JPG,
        ICO,
        SVG,
        Unknown
    }
}
