
namespace GenericParsingLibrary
{
    /// <summary>
    /// Base parser class from which all parsers should derive.
    /// </summary>
    public interface IParser<TToken>
        where TToken : IToken
    {
        #region Token properties
        /// <summary>
        /// Gets or sets the list of <typeparamref name="TToken"/> objects to parse through.
        /// </summary>
        List<TToken>? Tokens { get; }
        /// <summary>
        /// Gets the current index of <see cref="Tokens"/>.
        /// </summary>
        int Index { get; }
        /// <summary>
        /// Get the current token, the one waiting to be consumed.
        /// </summary>
        /// <exception cref="ParserEOFException">Thrown when there are no tokens left.</exception>
        TToken CurrentToken { get; }
        #endregion Handling tokens

        /// <summary>
        /// Parses.
        /// </summary>
        void Parse();
        /// <summary>
        /// Safely parses, returning the success result.
        /// </summary>
        /// <returns></returns>
        bool TryParse();
    }
}
