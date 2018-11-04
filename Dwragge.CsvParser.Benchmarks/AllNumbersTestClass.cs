using System;
using System.Collections.Generic;
using System.Text;
using TinyCsvParser.Mapping;

namespace Dwragge.CsvParser.Benchmarks
{
    public class AllNumbersTestClass
    {
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float D { get; set; }
        public float E { get; set; }
        public float F { get; set; }
        public float G { get; set; }
        public float H { get; set; }
        public float I { get; set; }
        public float J { get; set; }
    }

    public class AllNumbersTestMapping : CsvMapping<AllNumbersTestClass>
    {
        public AllNumbersTestMapping()
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

    public class AllNumbersTestMapper : CsvMapper<AllNumbersTestClass>
    {
        public AllNumbersTestMapper()
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

    public class AllNumbersTestMapperUtf8 : Utf8CsvMapping1<AllNumbersTestClass>
    {
        public AllNumbersTestMapperUtf8()
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
