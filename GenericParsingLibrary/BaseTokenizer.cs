using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    /// <summary>
    /// Base tokenizing class with everything required by <see cref="EasyParser"/>.
    /// </summary>
    public abstract class BaseTokenizer : ITokenizer
    {
        /// <inheritdoc/>
        public List<GenericToken> Tokens { get; protected set; } = new();
        /// <inheritdoc/>
        public GenericToken? LastToken { get; protected set; }
        /// <inheritdoc/>
        public string Source { get; protected set; }
        /// <inheritdoc/>
        public string ExceptionMessage { get; protected set; } = "";
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTokenizer"/> class.
        /// </summary>
        /// <param name="source"></param>
        public BaseTokenizer(string source)
        {
            Source = source;
        }
        /// <inheritdoc/>
        public abstract void Tokenize();
        /// <inheritdoc/>
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
