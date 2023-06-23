using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultIndicator_MainIPConfig.Utilities
{
    public class Checksum
    {
        public byte CheckSum_int(byte[] ptr, int index, int unLen)
        {
            byte b = 0;
            for (int i = index; i < unLen - 2; i++)
            {
                b += ptr[i];
            }
            return b;
        }

        public ushort CheckSum_CRC(string[] ptr, int index, int unLen)
        {
            byte b = 0;
            byte b2 = 0;
            for (int i = index; i < unLen - 3; i++)
            {
                b += Convert.ToByte(ptr[i], 16);
                b2 ^= Convert.ToByte(ptr[i], 16);
            }
            return (ushort)((int)b << 8 | (int)b2);
        }
    }
}
