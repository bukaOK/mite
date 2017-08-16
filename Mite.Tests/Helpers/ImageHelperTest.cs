using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mite.Helpers;
using Mite.BLL.Helpers;

namespace Mite.Tests.Helpers
{
    [TestClass]
    public class ImageHelperTest
    {
        private const string ImagesDir = @"C:\Users\Buka\Desktop\TestImages\";

        public ImageHelperTest()
        {
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CompressPngTest()
        {
            var path = ImagesDir + "mite-crop-bug.png";
            var result = ImagesHelper.Compressed.Compress(path);
            if (!result.Succeeded)
            {
                Assert.Fail("Неудачное сжатие gif");
            }
            else
            {
                if (!ImagesHelper.Compressed.CompressedExists(path))
                {
                    Assert.Fail("Не существует сжатый аналог");
                }
            }
        }
        [TestMethod]
        public void CompressJpgTest()
        {
            var path = ImagesDir + "testjpg.jpg";
            var result = ImagesHelper.Compressed.Compress(path);
            if (!result.Succeeded)
            {
                Assert.Fail("Неудачное сжатие gif");
            }
            else
            {
                if (!ImagesHelper.Compressed.CompressedExists(path))
                {
                    Assert.Fail("Не существует сжатый аналог");
                }
            }
        }
        [TestMethod]
        public void CompressGifTest()
        {
            var path = ImagesDir + "testgif.gif";
            var result = ImagesHelper.Compressed.Compress(path);
            if (!result.Succeeded)
            {
                Assert.Fail("Неудачное сжатие gif");
            }
            else
            {
                if (!ImagesHelper.Compressed.CompressedExists(path))
                {
                    Assert.Fail("Не существует сжатый аналог");
                }
            }
        }
    }
}
