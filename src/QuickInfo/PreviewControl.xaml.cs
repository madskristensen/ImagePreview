using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.PlatformUI;

namespace ImagePreview.QuickInfo
{
    /// <summary>
    /// Interaction logic for PreviewControl.xaml
    /// </summary>
    public partial class PreviewControl : UserControl
    {
        public PreviewControl()
        {
            InitializeComponent();
            
            lblSize.SetResourceReference(TextBlock.ForegroundProperty, EnvironmentColors.ComboBoxFocusedTextBrushKey);
        }

        public bool SetImage(BitmapImage bitmap, ImageReference result, string url)
        {
            if (bitmap == null)
            {
                lblSize.Content = "Could not resolve image for preview";
                return false;
            }

            if (result.Format == ImageFormat.GIF && Uri.TryCreate(url, UriKind.Absolute, out Uri absoluteUri))
            {
                mediaPreview.Source = absoluteUri;
                mediaPreview.Visibility = Visibility.Visible;
            }
            else
            {
                imgPreview.Source = bitmap;
                imgPreview.Visibility = Visibility.Visible;
            }

            lblSize.Content = $"{Math.Round(bitmap.Width)}x{Math.Round(bitmap.Height)} ({result.FileSize.ToFileSize(2)})";
            panel.Visibility = Visibility.Visible;

            return true;
        }
    }
}
