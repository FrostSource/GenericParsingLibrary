using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    public abstract class AbstractTokenizer
    {
        public List<GenericToken> Tokens { get; protected set; } = new();
        public GenericToken? LastToken { get; protected set; }
        public string Source { get; protected set; }
        public string ExceptionMessage { get;protected set; }
        protected AbstractTokenizer(string source)
        {
            Source = source;
        }

        public abstract void Tokenize();
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
