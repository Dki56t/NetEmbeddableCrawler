using System;
using System.IO;
using Crawler;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Unit
{
    [TestClass]
    public class UnitTest1
    {
        private string _testHtml;
        private string _testFormHtml;
        private string _testJs;
        private string _testCss;
        private string _testImage;
        private string _testText;

        [TestInitialize]
        public void Init()
        {
            _testHtml = File.ReadAllText("TestPageSimple.html");
            _testFormHtml = "<form action=\"http://www.dev2dev.ru/visitors/apply/\"></form>";
            _testJs = "<script src=\"https://ajax.googleapis.com/ajax/libs/webfont/1.4.7/webfont.js\"></script>";
        }


        [TestMethod]
        public void TestLinkCount()
        {
            //8 link in document
        }


        [TestMethod]
        public void TestSkipForm()
        {
            var t = new ItemBuilder(new Configuration
            {
                RootLink = "http://html-agility-pack.net/",
                Depth = 2
            });
            t.Build();
        }
    }
}
