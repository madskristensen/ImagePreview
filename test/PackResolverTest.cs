using System.IO;
using System.Text.RegularExpressions;
using ImagePreview.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagePreview.Test
{
    [TestClass]
    public class PackResolverTest
    {
        private PackResolver _resolver;


        [TestInitialize]
        public void Setup()
        {
            _resolver = new PackResolver();
        }

        [DataTestMethod]
        [DataRow("/MyAssembly;component/foo.png", "foo.png")]
        [DataRow("/MyAssembly;component/bar/foo.png", "bar/foo.png")]
        [DataRow("\"/MyAssembly;component/bar/foo.png\"", "bar/foo.png")]
        public void Short(string path, string match)
        {
            _resolver.TryGetMatches(path, out MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(match, matches[0].Groups["image"].Value);
        }

        [DataTestMethod]
        [DataRow("pack://application:,,,/MyAssembly;component/foo.png", @"foo.png")]
        [DataRow("pack://application:,,,/MyAssembly;component/bar/foo.png", @"bar/foo.png")]
        [DataRow("\"pack://application:,,,/MyAssembly;component/bar/foo.png\"", @"bar/foo.png")]
        public void Long(string path, string match)
        {
            _resolver.TryGetMatches(path, out MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(match, matches[0].Groups["image"].Value);
        }

        [DataTestMethod]
        [DataRow("/MyAssembly;component/foo.png", "PNG")]
        [DataRow("/MyAssembly;component/foo.Gif", "GIF")]
        [DataRow("/MyAssembly;component/foo.jpG", "JPG")]
        [DataRow("/MyAssembly;component/foo.ico", "ICO")]
        [DataRow("/MyAssembly;component/foo.png", "PNG")]
        public void ImageFormatType(string path, string format)
        {
            _resolver.TryGetMatches(path, out MatchCollection matches);

            Assert.AreEqual(format, matches[0].GetImageFormat());
        }
    }
}
