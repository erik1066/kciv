using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenCiv.Engine
{
    public sealed class PopupMessageViewModel : ObservableObject
    {
        private bool _isShowing = false;
        private string _message = string.Empty;
        private string _title = string.Empty;

        public bool IsShowing
        {
            get { return _isShowing; }
            set { _isShowing = value; RaisePropertyChanged(nameof(IsShowing)); }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(nameof(Message)); }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(nameof(Title)); }
        }

        #region Commands
        public ICommand CloseCommand { get { return new RelayCommand(Close, () => true); } }
        private void Close()
        {
            IsShowing = false;
            Message = string.Empty;
            Title = string.Empty;
        }
        #endregion // Commands
    }
}
