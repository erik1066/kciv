using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFToolkit.Net.XBee;

namespace OpenCiv.Engine.Net
{
    public class MockNetworkAdapter : INetworkAdapter
    {
        public bool HasNextAction => false;

        public void AddLongTermObjective(int x, int y)
        {
            
        }

        public void AssignUnitToNode(ulong nodeAddress, Unit unit, int x, int y)
        {
           
        }

        public void BroadcastEndTurn()
        {
            
        }

        public void BroadcastStartTurn(uint turn)
        {
           
        }

        public IDictionary<ulong, string> GetNetworkNodes()
        {
            return new Dictionary<ulong, string>(0);
        }

        public void KillUnit(ulong nodeId)
        {
            
        }

        public NodeAction ReadNextAction()
        {
            return null;
        }

        public void RemoveLongTermObjective(int x, int y)
        {
            
        }

        public XBeeResponse SendAck(NodeActionAcknowledgement ack)
        {
            return null;
        }

        public bool Start()
        {
            return true;
        }

        public void Stop()
        {
            return;
        }
    }
}
