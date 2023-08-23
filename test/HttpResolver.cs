using System.IO;
using ImagePreview.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagePreview.Test
{
    [TestClass]
    public class FileResolverTest
    {
        private FileImageResolver _resolver;
        private readonly DirectoryInfo _folder = new DirectoryInfo("../../Images/");

        [TestInitialize]
        public void Setup()
        {
            _resolver = new FileImageResolver();
        }

        [DataTestMethod]
        [DataRow("foo.png")]
        [DataRow("(foo.png)")]
        [DataRow(">foo.png<")]
        [DataRow("[foo.png]")]
        [DataRow("bar/foo.png")]
        [DataRow("/bar/foo.png")]
        [DataRow("../bar/foo.png")]
        public void Relative(string path)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(path.Trim('>', '<', '(', ')', '[', ']'), matches[0].Groups["image"].Value);
        }

        [DataTestMethod]
        [DataRow("test.png")]
        [DataRow("test.png<")]
        [DataRow("test.png)")]
        [DataRow("test.png]")]
        [DataRow("test.svg")]
        public void Absolute(string path)
        {
            string absolute = _folder.FullName + path;
            _resolver.TryGetMatches(absolute, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(absolute.Trim('>', '<', '(', ')', '[', ']'), matches[0].Groups["image"].Value);
        }
    }
}
