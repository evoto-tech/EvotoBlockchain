using System;

namespace Blockchain.Exceptions
{
    public class CouldNotStartProcessException : Exception
    {
        public CouldNotStartProcessException()
        {
        }

        public CouldNotStartProcessException(string msg) : base(msg)
        {
        }
    }
}