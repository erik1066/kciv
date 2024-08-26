using System;
using System.Collections.Generic;
using MFToolkit.Net.XBee;

namespace OpenCiv.Engine
{
    public interface INetworkAdapter
    {
        bool HasNextAction { get; }
        
        bool Start();
        void Stop();
        IDictionary<ulong, string> GetNetworkNodes();
        NodeAction ReadNextAction();
        XBeeResponse SendAck(NodeActionAcknowledgement ack);
        void AddLongTermObjective(int x, int y);
        void RemoveLongTermObjective(int x, int y);
        void KillUnit(ulong nodeId);
        void AssignUnitToNode(ulong nodeAddress, Unit unit, int x, int y);
        void BroadcastStartTurn(uint turn);
        void BroadcastEndTurn();
    }
}
