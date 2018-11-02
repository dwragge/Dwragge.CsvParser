using System;
using System.Collections.Generic;
using System.Text;

namespace Dwragge.CsvParser
{
    public interface IPropertyMapper<in TEntity>
    {
        void Map(TEntity entity, ReadOnlySpan<char> value);
    }
}
