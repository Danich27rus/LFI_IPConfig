using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheRFramework.Utilities;

namespace FaultIndicator_MainIPConfig.BaseBlock
{
    public class SIMDataModel : BaseViewModel
    {
        private string _remoteIPAddress;
        private int _remotePort;
        private string _APN;
        private int _CSQ;
        private string _acquiredIPAddress;
        private string _username;
        private string _password;

        public string RemoteIPAdress
        {
            get => _remoteIPAddress;
            set => RaisePropertyChanged(ref _remoteIPAddress, value);
        }

        public int RemotePort
        {
            get => _remotePort;
            set => RaisePropertyChanged(ref _remotePort, value);
        }

        public string APN
        {
            get => _APN; 
            set => RaisePropertyChanged(ref _APN, value);
        }

        public int CSQ
        {
            get => _CSQ; 
            set => RaisePropertyChanged(ref _CSQ, value);
        }

        public string AcquiredIPAddress
        {
            get => _acquiredIPAddress; 
            set => RaisePropertyChanged(ref _acquiredIPAddress, value);
        }

        public string Username
        {
            get => _username; 
            set => RaisePropertyChanged(ref _username, value);
        }

        public string Password
        {
            get => _password; 
            set => RaisePropertyChanged(ref _password, value);
        }
    }
}
