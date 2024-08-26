using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCiv.Engine
{
    public sealed class NodeMessageViewModel : ObservableObject
    {
        private NodeActionType _type = NodeActionType.None;
        private string _message = string.Empty;
        private ulong _addr16 = 0;
        private ulong _addr64 = 0;
        private string _nodeId = string.Empty;
        private bool _serverAck = false;

        public bool ServerAck
        {
            get
            {
                return _serverAck;
            }
            set
            {
                _serverAck = value;
                RaisePropertyChanged(nameof(ServerAck));
            }
        }

        public string NodeId
        {
            get
            {
                return _nodeId;
            }
            set
            {
                _nodeId = value;
                RaisePropertyChanged(nameof(NodeId));
            }
        }

        public NodeActionType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                RaisePropertyChanged(nameof(Type));
            }
        }
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                RaisePropertyChanged(nameof(Message));
            }
        }

        public ulong Addr16
        {
            get
            {
                return _addr16;
            }
            set
            {
                _addr16 = value;
                RaisePropertyChanged(nameof(Addr16));
            }
        }

        public ulong Addr64
        {
            get
            {
                return _addr64;
            }
            set
            {
                _addr64 = value;
                RaisePropertyChanged(nameof(Addr64));
            }
        }
    }
}
