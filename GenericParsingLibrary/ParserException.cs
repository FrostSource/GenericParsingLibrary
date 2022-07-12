using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    /// <summary>
    /// Represents errors that occur during parsing.
    /// </summary>
    public class ParserException : Exception
    {
        /// <summary>
        /// Initalizes a new instance of the <see cref="ParserException"/> class.
        /// </summary>
        public ParserException()
        {
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="ParserException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ParserException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="ParserException"/> class
        /// with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ParserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
