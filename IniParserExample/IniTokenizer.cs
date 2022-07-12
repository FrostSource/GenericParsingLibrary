using GenericParsingLibrary;

namespace IniParserExample
{
    public class IniTokenizer : GenericTokenizer
    {
        // Our ini files only allow ; for comments. Some ini files allow # and this can be easily
        // hard coded into our custom tokenizer if desired by extending the SkipCommentLine method.
        public override string CommentLineStart { get; set; } = ";";
        // Our ini files do not have block comments
        public override string CommentBlockStart { get; set; } = string.Empty;
        public override string CommentBlockEnd { get; set; } = string.Empty;
        // \n Is removed from white space because it is part of the language.
        public override string WhiteSpaceCharacters { get; set; } = " \t\r\f";
        // Square brackets are not boundary chars automatically, only when inside a section
        // this is handled in TokenizeNext().
        public override string BoundaryChars { get; set; } = "\n=";
                                    // Adding \n as a symbol allows it to be captured for the parser
        public override string[] Symbols { get; set; } = { "[", "]", "\n", "=" };
        public override bool UseNumberTokenOnly { get; set; } = true;
        public override string[] Keywords { get; set; } = { "false", "true" };
        public override bool CaseSensitiveKeywords { get; set; } = false;

        // We will use this to provide special rules for section names.
        private bool encounteredSection = false;

        public IniTokenizer(string source) : base(source)
        {
        }

        /// <summary>
        /// Overridden to track encounters of section start.
        /// </summary>
        /// <returns></returns>
        protected override bool TokenizeSymbol()
        {
            foreach (string symbol in Symbols)
            {
                if (IsNext(symbol))
                {
                    Advance(symbol.Length);
                    // Track encounter of section start
                    if (symbol == "[") encounteredSection = true;
                    AddToken(TokenType.Symbol, symbol);
                    return true;
                }
            }
            return false;
        }

        /***
        * The most important part of extending the GenericTokenizer is the TokenizeNext method
        * which figures out which figures out which token is up next to consume and adds it to
        * the list.
        * This tokenizer does not look for many of the things that most languages have, like strings
        * so they are stripped out in this overrided version.
        */
        protected override void TokenizeNext()
        {
            // Saving the position is important in case we need to roll back.
            //SavePosition();

            // Encounter and tokenize the basic 4 symbols.
            // Done first to make sure they're not consumed as part of a string.
            // This is an overridden function.
            if (TokenizeSymbol()) return;

            // Usually the tokenizer doesn't understand the context of the language but in this
            // case it's useful.
            if (encounteredSection)
            {
                var value = NextWord("section name", boundaryChars: "]", allowWhiteSpace: true);
                // Section names can't be blank
                value = value.Trim();
                if (value.Length == 0)
                {
                    throw SyntaxError($"Expecting section name");
                }
                // On success we add the token
                AddToken(TokenType.Identifier, value);
                // Also unencounter section
                encounteredSection = false;
                return;
            }

            // Differentiating numbers from strings can be useful for the parser.
            if (TokenizeKeyword()) return;

            if (TokenizeIdentifier()) return;

            if (TokenizeNumber()) return;

            if (TokenizeString()) return;

            //var val = RestOfLine().Trim();
            //if (val.Length == 0)
            //{
            //    throw SyntaxError($"Expecting string of characters");
            //}
            //else if (IsNumber(val))
            //{
            //    AddToken(TokenType.Number, val);
            //}
            //else if (val == "true" || val == "false")
            //{
            //    AddToken(TokenType.Boolean, val);
            //}
            //else if (StartsEndsWith(val, '"') || StartsEndsWith(val, '\''))
            //{
            //    AddToken(TokenType.String, val[1..^1]);
            //}
            //else
            //{
            //    AddToken(TokenType.String, val);
            //}

            // Anything else is a string of characters used for key/value
            // why can't I use 'value' here? Does C# not scope variables by code blocks?
            //var val = NextWord("string of characters", allowWhiteSpace: true);
            //val = val.Trim();
            //if (val.Length == 0)
            //{
            //    throw SyntaxError($"Expecting string of characters");
            //}
            //AddToken(TokenType.Identifier, val);
            //return;

            throw SyntaxError($"unknown character \"{CurrentChar}\"");

            // If you're just extending the function with more behaviour you can of course
            // call the base function to retain standard behaviour afterwards, just make sure
            // you return from this function before this point if you encounter a valid token.

            //base.TokenizeNext();
        }
    }
}
