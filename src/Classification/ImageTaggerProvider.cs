using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace ImagePreview.Classification
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(IClassificationTag))]
    internal class ImageTaggerProvider : ITaggerProvider
    {
        [Import]
        internal IClassificationTypeRegistryService _classificationRegistry = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ImageTagger tagger = new(_classificationRegistry);
            return buffer.Properties.GetOrCreateSingletonProperty(() => tagger) as ITagger<T>;
        }
    }
}
