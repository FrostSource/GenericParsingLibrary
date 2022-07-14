
namespace GenericParsingLibrary;

/// <summary>
/// Base parser class from which all parsers should derive.
/// </summary>
public abstract class BaseParser<TToken> : IParser<TToken>
    where TToken : IToken
{
    #region Token properties
    /// <summary>
    /// Gets or sets the list of <see cref="GenericToken"/> objects to parse through.
    /// </summary>
    public List<TToken>? Tokens { get; protected set; }
    /// <summary>
    /// Gets the current index of <see cref="Tokens"/>.
    /// </summary>
    public int Index { get; protected set; }
    /// <summary>
    /// Get the current token, the one waiting to be consumed.
    /// </summary>
    /// <exception cref="ParserEOFException">Thrown when there are no tokens left.</exception>
    public virtual TToken CurrentToken
    {
        get
        {
            return Tokens == null || Tokens.Count == 0 || Index >= Tokens.Count ? throw EOFError() : Tokens[Index];
        }
    }
    /// <summary>
    /// Get the current token, the one waiting to be consumed, if it exists.
    /// This will not throw an exception and returns null if not found.
    /// </summary>
    protected virtual TToken? CurrentTokenSafe
    {
        get
        {
            return Tokens == null || Tokens.Count == 0 || Index >= Tokens.Count ? default : Tokens[Index];
        }
    }
    /// <summary>
    /// Gets the next token available, or null if not.
    /// </summary>
    protected virtual TToken? NextToken
    {
        get
        {
            return Tokens == null || Tokens.Count == 0 || Index >= Tokens.Count ? default : Tokens[Index];
        }
    }
    /// <summary>
    /// Get the previous token.
    /// </summary>
    /// <exception cref="ParserException"></exception>
    protected virtual TToken PreviousToken
    {
        get
        {
            return Tokens == null || Tokens.Count == 0 || Index < 0 ? throw EOFError() : Tokens[Index - 1];
        }
    }
    /// <summary>
    /// Resets the parser to the beginning state.
    /// </summary>
    protected virtual void Reset()
    {
        Index = 0;
    }
    #endregion Handling tokens

    #region Error reporting
    /// <summary>
    /// Creates a new <see cref="ParserSyntaxException"/> from an array of expected types.
    /// </summary>
    /// <param name="expectedTypes"></param>
    /// <returns></returns>
    protected ParserSyntaxException SyntaxError(TokenType[] expectedTypes)
    {
        return new ParserSyntaxException(expectedTypes, CurrentToken);
    }
    /// <summary>
    /// Creates a new <see cref="ParserSyntaxException"/> from a type and value.
    /// </summary>
    /// <param name="expectedType"></param>
    /// <param name="expectedValue"></param>
    /// <returns></returns>
    protected ParserSyntaxException SyntaxError(TokenType expectedType, string expectedValue = "")
    {
        return new ParserSyntaxException(expectedType, expectedValue, CurrentToken);
    }
    /// <summary>
    /// Creates a new <see cref="ParserSyntaxException"/> from types and values.
    /// </summary>
    /// <param name="expectedTypes"></param>
    /// <param name="expectedValues"></param>
    /// <returns></returns>
    protected ParserSyntaxException SyntaxError(TokenType[] expectedTypes, string[] expectedValues)
    {
        return new ParserSyntaxException(expectedTypes, expectedValues, CurrentToken);
    }
    /// <summary>
    /// Creates a new <see cref="ParserSyntaxException"/> with a message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected ParserSyntaxException SyntaxError(string message)
    {
        return new ParserSyntaxException(message, CurrentToken);
    }
    /// <summary>
    /// Creates a <see cref="ParserEOFException"/> with a message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns><see cref="ParserEOFException"/> or <see cref="ParserSyntaxException"/></returns>
    protected ParserException EOFError(string message = "Unexpected end of file")
    {
        return new ParserEOFException(message, CurrentTokenSafe);
    }
    #endregion Error reporting

    /// <summary>
    /// Initialize an instance of the <see cref="BaseParser{TToken}"/> class.
    /// </summary>
    public BaseParser() { }
    /// <summary>
    /// Initialize an instance of the <see cref="BaseParser{TToken}"/> class
    /// with a list of tokens.
    /// </summary>
    public BaseParser(List<TToken> tokens)
    {
        Tokens = tokens;
    }

    /// <summary>
    /// Gets the exception message that occured if <see cref="TryParse"/> failed.
    /// </summary>
    public string ExceptionMessage { get; protected set; } = "";
    /// <summary>
    /// Top level parsing method should call all other elements and create any resulting tree/objects.
    /// </summary>
    /// <returns><see langword="true"/> if parsing was successful, <see langword="false"/> otherwise.</returns>
    protected abstract bool ParseTop();
    /// <inheritdoc/>
    public virtual void Parse()
    {
        // TODO: Property to allow failing if not at end of tokens with default true?
        ParseTop();
    }
    /// <summary>
    /// Safely parses, returning true if successful, otherwise false and also sets <see cref="ExceptionMessage"/>.
    /// </summary>
    /// <returns></returns>
    public virtual bool TryParse()
    {
        try
        {
            Parse();
            return true;
        }
        catch (ParserException e)
        {
            ExceptionMessage = e.Message;
            return false;
        }
    }
}
