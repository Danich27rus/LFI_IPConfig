using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;
using FaultIndicator_MainIPConfig.Utilities;
using FaultIndicator_MainIPConfig.BaseBlock;
//using test_app2.FaultIndicators;

namespace FaultIndicator_MainIPConfig.SerialPMessages
{
    /// <summary>
    /// Used for receiving messages from serial port
    /// </summary>
    public class SerialPortMessagesReceive
    {
        public static CancellationTokenSource cancelSource = new CancellationTokenSource();
        //private HEXConverter hexConverter { get; set; }
        public bool CanReceive { get; set; }
        public bool ShouldShutDownPermanently { get; set; }
        public bool IsHEX { get; set; }
        public string IndicatorConfirm { get; set; }
        //public SerialPortMessagesViewModel SerialPortModel { get; set; }
        public SerialPort Port { get; set; }
        public SerialPortMessagesViewModel Messages { get; set; }
        public BlockDataViewModel BaseBlock { get; set; }
        //public IndicatorDataViewModel IndicatorData { get; set; }
        public SerialPortMessagesReceive() 
        {
            IndicatorConfirm = "";

            //hexConverter = new HEXConverter();
            CanReceive = true; 
            ShouldShutDownPermanently = false;

            new Thread(() =>
            {
                try
                {
                    ReceiveLoop(cancelSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Messages.AddReceivedMessage("Галя, отмена по токену!");
                }
            }).Start();
            //ReceiverThread.Start();
        }

        private void ReceiveLoop(CancellationToken cancelToken)
        {
            string message = "";
            //string byteMessage = "";
            char readChar;
            int readByte;

            while (true)
            {
                if (cancelToken.IsCancellationRequested) {
                    ShouldShutDownPermanently = true;
                }
                if (ShouldShutDownPermanently)
                {
                    return;
                }
                if (CanReceive)
                {
                    if (Port != null && Port.IsOpen)
                    {
                        while(Port.BytesToRead > 0)
                        {
                            //if (Messages.IsHEX)
                            //{
                                readByte = Port.ReadByte();
                                message += readByte.ToString("X2") + " ";
                                if (Port.BytesToRead == 0 && Messages.IsHEX)
                                {
                                    //message = hexConverter.ToHexString(message);
                                    BaseBlock.BlockResponse = message;
                                    BaseBlock.ParseCommand();
                                    //Messages.AddReceivedMessage(message);
                                    message = "";
                                }
                            /*}
                            else
                            {
                                readChar = (char)Port.ReadChar();
                                switch (readChar)
                                {
                                    case '\r':
                                        break;
                                    case '\n':
                                        Messages.AddReceivedMessage(message);
                                        message = "";
                                        break;
                                    default:
                                        message += readChar;
                                        break;
                                }
                            }*/
                            //read2 = Port.ReadByte();
                            //read3 = Port.ReadByte();
                            //TODO: Нет обработки без '/n'
                        }
                        //Port.DiscardInBuffer();
                    }
                }
                Thread.Sleep(80);
            }
            //Dispatcher.Thread.Interrupt();
        }



        public void StopThreadLoop()//CancellationTokenSource cancelToken)
        {
            CanReceive = false;
            ShouldShutDownPermanently = true; //&= ReceiverThread.IsAlive;
            cancelSource.Cancel();
            //if (ReceiverThread != null)
            //{
                //ReceiverThread.Interrupt();
            //}
        }
    }
}
