using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    //TODO: This should probably use a different noun from 'syntax'.
    /// <summary>
    /// Represents syntax errors that occur during tokenization.
    /// </summary>
    public class TokenizerSyntaxException : TokenizerException
    {
        /// <summary>
        /// Initalizes a new instance of the <see cref="TokenizerSyntaxException"/> class.
        /// </summary>
        public TokenizerSyntaxException()
        {
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="TokenizerSyntaxException"/> class
        /// with a specified error message
        /// and position in the source string the exception occured at.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="line">Line number the exception occured at (starts at 1).</param>
        /// <param name="position">Line position the exception occured at (starts at 1).</param>
        public TokenizerSyntaxException(string message, int line, int position)
            : base(SyntaxErrorMessage(message, line, position))
        {
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="TokenizerSyntaxException"/> class
        /// with a specified error message,
        /// position in the source string the exception occured at,
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="line">Line number the exception occured at (starts at 1).</param>
        /// <param name="position">Line position the exception occured at (starts at 1).</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public TokenizerSyntaxException(string message, int line, int position, Exception inner)
            : base(SyntaxErrorMessage(message, line, position), inner)
        {
        }

        /// <summary>
        /// Builds an error message with a readable format.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="line">Line number the exception occured at (starts at 1).</param>
        /// <param name="position">Line position the exception occured at (starts at 1).</param>
        /// <returns></returns>
        private static string SyntaxErrorMessage(string message, int line, int position)
        {
            return $"Syntax Error: {message} at line {line}, pos {position}.";
        }
    }
}
