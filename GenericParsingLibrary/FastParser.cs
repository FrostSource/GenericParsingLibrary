using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    /// <summary>
    /// Fast parser uses bool type for method signatures to avoid exception throwing, leaving it up to the programmer
    /// to return from a method when an error is encountered.
    /// This requires more code and thoughtfulness but will net 3x speed improvement especially in debug mode where
    /// <see cref="EasyParser"/> might take too long with complex languages or large source strings.
    /// </summary>
    public abstract class FastParser<TToken> : BaseParser<TToken>
        where TToken : IToken
    {
        #region Consuming tokens
        /// <summary>
        /// Eat the current token waiting to be consumed.
        /// Can optionally provide a token value that must be matched.
        /// </summary>
        /// <param name="tokenType">The token that must match, otherwise an error is thrown.</param>
        /// <param name="value">Token value must match this. Leave blank to ignore.</param>
        /// <param name="caseSensitive">If the token value must be case sensitive.</param>
        /// <returns><see langword="true"/> if the token is correct, <see langword="false"/> otherwise.</returns>
        protected bool Eat(TokenType tokenType, string value = "", bool caseSensitive = true)
        {
            var t = CurrentTokenSafe;
            if (t != null &&
                t.TokenType == tokenType &&
                (
                value == "" || (caseSensitive ? t.Value.ToLower() : value.ToLower()) == (caseSensitive ? value : value.ToLower())
                )
               )
            {
                Index++;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Eat the current waiting token if it matches any given <see cref="TokenType"/>s.
        /// </summary>
        /// <param name="tokenTypes"></param>
        /// <returns><see langword="true"/> if the token is correct, <see langword="false"/> otherwise.</returns>
        protected bool Eat(params TokenType[] tokenTypes)
        {
            var t = CurrentTokenSafe;
            if (t != null)
            {
                foreach (var tokenType in tokenTypes)
                {
                    // Does this not advance token? Why did I leave it like this?
                    if (t.TokenType == tokenType)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Eat the current token waiting to be consumed or not.
        /// Can optionally provide a token value that must be matched.
        /// </summary>
        /// <param name="tokenType">The token that must match, otherwise an error is thrown.</param>
        /// <param name="value">Token value must match this. Leave blank to ignore.</param>
        /// <param name="caseSensitive">If the token value must be case sensitive.</param>
        /// <returns><see langword="true"/> if the token was eaten, <see langword="false"/> otherwise.</returns>
        protected bool EatOptional(TokenType tokenType, string value = "", bool caseSensitive = true)
        {
            var t = CurrentTokenSafe;
            if (t != null &&
                t.TokenType == tokenType &&
                (
                value == "" || (caseSensitive ? t.Value.ToLower() : value.ToLower()) == (caseSensitive ? value : value.ToLower())
                )
               )
            {
                Index++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Performs the given delegate one or more times until it returns <see langword="false"/>.
        /// </summary>
        /// <param name="func"></param>
        /// <returns><see langword="true"/> if <paramref name="func"/> ran successfully at least once, <see langword="false"/> otherwise.</returns>
        protected bool OneOrMore(Func<bool> func)
        {
            int savedIndex;
            // Try func once first because this can fail on zero matches
            if (!func()) return false;
            do
            {
                savedIndex = Index;
            }
            while (func());
            Index = savedIndex;
            return true;
        }
        /// <summary>
        /// Performs the given delegate zero or more times until it returns <see langword="false"/>.
        /// </summary>
        /// <param name="func"></param>
        protected void ZeroOrMore(Func<bool> func)
        {
            int savedIndex;
            do
            {
                savedIndex = Index;
            }
            while (func());
            Index = savedIndex;
        }
        /// <summary>
        /// Runs each passed function until <see langword="true"/> is returned, at which point the method exits.
        /// Every syntax exception is gathered and sent in the case of no function returning succesfully.
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        protected bool EitherOr(params Func<bool>[] funcs)
        {
            //var expectedTokenTypes = new HashSet<TokenType>();
            //var expectedValues = new HashSet<string>();
            foreach (var func in funcs)
            {
                var savedIndex = Index;
                if (func()) return true;

                // false return, collect errors and revert index
                //TODO: Should force user to push error at beginning of each func?
                //var e = PopError();
                //expectedTokenTypes.UnionWith(e.ExpectedTokenTypes);
                //expectedValues.UnionWith(e.ExpectedValues);
                Index = savedIndex;
            }

            // Push collected errors here?
            //throw SyntaxError(expectedTokenTypes.ToArray(), expectedValues.ToArray());
            return false;
        }
        /// <summary>
        /// An optional set of instructions.
        /// </summary>
        /// <param name="func"></param>
        /// <returns><paramref name="func"/> result.</returns>
        protected bool Optional(Func<bool> func)
        {
            var savedIndex = Index;
            if (func())
            {
                return true;
            }
            else
            {
                Index = savedIndex;
                return false;
            }
        }
        #endregion Consuming tokens

        private object? cachedValue;
        /// <summary>
        /// Caches a value to be retrieved later with <see cref="GetCache"/>.
        /// <para/>
        /// This is used instead of returning values from your parse methods,
        /// as they must return <see cref="bool"/> for <see cref="FastParser"/>.
        /// </summary>
        /// <param name="value">The value to be cached.</param>
        protected void CacheValue(object value) => cachedValue = value;
        /// <summary>
        /// Retrieves a cached value after storing it with <see cref="CacheValue(object)"/>.
        /// You should be sure that <see cref="CacheValue(object)"/> has been called first!.
        /// The value should be cast when retrieving it.
        /// <example>
        /// For example:
        /// <code>
        /// CacheValue("John");
        /// var name = (string)GetCache();
        /// </code>
        /// </example>
        /// </summary>
        /// <returns></returns>
        protected object GetCache() => cachedValue!;

        #region Error reporting
        /// <summary>
        /// Gets or sets the stack of error messages that will be reported when a parse exception falls through.
        /// </summary>
        private Stack<ParserException> ErrorStack { get; set; } = new();
        /// <summary>
        /// Pushes an error onto the stack to be reported in the case of parsing failure.
        /// </summary>
        /// <param name="exception"></param>
        protected void PushError(ParserException exception) => ErrorStack.Push(exception);
        /// <summary>
        /// Pops an error off the top of the stack so it will no longer be reported when parsing fails.
        /// This should be done as soon as code has passed the point where this error can occur.
        /// </summary>
        /// <returns>The popped exception.</returns>
        protected ParserException PopError()
        {
            return ErrorStack.Pop();
        }
        #endregion
        /// <summary>
        /// Parses the source string.
        /// </summary>
        /// <exception cref="ParserException"/>
        /// <exception cref="ParserSyntaxException"></exception>
        /// <exception cref="ParserEOFException"></exception>
        public override void Parse()
        {
            if (!ParseTop())
            {
                if (ErrorStack.Count > 0)
                    throw PopError();
                else
                    throw SyntaxError("Unexpected error.");
            }
        }

        /// <summary>
        /// Initialize an instance of the <see cref="FastParser{TToken}"/> class.
        /// </summary>
        public FastParser() { }
        /// <summary>
        /// Initialize an instance of the <see cref="FastParser{TToken}"/> class
        /// with a list of tokens.
        /// </summary>
        public FastParser(List<TToken> tokens) : base(tokens) { }
    }

    /// <summary>
    /// Fast parser changes its method signatures to avoid exception throwing, leaving it up to the programmer.
    /// This requires more code and thoughtfulness but will net 3x speed improvement especially in debug mode where
    /// <see cref="EasyParser"/> might take too long with complex languages.
    /// <para>
    /// This non-generic version uses <see cref="GenericToken"/> as the token type. 
    /// </para>
    /// </summary>
    public abstract class FastParser : FastParser<GenericToken>
    {
        /// <summary>
        /// Initialize a new <see cref="FastParser"/> with a list of <see cref="GenericToken"/> objects.
        /// </summary>
        /// <param name="tokens"></param>
        public FastParser(List<GenericToken> tokens) : base(tokens) { }
    }
}
