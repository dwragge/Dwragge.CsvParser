using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Dwragge.CsvParser.Tests
{
    public class FileCsvReaderTests
    {
        [Fact]
        public void FileCsvReader_CanRead_FromStream()
        {
            var csvData = "abc,1234,123.45\r\ndef,2345,234.56";
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(csvData)))
            {
                var reader = new FileCsvReader<TestRecordClass>(new TestRecordClassMapper());
                var results = reader.ReadAll(memStream);

                Assert.Equal(2, results.Count);
                var entity1 = results[0];
                var entity1Expected = new TestRecordClass
                {
                    A = "abc",
                    B = 1234,
                    C = 123.45f
                };
                Assert.Equal(entity1Expected.A, entity1.A);
                Assert.Equal(entity1Expected.B, entity1.B);
                Assert.Equal(entity1Expected.C, entity1.C);

                var entity2 = results[1];
                var entity2Expected = new TestRecordClass
                {
                    A = "def",
                    B = 2345,
                    C = 234.56f
                };
                Assert.Equal(entity2Expected.A, entity2.A);
                Assert.Equal(entity2Expected.B, entity2.B);
                Assert.Equal(entity2Expected.C, entity2.C);
            }
        }
    }
}
