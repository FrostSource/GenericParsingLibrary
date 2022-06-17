using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    public class GenericParser
    {
        protected List<GenericToken>? Tokens { get; set; }
        protected int Index { get; private set; }
        protected Stack<string> ErrorStack { get; set; } = new();
        /// <summary>
        /// Get the next token available, or null if not.
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
        /// An optional set of instructions. Will catch a syntax exception and ignore it.
        /// Passed function does not need to return anything.
        /// </summary>
        /// <param name="func"></param>
        protected void Optional(Action func)
        {
            var savedIndex = Index;
            try
            {
                func();
            }
            catch (ParserSyntaxException)
            {
                Index = savedIndex;
            }
        }

        protected void PushError(string message) => ErrorStack.Push(message);
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
    }
}
