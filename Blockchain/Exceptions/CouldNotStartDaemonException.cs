using System;

namespace Blockchain.Exceptions
{
    public class CouldNotStartDaemonException : Exception
    {
        public CouldNotStartDaemonException()
        {
        }

        public CouldNotStartDaemonException(string msg) : base(msg)
        {
        }
    }
}