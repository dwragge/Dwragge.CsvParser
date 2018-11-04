using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dwragge.CsvParser.Tests
{
    public class CsvMappingTests
    {
        public class TestMapper : CsvMapper<HalfNumbers>
        {
            public TestMapper()
            {
                MapProperty(0, x => x.A);
                MapProperty(1, x => x.B);
            }
        }
        [Fact]
        public void CsvMapping_Factory_CanCreateMapper()
        {
            var mapper = new TestMapper();
            var data = "value1,123".AsSpan();
            var index = new [] {0, 6, 7, 3};
            //var entity = mapper.Map(data, ref index);
        }
    }
}
