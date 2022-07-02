using System.Text;

namespace GenericParsingLibrary
{
    /// <summary>
    /// Represents syntax errors that occur during parsing.
    /// </summary>
    public class ParserSyntaxException : ParserException
    {
        /// <summary>
        /// Gets the token types that were expected when this exception occured.
        /// </summary>
        public TokenType[] ExpectedTokenTypes { get; private set; }
        /// <summary>
        /// Gets the token values that were expected when this exception occured.
        /// </summary>
        public string[] ExpectedValues { get; private set; }

        /// <summary>
        /// Initalizes a new instance of the <see cref="ParserSyntaxException"/> class.
        /// </summary>
        public ParserSyntaxException()
        {
            ExpectedTokenTypes = Array.Empty<TokenType>();
            ExpectedValues = Array.Empty<string>();
        }

        /// <summary>
        /// Initalizes a new instance of the <see cref="ParserSyntaxException"/> class
        /// with an expected token type and incorrect token that was encountered.
        /// </summary>
        /// <param name="expectedType">Token the parser was expecting.</param>
        /// <param name="encounteredToken">Token the parser encountered.</param>
        public ParserSyntaxException(TokenType expectedType, IToken encounteredToken)
            : base(BuildExceptionMessage(expectedType, encounteredToken))
        {
            ExpectedTokenTypes = new TokenType[] { expectedType };
            ExpectedValues = new string[1] { string.Empty };
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="ParserSyntaxException"/> class
        /// with an array of expected token types and incorrect token that was encountered.
        /// </summary>
        /// <param name="expectedTypes">Possible tokens the parser was expecting.</param>
        /// <param name="encounteredToken">Token the parser encountered.</param>
        public ParserSyntaxException(TokenType[] expectedTypes, IToken encounteredToken)
            : base(BuildExceptionMessage(expectedTypes, encounteredToken))
        {
            ExpectedTokenTypes = expectedTypes;
            ExpectedValues = Enumerable.Repeat(string.Empty, ExpectedTokenTypes.Length).ToArray();
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="ParserSyntaxException"/> class
        /// with an expected token type and value, and incorrect token that was encountered.
        /// </summary>
        /// <param name="expectedType">Token the parser was expecting.</param>
        /// <param name="expectedValue">Token value the parser was expecting.</param>
        /// <param name="encounteredToken">Token the parser encountered.</param>
        public ParserSyntaxException(TokenType expectedType, string expectedValue, IToken encounteredToken)
            : base(BuildExceptionMessage(expectedType, expectedValue, encounteredToken))
        {
            ExpectedTokenTypes = new TokenType[] { expectedType };
            ExpectedValues = new string[] { expectedValue };
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="ParserSyntaxException"/> class
        /// with an array of expected token types/values, and incorrect token that was encountered.
        /// </summary>
        /// <param name="expectedTypes">Possible tokens the parser was expecting.</param>
        /// <param name="expectedValues">Possible token values the parser was expecting.</param>
        /// <param name="encounteredToken">Token the parser encountered.</param>
        public ParserSyntaxException(TokenType[] expectedTypes, string[] expectedValues, IToken encounteredToken)
            : base(BuildExceptionMessage(expectedTypes, expectedValues, encounteredToken))
        {
            ExpectedTokenTypes = expectedTypes;
            ExpectedValues = expectedValues;
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="ParserSyntaxException"/> class
        /// with a specified error message
        /// and incorrect token that was encountered.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="encounteredToken">Token the parser encountered.</param>
        public ParserSyntaxException(string message, IToken encounteredToken)
            : base(BuildExceptionMessage(message, encounteredToken))
        {
            ExpectedTokenTypes = Array.Empty<TokenType>();
            ExpectedValues = Array.Empty<string>();
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="ParserSyntaxException"/> class
        /// with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        // TODO: Update this with other constructors.
        // TODO: Does this need 4 other versions to match other constructors?
        public ParserSyntaxException(string message, Exception inner)
            : base(message, inner)
        {
            ExpectedTokenTypes = Array.Empty<TokenType>();
            ExpectedValues = Array.Empty<string>();
        }

        private static string BuildExceptionMessage(string message, IToken encounteredToken)
        {
            return $"{message} at line {encounteredToken.LineNumber}, pos {encounteredToken.LinePosition}";
        }
        private static string BuildExceptionMessage(TokenType expectedType, IToken encounteredToken)
        {
            return BuildExceptionMessage($"Expecting {OneOf(expectedType)}", encounteredToken);
        }
        private static string BuildExceptionMessage(TokenType[] expectedTypes, IToken encounteredToken)
        {
            return BuildExceptionMessage($"Expecting {OneOf(expectedTypes)}", encounteredToken);
        }
        private static string BuildExceptionMessage(TokenType expectedType, string expectedValue, IToken encounteredToken)
        {
            return BuildExceptionMessage($"Expecting {OneOf(expectedType)} {WithValue(expectedValue)}", encounteredToken);
        }
        private static string BuildExceptionMessage(TokenType[] expectedTypes, string[] expectedValues, IToken encounteredToken)
        {
            return BuildExceptionMessage($"Expecting {OneOf(expectedTypes)} {WithValue(expectedValues)}", encounteredToken);
        }

        // Helper methods

        /// <summary>
        /// Builds a reference string from a token type.
        /// </summary>
        /// <param name="expectedType"></param>
        /// <returns></returns>
        private static string OneOf(TokenType expectedType)
        {
            return $"({Enum.GetName(typeof(TokenType), expectedType)})";
        }
        /// <summary>
        /// Builds a reference string from a list of token types.
        /// </summary>
        /// <param name="expectedTypes"></param>
        /// <returns></returns>
        private static string OneOf(TokenType[] expectedTypes)
        {
            if (expectedTypes.Length == 1)
            {
                return OneOf(expectedTypes[0]);
            }

            var sb = new StringBuilder($"one of ({Enum.GetName(typeof(TokenType), expectedTypes[0])}");
            for (var i = 1; i < expectedTypes.Length; i++)
            {
                sb.Append($", {Enum.GetName(typeof(TokenType), expectedTypes[i])}");
            }
            return sb.Append(')').ToString();
        }

        /// <summary>
        /// Builds a reference string from a token value.
        /// </summary>
        /// <param name="expectedValue"></param>
        /// <returns></returns>
        private static string WithValue(string expectedValue)
        {
            if (expectedValue == "")
                return "";

            return $"with value ({expectedValue})";
        }
        /// <summary>
        /// Builds a reference string from a list of token values.
        /// </summary>
        /// <param name="expectedValues"></param>
        /// <returns></returns>
        private static string WithValue(string[] expectedValues)
        {
            if (expectedValues.Length == 0) return "";
            if (expectedValues.Length == 1) return WithValue(expectedValues[0]);

            var sb = new StringBuilder($"with values ({expectedValues[0]}");
            var count = 0;
            var lastFound = "";
            // for loop instead of foreach because we start at 1
            for (var i = 1; i < expectedValues.Length; i++)
            {
                if (expectedValues[i] != "")
                {
                    count++;
                    lastFound = expectedValues[i];
                    sb.Append($", {expectedValues[i]}");
                }
            }

            if (count == 0) return "";
            if (count == 1) return WithValue(lastFound);

            return sb.Append(')').ToString();
        }
    }
}
