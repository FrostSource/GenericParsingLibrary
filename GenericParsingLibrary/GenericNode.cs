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
    public interface INode
    {
        /// <summary>
        /// Gets the value that this node represents. This can be anything and is up to the programmer to handle.
        /// </summary>
        public dynamic? Value { get; }
        /// <summary>
        /// Gets the left hand <see cref="INode"/> associated with this <see cref="INode"/>.
        /// </summary>
        public INode? Left { get; }
        /// <summary>
        /// Gets the right hand <see cref="INode"/> associated with this <see cref="INode"/>.
        /// </summary>
        public INode? Right { get; }

        //public INode(dynamic? value = null, INode? left = null, INode? right = null)
        //{
        //    Value = value;
        //    Left = left;
        //    Right = right;
        //}
    }
}
