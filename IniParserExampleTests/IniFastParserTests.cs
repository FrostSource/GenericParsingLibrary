using Microsoft.VisualStudio.TestTools.UnitTesting;
using IniParserExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniParserExample.Tests
{
    [TestClass()]
    public class IniFastParserTests
    {
        [TestMethod()]
        public void IniFastParserTest()
        {
            IniFastParser p;

            p = Parse("[MySection]");
            Assert.IsNotNull(p.IniFile);
            Assert.IsTrue(p.IniFile.Count == 1);
            Assert.IsTrue(p.IniFile.ContainsKey("MySection"));
            Assert.IsNotNull(p.IniFile["MySection"]);
            Assert.IsTrue(p.IniFile["MySection"].Count == 0);

            p = Parse("[ My Section ]\nkey=1234");
            Assert.IsNotNull(p.IniFile);
            Assert.IsTrue(p.IniFile.Count == 1);
            Assert.IsTrue(p.IniFile.ContainsKey("My Section"));
            Assert.IsNotNull(p.IniFile["My Section"]);
            Assert.IsTrue(p.IniFile["My Section"].Count == 1);
            Assert.IsTrue(p.IniFile["My Section"]["key"] is double);
            Assert.IsTrue((double)p.IniFile["My Section"]["key"] == 1234);

            p = Parse("[ ]");
            Assert.IsTrue(p.ExceptionMessage == "");
        }

        private IniFastParser Parse(string source)
        {
            var t = new IniTokenizer(source);
            t.TryTokenize();
            var p = new IniFastParser(t);
            p.TryParse();
            return p;
        }
    }
}