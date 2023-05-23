using IpAddressControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IpAddressControlDemo
{
    public class MainWindowViewModel : BaseViewModel
    {
        private string heading;

        public string Heading
        {
            get => heading; 
            set => OnPropertyChanged(ref heading, value);
        }

        private IpAddressViewModel ipCtrlVm;

        public IpAddressViewModel IpAddressControlVM
        {
            get => ipCtrlVm; 
            set => OnPropertyChanged(ref ipCtrlVm, value);
        }

        public MainWindowViewModel()
        {
            IpAddressControlVM = new IpAddressViewModel();
            Heading = "Use invalid characters for validation." + Environment.NewLine 
                + "Use . to move to next box automatically." + Environment.NewLine 
                + "Use out of range values.";
        }
    }
}
