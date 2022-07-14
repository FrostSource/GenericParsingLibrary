using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenericParsingLibrary;


namespace IniParserExample.Tests;

[TestClass()]
public class IniTokenizerTests
{
    [TestMethod()]
    public void IniTokenizerTest()
    {
        IniTokenizer t;

        t = Tokenize("[MySection]");
        Assert.IsTrue(t.Tokens.Count == 3);

        t = Tokenize("[ My Space Section ]");
        Assert.IsTrue(t.Tokens.Count == 3);

        t = Tokenize("key = 1.69");
        Assert.IsTrue(t.Tokens[0].TokenType == TokenType.Identifier);
        Assert.IsTrue(t.Tokens[1].TokenType == TokenType.Symbol);
        Assert.IsTrue(t.Tokens[2].TokenType == TokenType.Number);

        t = Tokenize("(MySection)");
        Assert.IsTrue(t.ExceptionMessage == "Syntax Error: unknown character \"(\" at line 1, pos 1.");

        t = Tokenize("[MySection]\n\n\t   )");
        Assert.IsTrue(t.ExceptionMessage == "Syntax Error: unknown character \")\" at line 3, pos 5.");
    }

    private IniTokenizer Tokenize(string source)
    {
        var t = new IniTokenizer(source);
        t.TryTokenize();
        return t;
    }
}