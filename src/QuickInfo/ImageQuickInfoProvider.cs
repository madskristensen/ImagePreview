using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace ImagePreview
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name(nameof(ImageQuickInfoProvider))]
    [ContentType("any")]
    [Order]
    internal class ImageQuickInfoProvider : IAsyncQuickInfoSourceProvider
    {
        [Import]
        internal ITextDocumentFactoryService _documentService = null;

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            if (_documentService.TryGetTextDocument(textBuffer, out ITextDocument document))
            {
                return textBuffer.Properties.GetOrCreateSingletonProperty(() => new ImageQuickInfoSource(textBuffer, document));
            }

            return null;
        }
    }
}
