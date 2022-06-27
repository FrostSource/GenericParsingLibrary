using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    public abstract class AbstractTokenizer
    {
        public abstract List<GenericToken> Tokens { get; set; }
        public abstract GenericToken? LastToken { get; }
    }
}
