using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace ImagePreview.Classification
{
    internal class ImageTagger : ITagger<IntraTextAdornmentTag>, IDisposable
    {
        public IEnumerable<ITagSpan<IntraTextAdornmentTag>> GetTags(NormalizedSnapshotSpanCollection spans)
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
                            SnapshotSpan matchSpan = new(span.Snapshot, span.Start + match.Index + match.Length, 0);
                            CrispImage image = new()
                            {
                                Moniker = KnownMonikers.Image,
                                Width = 10,
                                Margin = new System.Windows.Thickness(0, -20, 0, 0),
                            };

                            IntraTextAdornmentTag tag = new(image, null, PositionAffinity.Predecessor);

                            yield return new TagSpan<IntraTextAdornmentTag>(matchSpan, tag);
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