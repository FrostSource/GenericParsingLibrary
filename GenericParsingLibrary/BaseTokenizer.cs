using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    /// <summary>
    /// Base tokenizing class with everything required by <see cref="BaseParser"/>.
    /// </summary>
    public abstract class BaseTokenizer
    {
        /// <summary>
        /// Gets the list of tokens processed by the tokenizer.
        /// </summary>
        public List<GenericToken> Tokens { get; protected set; } = new();
        /// <summary>
        /// Gets the last token processed by the tokenizer.
        /// </summary>
        public GenericToken? LastToken { get; protected set; }
        /// <summary>
        /// Gets the source string.
        /// </summary>
        public string Source { get; protected set; }
        /// <summary>
        /// Gets the last exception message the tokenizer encountered during tokenization.
        /// </summary>
        public string ExceptionMessage { get; protected set; } = "";
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTokenizer"/> class.
        /// </summary>
        /// <param name="source"></param>
        protected BaseTokenizer(string source)
        {
            Source = source;
        }
        /// <summary>
        /// Generates tokens from the source string.
        /// </summary>
        public abstract void Tokenize();
        /// <summary>
        /// Generates tokens from the source string and catches any exceptions.
        /// </summary>
        /// <returns><see langword="true"/> if no exception occured</returns>
        public virtual bool TryTokenize()
        {
            try
            {
                Tokenize();
                return true;
            }
            catch (TokenizerException e)
            {
                ExceptionMessage = e.Message;
                return false;
            }
        }
    }
}
