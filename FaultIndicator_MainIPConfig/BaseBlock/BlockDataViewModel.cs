﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using FaultIndicator_MainIPConfig.SerialPMessages;
using FaultIndicator_MainIPConfig.SerialPortDevice;
using Microsoft.VisualBasic;
using TheRFramework.Utilities;

namespace FaultIndicator_MainIPConfig.BaseBlock
{
    public class BlockDataViewModel : BaseViewModel
    {
        private List<string> _request;

        private string[] _patterns =
{
            "31 00 00 04",        //SIM1
            "B1 00 00 04",        //SIM2
        };

        public List<string> Request
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
            List<string> msg;
            int sub = 0;
            if (string.IsNullOrEmpty(BlockResponse))
            {
                return;// "В ответ ничего не пришло";
            }

            msg = BlockResponse.Split(' ').ToList();


            if (BlockResponse.Contains(_patterns[0]))
            {
                if (msg.Count < 145)
                {
                    Messages.AddMessage("Ответ не полный");
                    return;
                }
                Messages.AddMessage("SIM1 ответ");
                Information.Add(new SIMDataModel { RemotePort = (Convert.ToInt16(msg[93], 16) + (Convert.ToInt16(msg[94], 16) << 8)) });
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
                if (msg.Count < 145)
                {
                    Messages.AddMessage("Ответ не полный");
                    return;
                }
                Messages.AddMessage("SIM2 ответ");
                Information.Add(new SIMDataModel { RemotePort = (Convert.ToInt16(msg[93], 16) + (Convert.ToInt16(msg[94], 16) << 8)) });
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
                    Request = new List<string>
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
                Request = new List<string>
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

            string[] saveString = new string [Information[0].APN.Length];

            int insertionIndex = -1;
            int lengthIndex = -1;

            new Thread(() =>
            {
                if (Sender.InitializationPackage)
                {
                    Request = new List<string>
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
                Request = new List<string>
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

                Thread.Sleep(4000);

                Request = new List<string>
                {
                "68",
                "8C",
                "EC",
                "00",
                "6E",
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
                "7B",
                "02",
                "10",
                "00",
                "00", //длина нулёвка по дефолту
                //"0E",  -- длина
                //"70",  -- начало APN
                //"75",
                //"62",
                //"6C",
                //"69",
                //"63",
                //"2E",
                //"70",
                //"72",
                //"6F",
                //"66",
                //"69",
                //"6C",
                //"65",  -- конец APN
                "03",
                "10",
                "00",
                "00", //длина нулёвка по дефолту
                //"04", -- длина
                //"63", -- начало username
                //"61",
                //"72",
                //"64", -- конец username
                "04",
                "10",
                "00",
                "00", //длина нулёвка по дефолту
                //"04", -- длина
                //"63", -- начало passsword
                //"61",
                //"72",
                //"64", -- конец password
                "08",
                "10",
                "00",
                "01",
                "00",
                "AD",
                "00",
                "00",
                "01",
                "01",
                "AE",
                "00",
                "00",
                "01",
                "01",
                "AF",
                "00",
                "00",
                "01",
                "00",
                "B0",
                "00",
                "00",
                "01",
                "00",
                "B1",
                "00",
                "00",
                "04",
                "00",
                "00",
                "00",
                "00",
                "B2",
                "00",
                "00",
                "01",
                "00",
                "81",
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
                "23",
                "82",
                "10",
                "00",
                "05",
                "63",
                "6D",
                "6E",
                "65",
                "74",
                "83",
                "10",
                "00",
                "04",
                "63",
                "61",
                "72",
                "64",
                "84",
                "10",
                "00",
                "04",
                "63",
                "61",
                "72",
                "64",
                "88",
                "10",
                "00",
                "01",
                "00",
                "A1",
                "FF",
                "00",
                "01",
                "00",
                "A2",
                "FF",
                "00",
                "00"
                };

                for (int i = 0; i < Request.Count - 1; ++i)
                {
                    if (Request[i] == "02" && Request[i + 1] == "10")
                    {
                        insertionIndex = i + 4; // Вставка после длины
                        lengthIndex = i + 3;
                        break;
                    }
                }

                if (insertionIndex != -1)
                {
                    for (int i = 0; i < Information[0].APN.Length; ++i)
                    {
                        saveString[i] = $"{Convert.ToInt16(Information[0].APN[i]):X2}";
                    }
                    Request[lengthIndex] = $"{Convert.ToInt16(saveString.Length):X2}";
                    Request.InsertRange(insertionIndex, saveString);
                }

                insertionIndex = -1;
                lengthIndex = -1;
                saveString = new string[Information[0].Username.Length];

                for (int i = 0; i < Request.Count - 1; ++i)
                {
                    if (Request[i] == "03" && Request[i + 1] == "10")
                    {
                        insertionIndex = i + 4; // Вставка после длины
                        lengthIndex = i + 3;
                        break;
                    }
                }

                if (insertionIndex != -1)
                {
                    for (int i = 0; i < Information[0].Username.Length; ++i)
                    {
                        saveString[i] = $"{Convert.ToInt16(Information[0].Username[i]):X2}";
                    }
                    Request[lengthIndex] = $"{Convert.ToInt16(saveString.Length):X2}";
                    Request.InsertRange(insertionIndex, saveString);
                }

                insertionIndex = -1;
                lengthIndex = -1;
                saveString = new string[Information[0].Password.Length];

                for (int i = 0; i < Request.Count - 1; ++i)
                {
                    if (Request[i] == "04" && Request[i + 1] == "10")
                    {
                        insertionIndex = i + 4; // Вставка после длины
                        lengthIndex = i + 3;
                        break;
                    }
                }

                if (insertionIndex != -1)
                {
                    for (int i = 0; i < Information[0].Password.Length; ++i)
                    {
                        saveString[i] = $"{Convert.ToInt16(Information[0].Password[i]):X2}";
                    }
                    Request[lengthIndex] = $"{Convert.ToInt16(saveString.Length):X2}";
                    Request.InsertRange(insertionIndex, saveString);
                }

                Request[1] = $"{(Request.Count - 2):X2}";
                Request[18] = $"{(Request.Count - 19):X2}";


                Sender.SendHEXMessage(string.Join(" ", Request));
                Messages.AddSentMessage(string.Join(" ", Request));
                Thread.Sleep(1000);

            }).Start();
        }
    }
}
