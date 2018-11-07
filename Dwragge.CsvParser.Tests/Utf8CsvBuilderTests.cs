using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dwragge.CsvParser.Tests
{
    public class Utf8CsvBuilderTests
    {
        


        public class FloatClass
        {
            public float A { get; set; }
        }

        public class FloatClassMapper : Utf8CsvMapping1<FloatClass>
        {
            public FloatClassMapper()
            {
                MapProperty(0, x => x.A);
            }
        }

        [Fact]
        public void Utf8CsvReader_CanReadFloat()
        {
            var csvData = "123.4567";
            var reader = Utf8CsvReader<FloatClass>.CreateFromString(csvData, new FloatClassMapper());
            FloatClass obj = default;
            while (reader.Read())
            {
                obj = reader.GetCurrentRecordDynamic();
            }

            Assert.Equal(123.4567f, obj.A);
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
