using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace ImagePreview
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name(nameof(ImageQuickInfoProvider))]
    [ContentType("text")]
    internal class ImageQuickInfoProvider : IAsyncQuickInfoSourceProvider
    {
        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new ImageQuickInfoSource(textBuffer));
        }
    }
}
