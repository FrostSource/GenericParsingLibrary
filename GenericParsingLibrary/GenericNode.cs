using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericParsingLibrary
{
    /// <summary>
    /// Used mainly for programming languages that will generate node trees.
    /// </summary>
    public class GenericNode
    {
        /// <summary>
        /// Gets the value that this node represents. This can be anything and is up to the programmer to handle.
        /// </summary>
        public dynamic? Value { get; }
        /// <summary>
        /// Gets the left hand <see cref="GenericNode"/> associated with this <see cref="GenericNode"/>.
        /// </summary>
        public GenericNode? Left { get; }
        /// <summary>
        /// Gets the right hand <see cref="GenericNode"/> associated with this <see cref="GenericNode"/>.
        /// </summary>
        public GenericNode? Right { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericNode"/> class with a value and left/right nodes.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public GenericNode(dynamic? value = null, GenericNode? left = null, GenericNode? right = null)
        {
            Value = value;
            Left = left;
            Right = right;
        }
    }
}
