using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FaultIndicator_MainIPConfig.SerialPMessages;
using FaultIndicator_MainIPConfig.SerialPortDevice;
using Microsoft.VisualBasic;
using TheRFramework.Utilities;

namespace FaultIndicator_MainIPConfig.BaseBlock
{
    public class BlockDataViewModel : BaseViewModel
    {
        private string[] _request;

        private string[] _patterns =
{
            "68 AC",        //SIM1
            "68 A1",        //SIM2
        };

        public string[] Request
        {
            get => _request;
            set => RaisePropertyChanged(ref _request, value);
        }
        public SerialPortMessagesViewModel Messages { get; set; }

        public SerialPortMessagesReceive Receiver { get; set; }

        public SerialPortMessagesSend Sender { get; set; }

        public SerialPortViewModel SerialPort { get; set; }

        public ObservableCollection<SIMDataModel> Information { get; set; }

        public Command ReadParamsCommand { get; set; }

        public Command WriteParamsCommand { get; set; }

        public string BlockResponse { get; set; }

        public BlockDataViewModel()
        {
            Messages = new SerialPortMessagesViewModel();
            Receiver = new SerialPortMessagesReceive();
            Sender = new SerialPortMessagesSend();
            BlockResponse = "";
            //checksum = new Checksum();

            Information = new ObservableCollection<SIMDataModel>
            {
                //new FaultIndicatorViewModel() { MACAdress="68-04-00-DA" },
                //new FaultIndicatorViewModel() { MACAdress="68-04-00-DB" },
                //new FaultIndicatorViewModel() { MACAdress="68-04-00-DC" }
            };
            ReadParamsCommand = new Command(ReadParams);
            WriteParamsCommand = new Command(WriteParams);
        }

        public void ParseCommand()
        {
            string[] msg;
            int sub = 0;
            if (string.IsNullOrEmpty(BlockResponse))
            {
                return;// "В ответ ничего не пришло";
            }

            msg = BlockResponse.Split(' ');


            if (BlockResponse.Contains(_patterns[0]))
            {
                if (msg.Length < 145)
                {
                    Messages.AddMessage("Ответ не полный");
                    return;
                }
                Messages.AddMessage("SIM1 ответ");
                Information.Add(new SIMDataModel { RemotePort = (Convert.ToInt16(msg[93], 16) + (Convert.ToInt16(msg[94], 16) << 8))});
                Information[0].RemoteIPAdress = "";
                for (int i = 39; i < 70; ++i)
                {
                    if (msg[i] == "00")
                    {
                        break;
                    }
                    Information[0].RemoteIPAdress += Convert.ToChar(Convert.ToByte(msg[i], 16));
                }
                Information[0].CSQ = Convert.ToInt16(msg[109], 16);
                Information[0].APN = "";
                for (int i = 145; i < 160; ++i)
                {
                    if (msg[i] == "00")
                    {
                        sub = i;
                        break;
                    }
                    Information[0].APN += Convert.ToChar(Convert.ToByte(msg[i], 16));
                }
                Information[0].Username = "";
                for (int i = sub + 5; i < sub + 15; ++i)
                {
                    if (msg[i] == "00")
                    {
                        sub = i;
                        break;
                    }
                    Information[0].Username += Convert.ToChar(Convert.ToByte(msg[i], 16));
                }
                Information[0].Password = "";
                for (int i = sub + 5; i < sub + 15; ++i)
                {
                    if (msg[i] == "00")
                    {
                        sub = i;
                        break;
                    }
                    Information[0].Password += Convert.ToChar(Convert.ToByte(msg[i], 16));
                }
                return;
            }
            if (BlockResponse.Contains(_patterns[1]))
            {
                if (msg.Length < 145)
                {
                    Messages.AddMessage("Ответ не полный");
                    return;
                }
                Messages.AddMessage("SIM2 ответ");
                Information.Add(new SIMDataModel { RemotePort = (Convert.ToInt16(msg[93], 16) + (Convert.ToInt16(msg[94], 16) << 8))});
                Information[1].RemoteIPAdress = "";
                for (int i = 39; i < 52; ++i)
                {
                    Information[1].RemoteIPAdress += Convert.ToChar(Convert.ToByte(msg[i], 16));
                }
                Information[1].CSQ = Convert.ToInt16(msg[109], 16);
                Information[1].APN = "";
                for (int i = 145; i < 160; ++i)
                {
                    if (msg[i] == "00")
                    {
                        sub = i;
                        break;
                    }
                    Information[1].APN += Convert.ToChar(Convert.ToByte(msg[i], 16));
                }
                Information[1].Username = "";
                for (int i = sub + 4; i < sub + 9; ++i)
                {
                    if (msg[i] == "00")
                    {
                        sub = i;
                        break;
                    }
                    Information[1].Username += Convert.ToChar(Convert.ToByte(msg[i], 16));
                }
                Information[1].Password = "";
                for (int i = sub + 4; i < sub + 9; ++i)
                {
                    if (msg[i] == "00")
                    {
                        sub = i;
                        break;
                    }
                    Information[1].Password += Convert.ToChar(Convert.ToByte(msg[i], 16));
                }
                return;
            }

            //return "default";
        }

        public void ReadParams()
        {
            if (!Sender.Port.IsOpen)
            {
                Messages.AddMessage("Порт не открыт, не удалось отправить сообщение");
                return;
            }
            /*if (Sender.InitializationPackage)
            {
                new Thread(() =>
                {
                    Request = new string[]
                    {
                    "68",
                    "04",
                    "07",
                    "00",
                    "00",
                    "00"
                    };

                    Sender.SendHEXMessage(string.Join(" ", Request));
                    Messages.AddSentMessage(string.Join(" ", Request));

                    Thread.Sleep(500);

                }).Start();

                Sender.InitializationPackage = false;
            }*/
            new Thread(() =>
            {
                if (Sender.InitializationPackage)
                {
                    Request = new string[]
                        {
                    "68",
                    "04",
                    "07",
                    "00",
                    "00",
                    "00"
                        };

                    Sender.SendHEXMessage(string.Join(" ", Request));
                    Messages.AddSentMessage(string.Join(" ", Request));
                    Sender.InitializationPackage = false;

                    Thread.Sleep(500);
                }
                Request = new string[]
                {
                    "68",
                    "11",
                    "06",
                    "00",
                    "1C",
                    "00",
                    "7A",
                    "01",
                    "0D",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "01",
                    "00",
                    "05",
                    "00"
                };

                Sender.SendHEXMessage(string.Join(" ", Request));
                Messages.AddSentMessage(string.Join(" ", Request));

                Thread.Sleep(500);
            }).Start();
        }

        public void WriteParams()
        {
            if (!Sender.Port.IsOpen)
            {
                Messages.AddMessage("Порт не открыт, не удалось отправить сообщение");
                return;
            }
            new Thread(() =>
            {
                if (Sender.InitializationPackage)
                {
                    Request = new string[]
                        {
                    "68",
                    "04",
                    "07",
                    "00",
                    "00",
                    "00"
                        };

                    Sender.SendHEXMessage(string.Join(" ", Request));
                    Messages.AddSentMessage(string.Join(" ", Request));
                    Sender.InitializationPackage = false;

                    Thread.Sleep(500);
                }
                Request = new string[]
                {
                    "68",
                    "CB",
                    "C2", 
                    "00",
                    "C6", 
                    "00",
                    "7D",
                    "01",
                    "0D",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "01",
                    "00",
                    "05",
                    "BA",
                    "A9",
                    "00",
                    "00",
                    "04",
                    "08",
                    "08",
                    "08",
                    "08",
                    "AA",
                    "00",
                    "00",
                    "04",
                    "72",
                    "72",
                    "72",
                    "72",
                    "2B",
                    "00",
                    "00",
                    "32",
                    "00", //39 - айпи
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00", 
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "2C",
                    "00",
                    "00",
                    "02",
                    "64", //93 - порт
                    "09",
                    "AB",
                    "00",
                    "00",
                    "32",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "00",
                    "AC",
                    "00",
                    "00",
                    "02",
                    "00",
                    "00",
                    "B3",
                    "00",
                    "00",
                    "01",
                    "00",
                    "2D",
                    "00",
                    "00",
                    "01",
                    "00",
                    "2E",
                    "00",
                    "00",
                    "01",
                    "00",
                    "2F",
                    "00",
                    "00",
                    "01",
                    "0E",
                    "30",
                    "00",
                    "00",
                    "01",
                    "00",
                    "31",
                    "00",
                    "00",
                    "04",
                    "0A",
                    "35",
                    "A2",
                    "43",
                    "32",
                    "00",
                    "00",
                    "01",
                    "00",
                    "01",
                    "10",
                    "00",
                    "08",
                    "2A",
                    "39",
                    "39",
                    "2A",
                    "2A",
                    "2A",
                    "31",
                    "23"
                };

                int k = 39;         // старт с ip
                int b = 0;

                for (int i = 0; i < Information[0].RemoteIPAdress.Length; ++i) 
                {
                    Request[k] = $"{Convert.ToByte(Information[0].RemoteIPAdress[i]):X2}";
                    ++k;
                }
                b = k;
                //k += Information[0].RemoteIPAdress.Length;
                for (int i = k; k < b + 20; ++k)
                {
                    Request[k] = $"00";
                }

                Request[93] = $"{Information[0].RemotePort & 255:X2}";
                Request[94] = $"{(Information[0].RemotePort >> 8) & 255:X2}";

                Sender.SendHEXMessage(string.Join(" ", Request));
                Messages.AddSentMessage(string.Join(" ", Request));

                Thread.Sleep(500);
            }).Start();
        }
    }
}
