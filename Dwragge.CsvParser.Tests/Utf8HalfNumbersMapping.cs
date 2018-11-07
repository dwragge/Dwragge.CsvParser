using System;
using System.Collections.Generic;
using System.Text;

namespace Dwragge.CsvParser.Tests
{
    public class Utf8HalfNumbersMapping : Utf8CsvMapping1<HalfNumbers>
    {
        public Utf8HalfNumbersMapping()
        {
            MapProperty(0, x => x.A);
            MapProperty(1, x => x.B);
            MapProperty(2, x => x.C);
            MapProperty(3, x => x.D);
            MapProperty(4, x => x.E);
            MapProperty(5, x => x.F);
            MapProperty(6, x => x.G);
            MapProperty(7, x => x.H);
            MapProperty(8, x => x.I);
            MapProperty(9, x => x.J);
        }
    }
}
