using System;

namespace Dwragge.CsvParser
{
    public class MappingTest : IPropertyMapper<HalfNumbers>
    {
        public void Map(HalfNumbers entity, ReadOnlySpan<char> value)
        {
            int parsedValue = ParseUtils.ParseInt(value);
            entity.B = parsedValue;
        }
    }

    public class MappingTestString : IUtf8PropertyMapper<HalfNumbers>
    {
        public void Map(HalfNumbers entity, ReadOnlySpan<byte> value)
        {
            string parsedValue = ParseUtils.SpanToString(value);
            entity.A = parsedValue;
        }
    }
}
