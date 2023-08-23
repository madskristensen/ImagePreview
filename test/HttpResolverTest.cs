﻿using System.IO;
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

        [TestMethod]
        public async Task GetImageAsync()
        {
            Span span = new Span(11, 8);
            ImageReference result = await _resolver.GetImageAsync(span, "//foo.com/file.png", null);

            Assert.AreEqual("http://foo.com/file.png", result.RawImageString);
            Assert.AreEqual(span, result.Span);
        }
    }
}