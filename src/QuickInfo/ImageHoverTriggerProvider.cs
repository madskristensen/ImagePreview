using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using ImagePreview.QuickInfo;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ImagePreview
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal class ImageHoverTriggerProvider : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView)
        {
            _ = new ImageHoverHandler(textView);
        }

        private class ImageHoverHandler
        {
            private readonly IWpfTextView _textView;
            private Popup _activePopup;

            public ImageHoverHandler(IWpfTextView textView)
            {
                _textView = textView;
                textView.MouseHover += OnMouseHover;
                textView.Closed += OnClosed;
            }

            private void OnClosed(object sender, EventArgs e)
            {
                _textView.MouseHover -= OnMouseHover;
                _textView.Closed -= OnClosed;
                ClosePopup();
            }

            private void OnMouseHover(object sender, MouseHoverEventArgs e)
            {
                OnMouseHoverAsync(e).FireAndForget();
            }

            private async Task OnMouseHoverAsync(MouseHoverEventArgs e)
            {
                try
                {
                    ITrackingPoint triggerPoint = _textView.TextSnapshot.CreateTrackingPoint(
                        e.Position, PointTrackingMode.Positive);

                    ImageReference reference = await triggerPoint.FindImageReferencesAsync();

                    if (reference != null && !_textView.IsClosed)
                    {
                        // FindImageReferencesAsync already switches to UI thread on success
                        ClosePopup();

                        PreviewControl control = new();

                        _activePopup = new Popup
                        {
                            Child = control,
                            Placement = PlacementMode.Mouse,
                            StaysOpen = false,
                            IsOpen = true,
                        };

                        ThreadHelper.JoinableTaskFactory.StartOnIdle(async () =>
                        {
                            BitmapSource bitmap = await reference.Resolver.GetBitmapAsync(reference);
                            string url = await reference.Resolver.GetResolvableUriAsync(reference);
                            control.SetImage(bitmap, reference, url);
                        }, VsTaskRunContext.UIThreadIdlePriority).FireAndForget();
                    }
                }
                catch (Exception ex)
                {
                    await ex.LogAsync();
                }
            }

            private void ClosePopup()
            {
                if (_activePopup != null)
                {
                    _activePopup.IsOpen = false;
                    _activePopup = null;
                }
            }
        }
    }
}
