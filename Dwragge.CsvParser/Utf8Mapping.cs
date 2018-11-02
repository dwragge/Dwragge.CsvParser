﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dwragge.CsvParser
{
    public class Utf8CsvMapping1<TEntity> where TEntity : class, new()
    {
        private class CsvPropertyMapping
        {
            public CsvPropertyMapping(int index, IUtf8PropertyMapper<TEntity> mapper)
            {
                Index = index;
                Mapper = mapper;
            }

            public IUtf8PropertyMapper<TEntity> Mapper { get; }
            public int Index { get; }
        }

        private List<CsvPropertyMapping> _mappings = new List<CsvPropertyMapping>();
        private int _numCols = 0;

        protected void MapProperty<TProperty>(int columnIndex,
            Expression<Func<TEntity, TProperty>> mappingExpression)
        {
            // get propertyname
            var propertyName = GetPropertyNameFromExpression(mappingExpression);
            // create ipropertymapper
            var mapper = PropertyMapperFactory.CreateMapperUtf8<TEntity>(propertyName);
            var mapping = new CsvPropertyMapping(columnIndex, mapper);
            _mappings.Insert(columnIndex, mapping);
            _numCols++;
        }

        private string GetPropertyNameFromExpression<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression.Body is UnaryExpression unary)
            {
                if (unary.Operand is MemberExpression memberExp)
                {
                    var property = (PropertyInfo) memberExp.Member;
                    return property.Name;
                }
            }
            else if (expression.Body is MemberExpression memberExp)
            {
                var property = (PropertyInfo) memberExp.Member;
                return property.Name;
            }

            throw new InvalidOperationException("expression was not a valid property setter");
        }

        // Hot Path
        public void Map(TEntity entity, ReadOnlySpan<byte> recordSpan, ref int[] indexData)
        {
            for (int i = 0; i < _numCols; i++)
            {
                var mapping = _mappings[i];
                var value = recordSpan.Slice(indexData[mapping.Index * 2], indexData[mapping.Index * 2 + 1]);
                mapping.Mapper.Map(entity, value);
            }
        }
    }
}
