using System;

namespace Blockchain.Exceptions
{
    public class CouldNotConnectToBlockchainException : Exception
    {
        public CouldNotConnectToBlockchainException()
        {
        }

        public CouldNotConnectToBlockchainException(string msg) : base(msg)
        {
        }
    }
}