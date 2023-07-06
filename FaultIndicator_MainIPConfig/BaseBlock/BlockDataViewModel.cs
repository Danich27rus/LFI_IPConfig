using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Interop;
using FaultIndicator_MainIPConfig.SerialPMessages;
using FaultIndicator_MainIPConfig.SerialPortDevice;
using TheRFramework.Utilities;

namespace FaultIndicator_MainIPConfig.BaseBlock
{
    public class BlockDataViewModel : BaseViewModel
    {
        private List<string> _request;

        private bool _sim1Added, _sim2Added;

        private string[] _patterns =
{
            "31 00 00 04",              //SIM1
            "B1 00 00 04",              //SIM2
            "68 04 0B",                 //Подтверждение инциализации
            "68 04 01",                 //Подтверждение чтения/записи
            "6C 6F 67 6F 66 66 0D 0A"   //logoff
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

        public Command ExitDebugCommand { get; set; }

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
            ExitDebugCommand = new Command(ExitDebug);
        }

        public void ParseCommand()
        {
            //System.ArgumentOutOfRangeException

            List<string> msg;
            int sub = 0, checkIndex = -1, size = -1;

            if (string.IsNullOrEmpty(BlockResponse))
            {
                Messages.AddMessage("В ответ ничего не пришло");
                return;// "В ответ ничего не пришло";
            }

            msg = BlockResponse.Split(' ').ToList();

            #region Парсинг по сообщению от SIM1

            try
            {
                if (BlockResponse.Contains(_patterns[0]))
                {
                    if (msg.Count < 145)
                    {
                        Messages.AddReceivedMessage("Ответ не полный");
                        return;
                    }
                    Messages.AddReceivedMessage($"SIM1 ответ: {BlockResponse}");
                    if (!_sim1Added)
                    {
                        for (int i = 0; i < msg.Count - 1; ++i)
                        {
                            if (msg[i] == "2C" && msg[i + 1] == "00" && msg[i + 2] == "00")
                            {
                                checkIndex = i + 4;
                                break;
                            }
                        }
                        Information.Add(new SIMDataModel
                        {
                            RemotePort = (Convert.ToInt16(msg[checkIndex], 16) + (Convert.ToInt16(msg[checkIndex + 1], 16) << 8))
                        });
                        _sim1Added = true;
                    }
                    else
                    {
                        for (int i = 0; i < msg.Count - 1; ++i)
                        {
                            if (msg[i] == "2C" && msg[i + 1] == "00" && msg[i + 2] == "00")
                            {
                                checkIndex = i + 4;
                                break;
                            }
                        }
                        Information[0].RemotePort = (Convert.ToInt16(msg[checkIndex], 16) + (Convert.ToInt16(msg[checkIndex + 1], 16) << 8));
                    }

                    checkIndex = -1;

                    //Поиск в массиве удалённого IP
                    Information[0].RemoteIPAdress = "";
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "2B" && msg[i + 1] == "00" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }

                    if (checkIndex != -1)
                    {
                        for (int i = checkIndex; i < (size + checkIndex); ++i)
                        {
                            if (msg[i] == "00")
                            {
                                break;
                            }
                            Information[0].RemoteIPAdress += Convert.ToChar(Convert.ToByte(msg[i], 16));
                        }
                    }

                    size = -1;
                    checkIndex = -1;

                    //Поиск в массиве CSQ
                    Information[0].CSQ = 0;
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "2F" && msg[i + 1] == "00" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            //size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }
                    if (checkIndex != -1)
                    {
                        Information[0].CSQ = Convert.ToInt16(msg[checkIndex], 16);
                    }

                    checkIndex = -1;

                    //Поиск в массиве APN
                    Information[0].APN = "";
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "02" && msg[i + 1] == "10" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }
                    if (checkIndex != -1)
                    {
                        for (int i = checkIndex; i < (size + checkIndex); ++i)
                        {
                            if (msg[i] == "00")
                            {
                                break;
                            }
                            Information[0].APN += Convert.ToChar(Convert.ToByte(msg[i], 16));
                        }
                    }

                    size = -1;
                    checkIndex = -1;

                    //Поиск в массиве IP от SIM
                    Information[0].AcquiredIPAddress = "";
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "31" && msg[i + 1] == "00" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }
                    if (checkIndex != -1)
                    {
                        for (int i = checkIndex; i < (size + checkIndex); ++i)
                        {
                            Information[0].AcquiredIPAddress += Convert.ToByte(msg[i], 16).ToString();
                            if (i < (size + checkIndex) - 1)
                            {
                                Information[0].AcquiredIPAddress += '.';
                            }
                        }
                    }

                    size = -1;
                    checkIndex = -1;

                    //Поиск в массиве Username
                    Information[0].Username = "";
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "03" && msg[i + 1] == "10" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }
                    if (checkIndex != -1)
                    {
                        for (int i = checkIndex; i < (size + checkIndex); ++i)
                        {
                            if (msg[i] == "00")
                            {
                                break;
                            }
                            Information[0].Username += Convert.ToChar(Convert.ToByte(msg[i], 16));
                        }
                    }

                    size = -1;
                    checkIndex = -1;

                    //Поиск в массиве Password
                    Information[0].Password = "";
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "04" && msg[i + 1] == "10" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }
                    if (checkIndex != -1)
                    {
                        for (int i = checkIndex; i < (size + checkIndex); ++i)
                        {
                            if (msg[i] == "00")
                            {
                                break;
                            }
                            Information[0].Password += Convert.ToChar(Convert.ToByte(msg[i], 16));
                        }
                    }
                    return;
                }
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Messages.AddReceivedMessage("Считывание данных произошло неккоректно, повторите попытку");
            }

            #endregion

            #region Парсинг по сообщению от SIM2

            try
            {
                if (BlockResponse.Contains(_patterns[1]))
                {
                    if (msg.Count < 145)
                    {
                        Messages.AddReceivedMessage("Ответ не полный");
                        return;
                    }
                    Messages.AddReceivedMessage($"SIM2 ответ: {BlockResponse}");
                    if (!_sim2Added)
                    {
                        for (int i = 0; i < msg.Count - 1; ++i)
                        {
                            if (msg[i] == "AC" && msg[i + 1] == "00" && msg[i + 2] == "00")
                            {
                                checkIndex = i + 4;
                                break;
                            }
                        }
                        Information.Add(new SIMDataModel
                        {
                            RemotePort = (Convert.ToInt16(msg[checkIndex], 16) + (Convert.ToInt16(msg[checkIndex + 1], 16) << 8))
                        });
                        _sim2Added = true;
                    }
                    else
                    {
                        for (int i = 0; i < msg.Count - 1; ++i)
                        {
                            if (msg[i] == "AC" && msg[i + 1] == "00" && msg[i + 2] == "00")
                            {
                                checkIndex = i + 4;
                                break;
                            }
                        }
                        Information[1].RemotePort = (Convert.ToInt16(msg[checkIndex], 16) + (Convert.ToInt16(msg[checkIndex + 1], 16) << 8));
                    }

                    checkIndex = -1;

                    //Поиск в массиве удалённого IP
                    Information[1].RemoteIPAdress = "";
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "AB" && msg[i + 1] == "00" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }

                    if (checkIndex != -1)
                    {
                        for (int i = checkIndex; i < (size + checkIndex); ++i)
                        {
                            if (msg[i] == "00")
                            {
                                break;
                            }
                            Information[1].RemoteIPAdress += Convert.ToChar(Convert.ToByte(msg[i], 16));
                        }
                    }

                    size = -1;
                    checkIndex = -1;

                    //Поиск в массиве CSQ
                    Information[1].CSQ = 0;
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "AF" && msg[i + 1] == "00" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            //size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }
                    if (checkIndex != -1)
                    {
                        Information[1].CSQ = Convert.ToInt16(msg[checkIndex], 16);
                    }

                    checkIndex = -1;

                    //Поиск в массиве APN
                    Information[1].APN = "";
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "82" && msg[i + 1] == "10" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }
                    if (checkIndex != -1)
                    {
                        for (int i = checkIndex; i < (size + checkIndex); ++i)
                        {
                            if (msg[i] == "00")
                            {
                                break;
                            }
                            Information[1].APN += Convert.ToChar(Convert.ToByte(msg[i], 16));
                        }
                    }

                    size = -1;
                    checkIndex = -1;

                    //Поиск в массиве IP от SIM
                    Information[1].AcquiredIPAddress = "";
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "B1" && msg[i + 1] == "00" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }
                    if (checkIndex != -1)
                    {
                        for (int i = checkIndex; i < (size + checkIndex); ++i)
                        {
                            Information[1].AcquiredIPAddress += Convert.ToByte(msg[i], 16).ToString();
                            if (i < (size + checkIndex) - 1)
                            {
                                Information[1].AcquiredIPAddress += '.';
                            }
                        }
                    }

                    size = -1;
                    checkIndex = -1;

                    //Поиск в массиве Username
                    Information[1].Username = "";
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "83" && msg[i + 1] == "10" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }
                    if (checkIndex != -1)
                    {
                        for (int i = checkIndex; i < (size + checkIndex); ++i)
                        {
                            if (msg[i] == "00")
                            {
                                break;
                            }
                            Information[1].Username += Convert.ToChar(Convert.ToByte(msg[i], 16));
                        }
                    }

                    size = -1;
                    checkIndex = -1;

                    //Поиск в массиве Password
                    Information[1].Password = "";
                    for (int i = 0; i < msg.Count - 1; ++i)
                    {
                        if (msg[i] == "84" && msg[i + 1] == "10" && msg[i + 2] == "00")
                        {
                            checkIndex = i + 4;
                            size = Convert.ToInt16(msg[i + 3], 16);
                            break;
                        }
                    }
                    if (checkIndex != -1)
                    {
                        for (int i = checkIndex; i < (size + checkIndex); ++i)
                        {
                            if (msg[i] == "00")
                            {
                                break;
                            }
                            Information[1].Password += Convert.ToChar(Convert.ToByte(msg[i], 16));
                        }
                    }
                    return;
                }
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Messages.AddMessage("Считывание данных произошло неккоректно, повторите попытку");
            }
            #endregion

            if (BlockResponse.Contains(_patterns[2]))
            {
                Messages.AddReceivedMessage($"Подтверждение инициализации: {BlockResponse}");
            }

            if (BlockResponse.Contains(_patterns[3]))
            {
                Messages.AddReceivedMessage($"Подтверждение чтения/записи: {BlockResponse}");
            }

            if (BlockResponse.Contains(_patterns[4]))
            {
                Messages.AddReceivedMessage($"Базовый блок получил запрос на выход из дебаг режима: {BlockResponse}");
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
                    Messages.AddSentMessage("Запрос на инициализацию: " + string.Join(" ", Request));
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
                Messages.AddSentMessage("Запрос на чтение данных: " + string.Join(" ", Request));

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

            string[] saveString = new string[Information[0].APN.Length];

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
                    "00",
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
                for (; k < b + 20; ++k)
                {
                    Request[k] = $"00";
                }

                for (int i = 0; i < Request.Count - 1; ++i)
                {
                    if (Request[i] == "2C" && Request[i + 1] == "00" && Request[i + 2] == "00")
                    {
                        insertionIndex = i + 4;
                        break;
                    }
                }

                if (insertionIndex != -1)
                {
                    Request[insertionIndex] = $"{Information[0].RemotePort & 255:X2}";
                    Request[insertionIndex + 1] = $"{(Information[0].RemotePort >> 8) & 255:X2}";
                }

                insertionIndex = -1;

                for (int i = 0; i < Request.Count - 1; ++i)
                {
                    if (Request[i] == "AB" && Request[i + 1] == "00" && Request[i + 2] == "00")
                    {
                        insertionIndex = i + 4;
                        break;
                    }
                }

                if (insertionIndex != -1)
                {
                    for (int i = 0; i < Information[1].RemoteIPAdress.Length; ++i)
                    {
                        Request[insertionIndex] = $"{Convert.ToByte(Information[1].RemoteIPAdress[i]):X2}";
                        ++insertionIndex;
                    }
                }

               //b = insertionIndex;

                for (int i = insertionIndex; i < insertionIndex + 20; ++i)
                {
                    Request[i] = $"00";
                }

                insertionIndex = -1;
                //Request[93] = $"{Information[0].RemotePort & 255:X2}";
                //Request[94] = $"{(Information[0].RemotePort >> 8) & 255:X2}";

                Request[1] = $"{(Request.Count - 2):X2}";
                Request[18] = $"{(Request.Count - 19):X2}";

                Sender.SendHEXMessage(string.Join(" ", Request));
                Messages.AddSentMessage("Запись первой половины данных: " + string.Join(" ", Request));

                Thread.Sleep(4000);

                Request = new List<string>
                {
                "68",
                "00", //длина сообщения с контрольными полями
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
                "7B", //длина сообщения без полей
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
                "00", //длина APN, нулёвка по дефолту
                //"63", //начало при 5
                //"6D",
                //"6E",
                //"65",
                //"74", //конец при 5
                "83",
                "10",
                "00",
                "00", //длина username, нулёвка по дфеолту
                //"63", //начало при 4
                //"61",
                //"72",
                //"64", //конец при 4
                "84",
                "10",
                "00",
                "00", //длина password, нулёвка по дфеолту
                //"63",
                //"61",
                //"72",
                //"64",
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

                #region Сохранение данных для SIM1

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

                #endregion

                #region Сохранение данных для SIM2

                insertionIndex = -1;
                lengthIndex = -1;
                saveString = new string[Information[1].APN.Length];

                for (int i = 0; i < Request.Count - 1; ++i)
                {
                    if (Request[i] == "82" && Request[i + 1] == "10")
                    {
                        insertionIndex = i + 4; // Вставка после длины
                        lengthIndex = i + 3;
                        break;
                    }
                }

                if (insertionIndex != -1)
                {
                    for (int i = 0; i < Information[1].APN.Length; ++i)
                    {
                        saveString[i] = $"{Convert.ToInt16(Information[1].APN[i]):X2}";
                    }
                    Request[lengthIndex] = $"{Convert.ToInt16(saveString.Length):X2}";
                    Request.InsertRange(insertionIndex, saveString);
                }

                insertionIndex = -1;
                lengthIndex = -1;
                saveString = new string[Information[1].Username.Length];

                for (int i = 0; i < Request.Count - 1; ++i)
                {
                    if (Request[i] == "83" && Request[i + 1] == "10")
                    {
                        insertionIndex = i + 4; // Вставка после длины
                        lengthIndex = i + 3;
                        break;
                    }
                }

                if (insertionIndex != -1)
                {
                    for (int i = 0; i < Information[1].Username.Length; ++i)
                    {
                        saveString[i] = $"{Convert.ToInt16(Information[1].Username[i]):X2}";
                    }
                    Request[lengthIndex] = $"{Convert.ToInt16(saveString.Length):X2}";
                    Request.InsertRange(insertionIndex, saveString);
                }

                insertionIndex = -1;
                lengthIndex = -1;
                saveString = new string[Information[1].Password.Length];

                for (int i = 0; i < Request.Count - 1; ++i)
                {
                    if (Request[i] == "84" && Request[i + 1] == "10")
                    {
                        insertionIndex = i + 4; // Вставка после длины
                        lengthIndex = i + 3;
                        break;
                    }
                }

                if (insertionIndex != -1)
                {
                    for (int i = 0; i < Information[1].Password.Length; ++i)
                    {
                        saveString[i] = $"{Convert.ToInt16(Information[1].Password[i]):X2}";
                    }
                    Request[lengthIndex] = $"{Convert.ToInt16(saveString.Length):X2}";
                    Request.InsertRange(insertionIndex, saveString);
                }

                Request[1] = $"{(Request.Count - 2):X2}";
                Request[18] = $"{(Request.Count - 19):X2}";


                Sender.SendHEXMessage(string.Join(" ", Request));
                Messages.AddSentMessage("Запись второй половины данных: " + string.Join(" ", Request));
                Thread.Sleep(1000);

                #endregion

            }).Start();
        }

        public void ExitDebug()
        {
            new Thread(() =>
            {
                Request = new List<string>
                {
                    "61",
                    "74",
                    "2B",
                    "6C",
                    "6F",
                    "67",
                    "6F",
                    "66",
                    "66",
                    "0D",
                    "0A",
                    "0D",
                    "0A",
                };

                Sender.SendHEXMessage(string.Join(" ", Request));
                Messages.AddSentMessage("Запрос на выход из дебаг режима: " + string.Join(" ", Request));

                Thread.Sleep(1000);

            }).Start();
        }
    }
}


// 61 74 2B 6C 6F 67 6F 66 66 0D 0A 0D 0A debug close