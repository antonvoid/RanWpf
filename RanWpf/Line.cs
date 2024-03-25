using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RanWpf
{
    class Line
    {
        public string Name {  get; set; }
        public int Quantity { get; set; }
        public int NumberingError { get; set; } 
        public int CrcError { get; set; }
        public Line(string name, int quantity, int numberingError, int crcError)
        {
            Name = name;
            Quantity = quantity;
            NumberingError = numberingError;
            CrcError = crcError;
        }
    }
}
