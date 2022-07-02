using System.Text;
using System.Text.RegularExpressions;

namespace GenericParsingLibrary
{
    //TODO: Allow characters to stop enclosed, like \n
    /// <summary>
    /// Generic tokenizer can be used to tokenize simple languages by modifying the public properties.
    /// For more complex languages this class should be extended.
    /// </summary>
    public class GenericTokenizer : BaseTokenizer
    {
        #region Constants
        /// <summary>
        /// English letters with both case.
        /// </summary>
        public const string LetterChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        /// <summary>
        /// English digits.
        /// </summary>
        public const string DigitChars = "1234567890";
        /// <summary>
        /// English alphanumeric characters.
        /// </summary>
        public const string AlphaChars = LetterChars + DigitChars;
        /// <summary>
        /// Hexidecimal characters.
        /// </summary>
        public const string HexChars = DigitChars + "abcdefABCDEF";
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets if white space should be skipped by the tokenizer when finding elements such as numbers or words.
        /// </summary>
        public virtual bool AutoSkipGarbage { get; private set; } = true;
        /// <summary>
        /// Gets or sets the chars that define the default boundary between sequences such as words and numbers.
        /// A sequence will stop when it encounters one of the following: White space, boundary char, EOF.
        /// </summary>
        public virtual string BoundaryChars { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets characters that represent white space. Usually garbage.
        /// </summary>
        public virtual string WhiteSpaceCharacters { get; set; } = " \t\r\f\n";
        /// <summary>
        /// Gets or sets the string that indicates the start of a line comment.
        /// </summary>
        public virtual string CommentLineStart { get; set; } = "//";
        /// <summary>
        /// Gets or sets the char sequence that starts a block comment.
        /// </summary>
        public virtual string CommentBlockStart { get; set; } = "/*";
        /// <summary>
        /// Gets or sets the char sequence that ends a block comment.
        /// </summary>
        public virtual string CommentBlockEnd { get; set; } = "*/";
        /// <summary>
        /// Gets or sets the array of symbols. Symbols can be multiple chars like '>='.
        /// </summary>
        public virtual string[] Symbols { get; set; } = { "-=", "+=", "*=", "/=", "-", "+", "{", "}", "(", ")", "[", "]", "=", "!", ";", ",", ".", "/", "%", "*" };
        /// <summary>
        /// Gets or sets the array of reserved keywords.
        /// </summary>
        public virtual string[] Keywords { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Valid chars when searching for keywords. If blank at time of construction it will be generated based on <see cref="Keywords"/>.
        /// </summary>
        public virtual string KeywordChars { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets if keywords are case sensitive.
        /// </summary>
        public virtual bool CaseSensitiveKeywords { get; set; } = true;
        /// <summary>
        /// Gets or sets the characters that can start and end a string.
        /// </summary>
        public virtual string StringBoundaryCharacters { get; set; } = "\"'";
        /// <summary>
        /// Gets or sets the characters that an identifier can start with.
        /// </summary>
        public virtual string IdentifierStartCharacters { get; set; } = "_" + LetterChars;
        /// <summary>
        /// Gets or sets the characters that an identifier can contain.
        /// </summary>
        public virtual string IdentifierCharacters { get; set; } = "_" + LetterChars + DigitChars;
        /// <summary>
        /// Gets or sets the character that defines a decimal place in a number. 
        /// </summary>
        public virtual char DecimalChar { get; set; } = '.';
        /// <summary>
        /// Gets or sets if the tokenizer should not distinguish between <see cref="float"/> and <see cref="int"/>, and instead lump all numbers into <see cref="TokenType.Number"/>.
        /// </summary>
        public virtual bool UseNumberTokenOnly { get; set; } = false;
        /// <summary>
        /// Gets or sets if escaping is captured in strings.
        /// </summary>
        public virtual bool AllowCharacterEscaping { get; set; } = true;
        /// <summary>
        /// Gets or sets if escape sequences are automatically converted into their literal counterpart.
        /// </summary>
        public virtual bool AutomaticallyConvertEscapedCharacters { get; set; } = true;
        /// <summary>
        /// Gets or sets the character that starts an escape sequence.
        /// </summary>
        public virtual char EscapeCharacter { get; set; } = '\\';
        /// <summary>
        /// Gets or sets the string of chars that will throw a syntax exception if encountered.
        /// Will not throw exception if encountered during garbage skipping.
        /// </summary>
        public virtual string InvalidChars { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the string of chars that will throw a syntax exception if NOT encountered.
        /// Set as empty string to ignore.
        /// </summary>
        public virtual string ValidChars { get; set; } = string.Empty;
        /// <summary>
        /// Gets the current character in the <see cref="BaseTokenizer.Source"/> <see cref="string"/> waiting to be processed.
        /// </summary>
        protected char CurrentChar
        {
            get
            {
                if (Index < 0 || Index >= Source.Length)
                    return '\0';
                else
                    return Source[Index];
            }
        }
        /// <summary>
        /// Gets the next char in the <see cref="BaseTokenizer.Source"/> <see cref="string"/> after <see cref="CurrentChar"/>.
        /// </summary>
        protected char NextChar
        {
            get
            {
                if (Index < -1 || Index >= Source.Length - 1)
                    return '\0';
                else
                    return Source[Index + 1];
            }
        }
        /// <summary>
        /// Gets the current index in the <see cref="BaseTokenizer.Source"/> <see cref="string"/>.
        /// </summary>
        protected int Index { get; private set; } = 0;
        /// <summary>
        /// Gets the current line number (row) in the <see cref="BaseTokenizer.Source"/> <see cref="string"/>.
        /// </summary>
        protected int LineNumber { get; private set; } = 1;
        /// <summary>
        /// Gets the current line position (column) in the <see cref="BaseTokenizer.Source"/> <see cref="string"/>.
        /// </summary>
        protected int LinePosition { get; private set; } = 1;
        /// <summary>
        /// Gets the previous line number (row) in the <see cref="BaseTokenizer.Source"/> <see cref="string"/>.
        /// </summary>
        protected int PreviousLineNumber { get; private set; } = 1;
        /// <summary>
        /// Gets the previous line position (column) in the <see cref="BaseTokenizer.Source"/> <see cref="string"/>.
        /// </summary>
        protected int PreviousLinePosition { get; private set; } = 1;
        /// <summary>
        /// Gets the line number (row) in the <see cref="BaseTokenizer.Source"/> <see cref="string"/>, as saved with <see cref="SavePosition"/>.
        /// </summary>
        protected int SavedLineNumber { get; private set; } = 1;
        /// <summary>
        /// Gets the line position (column) in the <see cref="BaseTokenizer.Source"/> <see cref="string"/>, as saved with <see cref="SavePosition"/>.
        /// </summary>
        protected int SavedLinePosition { get; private set; } = 1;
        /// <summary>
        /// Gets if the tokenizer has arrived at the end of the <see cref="BaseTokenizer.Source"/> <see cref="string"/>.
        /// </summary>
        protected bool EOF { get => Index >= Source.Length; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericTokenizer"/> class with a source string.
        /// </summary>
        /// <param name="source"></param>
        public GenericTokenizer(string source) : base(source)
        {
            if (KeywordChars == string.Empty && Keywords.Length > 0)
            {
                KeywordChars = new string(string.Concat(Keywords).Distinct().ToArray()) + IdentifierCharacters;
            }
            if (BoundaryChars == string.Empty && Symbols.Length > 0)
            {
                BoundaryChars += new string(string.Concat(Symbols).Distinct().ToArray());
            }
        }
        #endregion

        #region Char handling
        /// <summary>
        /// Saves the current line number and position for later retrieval.
        /// Usually for more accurate syntax error messages.
        /// </summary>
        protected void SavePosition()
        {
            SavedLineNumber = LineNumber;
            SavedLinePosition = LinePosition;
        }
        /// <summary>
        /// Advances the next character in the source text.
        /// </summary>
        /// <exception cref="TokenizerEOFException">Thrown when the end of the source is found instead of a character.</exception>
        protected void Advance()
        {
            if (EOF) throw new TokenizerEOFException();
            PreviousLineNumber = LineNumber;
            PreviousLinePosition = LinePosition;
            if (Source[Index] == '\n')
            {
                LineNumber++;
                LinePosition = 1;
            }
            else
            {
                LinePosition++;
            }
            Index++;
        }
        /// <summary>
        /// Advances the given number of characters in the source text.
        /// </summary>
        /// <param name="count"></param>
        /// <exception cref="TokenizerEOFException">Thrown when the end of the source is found instead of a character.</exception>
        protected void Advance(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Advance();
            }
        }
        /// <summary>
        /// Gets the next <see cref="char"/> in the source text and advances, skipping any white space at the beginning.
        /// </summary>
        /// <returns>
        /// The next <see cref="char"/> or \0 if at the end.
        /// </returns>
        /// <exception cref="TokenizerEOFException">Thrown when the end of the source is found instead of a character.</exception>
        protected char Next()
        {
            if (AutoSkipGarbage) SkipGarbage();
            if (EOF) return '\0';
            char ch = CurrentChar;
            Advance();
            return ch;
        }
        /// <summary>
        /// Gets the previous <see cref="char"/> in the source text.
        /// </summary>
        /// <returns>The previous <see cref="char"/> or \0 if at the beginning.</returns>
        protected char Previous()
        {
            if (Index < 0)
                return '\0';
            return Source[Index - 1];
        }
        /// <summary>
        /// Returns the next <see cref="char"/> in the source text without advancing.
        /// </summary>
        /// <returns></returns>
        protected char Peek()
        {
            //var index = Index;
            //TODO: This needs to implement a versin of SkipGarbage without consuming!
            //if (AutoSkipGarbage) index = NextIndexAfterWhiteSpace();
            //if (index >= Source.Length) return '\0';
            //return Source[index];
            var prevLineNumber = LineNumber;
            var prevLinePosition = LinePosition;
            var prevIndex = Index;
            if (AutoSkipGarbage) SkipGarbage();
            char ch = Next();
            LineNumber = prevLineNumber;
            LinePosition = prevLinePosition;
            Index = prevIndex;
            return ch;
        }
        /// <summary>
        /// Checks if <paramref name="str"/> is at the current position of the source text.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected bool IsNext(string str)
        {
            if (str == "") return false;
            if (str.Length > Source.Length - Index) return false;

            var prevLineNumber = LineNumber;
            var prevLinePosition = LinePosition;
            var prevIndex = Index;
            if (AutoSkipGarbage) SkipGarbage();
            var ret = IsNextNoSkip(str);
            LineNumber = prevLineNumber;
            LinePosition = prevLinePosition;
            Index = prevIndex;
            return ret;
        }
        /// <summary>
        /// Checks if <paramref name="str"/> is next at the current position of the source text
        /// without skipping any garbage.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected bool IsNextNoSkip(string str)
        {
            if (str == "") return false;
            if (str.Length > Source.Length - Index) return false;
            return (Source.Substring(Index, str.Length) == str);
        }
        /// <summary>
        /// Eats the given <see cref="string"/> at the front of the source text.
        /// </summary>
        /// <param name="str"></param>
        /// <exception cref="TokenizerEOFException">Thrown when the tokenizer unexpectedly reaches the end of the source.</exception>
        /// <exception cref="TokenizerSyntaxException">Thrown when <paramref name="str"/> is not next in the source.</exception>
        // TODO: Remove this? Confusing with parser eat?
        protected void Eat(string str)
        {
            if (str.Length > Source.Length - Index)
            {
                throw new TokenizerEOFException();
            }
            if (!IsNext(str))
            {
                throw SyntaxError($"Expected '{str}'");
            }
            if (AutoSkipGarbage) SkipGarbage();
            Advance(str.Length);
        }
        /// <summary>
        /// Gets the next string of characters in the source text using a set of parameters.
        /// </summary>
        /// <param name="expecting">Optionally specify the word that is expected to provide better error reporting.</param>
        /// <param name="boundaryChars">Chars to stop at. If not provided will use <see cref="BoundaryChars"/></param>
        /// <param name="invalidChars">Chars that will throw an error if encountered. If not provided will use <see cref="InvalidChars"/></param>
        /// <param name="validChars">Chars that MUST be encountered or will throw an error. If not provided will use <see cref="ValidChars"/></param>
        /// <param name="failOnEmpty">Will throw an error if final string is empty.</param>
        /// <param name="allowWhiteSpace"></param>
        /// <param name="allowEscaping"></param>
        /// <returns></returns>
        protected virtual string NextWord(string expecting = "", string? boundaryChars = null, string? invalidChars = null, string? validChars = null, bool failOnEmpty = true, bool allowWhiteSpace = false, bool? allowEscaping = null)
        {
            boundaryChars ??= BoundaryChars;
            invalidChars ??= InvalidChars;
            validChars ??= ValidChars;
            allowEscaping ??= AllowCharacterEscaping;
            expecting = expecting == "" ? "word" : expecting;

            if (AutoSkipGarbage) SkipGarbage();
            SavePosition();
            var str = new StringBuilder();
            while (!EOF && !boundaryChars.Contains(CurrentChar) && (validChars == string.Empty || validChars.Contains(CurrentChar)) && (allowWhiteSpace || !IsWhiteSpace(CurrentChar)))
            {
                if (invalidChars.Contains(CurrentChar)) throw SyntaxError($"Invalid character \"{CurrentChar}\", expecting {expecting}");
                // valid chars shouldn't throw exception when not encountered, just stop gathering (update doc)
                //if (validChars != "" && !validChars.Contains(CurrentChar)) throw SyntaxError($"Invalid character \"{CurrentChar}\", expecting {expecting}");

                string escape;
                if (allowEscaping.Value && (escape = NextEscapeSequence()) != "")
                {
                    if (AutomaticallyConvertEscapedCharacters)
                        str.Append(ConvertStringToEscape(escape));
                    else
                        str.Append(escape);
                }
                else
                {
                    str.Append(CurrentChar);
                    Advance();
                }
            }
            if (failOnEmpty && str.Length == 0)
            {
                throw SyntaxError($"Expecting {expecting}");
            }
            return str.ToString();
        }
        /// <summary>
        /// Returns the next <see cref="string"/> enclosed between a given <see cref="char"/>.
        /// <para>Makes use of <see cref="EscapeCharacter"/>.</para>
        /// <para>Does not return the start or end <paramref name="boundaryChar"/>.</para>
        /// </summary>
        /// <param name="boundaryChar">The <see cref="char"/> that defines the boundary.</param>
        /// <param name="invalidChars">Chars that will cause an exception to be thrown.</param>
        /// <returns></returns>
        /// <exception cref="TokenizerEOFException">Thrown when the end of the source is found instead of a character.</exception>
        /// <exception cref="TokenizerSyntaxException">Thrown when <paramref name="boundaryChar"/> is not at the beginning or end.</exception>
        protected virtual string NextEnclosed(char boundaryChar = '"', string? invalidChars = null)
        {
            invalidChars ??= InvalidChars;

            if (AutoSkipGarbage) SkipGarbage();
            if (Next() != boundaryChar)
            {
                SyntaxErrorPrevious($"Expected '{boundaryChar}'");
            }
            // Previous values saved for more accurate error reporting
            // separately from SavePosition so it doesn't override child classes.
            var linePrev = LineNumber;
            var posPrev = LinePosition;

            var enclosedValue = NextWord("", boundaryChar.ToString(), invalidChars, failOnEmpty: false, allowWhiteSpace: true);
            if (EOF || CurrentChar != boundaryChar) SyntaxError($"Missing closing '{boundaryChar}'", linePrev, posPrev);
            Advance();
            return enclosedValue;
        }
        /// <summary>
        /// Gets the next integer in the source text
        /// </summary>
        /// <returns></returns>
        protected virtual string NextInteger()
        {
            // Unneeded because NextWord does this
            //if (AutoSkipGarbage) SkipGarbage();

            var integer = NextWord("integer", validChars: DigitChars);
            return integer;
        }
        /// <summary>
        /// Gets the next decimal number in the source text.
        /// </summary>
        /// <remarks>May return a similar result to <see cref="NextInteger"/> if no decimal point is found but a valid number is.</remarks>
        /// <returns></returns>
        protected virtual string NextDecimal(char? decimalChar = null)
        {
            decimalChar ??= DecimalChar;

            // Unneeded because NextWord does this
            //if (AutoSkipGarbage) SkipGarbage();

            string decimalPre = NextWord("decimal number", BoundaryChars + decimalChar, validChars: DigitChars);
            string decimalPost = "";
            if (CurrentChar == decimalChar)
            {
                decimalPost += decimalChar;
                Advance();
                decimalPost += NextWord("number after decimal", BoundaryChars, validChars: DigitChars, invalidChars: decimalChar.ToString());
            }

            return decimalPre + decimalPost;
        }
        /// <summary>
        /// Gets a sequence of characters with a give ruleset of characters.
        /// </summary>
        /// <param name="validStartChars"><see cref="string"/> of characters that the sequence may have as its first character.</param>
        /// <param name="validChars"><see cref="string"/> of characters that the sequence may contain after the first character.</param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// // Getting a variable name that can't start with numbers but can contain them.
        /// var name = tokenizer.NextSequence("_abc","_abc123");
        /// </code>
        /// </example>
        protected virtual string NextSequence(string validStartChars, string validChars)
        {
            if (AutoSkipGarbage) SkipWhiteSpace();


            if (!validStartChars.Contains(CurrentChar)) throw SyntaxError($"Expecting one of '{validStartChars}' but found '{SpecialCharToString(CurrentChar)}'");
            string sequence = CurrentChar.ToString();
            Advance();
            sequence += NextWord($"one of '{validStartChars}'", validChars: validChars, failOnEmpty: false);
            return sequence;
        }
        /// <summary>
        /// Gets the next escape sequence as a string if an escape character is next.
        /// </summary>
        /// <remarks>
        /// This method does not strip any leading garbage by default.
        /// </remarks>
        /// <returns>The next escape sequence with the leading marking character, or blank if not found.</returns>
        protected virtual string NextEscapeSequence()
        {
            if (AllowCharacterEscaping && CurrentChar == EscapeCharacter)
            {
                if (IsWhiteSpace(NextChar))
                {
                    throw SyntaxError("Expecting escape sequence");
                }
                Advance();
                // Single character escapes
                switch (CurrentChar)
                {
                    case '\'': Advance(); return EscapeCharacter + @"'";
                    case '"':  Advance(); return EscapeCharacter + @"""";
                    case '\\': Advance(); return EscapeCharacter + @"\";
                    case '0':  Advance(); return EscapeCharacter + @"0";
                    case 'a':  Advance(); return EscapeCharacter + @"a";
                    case 'b':  Advance(); return EscapeCharacter + @"b";
                    case 'f':  Advance(); return EscapeCharacter + @"f";
                    case 'n':  Advance(); return EscapeCharacter + @"n";
                    case 'r':  Advance(); return EscapeCharacter + @"r";
                    case 't':  Advance(); return EscapeCharacter + @"t";
                    case 'v':  Advance(); return EscapeCharacter + @"v";
                }
                // More complex escapes
                var str = new StringBuilder();
                // Unicode escape sequence for character with hex value xxxx
                if (CurrentChar == 'u')
                {
                    Advance();
                    for (int i = 0; i < 4; i++)
                    {
                        if (!CharIsHexidecimal(CurrentChar))
                        {
                            SyntaxErrorPrevious($"Invalid character in Unicode escape {CurrentChar}");
                        }
                        str.Append(CurrentChar);
                        Advance();
                    }
                    return $"\\u{str}";
                }
                //TODO: \xn[n][n][n] – Unicode escape sequence for character with hex value nnnn (variable length version of \uxxxx)
                //TODO: \Uxxxxxxxx – Unicode escape sequence for character with hex value xxxxxxxx (for generating surrogates)

                throw SyntaxError("Unrecognized escape sequence");
            }
            return "";
        }
        /// <summary>
        /// Gets all characters up to a given <see cref="char"/>.
        /// <para>This does not skip garbage.</para>
        /// </summary>
        /// <param name="ch"></param>
        /// <returns>The <see cref="string"/> up until the next <paramref name="ch"/> or empty if nothing found. </returns>
        protected string NextUntil(char ch)
        {
            var sb = new StringBuilder();
            while (!EOF && CurrentChar != ch)
            {
                sb.Append(CurrentChar);
            }
            return sb.ToString();
        }
        /// <summary>
        /// Gets all characters left on the current line and moves to the beginning of the next line.
        /// </summary>
        /// <returns>All characters left on the current line, not including \n.</returns>
        protected string RestOfLine()
        {
            var sb = new StringBuilder();
            // TODO: Should be checking for \r?
            while (!EOF && CurrentChar != '\n' && CurrentChar != '\r')
            {
                sb.Append(CurrentChar);
                Advance();
            }
            SkipLine();
            return sb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        protected string NextRegex(string regex)
        {
            var rx = new Regex(@"$" + regex);
            var match = rx.Match(Source);
            return match.Value;
        }
        /// <summary>
        /// Skips garbage such as white space and comments.
        /// </summary>
        protected void SkipGarbage()
        {
            while (IsWhiteSpace(CurrentChar) || IsNextNoSkip(CommentLineStart) || IsNextNoSkip(CommentBlockStart))
            {
                SkipWhiteSpace();
                SkipCommentLine();
                SkipCommentBlock();
            }
        }
        /// <summary>
        /// Skips a comment line if it's next.
        /// </summary>
        protected virtual void SkipCommentLine()
        {
            //TODO: Should this be IsNext?
            if (IsNextNoSkip(CommentLineStart)) SkipLine();
        }
        /// <summary>
        /// Skips over a comment block if it's next.
        /// </summary>
        protected virtual void SkipCommentBlock()
        {
            if (IsNextNoSkip(CommentBlockStart))
            {
                Advance(CommentBlockStart.Length);
                while (!IsNext(CommentBlockEnd)) Advance();
                Advance(CommentBlockEnd.Length);
            }
        }
        /// <summary>
        /// Advances the source string to the first non-whitespace character.
        /// </summary>
        protected virtual void SkipWhiteSpace()
        {
            while (!EOF && IsWhiteSpace(CurrentChar))
            {
                Advance();
            }
        }
        /// <summary>
        /// Gets the index at the end of any current whitespace. That is, the first index with a non-whitespace character.
        /// </summary>
        /// <returns></returns>
        protected int NextIndexAfterWhiteSpace()
        {
            var index = Index;
            while (index < Source.Length && IsWhiteSpace(Source[index]))
            {
                index++;
            }
            return index;
        }
        /// <summary>
        /// Moves to the next line in the source if one exists.
        /// </summary>
        protected void SkipLine()
        {
            while (!EOF && CurrentChar != '\n')
            {
                Advance();
            }
            if (CurrentChar == '\n') Advance();
        }
        /// <summary>
        /// Checks if a given <see cref="char"/> is a white space char.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        protected bool IsWhiteSpace(char ch)
        {
            //return Char.IsWhiteSpace(ch);
            return WhiteSpaceCharacters.Contains(ch);
        }
        #endregion

        #region Static char checking
        /// <summary>
        /// Converts a special <see cref="char"/> like \n and \t to a name.
        /// Used to make invisible characters readable for error reporting.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static string SpecialCharToString(char ch)
        {
            return ch switch
            {
                '\0' => "null",
                '\r' => "carriage return",
                '\n' => "new line",
                '\t' => "tab",
                '\f' => "form feed",
                '"' => "double quotation mark",
                '\'' => "single quotation mark",
                _ => ch.ToString(),
            };
        }
        /// <summary>
        /// Replaces all invisible characters in a string with a named version.
        /// Used to make error reports more readable.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceSpecialCharsInString(string str)
        {
            return str.Replace("\0", "[null]")
                      .Replace("\r", "[carriage return]")
                      .Replace("\n", "[new line]")
                      .Replace("\t", "[tab]")
                      .Replace("\f", "[form feed]");
        }
        /// <summary>
        /// Converts any escape sequences to their literal form, e.g. @"\x07" => "a".
        /// </summary>
        /// <param name="escapeString"></param>
        /// <returns></returns>
        public static string ConvertStringToEscape(string escapeString)
        {
            //TODO: Do full testing for this.
            var p = Regex.Unescape(escapeString);
            return p;
            /*return escapeString switch
            {
                "'" => '\'',
                "\"" => '\"',
                "\\" => '\\',
                "0" => '\0',
                "a" => '\a',
                "b" => '\b',
                "f" => '\f',
                "n" => '\n',
                "r" => '\r',
                "t" => '\t',
                "v" => '\v',
                "xFF" => '\xFF'
            };*/
        }
        /// <summary>
        /// Checks if a character is part hexidecimal.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool CharIsHexidecimal(char ch)
        {
            return HexChars.IndexOf(ch) > -1;
        }
        /// <summary>
        /// Checks if a character is a digit.
        /// This does not use <see cref="DigitChars"/>.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool IsDigit(char ch)
        {
            return (ch == '0' || ch == '1' || ch == '2' || ch == '3' || ch == '4' || ch == '5' || ch == '6' || ch == '7' || ch == '8' || ch == '9');
        }
        /// <summary>
        /// Check if <paramref name="value"/> is a number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumber(string value)
        {
            return double.TryParse(value, out _);
        }
        /// <summary>
        /// Checks if <paramref name="value"/> starts and ends with <paramref name="ch"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool StartsEndsWith(string value, char ch)
        {
            return value.StartsWith(ch) && value.EndsWith(ch);
        }
        #endregion

        #region Error reporting
        /// <summary>
        /// Gets a string labelling the current line number and position.
        /// </summary>
        /// <returns></returns>
        protected string LinePositionFormatted()
        {
            return $"line {LineNumber}, pos {LinePosition}";
        }
        /// <summary>
        /// Gets a string labelling a given line number and position.
        /// </summary>
        /// <param name="lineNumber">The line number (row) starting at 1.</param>
        /// <param name="linePosition">The line position (column) starting at 1.</param>
        /// <returns></returns>
        public static string LinePositionFormatted(int lineNumber, int linePosition)
        {
            return $"line {lineNumber}, pos {linePosition}";
        }
        /// <summary>
        /// Creates a new tokenizer exception with a message and specific line position.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="lineNumber"></param>
        /// <param name="linePosition"></param>
        /// <returns></returns>
        public static TokenizerSyntaxException SyntaxError(string message, int lineNumber, int linePosition)
        {
            return new TokenizerSyntaxException(ReplaceSpecialCharsInString(message), lineNumber, linePosition);
        }
        /// <summary>
        /// Creates a new tokenizer exception with a message at the current line position.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected TokenizerSyntaxException SyntaxError(string message)
        {
            return SyntaxError(message, LineNumber, LinePosition);
        }
        /// <summary>
        /// Creates a new tokenizer exception with a message at the previous line position.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected TokenizerSyntaxException SyntaxErrorPrevious(string message)
        {
            return SyntaxError(message, PreviousLineNumber, PreviousLinePosition);
        }
        /// <summary>
        /// Creates a new tokenizer exception with a message at the saved line position.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected TokenizerSyntaxException SyntaxErrorSaved(string message)
        {
            return SyntaxError(message, SavedLineNumber, SavedLinePosition);
        }
        #endregion

        /// <summary>
        /// Sets a new source string and resets positioning properties.
        /// This does <i>not</i> clear the token list.
        /// </summary>
        /// <param name="source"></param>
        public void SetSource(string source)
        {
            Source = source;
            Index = 0;
            LineNumber = 1;
            LinePosition = 1;
            PreviousLineNumber = 1;
            PreviousLinePosition = 1;
            SavedLineNumber = 1;
            SavedLinePosition = 1;
        }

        #region Token creation
        /// <summary>
        /// Adds a new token with a type and value.
        /// </summary>
        /// <param name="tokenType"></param>
        /// <param name="value"></param>
        protected virtual void AddToken(TokenType tokenType, string value)
        {
            var token = new GenericToken(tokenType, value, Index, SavedLineNumber, SavedLinePosition);
            Tokens.Add(token);
            LastToken = token;
        }

        /// <summary>
        /// Helper method to tokenize an identifier.
        /// Useful when overriding or extending <see cref="TokenizeNext"/>
        /// </summary>
        /// <returns></returns>
        protected virtual bool TokenizeIdentifier()
        {
            if (IdentifierStartCharacters.Contains(CurrentChar))
            {
                var value = NextWord("identifier", validChars: IdentifierCharacters, allowEscaping: false);
                AddToken(TokenType.Identifier, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Helper method to tokenize an integer or float.
        /// Useful when overriding or extending <see cref="TokenizeNext"/>
        /// </summary>
        /// <returns></returns>
        protected virtual bool TokenizeNumber()
        {
            if (DigitChars.Contains(CurrentChar))
            {
                var value = NextDecimal();
                if (UseNumberTokenOnly)
                {
                    AddToken(TokenType.Number, value);
                }
                else
                {
                    if (value.Contains(DecimalChar))
                        AddToken(TokenType.Float, value);
                    else
                        AddToken(TokenType.Integer, value);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Helper method to tokenize a string.
        /// Useful when overriding or extending <see cref="TokenizeNext"/>
        /// </summary>
        /// <returns></returns>
        protected virtual bool TokenizeString()
        {
            if (StringBoundaryCharacters.Contains(CurrentChar))
            {
                var value = NextEnclosed(CurrentChar);
                AddToken(TokenType.String, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Helper method to tokenize a symbol.
        /// Useful when overriding or extending <see cref="TokenizeNext"/>
        /// </summary>
        /// <returns></returns>
        protected virtual bool TokenizeSymbol()
        {
            foreach (string symbol in Symbols)
            {
                if (IsNext(symbol))
                {
                    Advance(symbol.Length);
                    AddToken(TokenType.Symbol, symbol);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Helper method to tokenize a keyword.
        /// Useful when overriding or extending <see cref="TokenizeNext"/>
        /// </summary>
        /// <returns></returns>
        protected virtual bool TokenizeKeyword()
        {
            var kwdChars = KeywordChars;
            if (!CaseSensitiveKeywords)
            {
                //TODO: Consider putting this in constructor
                kwdChars = kwdChars.ToLower() + kwdChars.ToUpper();
            }
            // Checking for valid keyword char first hopefully improves performance.
            if (kwdChars.Contains(CurrentChar))
            {
                foreach (string keyword in Keywords)
                {
                    var prevIndex = Index;
                    // This might need to be in try-catch
                    var kwd = NextWord("keyword", validChars: kwdChars, allowEscaping: false);
                    if (kwd == keyword)
                    {
                        // kwd is added instead of keyword to preserve casing.
                        AddToken(TokenType.Keyword, kwd);
                        return true;
                    }
                    Index = prevIndex;
                }
            }
            return false;
        }
        /// <summary>
        /// Processes the next possible token.
        /// </summary>
        protected virtual void TokenizeNext()
        {
            if (EOF) return;

            // Position is saved before tokenizing so error messages are more accurate.
            SavePosition();

            if (TokenizeKeyword()) return;

            if (TokenizeIdentifier()) return;

            if (TokenizeNumber()) return;

            if (TokenizeString()) return;

            if (TokenizeSymbol()) return;

            throw SyntaxError($"unknown character \"{CurrentChar}\"");
        }
        /// <summary>
        /// Creates all possible tokens from the source string.
        /// </summary>
        public override void Tokenize()
        {
            while (!EOF)
            {
                if (AutoSkipGarbage) SkipGarbage();

                TokenizeNext();
            }
        }
        #endregion
    }
}
