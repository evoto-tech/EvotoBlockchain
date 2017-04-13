using System;
using System.Collections.Generic;

namespace Blockchain.Models
{
    public enum EPortType
    {
        MultichainD,
        // Strictly this doesn't have to be a different range, however there seems to
        // be an issue running both client and server on the same machine, the client
        // can't detect that the port is in use by the server. This is a workaround
        // TODO: Refactor so this isn't necessary, or fix port detection
        ClientMultichainD,

        Rpc,
        // Same again
        ClientRpc
    }

    public static class PortTypeUtils
    {
        private static readonly Dictionary<EPortType, int> Ports = new Dictionary<EPortType, int>();

        /// <summary>
        ///     Clear port memory, meaning allocations will start again from default
        ///     Initially for testing, however this could be needed under other circumstances
        /// </summary>
        public static void ResetPorts()
        {
            Ports.Clear();
        }

        public static int GetPortNumber(EPortType type)
        {
            int port;
            if (Ports.TryGetValue(type, out port))
            {
                Ports[type]++;
                return port;
            }


            switch (type)
            {
                case EPortType.MultichainD:
                    port = 7211;
                    break;
                case EPortType.ClientMultichainD:
                    port = 8211;
                    break;
                case EPortType.Rpc:
                    port = 24533;
                    break;
                case EPortType.ClientRpc:
                    port = 25533;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Ports[type] = port + 1;
            return port;
        }
    }
}