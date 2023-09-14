using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.PlatformUI;

namespace ImagePreview.QuickInfo
{
    /// <summary>
    /// Interaction logic for PreviewControl.xaml
    /// </summary>
    public partial class PreviewControl : UserControl, IDisposable
    {
        public PreviewControl()
        {
            InitializeComponent();

            lblSize.SetResourceReference(TextBlock.ForegroundProperty, EnvironmentColors.ComboBoxFocusedTextBrushKey);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool SetImage(BitmapSource bitmap, ImageReference result, string url)
        {
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
                element.MouseUp += (s, e) => { System.Diagnostics.Process.Start(absoluteUri.OriginalString); };
            }
        }
    }
}
