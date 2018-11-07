using System;
using System.Collections.Generic;
using System.Text;

namespace Dwragge.CsvParser.Tests
{
    public class TestRecordClass
    {
        public string A { get; set; }
        public int B { get; set; }
        public float C { get; set; }
    }

    public class TestRecordClassMapper : Utf8CsvMapping1<TestRecordClass>
    {
        public TestRecordClassMapper()
        {
            MapProperty(0, x => x.A);
            MapProperty(1, x => x.B);
            MapProperty(2, x => x.C);
        }
    }
}
