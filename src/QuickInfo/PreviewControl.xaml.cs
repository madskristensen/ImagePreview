using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Telemetry;

namespace ImagePreview.QuickInfo
{
    /// <summary>
    /// Interaction logic for PreviewControl.xaml
    /// </summary>
    public partial class PreviewControl : UserControl
    {
        private ImageReference _imageReference;

        public PreviewControl()
        {
            InitializeComponent();
            lblSize.SetResourceReference(TextBlock.ForegroundProperty, EnvironmentColors.ComboBoxFocusedTextBrushKey);
        }

        public bool SetImage(BitmapSource bitmap, ImageReference result, string url)
        {
            _imageReference = result;

            if (bitmap == null)
            {
                lblSize.Content = "Could not resolve image for preview";
                return false;
            }

            bool isAbsoluteUri = Uri.TryCreate(url, UriKind.Absolute, out Uri absoluteUri);

            if (result.ImageFileType == "GIF" && isAbsoluteUri)
            {
                mediaPreview.Source = absoluteUri;
                mediaPreview.Visibility = Visibility.Visible;
                SetupClickHandler(isAbsoluteUri, absoluteUri, mediaPreview);

            }
            else
            {
                imgPreview.Source = bitmap;
                imgPreview.Visibility = Visibility.Visible;
                SetupClickHandler(isAbsoluteUri, absoluteUri, imgPreview);
            }

            lblSize.Content = $"{Math.Round(bitmap.Width)}x{Math.Round(bitmap.Height)} ({result.FileSize.ToFileSize(2)})";
            panel.Visibility = Visibility.Visible;

            return true;
        }

        private void SetupClickHandler(bool isAbsoluteUri, Uri absoluteUri, FrameworkElement element)
        {
            if (isAbsoluteUri)
            {
                element.Cursor = Cursors.Hand;
                element.ToolTip = "Click to open image in default application";
                element.MouseUp += (s, e) =>
                {
                    TelemetryEvent tel = Telemetry.CreateEvent("click_image");
                    tel.Properties["resolver"] = _imageReference?.Resolver?.DisplayName;
                    tel.Properties["format"] = _imageReference?.ImageFileType ?? "Unknown";
                    tel.Properties["filetype"] = Path.GetExtension(_imageReference?.SourceFilePath ?? "").ToLowerInvariant();
                    Telemetry.TrackEvent(tel);
                    System.Diagnostics.Process.Start(absoluteUri.OriginalString);
                };
            }
        }
    }
}
