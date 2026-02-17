// Author:  Kyle Chapman
// Created: February 17, 2026
// Updated: February 17, 2026
// Description:
// An exception to be thrown when a player makes an invalid move.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovingCards.Exceptions
{
    internal class InvalidMoveException : Exception
    {
        public InvalidMoveException() { }

        /// <summary>
        /// Exception related to invalid moves with a draggable rectangle?
        /// </summary>
        /// <param name="message">Message describing the invalid move</param>
        public InvalidMoveException(string message) : base("Invalid Move: " + message)
        { }

    }
}
