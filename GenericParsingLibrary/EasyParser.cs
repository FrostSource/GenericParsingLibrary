
namespace GenericParsingLibrary
{
    /// <summary>
    /// The easy parser uses exceptions to escape from parsing errors and continue from the latest successful point.
    /// </summary>
    public abstract class EasyParser<TToken> : BaseParser<TToken>
        where TToken : IToken
    {
        #region Consuming tokens
        /// <summary>
        /// Eat the current token waiting to be consumed.
        /// If the given <see cref="TokenType"/> doesn't match, a syntax error will be thrown.
        /// Can optionally provide a token value that must be matched and case sensitivity.
        /// </summary>
        /// <param name="tokenType">The token that must match, otherwise an error is thrown.</param>
        /// <param name="value">Token value must match this. Leave blank to ignore.</param>
        /// <param name="caseSensitive">If the token value must be case sensitive.</param>
        /// <returns></returns>
        /// <exception cref="ParserSyntaxException"></exception>
        protected TToken Eat(TokenType tokenType, string value = "", bool caseSensitive = true)
        {
            if (CurrentToken.TokenType != tokenType ||
                (value != "" && CurrentToken.Value.ToLower() != (caseSensitive ? value : value.ToLower())))
            {
                throw SyntaxError(tokenType, value);
            }
            var token = CurrentToken;
            Index++;
            return token;
        }
        /// <summary>
        /// Eat the current waiting token if it matches any given <see cref="TokenType"/>s.
        /// </summary>
        /// <param name="tokenTypes"></param>
        /// <returns></returns>
        protected TToken Eat(params TokenType[] tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                // Does this not advance token? Why did I leave it like this?
                if (CurrentToken.TokenType == tokenType) return CurrentToken;
            }
            throw SyntaxError(tokenTypes);
        }
        /// <summary>
        /// Eat the current token waiting to be consumed or not.
        /// Can optionally provide a token value that must be matched and case sensitivity.
        /// </summary>
        /// <param name="tokenType">The token that must match, otherwise an error is thrown.</param>
        /// <param name="value">Token value must match this. Leave blank to ignore.</param>
        /// <param name="caseSensitive">If the token value must be case sensitive.</param>
        /// <returns></returns>
        protected TToken? EatOptional(TokenType tokenType, string value = "", bool caseSensitive = true)
        {
            // Make sure this if inversion works.
            if (
                CurrentToken.TokenType == tokenType &&
                (
                value == "" || (caseSensitive ? CurrentToken.Value.ToLower() : value.ToLower()) == (caseSensitive ? value : value.ToLower())
                )
               )
            {
                var token = CurrentToken;
                Index++;
                return token;
            }
            return default;
        }

        /// <summary>
        /// Performs the given delegate one or more times until a <see cref="ParserException"/> occurs.
        /// The first execution will let any exception fall through (one), but any subsequent exceptions will be caught and ignored (or more).
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        protected bool OneOrMore(Action func)
        {
            var savedIndex = Index;
            // first func out of try because this can fail on zero matches
            func();
            try
            {
                while (true)
                {
                    savedIndex = Index;
                    func();
                }
            }
            catch (ParserException)
            {
                Index = savedIndex;
            }
            return true;
        }
        /// <summary>
        /// Performs the given delegate zero or more times until a <see cref="ParserException"/> occurs.
        /// </summary>
        /// <param name="func"></param>
        protected void ZeroOrMore(Action func)
        {
            var savedIndex = Index;
            try
            {
                while (true)
                {
                    savedIndex = Index;
                    func();
                }
            }
            catch (ParserException)
            {
                Index = savedIndex;
            }
        }
        /// <summary>
        /// Performs the given function zero or more times until a <see cref="ParserException"/> occurs or the function returns <see langword="false"/>.
        /// </summary>
        /// <param name="func"></param>
        protected void ZeroOrMore(Func<bool> func)
        {
            var savedIndex = Index;
            try
            {
                while (true)
                {
                    savedIndex = Index;
                    var result = func();
                    if (result == false) break;
                }
            }
            catch (ParserException)
            {
                Index = savedIndex;
            }
        }
        /// <summary>
        /// Runs each passed function until <see langword="true"/> is returned, at which point the method exits.
        /// Every syntax exception is gathered and sent in the case of no function returning succesfully.
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        /// <exception cref="ParserSyntaxException">If no function returns <see langword="true"/>.</exception>
        protected bool EitherOr(params Func<bool>[] funcs)
        {
            var expectedTokenTypes = new HashSet<TokenType>();//List<TokenType>();
            var expectedValues = new HashSet<string>(); //List<string>();
            foreach (var func in funcs)
            {
                var savedIndex = Index;
                try
                {
                    var result = func();
                    if (result) return true;
                }
                catch (ParserSyntaxException e)
                {
                    expectedTokenTypes.UnionWith(e.ExpectedTokenTypes);
                    expectedValues.UnionWith(e.ExpectedValues);
                    Index = savedIndex;
                }
                catch (ParserException)
                {

                    Index = savedIndex;
                }
            }

            throw SyntaxError(expectedTokenTypes.ToArray(), expectedValues.ToArray());
        }
        /// <summary>
        /// Generic version of <see cref="EitherOr(Func{bool}[])"/>
        /// <para/>
        /// Returns <typeparamref name="T"/> from the first function <paramref name="funcs"/> which does not throw an exception.
        /// Every syntax exception is gathered and compiled into a single exception in the case of no function returning succesfully.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="funcs"></param>
        /// <returns><typeparamref name="T"/> from the first successful function.</returns>
        protected T EitherOr<T>(params Func<T>[] funcs)
        {
            var expectedTokenTypes = new HashSet<TokenType>();
            var expectedValues = new HashSet<string>();
            foreach (var func in funcs)
            {
                var savedIndex = Index;
                try
                {
                    return func();
                }
                catch (ParserSyntaxException e)
                {
                    expectedTokenTypes.UnionWith(e.ExpectedTokenTypes);
                    expectedValues.UnionWith(e.ExpectedValues);
                    Index = savedIndex;
                }
                catch (ParserException)
                {
                    Index = savedIndex;
                }
            }

            throw SyntaxError(expectedTokenTypes.ToArray(), expectedValues.ToArray());
        }
        /// <summary>
        /// An optional set of instructions. Will catch a syntax exception and ignore it.
        /// Passed function does not need to return anything.
        /// </summary>
        /// <param name="func"></param>
        /// <returns><see langword="true"/> if no exception occured, <see langword="false"/> otherwise.</returns>
        protected bool Optional(Action func)
        {
            var savedIndex = Index;
            try
            {
                func();
                return true;
            }
            catch (ParserSyntaxException)
            {
                Index = savedIndex;
                return false;
            }
        }
        #endregion Consuming tokens
    }

    /// <summary>
    /// The easy parser uses exceptions to escape from parsing errors and continue from the latest successful point.
    /// <para>
    /// This non-generic version uses <see cref="GenericToken"/> as the token type. 
    /// </para>
    /// </summary>
    public abstract class EasyParser : EasyParser<GenericToken>
    {

    }
}
