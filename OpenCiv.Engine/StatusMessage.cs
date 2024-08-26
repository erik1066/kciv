using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCiv.Engine
{
    public sealed class StatusMessage : ObservableObject
    {
        private StatusMessageType _type = StatusMessageType.Generic;
        private string _message = string.Empty;

        public StatusMessageType Type
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
    }
}
