﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    /// <summary>
    /// Represents an end-of-file error that occurs during parsing.
    /// </summary>
    public class TokenizerEOFException : TokenizerException
    {
        /// <summary>
        /// Initalizes a new instance of the <see cref="TokenizerEOFException"/> class.
        /// </summary>
        public TokenizerEOFException()
        {
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="TokenizerEOFException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TokenizerEOFException(string message)
            : base("Unexpected end of file. " + message)
        {
        }
        /// <summary>
        /// Initalizes a new instance of the <see cref="TokenizerEOFException"/> class
        /// with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public TokenizerEOFException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
