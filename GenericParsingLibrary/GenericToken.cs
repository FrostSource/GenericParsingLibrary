namespace GenericParsingLibrary;

/// <summary>
/// A generic token which contains the most basic information about a token.
/// Generated by <see cref="GenericTokenizer"/> or a sub class.
/// </summary>
public class GenericToken : IToken, IEquatable<GenericToken?>
{
    /// <inheritdoc/>
    public TokenType TokenType { get; private set; }
    /// <inheritdoc/>
    public string Value { get; private set; }
    /// <inheritdoc/>
    public int Index { get; private set; }
    /// <inheritdoc/>
    public int LineNumber { get; private set; }
    /// <inheritdoc/>
    public int LinePosition { get; private set; }
    /// <summary>
    /// Instantiates a new instance of the <see cref="GenericToken"/> class.
    /// </summary>
    /// <param name="tokenType">Type is relevant only to the designer and his parser.</param>
    /// <param name="value">Text value of the token.</param>
    /// <param name="index">Index where the first char appeared in the source.</param>
    /// <param name="lineNumber"></param>
    /// <param name="linePosition"></param>
    public GenericToken(TokenType tokenType, string value, int index, int lineNumber, int linePosition)
    {
        TokenType = tokenType;
        Value = value;
        Index = index;
        LineNumber = lineNumber;
        LinePosition = linePosition;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"GenericToken ({TokenType}) [{GenericTokenizer.ReplaceSpecialCharsInString(Value)}]";
    }

    ///<inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as GenericToken);
    }
    ///<inheritdoc/>
    public bool Equals(GenericToken? other)
    {
        return other is not null &&
               TokenType == other.TokenType &&
               Value == other.Value;
    }
    ///<inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(TokenType, Value);
    }
}
