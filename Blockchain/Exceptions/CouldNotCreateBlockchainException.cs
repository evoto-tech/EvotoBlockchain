using System;

namespace Blockchain.Exceptions
{
    public class CouldNotCreateBlockchainException : Exception
    {
        public CouldNotCreateBlockchainException()
        {
        }

        public CouldNotCreateBlockchainException(string msg) : base(msg)
        {
        }
    }
}