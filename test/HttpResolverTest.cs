using System.Threading.Tasks;
using ImagePreview.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Test
{
    [TestClass]
    public class HttpResolver
    {
        private HttpImageResolver _resolver;

        [TestInitialize]
        public void Setup()
        {
            _resolver = new HttpImageResolver();
        }

        [DataTestMethod]
        [DataRow("http://foo.com/image.png", "http://foo.com/image.png")]
        [DataRow("before http://foo.com/image.png after", "http://foo.com/image.png")]
        [DataRow("(http://foo.com/image.png)", "http://foo.com/image.png")]
        [DataRow(">http://foo.com/image.png)<", "http://foo.com/image.png")]
        [DataRow("[http://foo.com/image.png]", "http://foo.com/image.png")]
        [DataRow("'http://foo.com/image.png'", "http://foo.com/image.png")]
        [DataRow("\"http://foo.com/image.png\"", "http://foo.com/image.png")]
        public void Http(string path, string match)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(match, matches[0].Groups["image"].Value);
        }

        [DataTestMethod]
        [DataRow("https://foo.com/image.png", "https://foo.com/image.png")]
        [DataRow("before https://foo.com/image.png after", "https://foo.com/image.png")]
        [DataRow("(https://foo.com/image.png)", "https://foo.com/image.png")]
        [DataRow(">https://foo.com/image.png)<", "https://foo.com/image.png")]
        [DataRow("[https://foo.com/image.png]", "https://foo.com/image.png")]
        [DataRow("'https://foo.com/image.png'", "https://foo.com/image.png")]
        [DataRow("\"https://foo.com/image.png\"", "https://foo.com/image.png")]
        public void Https(string path, string match)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(match, matches[0].Groups["image"].Value);
        }

        [DataTestMethod]
        [DataRow("//foo.com/image.svg", "//foo.com/image.svg")]
        [DataRow("before //foo.com/image.svg after", "//foo.com/image.svg")]
        [DataRow("//foo.com/image.png", "//foo.com/image.png")]
        [DataRow("(//foo.com/image.png)", "//foo.com/image.png")]
        [DataRow(">//foo.com/image.png)<", "//foo.com/image.png")]
        [DataRow("[//foo.com/image.png]", "//foo.com/image.png")]
        [DataRow("'//foo.com/image.png'", "//foo.com/image.png")]
        [DataRow("\"//foo.com/image.png\"", "//foo.com/image.png")]
        public void Relative(string path, string match)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(match, matches[0].Groups["image"].Value);
        }

        [DataTestMethod]
        [DataRow(@"http://foo.com/test.png", ImageFormat.PNG)]
        [DataRow(@"http://foo.com/test.svg", ImageFormat.SVG)]
        [DataRow(@"http://foo.com/test.ico", ImageFormat.ICO)]
        [DataRow(@"http://foo.com/test.jpg", ImageFormat.JPG)]
        [DataRow(@"http://foo.com/test.jpeg", ImageFormat.JPG)]
        [DataRow(@"http://foo.com/test.gif", ImageFormat.GIF)]
        public void ImageFormatType(string path, ImageFormat format)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(format, matches[0].GetImageFormat());
        }

        [TestMethod]
        public async Task GetImageReferenceAsync()
        {
            Span span = new Span(11, 8);
            ImageReference result = await _resolver.GetImageReferenceAsync(span, "//foo.com/file.png", null);

            Assert.AreEqual("http://foo.com/file.png", result.RawImageString);
            Assert.AreEqual(span, result.Span);
        }
    }
}
