using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultIndicator_MainIPConfig.Utilities
{
    internal class HEXConverter
    {
        private string[]? _lettersString;
        private byte[]? _hexString;

        public string[] LettersString
        { 
            get => _lettersString;  
            set => _lettersString = value;
        }

        public byte[] HEXString
        {
            get => _hexString;
            set => _hexString = value;
        }

        public byte[] StringToByteArray(String hex)
        {
            LettersString = hex.Split(' ');
            HEXString = new byte[LettersString.Length];
            for (var i = 0; i < LettersString.Length; i++)
            {
                HEXString[i] = Convert.ToByte(LettersString[i], 16);
            }
            return HEXString;
        }

        //TODO: Не используется, возможно потребутеся удалить
        public string ToHexString(String str)
        {
            int value, i = 0;
            char[] charValues = str.ToCharArray();
            LettersString = new string[charValues.Length];
            foreach (var _char in charValues)
            {
                value = Convert.ToInt32(_char);
                LettersString[i] = String.Format("{0:X}", value);
                i += 1;
            }
            return string.Join(" ", LettersString);
        }
    }
}
