using Blockchain.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blockchain.Tests
{
    [TestClass]
    public class PortTypeUtilsTests
    {
        [TestMethod]
        public void GetPort_Rpc_DefaultPort()
        {
            PortTypeUtils.ResetPorts();
            var port = PortTypeUtils.GetPortNumber(EPortType.Rpc);
            Assert.AreEqual(24533, port);
        }

        [TestMethod]
        public void GetPort_MultichainD_DefaultPort()
        {
            PortTypeUtils.ResetPorts();
            var port = PortTypeUtils.GetPortNumber(EPortType.MultichainD);
            Assert.AreEqual(7211, port);
        }

        [TestMethod]
        public void GetPortTwice_Rpc_NextPort()
        {
            PortTypeUtils.ResetPorts();
            PortTypeUtils.GetPortNumber(EPortType.Rpc);

            var port = PortTypeUtils.GetPortNumber(EPortType.Rpc);
            Assert.AreEqual(24534, port);
        }

        [TestMethod]
        public void GetPortTwice_MultichainD_NextPort()
        {
            PortTypeUtils.ResetPorts();
            PortTypeUtils.GetPortNumber(EPortType.MultichainD);

            var port = PortTypeUtils.GetPortNumber(EPortType.MultichainD);
            Assert.AreEqual(7212, port);
        }

        [TestMethod]
        public void ResetPorts_GetPortTwice_DefaultPort()
        {
            PortTypeUtils.ResetPorts();
            var port1 = PortTypeUtils.GetPortNumber(EPortType.MultichainD);

            PortTypeUtils.ResetPorts();
            var port2 = PortTypeUtils.GetPortNumber(EPortType.MultichainD);

            Assert.AreEqual(port1, port2);
        }
    }
}