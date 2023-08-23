using System.Threading.Tasks;
using ImagePreview.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Test
{
    [TestClass]
    public class Base64ResolverTest
    {
        private Base64Resolver _resolver;

        [TestInitialize]
        public void Setup()
        {
            _resolver = new Base64Resolver();
        }

        [DataTestMethod]
        [DataRow("data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==", "R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==")]
        [DataRow("data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7", "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7")]
        public void Relative(string path, string match)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(match, matches[0].Groups["image"].Value);
        }

        [TestMethod]
        public async Task GetBitmapAsync()
        {
            ImageReference result = new ImageReference(new Span(), "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
            System.Windows.Media.Imaging.BitmapSource bitmap = await _resolver.GetBitmapAsync(result);

            Assert.IsNotNull(bitmap);
            Assert.AreEqual(42, result.FileSize);
        }
    }
}
