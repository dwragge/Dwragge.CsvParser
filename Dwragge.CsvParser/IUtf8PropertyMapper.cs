using System;
using System.Collections.Generic;
using System.Text;

namespace Dwragge.CsvParser
{
    public interface IUtf8PropertyMapper<in TEntity>
    {
        void Map(TEntity entity, ReadOnlySpan<byte> value);
    }
}
