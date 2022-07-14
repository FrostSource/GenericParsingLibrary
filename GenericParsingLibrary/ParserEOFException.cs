
namespace GenericParsingLibrary;

/// <summary>
/// Thrown when a parser reaches the end of a token list unexpectedly.
/// </summary>
public class ParserEOFException : ParserException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParserEOFException"/> class. 
    /// </summary>
    public ParserEOFException()
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ParserEOFException"/> class with a message and optionally token.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="encounteredToken"></param>
    public ParserEOFException(string message, IToken? encounteredToken = null)
        : base(BuildExceptionMessage(message, encounteredToken))
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ParserEOFException"/> class with a message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public ParserEOFException(string message, Exception inner)
        : base(message, inner)
    {
    }

    private static string BuildExceptionMessage(string message, IToken? encounteredToken)
    {
        return encounteredToken == null
            ? $"{message}."
            : $"{message} at line {encounteredToken.LineNumber}, pos {encounteredToken.LinePosition}.";
    }
}
