using System.IO;
using ImagePreview.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagePreview.Test
{
    [TestClass]
    public class HttpResolver
    {
        private HttpImageResolver _resolver;
        private readonly DirectoryInfo _folder = new DirectoryInfo("../../Images/");

        [TestInitialize]
        public void Setup()
        {
            _resolver = new HttpImageResolver();
        }

        [DataTestMethod]
        [DataRow("http://foo.com/image.png")]
        [DataRow("(http://foo.com/image.png)")]
        [DataRow(">http://foo.com/image.png)<")]
        [DataRow("[http://foo.com/image.png]")]
        [DataRow("'http://foo.com/image.png'")]
        [DataRow("\"http://foo.com/image.png\"")]
        public void Http(string path)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(path.Trim('>', '<', '(', ')', '[', ']', '"', '\''), matches[0].Groups["image"].Value);
        }

        [DataTestMethod]
        [DataRow("https://foo.com/image.png")]
        [DataRow("(https://foo.com/image.png)")]
        [DataRow(">https://foo.com/image.png)<")]
        [DataRow("[https://foo.com/image.png]")]
        [DataRow("'https://foo.com/image.png'")]
        [DataRow("\"https://foo.com/image.png\"")]
        public void Https(string path)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(path.Trim('>', '<', '(', ')', '[', ']', '"', '\''), matches[0].Groups["image"].Value);
        }

        [DataTestMethod]
        [DataRow("//foo.com/image.svg")]
        [DataRow("//foo.com/image.png")]
        [DataRow("(//foo.com/image.png)")]
        [DataRow(">//foo.com/image.png)<")]
        [DataRow("[//foo.com/image.png]")]
        [DataRow("'//foo.com/image.png'")]
        [DataRow("\"//foo.com/image.png\"")]
        public void Relative(string path)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(path.Trim('>', '<', '(', ')', '[', ']', '"', '\''), matches[0].Groups["image"].Value);
        }
    }
}
