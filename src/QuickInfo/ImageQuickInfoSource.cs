using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImagePreview.QuickInfo;
using Microsoft.VisualStudio.Language.Intellisense;
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

            return reference != null ? await GetQuickInfoItemAsync(reference) : null;
        }

        private async Task<QuickInfoItem> GetQuickInfoItemAsync(ImageReference result)
        {
            TelemetryEvent tel = Telemetry.CreateEvent("showpreview");
            tel.Properties["resolver"] = result?.Resolver?.DisplayName;
            tel.Properties["format"] = result?.Format;
            tel.Properties.Add("success", false);

            if (result?.RawImageString == null)
            {
                Telemetry.TrackEvent(tel);
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

                    tel.Properties["success"] = bitmap != null;
                    Telemetry.TrackEvent(tel);

                }, VsTaskRunContext.UIThreadIdlePriority).FireAndForget();

                return new QuickInfoItem(trackingSpan, control);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
                Telemetry.TrackEvent(tel);
            }

            return new QuickInfoItem(trackingSpan, "Could not resolve image for preview");
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
