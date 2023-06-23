using System;
using System.Text.RegularExpressions;
using FaultIndicator_MainIPConfig.Interfaces;
using TheRFramework.Utilities;

namespace FaultIndicator_MainIPConfig.SerialPMessages
{
    public class SerialPortMessagesViewModel : BaseViewModel
    {
        private string _messagesText;
        private string _toBeSentText;
        private bool _isHEX;
        private bool _inLog;
        private bool _isRepeat; //тест репетативной отправки
        private bool _addNewLine;

        public string MessagesText
        {
            get => _messagesText;
            set => RaisePropertyChanged(ref _messagesText, value);
        }

        public string ToBeSentText
        {
            get => _toBeSentText;
            set => RaisePropertyChanged(ref _toBeSentText, value);
        }

        public bool IsHEX
        {
            get => _isHEX;
            set => RaisePropertyChanged(ref _isHEX, value);
        }

        public bool InLog
        {
            get => _inLog;
            set => RaisePropertyChanged(ref _inLog, value);
        }

        public bool AddNewLine
        {
            get => _addNewLine;
            set => RaisePropertyChanged(ref _addNewLine, value);
        }

        public bool Repeat
        {
            get => _isRepeat;
            set => RaisePropertyChanged(ref _isRepeat, value);
        }

        public Command ClearMessagesCommand { get; }
        public Command SendMessageCommand { get; }
        public Command SendIndicatorMessageCommand { get; }

        public IMessageBoxes MessageBoxes { get; internal set; }

        public SerialPortMessagesSend Sender { get; set; }


        public SerialPortMessagesViewModel()
        {
            //Indicators = new ObservableCollection<FaultIndicatorViewModel>();
            MessagesText = "";
            ToBeSentText = "";
            IsHEX = true;
            AddNewLine = false;

            ClearMessagesCommand = new Command(ClearMessages);
            SendMessageCommand = new Command(SendMessage);
        }

        private void SendMessage()
        {
            if (!Sender.Port.IsOpen)
            {
                AddMessage("Порт не открыт, не удалось отправить сообщение");
                return;
            }
            if (!string.IsNullOrEmpty(ToBeSentText))
            {
                try
                {
                    if (IsHEX)
                    {
                        string HEXpattern = @"^[0-9A-Fa-f]{2}( [0-9A-Fa-f]{2})*$";
                        if (!Regex.IsMatch(ToBeSentText, HEXpattern))
                        {
                            AddMessage("Сообщение не соотвествует формату\r\nФормат: 6A 80 BC 73 70 E2");
                        }
                        else
                        {
                            Sender.SendHEXMessage(ToBeSentText, AddNewLine);
                            AddSentMessage(ToBeSentText);
                        }
                    }
                    else
                    {
                        Sender.SendMessage(ToBeSentText, AddNewLine);
                        AddSentMessage(ToBeSentText);
                    }
                    ToBeSentText = "";
                }
                catch (TimeoutException timeout)
                {
                    AddMessage("Время ожидания отправки истекло. Не удалось отправить сообщение");
                }
                catch (Exception e)
                {
                    AddMessage("Ошибка: " + e.ToString());
                }

            }
        }

        private void ClearMessages()
        {
            MessagesText = "";
            //MessagesCount = 0;
        }

        public void AddSentMessage(string message)
        {
            // (Date) | TX> hello there
            // byte[] bytes = Encoding.UTF8.GetBytes(message);
            // AddMessage($"{DateTime.Now} | TX> {bytes}");
            AddMessage($"{DateTime.Now} | TX> {message}");
            if (InLog)
            {

            }
        }

        public void AddReceivedMessage(string message)
        {
            // (Date) | RX> hello there
            AddMessage($"{DateTime.Now} | RX> {message}");
        }

        public void AddMessage(string message)
        {
            WriteLine(message);
        }

        public void WriteLine(string text)
        {
            MessagesText += text + '\n';
            MessageBoxes.ScrollToBottom();
        }
    }
}
