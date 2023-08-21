using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace ImagePreview.Classification
{
    internal class ImageTagger : ITagger<IClassificationTag>, IDisposable
    {
        private readonly IClassificationType _italic;

        public ImageTagger(IClassificationTypeRegistryService _classificationRegistry)
        {
            _italic = _classificationRegistry.GetClassificationType(ClassificationTypeDefinitions.ImageReference);
        }

        public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan span in spans)
            {
                if (span.IsEmpty)
                {
                    yield break;
                }

                string text = span.GetText();

                foreach (Resolvers.IImageResolver resolver in ImageQuickInfoSource.Resolvers)
                {
                    if (resolver.TryGetMatches(text, out MatchCollection matches))
                    {
                        foreach (Match match in matches)
                        {
                            SnapshotSpan matchSpan = new(span.Snapshot, span.Start + match.Index, match.Length);
                            yield return new TagSpan<IClassificationTag>(matchSpan, new ClassificationTag(_italic));
                        }
                    }
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}