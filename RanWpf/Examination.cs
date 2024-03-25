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
        private const ushort polynomial = 0x8408;
        private static readonly ushort[] table = new ushort[256];

        // вычисление crc
        private int SearchForCrcErrors(Byte[] bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < bytes.Length-2; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }
            Byte[] crc1 = BitConverter.GetBytes(crc);
            if (crc1[1] == bytes[2046] && crc1[0] == bytes[2047])
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
            // инициализирование таблицы temp
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }
        // проверка типа кадра
        private int TypeChecking(Byte[] bytes)
        {
            (int, int)[] frame1_8 = new (int, int)[] { (0x2c, 0xfa), (0x2d, 0), (0x2d, 1), (0x2d, 2), (0x2d, 3), (0x2d, 4), (0x2d, 5), (0x2d, 6) };
            int[] frame9_10 = new int[] { 0x2f, 30 };
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
            if (type == 9)
            {
                return 0;
            }
            else if (NumPuck[type] == 0)
            {
                if(type<=7 && type>=0)
                {
                    NumPuck[type] = BitConverter.ToInt32(bytes, 5);
                }
                else
                {
                    NumPuck[type] = BitConverter.ToInt32(bytes, 4);
                }
                
                return 0;
            }
            else
            {
                int NewNumPuck = 0;
                if (type <= 7 && type >= 0)
                {
                    NewNumPuck = BitConverter.ToInt32(bytes, 5);
                }
                else
                {
                    NewNumPuck = BitConverter.ToInt32(bytes, 4);
                }
                if ( NewNumPuck - NumPuck[type] > 1)
                {
                    return 1;
                }
                else 
                {
                    return 0;
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
