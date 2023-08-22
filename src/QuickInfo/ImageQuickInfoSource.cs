using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        public ImageQuickInfoSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        public static readonly List<IImageResolver> Resolvers = new()
        {
            new Base64Resolver(),
            new PackResolver(),
            new HttpImageResolver(),
            new FileImageResolver(),
        };

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            ITrackingPoint point = session.GetTriggerPoint(_textBuffer);
            int cursorPosition = point.GetPosition(_textBuffer.CurrentSnapshot);
            ITextSnapshotLine line = _textBuffer.CurrentSnapshot.GetLineFromPosition(cursorPosition);
            string lineText = line.GetText();

            foreach (IImageResolver resolver in Resolvers)
            {
                try
                {
                    if (!resolver.TryGetMatches(lineText, out MatchCollection matches))
                    {
                        continue;
                    }

                    foreach (Match match in matches)
                    {
                        Span span = new(line.Start + match.Index, match.Length);

                        // Perf: Break the loop if image refs are located after the cursor position
                        if (span.Start > cursorPosition)
                        {
                            break;
                        }

                        if (span.Contains(cursorPosition))
                        {
                            return await GenerateQuickInfoAsync(resolver, match, span);
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

        private async Task<QuickInfoItem> GenerateQuickInfoAsync(IImageResolver resolver, Match match, Span span)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            ImageResult result = await resolver.GetImageAsync(span, match.Groups["image"].Value.Trim(), _textBuffer.GetFileName());
            ITrackingSpan trackingSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(result.Span.Start, result.Span.Length, SpanTrackingMode.EdgeExclusive);

            if (result?.RawImageString != null)
            {
                BitmapSource bitmap = await resolver.GetBitmapAsync(result);

                if (bitmap != null)
                {
                    UIElement element = CreateUiElement(bitmap, result);
                    return new QuickInfoItem(trackingSpan, element);
                }
            }

            return new QuickInfoItem(trackingSpan, "Could not resolve image for preview");
        }

        private static UIElement CreateUiElement(BitmapSource bitmap, ImageResult result)
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
            return panel;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
