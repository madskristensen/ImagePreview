﻿using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImagePreview.QuickInfo;
using Microsoft.VisualStudio.Language.Intellisense;
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
            if (result?.RawImageString == null)
            {
                return null;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            ITrackingSpan trackingSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(result.Span.Start, result.Span.Length, SpanTrackingMode.EdgeExclusive);

            try
            {
                PreviewControl control = new();

                ThreadHelper.JoinableTaskFactory.StartOnIdle(async () =>
                {
                    BitmapImage bitmap = await result.Resolver.GetBitmapAsync(result);
                    string url = await result.Resolver.GetResolvableUriAsync(result);

                    if (control.SetImage(bitmap, result, url))
                    {
                        _prompt.RegisterSuccessfulUsage();
                    }
                }, VsTaskRunContext.UIThreadIdlePriority).FireAndForget();

                return new QuickInfoItem(trackingSpan, control);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }

            return new QuickInfoItem(trackingSpan, "Could not resolve image for preview");
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
