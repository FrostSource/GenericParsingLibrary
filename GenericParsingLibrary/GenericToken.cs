﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    /// <summary>
    /// A generic token which contains the most basic information about a token.
    /// Generated by <see cref="GenericTokenizer"/> or a sub class.
    /// </summary>
    public class GenericToken : IToken
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
        /// <summary>
        /// Determines whether the specified token is equal to the current token.
        /// </summary>
        /// <param name="obj">The token to compare with the current token.</param>
        /// <returns><see langword="true"/> if the token matches; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj)
        {
            return (obj is GenericToken t
                && t.Value == Value && t.TokenType == TokenType);
        }
    }
}
