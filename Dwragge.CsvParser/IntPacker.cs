using System;
using System.Collections.Generic;
using System.Text;

namespace Dwragge.CsvParser
{
    public class IntPacker
    {
        public static (int, int) Unpack(ref long value)
        {
            return ((int) (value >> 32), (int) (value & 0xffffffffL));
        }

        public static long Pack(ref int a, ref int b)
        {
            return (long) a << 32 | (uint) b;
        }
    }
}
