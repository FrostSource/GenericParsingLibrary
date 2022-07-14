namespace GenericParsingLibrary;

/// <summary>
/// Tokenizer interface.
/// </summary>
public interface ITokenizer
{
    /// <summary>
    /// Gets the list of tokens processed by the tokenizer.
    /// </summary>
    List<GenericToken> Tokens { get; }
    /// <summary>
    /// Gets the last token processed by the tokenizer.
    /// </summary>
    GenericToken? LastToken { get; }
    /// <summary>
    /// Gets the source string.
    /// </summary>
    string Source { get; }
    /// <summary>
    /// Gets the last exception message the tokenizer encountered during tokenization.
    /// </summary>
    string ExceptionMessage { get; }

    /// <summary>
    /// Generates tokens from the source string.
    /// </summary>
    abstract void Tokenize();
    /// <summary>
    /// Generates tokens from the source string and catches any exceptions.
    /// </summary>
    /// <returns><see langword="true"/> if no exception occured</returns>
    bool TryTokenize();
}
