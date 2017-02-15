using System;
using System.Threading.Tasks;

namespace Blockchain
{
    public interface IMultiChainHandler
    {
        bool Connected { get; }
        event EventHandler<EventArgs> OnConnect;
        Task Connect(string chainName, bool clean = true);
        void DisconnectAndClose();
        void Close();
    }
}