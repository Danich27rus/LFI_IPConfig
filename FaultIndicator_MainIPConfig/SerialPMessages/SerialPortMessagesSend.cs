using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using FaultIndicator_MainIPConfig.Utilities;
using System.Windows.Input;

namespace FaultIndicator_MainIPConfig.SerialPMessages
{
    /// <summary>
    /// Used for sending messages to the serial port (on the main thread...). Just an easier way to manage receiving/sending tbh
    /// </summary>
    public class SerialPortMessagesSend
    {
        private HEXConverter hexConverter { get; set; }
        public SerialPort Port { get; set; }
        public bool CanSend { get; set; }
        public SerialPortMessagesViewModel Messages { get; set; }
        public SerialPortMessagesSend()
        {
            hexConverter = new HEXConverter();
            CanSend = true;
        }

        public void SendMessage(string message, bool shouldSendNewLine = false)
        {
            // Adds a new line if needed
            string newMessage = message + (shouldSendNewLine ? "\n" : "");
            // Gets the bytes of the message using the serial port's encoding
            // Will be using a byte buffer because it's faster than sending strings using 
            // the serial port's build in methods... apparently
            byte[] buffer = Port.Encoding.GetBytes(newMessage);

            for (int i = 0; i < buffer.Length; i++)
            {
                Port.BaseStream.WriteByte(buffer[i]);
            }
        }

        public void SendHEXMessage(string message, bool shouldSendNewLine = false)
        {
            // Adds a new line if needed
            string newMessage = message + (shouldSendNewLine ? "\n" : "");
            byte[] convertedMessage = hexConverter.StringToByteArray(newMessage);
            // Gets the bytes of the message using the serial port's encoding
            // Will be using a byte buffer because it's faster than sending strings using 
            // the serial port's build in methods... apparently
            // byte[] buffer = Port.Encoding.GetBytes(convertedMessage);

            for (int i = 0; i < convertedMessage.Length; i++)
            {
                Port.BaseStream.WriteByte(convertedMessage[i]);
            }
        }

        public void ByteMessage(byte[] message, bool shouldSendNewLine = false)
        {
            // Adds a new line if needed
            //string newMessage = message + (shouldSendNewLine ? "\n" : "");
            //byte[] convertedMessage = hexConverter.StringToByteArray(newMessage);
            // Gets the bytes of the message using the serial port's encoding
            // Will be using a byte buffer because it's faster than sending strings using 
            // the serial port's build in methods... apparently
            // byte[] buffer = Port.Encoding.GetBytes(convertedMessage);

            for (int i = 0; i < message.Length; i++)
            {
                Port.BaseStream.WriteByte(message[i]);
            }
        }
    }
}
