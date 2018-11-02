using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dwragge.CsvParser.Tests
{
    public class Utf8CsvBuilderTests
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

        [Fact]
        public void Utf8CsvReader_CanRead()
        {
            var csvData = "value1,1234,value2,5678,value3,9101112,value4,314159,value5,999";
            var reader = Utf8CsvReader<HalfNumbers>.CreateFromString(csvData, new Utf8HalfNumbersMapping());

            HalfNumbers obj = default;
            while (reader.Read())
            {
                obj = reader.GetCurrentRecordDynamic();
            }

            var expected = new HalfNumbers
            {
                A = "value1",
                B = 1234,
                C = "value2",
                D = 5678,
                E = "value3",
                F = 9101112,
                G = "value4",
                H = 314159,
                I = "value5",
                J = 999
            };
            Assert.Equal(expected.A, obj.A);
            Assert.Equal(expected.B, obj.B);
            Assert.Equal(expected.C, obj.C);
            Assert.Equal(expected.D, obj.D);
            Assert.Equal(expected.E, obj.E);
            Assert.Equal(expected.F, obj.F);
            Assert.Equal(expected.G, obj.G);
            Assert.Equal(expected.H, obj.H);
            Assert.Equal(expected.I, obj.I);
            Assert.Equal(expected.J, obj.J);
        }
    }
}
