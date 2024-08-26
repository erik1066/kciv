using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MFToolkit.Net.XBee;

namespace OpenCiv.Engine
{
    public enum NodeActionType
    {
        None,
        Move,
        Attack,
        Fortify,
        Activate,
        Pillage,
        Report,
        RequestSitrep,
        RequestCombatReport,
        RequestExtendedSitrep,
        RequestPath,
        EndTurn
    }

    public enum ServerUpdate
    {
        None,
        AckSitrep,
        AckCombatReport,
        TurnStart,
        NodeKilled,
        AddObjective,
        RemoveObjective,
        AssignUnit,
        TurnOver,
        MovesRemaining,
        NodeAttackTarget, // attackerX, attackerY, defenderX, defenderY
        AckExtendedSitrep,
        AckPath
    }

    public sealed class NodeAction
    {
        public NodeActionType Type { get; private set; }
        public int CurrentX { get; private set; }
        public int CurrentY { get; private set; }
        public int ActionTargetX { get; private set; }
        public int ActionTargetY { get; private set; }
        public ulong NodeAddress { get; private set; }
        public UInt16 NodeAddress16 { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type of action</param>
        /// <param name="actionTargetX">The X coord where the action should take place</param>
        /// <param name="actionTargetY">The Y coord where the action should take place</param>
        /// <param name="currentX">The current X coord of the unit</param>
        /// <param name="currentY">The current Y coord of the unit</param>
        /// <param name="nodeAddress">64-bit address of the XBee node executing this action</param>
        public NodeAction(NodeActionType type, int actionTargetX, int actionTargetY, int currentX, int currentY, ulong nodeAddress)
        {
            Type = type;
            ActionTargetX = actionTargetX;
            ActionTargetY = actionTargetY;
            CurrentX = currentX;
            CurrentY = currentY;
            NodeAddress = nodeAddress;
        }
    }

    public sealed class NodeActionAcknowledgement
    {
        private List<Tile> _twoRangeTiles = new List<Tile>(12);
        public NodeActionType ActionType { get; private set; }
        public ServerUpdate AckType { get; private set; }
        public bool WasActionSuccessful { get; private set; }
        public bool UnitDestroyed { get; private set; }
        public ulong DestinationAddress { get; set; }
        public Tile SitrepTile { get; set; }
        public CombatReport CombatReport { get; set; }
        public int RemainingMoves { get; set; }
        public byte RemainingHitPoints { get; set; }
        public IEnumerable<Tile> TwoRangeTiles => _twoRangeTiles.AsEnumerable();
        public IEnumerable<Tile> Path { get; set; }
        public int NewX { get; set; }
        public int NewY { get; set; }

        public NodeActionAcknowledgement(
            NodeActionType actionType,
            ServerUpdate ackType,
            bool success, 
            bool unitDestroyed = false)
        {
            ActionType = actionType;
            AckType = ackType;
            WasActionSuccessful = success;
            UnitDestroyed = unitDestroyed;
        }

        public void AddTwoRangeTiles(IEnumerable<Tile> twoRangeTiles)
        {
            if (twoRangeTiles.Count() > 18) throw new ArgumentOutOfRangeException(nameof(twoRangeTiles));
            if (twoRangeTiles == null) throw new ArgumentNullException(nameof(twoRangeTiles));

            _twoRangeTiles.Clear();
            _twoRangeTiles.AddRange(twoRangeTiles);
        }
    }

    internal sealed class XBeeNetworkAdapter : INetworkAdapter
    {
        const int FORCED_DELAY_LARGE = 500;
        const int FORCED_DELAY_SMALL = 100;

        private const int BAUD_RATE = 9600;
        private const ulong BROADCAST_ADDRESS_64 = 0xFFFFFFFFFFFFFFFF;
        private const ushort BROADCAST_ADDRESS_16 = 0xFFFE;
        private readonly Queue<NodeAction> _actions = new Queue<NodeAction>(500);
        private readonly string _comPort = string.Empty;
        private readonly string _selfNodeId = string.Empty;
        private readonly Dictionary<ulong, string> _nodes = new Dictionary<ulong, string>();
        private readonly Dictionary<ulong, ushort> _nodes16 = new Dictionary<ulong, ushort>();
        private readonly XBee _xbee;

        public bool HasNextAction
        {
            get
            {
                return _actions.Count > 0;
            }
        }

        public XBeeNetworkAdapter(string comPort, string selfNodeId)
        {
            _comPort = comPort;
            _selfNodeId = selfNodeId;
            _xbee = new XBee(comPort, BAUD_RATE, ApiType.EnabledWithEscaped);
        }

        // Check if destructor is really needed, GC should handle this for us since XBee class implements IDisposable...
        ~XBeeNetworkAdapter()
        {
            if (_xbee != null)
            {
                _xbee.Close();
            }
        }

        public bool Start()
        {
            //Thread thd1 = new Thread(new ThreadStart(RunModule));
            //thd1.IsBackground = true;
            //thd1.Start();

            //return true;
            RunModule();
            return true;
        }

        public void RunModule()
        {
            _xbee.FrameReceived += new FrameReceivedEventHandler(xbee_FrameReceived);
            bool success = _xbee.Open(_comPort, BAUD_RATE);

            // discovering the network
            AtCommand at = new NodeDiscoverCommand();

            _xbee.Execute(at);
            Thread.Sleep(4 * 1000);

            _xbee.Execute(at);
            Thread.Sleep(4 * 1000);
        }

        private void xbee_FrameReceived(object sender, FrameReceivedEventArgs e)
        {
            if (e.Response.ApiID == XBeeApiType.AtCommandResponse)
            {
                NodeDiscover nd = NodeDiscover.Parse((e.Response as AtCommandResponse));

                ulong address = nd.SerialNumber.Value;
                string nodeId = nd.NodeIdentifier;

                if (!_nodes.ContainsKey(address) && !nodeId.Equals("SERVER", StringComparison.OrdinalIgnoreCase) && !nodeId.Equals("MUFFLEY", StringComparison.OrdinalIgnoreCase))
                {
                    _nodes.Add(address, nd.NodeIdentifier);
                    _nodes16.Add(address, nd.ShortAddress.Value);
                }                
            }
            else if (e.Response.ApiID == XBeeApiType.ZNetRxPacket)
            {
                ZNetRxResponse response = (e.Response as ZNetRxResponse);
                if (response != null)
                {
                    byte[] payload = response.Value;

                    if (payload.Length != 5) return;

                    NodeActionType action = (NodeActionType)payload[0];
                    ulong nodeAddress64 = response.SerialNumber.Value;
                    ushort nodeAddress16 = response.ShortAddress.Value;

                    NodeAction nodeAction = null;

                    switch (action)
                    {
                        case NodeActionType.Activate:
                        case NodeActionType.Fortify:
                        case NodeActionType.Pillage:
                        case NodeActionType.Move:
                        case NodeActionType.Attack:
                        case NodeActionType.RequestSitrep:
                        case NodeActionType.RequestCombatReport:
                        case NodeActionType.RequestExtendedSitrep:
                        case NodeActionType.RequestPath:
                            nodeAction = new NodeAction(action, payload[1], payload[2], payload[3], payload[4], nodeAddress64);
                            nodeAction.NodeAddress16 = nodeAddress16;
                            break;
                        case NodeActionType.EndTurn:
                            nodeAction = new NodeAction(action, payload[1], payload[2], payload[3], payload[4], nodeAddress64);
                            nodeAction.NodeAddress16 = nodeAddress16;
                            break;
                    }

                    if (nodeAction != null)
                    {
                        _actions.Enqueue(nodeAction);
                    }
                }
            }

            //throw new NotImplementedException();
        }

        public void Stop()
        {
            _xbee.Close();
        }

        public XBeeResponse SendAck(NodeActionAcknowledgement ack)
        {
            XBeeResponse res = null;

            ushort nodeAddress16 = _nodes16[ack.DestinationAddress];

            switch (ack.AckType)
            {
                case ServerUpdate.AckCombatReport:

                    byte[] ackComatReportPayload = new byte[8];
                    ackComatReportPayload[0] = (byte)ServerUpdate.AckCombatReport;

                    byte[] report = CreateCombatReportPayload(ack.CombatReport);

                    for (int i = 1; i < 8; i++)
                    {
                        ackComatReportPayload[i] = report[i - 1];
                    }

                    XBeeRequest cr = new ZNetTxRequest(new XBeeAddress64(ack.DestinationAddress), new XBeeAddress16(nodeAddress16), ackComatReportPayload);
                    res = _xbee.Execute(cr);

                    System.Threading.Thread.Sleep(FORCED_DELAY_LARGE);

                    break;
                case ServerUpdate.AckSitrep:

                    byte[] ackSitrepPayload = new byte[51];
                    ackSitrepPayload[0] = (byte)ServerUpdate.AckSitrep;

                    byte[] sitrep = CreateSitrepPayload(ack.SitrepTile);

                    for (int i = 1; i <= sitrep.Length; i++)
                    {
                        ackSitrepPayload[i] = sitrep[i - 1];
                    }

                    XBeeRequest sr = new ZNetTxRequest(new XBeeAddress64(ack.DestinationAddress), new XBeeAddress16(nodeAddress16), ackSitrepPayload);
                    res = _xbee.Execute(sr);
                    System.Threading.Thread.Sleep(FORCED_DELAY_SMALL);
                    break;

                case ServerUpdate.MovesRemaining:

                    byte[] movesPayload = new byte[5];
                    movesPayload[0] = (byte)ServerUpdate.MovesRemaining;
                    movesPayload[1] = (byte)ack.RemainingMoves;
                    movesPayload[2] = (byte)ack.RemainingHitPoints;
                    movesPayload[3] = (byte)ack.NewX;
                    movesPayload[4] = (byte)ack.NewY;

                    XBeeRequest moveRemainingAckRequest = new ZNetTxRequest(new XBeeAddress64(ack.DestinationAddress), new XBeeAddress16(nodeAddress16), movesPayload);
                    res = _xbee.Execute(moveRemainingAckRequest);

                    Thread.Sleep(FORCED_DELAY_SMALL);

                    break;

                case ServerUpdate.AckExtendedSitrep:

                    byte[] twoRangePayload = new byte[37];
                    twoRangePayload[0] = (byte)ServerUpdate.AckExtendedSitrep;

                    List<Tile> tilesToInclude = ack.TwoRangeTiles.ToList();

                    int k = 1;
                    for (int i = 0; i < 12; i++)
                    {
                        Tile t = tilesToInclude[i];
                        twoRangePayload[k] = t.HasPlayerUnit == true ? (byte)1 : (byte)0;
                        k++;
                        twoRangePayload[k] = (byte)t.X;
                        k++;
                        twoRangePayload[k] = (byte)t.Y;
                        k++;
                    }

                    XBeeRequest extSitrepAckReq = new ZNetTxRequest(new XBeeAddress64(ack.DestinationAddress), new XBeeAddress16(nodeAddress16), twoRangePayload);
                    res = _xbee.Execute(extSitrepAckReq);

                    System.Threading.Thread.Sleep(FORCED_DELAY_LARGE);

                    break;

                case ServerUpdate.AckPath:

                    // for sending full path, disregard this
                    //if (ack.Path == null) return null;
                    //List<Tile> path = ack.Path.ToList();

                    //byte[] pathPayload = new byte[path.Count > 30 ? 61 : (path.Count * 2) + 1];

                    //pathPayload[0] = (byte)ServerUpdate.AckPath;                    

                    //int j = 1;
                    //for (int i = 0; i < path.Count; i++)
                    //{
                    //    Tile t = path[i];

                    //    pathPayload[j] = (byte)t.X;
                    //    j++;
                    //    pathPayload[j] = (byte)t.Y;
                    //    j++;
                    //}
                    
                    if (ack.Path == null) return null;
                    List<Tile> path = ack.Path.ToList();

                    byte[] pathPayload = new byte[3];

                    pathPayload[0] = (byte)ServerUpdate.AckPath;
                    pathPayload[1] = (byte)path[0].X;
                    pathPayload[2] = (byte)path[0].Y;

                    XBeeRequest pathAckRequest = new ZNetTxRequest(new XBeeAddress64(ack.DestinationAddress), new XBeeAddress16(nodeAddress16), pathPayload);
                    res = _xbee.Execute(pathAckRequest);

                    Thread.Sleep(FORCED_DELAY_SMALL);

                    break;
            }

            return res;
        }

        private byte[] CreateCombatReportPayload(CombatReport report)
        {
            byte[] payload = new byte[7];

            byte attackerHpLoss = report.AttackerHitPointLoss > 100 ? (byte)100 : (byte)report.AttackerHitPointLoss; // in case HP loss is extreme we don't want to overflow
            byte defenderHpLoss = report.DefenderHitPointLoss > 100 ? (byte)100 : (byte)report.DefenderHitPointLoss; // in case HP loss is extreme we don't want to overflow

            payload[0] = report.Defender == null ? (byte)0 : (byte)report.Defender.Type;
            payload[1] = (byte)report.Tile.X;
            payload[2] = (byte)report.Tile.Y;

            payload[3] = attackerHpLoss;
            payload[4] = defenderHpLoss;
            payload[5] = report.AttackerHitPointLoss > report.Attacker.HitPoints ? (byte)1 : (byte)0;

            if (report.Defender == null)
            {
                payload[6] = report.DefenderHitPointLoss > report.City.HitPoints ? (byte)1 : (byte)0;
            }
            else
            {
                payload[6] = report.DefenderHitPointLoss > report.Defender.HitPoints ? (byte)1 : (byte)0;
            }
            

            return payload;
        }

        private byte[] CreateSitrepPayload(Tile tile)
        {
            byte[] payload = new byte[4];

            payload[0] = (tile.Improvement == ImprovementType.Farms || tile.Improvement == ImprovementType.Mines) ? (byte)1 : (byte)0;
            payload[1] = (byte)tile.Terrain;

            payload[2] = 0;
            payload[3] = 0;
            //payload[4] = 0;

            //if (tile.TileN.HasUnit) payload[2] = 1;
            //if (tile.TileNE.HasUnit) payload[2] += 2;
            //if (tile.TileSE.HasUnit) payload[2] += 4;
            //if (tile.TileS.HasUnit) payload[2] += 8;
            //if (tile.TileSW.HasUnit) payload[2] += 16;
            //if (tile.TileNW.HasUnit) payload[2] += 32;

            if (tile.TileN.HasUnit && tile.TileN.CurrentUnit.Owner != tile.CurrentUnit.Owner) payload[2] = 1;
            if (tile.TileNE.HasUnit && tile.TileNE.CurrentUnit.Owner != tile.CurrentUnit.Owner) payload[2] += 2;
            if (tile.TileSE.HasUnit && tile.TileSE.CurrentUnit.Owner != tile.CurrentUnit.Owner) payload[2] += 4;
            if (tile.TileS.HasUnit && tile.TileS.CurrentUnit.Owner != tile.CurrentUnit.Owner) payload[2] += 8;
            if (tile.TileSW.HasUnit && tile.TileSW.CurrentUnit.Owner != tile.CurrentUnit.Owner) payload[2] += 16;
            if (tile.TileNW.HasUnit && tile.TileNW.CurrentUnit.Owner != tile.CurrentUnit.Owner) payload[2] += 32;

            if (tile.TileN.HasCity && tile.TileN.City.Owner != tile.CurrentUnit.Owner) payload[3] = 1;
            if (tile.TileNE.HasCity && tile.TileNE.City.Owner != tile.CurrentUnit.Owner) payload[3] += 2;
            if (tile.TileSE.HasCity && tile.TileSE.City.Owner != tile.CurrentUnit.Owner) payload[3] += 4;
            if (tile.TileS.HasCity && tile.TileS.City.Owner != tile.CurrentUnit.Owner) payload[3] += 8;
            if (tile.TileSW.HasCity && tile.TileSW.City.Owner != tile.CurrentUnit.Owner) payload[3] += 16;
            if (tile.TileNW.HasCity && tile.TileNW.City.Owner != tile.CurrentUnit.Owner) payload[3] += 32;

            ///* Has unit */          payload[2] = tile.TileN.HasUnit ? (byte)1 : (byte)0;
            ///* Is enemy unit*/      payload[3] = tile.TileN.HasUnit && tile.TileN.CurrentUnit.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            ///* Enemy unit type*/    payload[4] = tile.TileN.HasUnit ? (byte)tile.TileN.CurrentUnit.Type : (byte)0;            
            ///* CP of enemy*/        payload[5] = tile.TileN.HasUnit ? (byte)tile.TileN.CurrentUnit.CombatPower : (byte)0;
            ///* RP of enemy*/        payload[6] = tile.TileN.HasUnit ? (byte)tile.TileN.CurrentUnit.RangedPower : (byte)0;
            ///* Has city*/           payload[7] = tile.TileN.HasCity ? (byte)1 : (byte)0;
            ///* Is enemy city? */    payload[8] = tile.TileN.HasCity && tile.TileN.City.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            ///* Pillage-able? */     payload[9] = (tile.TileN.Improvement == ImprovementType.Farms || tile.TileN.Improvement == ImprovementType.Mines) ? (byte)1 : (byte)0;

            //payload[10] = tile.TileNE.HasUnit ? (byte)1 : (byte)0;
            //payload[11] = tile.TileNE.HasUnit && tile.TileNE.CurrentUnit.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            //payload[12] = tile.TileNE.HasUnit ? (byte)tile.TileNE.CurrentUnit.Type : (byte)0;
            //payload[13] = tile.TileNE.HasUnit ? (byte)tile.TileNE.CurrentUnit.CombatPower : (byte)0;
            //payload[14] = tile.TileNE.HasUnit ? (byte)tile.TileNE.CurrentUnit.RangedPower : (byte)0;
            //payload[15] = tile.TileNE.HasCity ? (byte)1 : (byte)0;
            //payload[16] = tile.TileNE.HasCity && tile.TileNE.City.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;            
            //payload[17] = (tile.TileNE.Improvement == ImprovementType.Farms || tile.TileNE.Improvement == ImprovementType.Mines) ? (byte)1 : (byte)0;


            //payload[18] = tile.TileSE.HasUnit ? (byte)1 : (byte)0;
            //payload[19] = tile.TileSE.HasUnit && tile.TileSE.CurrentUnit.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            //payload[20] = tile.TileSE.HasUnit ? (byte)tile.TileSE.CurrentUnit.Type : (byte)0;
            //payload[21] = tile.TileSE.HasUnit ? (byte)tile.TileSE.CurrentUnit.CombatPower : (byte)0;
            //payload[22] = tile.TileSE.HasUnit ? (byte)tile.TileSE.CurrentUnit.RangedPower : (byte)0;
            //payload[23] = tile.TileSE.HasCity ? (byte)1 : (byte)0;
            //payload[24] = tile.TileSE.HasCity && tile.TileSE.City.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            //payload[25] = (tile.TileSE.Improvement == ImprovementType.Farms || tile.TileSE.Improvement == ImprovementType.Mines) ? (byte)1 : (byte)0;


            //payload[26] = tile.TileS.HasUnit ? (byte)1 : (byte)0;
            //payload[27] = tile.TileS.HasUnit && tile.TileS.CurrentUnit.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            //payload[28] = tile.TileS.HasUnit ? (byte)tile.TileS.CurrentUnit.Type : (byte)0;
            //payload[29] = tile.TileS.HasUnit ? (byte)tile.TileS.CurrentUnit.CombatPower : (byte)0;
            //payload[30] = tile.TileS.HasUnit ? (byte)tile.TileS.CurrentUnit.RangedPower : (byte)0;
            //payload[31] = tile.TileS.HasCity ? (byte)1 : (byte)0;
            //payload[32] = tile.TileS.HasCity && tile.TileS.City.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            //payload[33] = (tile.TileS.Improvement == ImprovementType.Farms || tile.TileS.Improvement == ImprovementType.Mines) ? (byte)1 : (byte)0;


            //payload[34] = tile.TileSW.HasUnit ? (byte)1 : (byte)0;
            //payload[35] = tile.TileSW.HasUnit && tile.TileSW.CurrentUnit.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            //payload[36] = tile.TileSW.HasUnit ? (byte)tile.TileSW.CurrentUnit.Type : (byte)0;
            //payload[37] = tile.TileSW.HasUnit ? (byte)tile.TileSW.CurrentUnit.CombatPower : (byte)0;
            //payload[38] = tile.TileSW.HasUnit ? (byte)tile.TileSW.CurrentUnit.RangedPower : (byte)0;
            //payload[39] = tile.TileSW.HasCity ? (byte)1 : (byte)0;
            //payload[40] = tile.TileSW.HasCity && tile.TileSW.City.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            //payload[41] = (tile.TileSW.Improvement == ImprovementType.Farms || tile.TileSW.Improvement == ImprovementType.Mines) ? (byte)1 : (byte)0;


            //payload[42] = tile.TileNW.HasUnit ? (byte)1 : (byte)0;
            //payload[43] = tile.TileNW.HasUnit && tile.TileNW.CurrentUnit.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            //payload[44] = tile.TileNW.HasUnit ? (byte)tile.TileNW.CurrentUnit.Type : (byte)0;
            //payload[45] = tile.TileNW.HasUnit ? (byte)tile.TileNW.CurrentUnit.CombatPower : (byte)0;
            //payload[46] = tile.TileNW.HasUnit ? (byte)tile.TileNW.CurrentUnit.RangedPower : (byte)0;
            //payload[47] = tile.TileNW.HasCity ? (byte)1 : (byte)0;
            //payload[48] = tile.TileNW.HasCity && tile.TileNW.City.Owner != tile.CurrentUnit.Owner ? (byte)1 : (byte)0;
            //payload[49] = (tile.TileNW.Improvement == ImprovementType.Farms || tile.TileNW.Improvement == ImprovementType.Mines) ? (byte)1 : (byte)0;

            return payload;
        }

        public void AddLongTermObjective(int x, int y)
        {
            byte serverAction = (byte)ServerUpdate.AddObjective;
            byte xCoord = (byte)x;
            byte yCoord = (byte)y;

            byte[] payload = new byte[3];

            payload[0] = serverAction;
            payload[1] = xCoord;
            payload[2] = yCoord;

            XBeeRequest request = new ZNetTxRequest(new XBeeAddress64(BROADCAST_ADDRESS_64), new XBeeAddress16(BROADCAST_ADDRESS_16), payload);
            var res = _xbee.Execute(request);

            Thread.Sleep(FORCED_DELAY_SMALL);
        }

        public void RemoveLongTermObjective(int x, int y)
        {
            byte serverAction = (byte)ServerUpdate.RemoveObjective;
            byte xCoord = (byte)x;
            byte yCoord = (byte)y;

            byte[] payload = new byte[3];

            payload[0] = serverAction;
            payload[1] = xCoord;
            payload[2] = yCoord;

            XBeeRequest request = new ZNetTxRequest(new XBeeAddress64(BROADCAST_ADDRESS_64), new XBeeAddress16(BROADCAST_ADDRESS_16), payload);
            var res = _xbee.Execute(request);

            Thread.Sleep(FORCED_DELAY_SMALL);
        }

        public IDictionary<ulong, string> GetNetworkNodes()
        {
            Dictionary<ulong, string> nodes = new Dictionary<ulong, string>();

            foreach(var node in _nodes)
            {
                nodes.Add(node.Key, node.Value);
            }

            return nodes;
        }

        public void AssignUnitToNode(ulong nodeAddress, Unit unit, int x, int y)
        {
            byte serverAction = (byte)ServerUpdate.AssignUnit;
            byte unitType = (byte)unit.Type;
            byte xCoord = (byte)x;
            byte yCoord = (byte)y;

            byte[] payload = new byte[8];

            ushort nodeAddress16 = _nodes16[nodeAddress];

            payload[0] = serverAction;
            payload[1] = unitType;
            payload[2] = xCoord;
            payload[3] = yCoord;
            payload[4] = (byte)unit.CombatPower;
            payload[5] = (byte)unit.RangedPower;
            payload[6] = (byte)unit.AttackMethod;
            payload[7] = (byte)unit.HitPoints;

            XBeeRequest request = new ZNetTxRequest(new XBeeAddress64(nodeAddress), new XBeeAddress16(nodeAddress16), payload);            
            var res = _xbee.Execute(request);

            Thread.Sleep(FORCED_DELAY_SMALL);

            if (res.ApiID == XBeeApiType.ZNetTxStatus)
            {
                ZNetTxStatusResponse response = res as ZNetTxStatusResponse;
                if (response != null)
                {
                    if (response.DeliveryStatus != DeliveryStatusType.Success)
                    {
                        System.Diagnostics.Debug.Print("Delivery failure on unit assign. Retrying...");
                        res = _xbee.Execute(request);
                    }
                }
            }
        }

        public void KillUnit(ulong nodeAddress)
        {
            byte[] payload = new byte[1];
            payload[0] = (byte)ServerUpdate.NodeKilled;

            ushort nodeAddress16 = _nodes16[nodeAddress];

            XBeeRequest request = new ZNetTxRequest(new XBeeAddress64(nodeAddress), new XBeeAddress16(nodeAddress16), payload);
            var res = _xbee.Execute(request);

            Thread.Sleep(FORCED_DELAY_SMALL);
        }

        public NodeAction ReadNextAction()
        {
            return _actions.Dequeue();
        }

        public void BroadcastStartTurn(uint turn)
        {
            byte[] payload = new byte[2];
            payload[0] = (byte)ServerUpdate.TurnStart;
            payload[1] = (byte)turn;

            XBeeRequest request = new ZNetTxRequest(new XBeeAddress64(BROADCAST_ADDRESS_64 /* broadcast to all */), new XBeeAddress16(BROADCAST_ADDRESS_16), payload);
            var res = _xbee.Execute(request);

            Thread.Sleep(FORCED_DELAY_SMALL);
        }

        public void BroadcastEndTurn()
        {
            foreach (var node in _nodes)
            {
                byte[] payload = new byte[1];
                payload[0] = (byte)ServerUpdate.TurnOver;

                XBeeRequest request = new ZNetTxRequest(new XBeeAddress64(node.Key), new XBeeAddress16(_nodes16[node.Key]), payload);
                var res = _xbee.Execute(request);

                Thread.Sleep(FORCED_DELAY_SMALL);
            }
        }
    }
}
