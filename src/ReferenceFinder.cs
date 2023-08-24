using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImagePreview.Resolvers;
using Microsoft.VisualStudio.Text;

namespace ImagePreview
{
    internal class ReferenceFinder
    {
        public static readonly List<IImageResolver> Resolvers = new()
        {
            new Base64Resolver(),
            new PackResolver(),
            new HttpImageResolver(),
            new FileImageResolver(),
        };

        public static async Task<ImageReference> FindAsync(ITextBuffer buffer, ITrackingPoint triggerPoint)
        {
            int cursorPosition = triggerPoint.GetPosition(buffer.CurrentSnapshot);
            ITextSnapshotLine line = buffer.CurrentSnapshot.GetLineFromPosition(cursorPosition);
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
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            return new ImageReference(resolver, span, match, buffer.GetFileName());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Log();
                }
            }

            return null;
        }
    }
}
