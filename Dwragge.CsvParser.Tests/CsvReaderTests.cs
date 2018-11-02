using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dwragge.CsvParser.Tests
{
    public class CsvReaderTests
    {
        [Fact]
        public void CsvReader_CsvWithNoHeaderAndNoQuotes_CanReadRecords()
        {
            throw new NotImplementedException("FIX THE generic shit with a builder or something");

            var csvData = "value1,value2,value3\r\nvalue1a,value2a,value3a";
            var reader = CsvReader<HalfNumbers>.CreateFromString(csvData, null);

            var lines = new List<string[]>();
            while (reader.Read())
            {
                lines.Add(reader.GetCurrentRecord());
            }

            Assert.Equal(new[] {"value1", "value2", "value3"}, lines[0]);
            Assert.Equal(new[] {"value1a", "value2a", "value3a"}, lines[1]);
        }

        [Fact]
        public void CsvReader_CsvWithHalfNumbers_CanParseNumbersSuccessfully()
        {
            throw new NotImplementedException("FIX THE generic shit with a builder or something");

            var csvData = "value1,1234,value2,5678,value3,9101112,value4,314159,value5,999";
            var reader = CsvReader<HalfNumbers>.CreateFromString(csvData, null);

            HalfNumbers obj = default;
            while (reader.Read())
            {
                obj = reader.GetHalfNumbersRecord();
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

        [Fact]
        public void CsvReader_CsvWIthNoHeaderAndNoQuotes_CanReadAll()
        {
            throw new NotImplementedException("FIX THE generic shit with a builder or something");

            var csvData = "value1,value2,value3\r\nvalue1a,value2a,value3a";
            var reader = CsvReader<HalfNumbers>.CreateFromString(csvData, null);

            var data = reader.ReadToEnd();

            var expected = new []
                {new[] {"value1", "value2", "value3"}, new[] {"value1a", "value2a", "value3a"}};
            Assert.Equal(expected, data);
        }
    }
}
