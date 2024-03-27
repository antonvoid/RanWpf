using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace RanWpf
{
    class Examination
    {    
        private int[] NumPuck {  get; set; }
        public ushort[] table = new ushort[256] {
         0x0000,0x1021,0x2042,0x3063,0x4084,0x50a5,0x60c6,0x70e7,
         0x8108,0x9129,0xa14a,0xb16b,0xc18c,0xd1ad,0xe1ce,0xf1ef,
         0x1231,0x0210,0x3273,0x2252,0x52b5,0x4294,0x72f7,0x62d6,
         0x9339,0x8318,0xb37b,0xa35a,0xd3bd,0xc39c,0xf3ff,0xe3de,
         0x2462,0x3443,0x0420,0x1401,0x64e6,0x74c7,0x44a4,0x5485,
         0xa56a,0xb54b,0x8528,0x9509,0xe5ee,0xf5cf,0xc5ac,0xd58d,
         0x3653,0x2672,0x1611,0x0630,0x76d7,0x66f6,0x5695,0x46b4,
         0xb75b,0xa77a,0x9719,0x8738,0xf7df,0xe7fe,0xd79d,0xc7bc,
         0x48c4,0x58e5,0x6886,0x78a7,0x0840,0x1861,0x2802,0x3823,
         0xc9cc,0xd9ed,0xe98e,0xf9af,0x8948,0x9969,0xa90a,0xb92b,
         0x5af5,0x4ad4,0x7ab7,0x6a96,0x1a71,0x0a50,0x3a33,0x2a12,
         0xdbfd,0xcbdc,0xfbbf,0xeb9e,0x9b79,0x8b58,0xbb3b,0xab1a,
         0x6ca6,0x7c87,0x4ce4,0x5cc5,0x2c22,0x3c03,0x0c60,0x1c41,
         0xedae,0xfd8f,0xcdec,0xddcd,0xad2a,0xbd0b,0x8d68,0x9d49,
         0x7e97,0x6eb6,0x5ed5,0x4ef4,0x3e13,0x2e32,0x1e51,0x0e70,
         0xff9f,0xefbe,0xdfdd,0xcffc,0xbf1b,0xaf3a,0x9f59,0x8f78,
         0x9188,0x81a9,0xb1ca,0xa1eb,0xd10c,0xc12d,0xf14e,0xe16f,
         0x1080,0x00a1,0x30c2,0x20e3,0x5004,0x4025,0x7046,0x6067,
         0x83b9,0x9398,0xa3fb,0xb3da,0xc33d,0xd31c,0xe37f,0xf35e,
         0x02b1,0x1290,0x22f3,0x32d2,0x4235,0x5214,0x6277,0x7256,
         0xb5ea,0xa5cb,0x95a8,0x8589,0xf56e,0xe54f,0xd52c,0xc50d,
         0x34e2,0x24c3,0x14a0,0x0481,0x7466,0x6447,0x5424,0x4405,
         0xa7db,0xb7fa,0x8799,0x97b8,0xe75f,0xf77e,0xc71d,0xd73c,
         0x26d3,0x36f2,0x0691,0x16b0,0x6657,0x7676,0x4615,0x5634,
         0xd94c,0xc96d,0xf90e,0xe92f,0x99c8,0x89e9,0xb98a,0xa9ab,
         0x5844,0x4865,0x7806,0x6827,0x18c0,0x08e1,0x3882,0x28a3,
         0xcb7d,0xdb5c,0xeb3f,0xfb1e,0x8bf9,0x9bd8,0xabbb,0xbb9a,
         0x4a75,0x5a54,0x6a37,0x7a16,0x0af1,0x1ad0,0x2ab3,0x3a92,
         0xfd2e,0xed0f,0xdd6c,0xcd4d,0xbdaa,0xad8b,0x9de8,0x8dc9,
         0x7c26,0x6c07,0x5c64,0x4c45,0x3ca2,0x2c83,0x1ce0,0x0cc1,
         0xef1f,0xff3e,0xcf5d,0xdf7c,0xaf9b,0xbfba,0x8fd9,0x9ff8,
         0x6e17,0x7e36,0x4e55,0x5e74,0x2e93,0x3eb2,0x0ed1,0x1ef0};

        // вычисление crc
        private int SearchForCrcErrors(Byte[] bytes)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < bytes.Length-2; i++)
            {
                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ bytes[i])]);
            }
            Byte[] crc1 = [bytes[2047], bytes[2046]];
            ushort crc2 = BitConverter.ToUInt16(crc1);
            if (crc == crc2)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
        public Examination()
        {
            NumPuck = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        }
        // проверка типа кадра
        private int TypeChecking(Byte[] bytes)
        {
            (int, int)[] frame1_8 = new (int, int)[] { (0x2c, 0xfa), (0x2d, 0), (0x2d, 1), (0x2d, 2), (0x2d, 3), (0x2d, 4), (0x2d, 5), (0x2d, 6) };
            int[] frame9_10 = new int[] { 0x2f, 0x30 };
            if (bytes[3] == frame9_10[1])
            {
                return 10;
            }
            else if (bytes[3] == frame9_10[0])
            {
                return 9;
            }
            else if (bytes[3] == frame1_8[7].Item1 && bytes[4] == frame1_8[7].Item2)
            {
                return 8;
            }
            else if (bytes[3] == frame1_8[6].Item1 && bytes[4] == frame1_8[6].Item2)
            {
                return 7;
            }
            else if (bytes[3] == frame1_8[5].Item1 && bytes[4] == frame1_8[5].Item2)
            {
                return 6;
            }
            else if (bytes[3] == frame1_8[4].Item1 && bytes[4] == frame1_8[4].Item2)
            {
                return 5;
            }
            else if (bytes[3] == frame1_8[3].Item1 && bytes[4] == frame1_8[3].Item2)
            {
                return 4;
            }
            else if (bytes[3] == frame1_8[2].Item1 && bytes[4] == frame1_8[2].Item2)
            {
                return 3;
            }
            else if (bytes[3] == frame1_8[1].Item1 && bytes[4] == frame1_8[1].Item2)
            {
                return 2;
            }
            else if (bytes[3] == frame1_8[0].Item1 && bytes[4] == frame1_8[0].Item2)
            {
                return 1;
            }
            else
            {
                return -1;
            }
            
        }
        // вычисление ошибок нумерации
        private int SearchForNumberingErrors(Byte[] bytes)
        {
            int type = TypeChecking(bytes)-1;
            Byte[] nnp = new Byte[4];
            if (type == 9)
            {
                return 0;
            }
            else if (NumPuck[type] == 0)
            {
                if(type<=7 && type>=0)
                {
                    nnp = [bytes[8], bytes[7], bytes[6], bytes[5]];
                    NumPuck[type] = BitConverter.ToInt32(nnp);
                }
                else
                {
                    nnp = [bytes[7], bytes[6], bytes[5], bytes[4]];
                    NumPuck[type] = BitConverter.ToInt32(nnp);
                }
        
                return 0;
            }
            else
            {
                int NewNumPuck = 0;
        
                if (type==8)
                {
                    nnp = [bytes[7], bytes[6], bytes[5], bytes[4]];
                    NewNumPuck = BitConverter.ToInt32(nnp);          
                }
                else
                {
                    nnp = [bytes[8], bytes[7], bytes[6], bytes[5]];
                    NewNumPuck = BitConverter.ToInt32(nnp);
                }
                if ( NewNumPuck - NumPuck[type] == 1)
                {
                    NumPuck[type] = NewNumPuck;
                    return 0;
                }
                else 
                {
                    NumPuck[type] = NewNumPuck;
                    return 1;
                }
        
            }
    
        }

        // метод для поиска кадров
        public int SearchFrame(Byte[] bytes, Line[] lines)
        {
            int pointer = 0;
            int type;
            int[] marker = new int[] { 0x7c, 0x6e, 0xa1 };
            Byte[] frame = new Byte[2048];
            while (pointer <= bytes.Length - 2048)
            {
                Array.Copy(bytes, pointer, frame, 0, 2048);
                if (bytes[pointer] == marker[0] && bytes[pointer+1] == marker[1] && bytes[pointer+2] == marker[2] && TypeChecking(frame) != -1)
                {
                    // заполняем строчки таблицы у найденого кадра
                    type = TypeChecking(frame) - 1;
                    lines[type].Quantity += 1;
                    lines[type].NumberingError += SearchForNumberingErrors(frame);
                    lines[type].CrcError += SearchForCrcErrors(frame);
                    pointer += 2048;
                }
                else
                {
                    // ищем следующий кадр
                    int j = pointer+1;
                    while (j <= bytes.Length - 3 && !(bytes[j] == marker[0] && bytes[j+1] == marker[1] && bytes[j+2] == marker[2]) )
                    {
                        j++;
                    }
                    pointer = j;           
                }
            }
            return pointer;
        }
    }
}
