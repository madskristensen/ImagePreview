using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Telemetry;
using Microsoft.VisualStudio.Text;

namespace ImagePreview
{
    internal class ImageQuickInfoSource : IAsyncQuickInfoSource
    {
        private readonly ITextBuffer _textBuffer;
        private static readonly RatingPrompt _prompt = new("MadsKristensen.ImagePreview", Vsix.Name, General.Instance);

        public ImageQuickInfoSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            ImageReference reference = await session.GetTriggerPoint(_textBuffer).FindImageReferencesAsync();

            return reference != null ? await GenerateQuickInfoAsync(reference) : null;
        }

        private async Task<QuickInfoItem> GenerateQuickInfoAsync(ImageReference result)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            ITrackingSpan trackingSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(result.Span.Start, result.Span.Length, SpanTrackingMode.EdgeExclusive);

            try
            {
                if (result?.RawImageString != null)
                {
                    BitmapSource bitmap = await result.Resolver.GetBitmapAsync(result);

                    if (bitmap != null)
                    {
                        UIElement element = CreateUiElement(bitmap, result);

                        ImageFormat format = result.Format;
                        TelemetryEvent tel = Telemetry.CreateEvent("ShowPreview");
                        tel.Properties["Format"] = format;
                        tel.Properties["Success"] = bitmap != null;
                        Telemetry.TrackEvent(tel);

                        return new QuickInfoItem(trackingSpan, element);
                    }
                }
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }

            return new QuickInfoItem(trackingSpan, "Could not resolve image for preview");
        }

        private static UIElement CreateUiElement(BitmapSource bitmap, ImageReference result)
        {
            Image image = new()
            {
                Source = bitmap,
                MaxWidth = Math.Min(Application.Current.MainWindow.Width / 2, 500),
                MaxHeight = Math.Min(Application.Current.MainWindow.Height / 2, 500),
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly
            };

            Label label = new() { Content = $"{Math.Round(bitmap.Width)}x{Math.Round(bitmap.Height)} ({result.FileSize.ToFileSize(2)})" };
            label.SetResourceReference(TextBlock.ForegroundProperty, EnvironmentColors.ComboBoxFocusedTextBrushKey);

            StackPanel panel = new() { Orientation = Orientation.Vertical };
            panel.Children.Add(image);
            panel.Children.Add(label);

            _prompt.RegisterSuccessfulUsage();

            return panel;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
