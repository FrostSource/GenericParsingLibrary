using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    /// <summary>
    /// The generic parser contains properties and methods to simplify the parsing of <see cref="GenericToken"/> objects.
    /// </summary>
    // TODO: Turn this into an interface or inherit an interface.
    public class GenericParser
    {
        #region Handling tokens
        /// <summary>
        /// Gets or sets the list of <see cref="GenericToken"/> objects to parse through.
        /// </summary>
        protected List<GenericToken>? Tokens { get; set; }
        /// <summary>
        /// Gets the current index of <see cref="Tokens"/>.
        /// </summary>
        protected int Index { get; private set; }
        /// <summary>
        /// Gets the next token available, or null if not.
        /// </summary>
        protected GenericToken? NextToken
        {
            get
            {
                if (Tokens == null || Tokens.Count == 0 || Index >= Tokens.Count)
                {
                    return null;
                }
                else
                {
                    return Tokens[Index];
                }
            }
        }
        /// <summary>
        /// Get the current token, the one waiting to be consumed.
        /// </summary>
        /// <exception cref="ParserEOFException">Thrown when there are no tokens left.</exception>
        protected GenericToken CurrentToken
        {
            get
            {
                if (Tokens == null || Tokens.Count == 0 || Index >= Tokens.Count)
                {
                    throw EOFError();
                }
                return Tokens[Index];
            }
        }
        /// <summary>
        /// Get the current token, the one waiting to be consumed, if it exists.
        /// This will not throw an exception and returns null if not found.
        /// </summary>
        protected GenericToken? CurrentTokenSafe
        {
            get
            {
                if (Tokens == null || Tokens.Count == 0 || Index >= Tokens.Count)
                {
                    return null;
                }
                return Tokens[Index];
            }
        }
        /// <summary>
        /// Get the previous token.
        /// </summary>
        /// <exception cref="ParserException"></exception>
        protected GenericToken PreviousToken
        {
            get
            {
                if (Tokens == null || Tokens.Count == 0 || Index < 0)
                {
                    throw EOFError();
                }
                return Tokens[Index-1];
            }
        }
        #endregion Handling tokens

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
        protected GenericToken Eat(TokenType tokenType, string value = "", bool caseSensitive = true)
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
        protected GenericToken Eat(params TokenType[] tokenTypes)
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
        protected GenericToken? EatOptional(TokenType tokenType, string value = "", bool caseSensitive = true)
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
            return null;
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

        #region Error reporting
        /// <summary>
        /// Gets or sets the stack of error messages that will be reported when a parse exception falls through.
        /// </summary>
        protected Stack<string> ErrorStack { get; set; } = new();
        /// <summary>
        /// Pushes an error message onto the stack to be reported in the case of an exception being thrown.
        /// </summary>
        /// <param name="message"></param>
        protected void PushError(string message) => ErrorStack.Push(message);
        /// <summary>
        /// Pops an error off the top of the stack so it will no longer be reported when an exception is thrown.
        /// </summary>
        /// <returns></returns>
        protected string PopError()
        {
            if (ErrorStack.Count > 0)
                return ErrorStack.Pop();

            return "";
        }

        internal ParserSyntaxException SyntaxError(TokenType[] expectedTypes)
        {
            if (ErrorStack.Count > 0) return new ParserSyntaxException(ErrorStack.Peek(), CurrentToken);
            return new ParserSyntaxException(expectedTypes, CurrentToken);
        }
        internal ParserSyntaxException SyntaxError(TokenType expectedType, string expectedValue = "")
        {
            if (ErrorStack.Count > 0) return new ParserSyntaxException(ErrorStack.Peek(), CurrentToken);
            return new ParserSyntaxException(expectedType, expectedValue, CurrentToken);
        }
        internal ParserSyntaxException SyntaxError(TokenType[] expectedTypes, string[] expectedValues)
        {
            if (ErrorStack.Count > 0) return new ParserSyntaxException(ErrorStack.Peek(), CurrentToken);
            return new ParserSyntaxException(expectedTypes, expectedValues, CurrentToken);
        }
        internal ParserSyntaxException SyntaxError(string message)
        {
            if (ErrorStack.Count > 0) return new ParserSyntaxException(ErrorStack.Peek(), CurrentToken);
            return new ParserSyntaxException(message, CurrentToken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal ParserException EOFError(string message = "Unexpected end of file")
        {
            if (ErrorStack.Count > 0) return new ParserSyntaxException(ErrorStack.Peek(), CurrentToken);
            return new ParserEOFException(message, CurrentTokenSafe);
        }
        #endregion Error reporting
    }
}
