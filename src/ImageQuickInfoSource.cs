using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImagePreview.Resolvers;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;

namespace ImagePreview
{
    internal class ImageQuickInfoSource : IAsyncQuickInfoSource
    {
        private readonly ITextBuffer _textBuffer;
        private readonly ITextDocument _document;
        private readonly List<IImageResolver> _resolvers = new()
        {
            new HttpImageResolver(),
            new FileImageResolver(),
            new Base64Resolver(),
            new PackResolver(),
        };

        public ImageQuickInfoSource(ITextBuffer textBuffer, ITextDocument document)
        {
            _textBuffer = textBuffer;
            _document = document;
        }

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            ITrackingPoint point = session.GetTriggerPoint(_textBuffer);
            int position = point.GetPosition(_textBuffer.CurrentSnapshot);
            ITextSnapshotLine line = _textBuffer.CurrentSnapshot.GetLineFromPosition(position);
            string lineText = line.GetText();

            foreach (IImageResolver resolver in _resolvers)
            {
                try
                {
                    if (!resolver.HasPotentialMatch(lineText))
                    {
                        continue;
                    }

                    ImageResult result = await resolver.GetImageAsync(position - line.Start, lineText, _document.FilePath);

                    if (result != null)
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        BitmapSource bitmap = await resolver.GetBitmapAsync(result);

                        if (bitmap != null)
                        {
                            UIElement element = CreateUiElement(bitmap);
                            ITrackingSpan span = _textBuffer.CurrentSnapshot.CreateTrackingSpan(line.Start + result.Span.Start, result.Span.Length, SpanTrackingMode.EdgeExclusive);

                            return new QuickInfoItem(span, element);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await ex.LogAsync();
                }
            }

            return null;
        }

        private static UIElement CreateUiElement(BitmapSource bitmap)
        {
            Image image = new()
            {
                Source = bitmap,
                MaxWidth = Math.Min(Application.Current.MainWindow.Width / 2, 500),
                MaxHeight = Math.Min(Application.Current.MainWindow.Height / 2, 500),
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly
            };

            Label label = new() { Content = $"{Math.Round(bitmap.Width)}x{Math.Round(bitmap.Height)}" };
            label.SetResourceReference(TextBlock.ForegroundProperty, EnvironmentColors.ComboBoxFocusedTextBrushKey);

            StackPanel panel = new() { Orientation = Orientation.Vertical };
            panel.Children.Add(image);
            panel.Children.Add(label);
            return panel;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
