using System;
using System.Text;

namespace Dwragge.CsvParser
{
    public class Utf8CsvReader<T> where T : class, new()
    {
        private readonly ReadOnlyMemory<byte> _data;
        private int _currentRecordPos;
        private int _recordLength;
        private static byte[] Delimiter = Encoding.UTF8.GetBytes(",");
        private static byte[] LineBreak = Encoding.UTF8.GetBytes("\r\n");

        private readonly Utf8CsvMapping1<T> _mapping;
        //index then length
        private int[] _currentValueIndices;
        private readonly Func<T> TCreatorFunc;

        private Utf8CsvReader(ReadOnlyMemory<byte> data, Utf8CsvMapping1<T> mapping)
        {
            _data = data;
            _mapping = mapping;

            TCreatorFunc = ReflectionUtils.CreateConstructorCallFunc<T>();
        }

        private static int CalculateRecordLength(ReadOnlySpan<byte> span)
        {
            // need to figure out how long the header is
            int lfPos = span.IndexOf((byte)'\n');
            if (lfPos == -1)
            {
                lfPos = span.Length - 1;
            }

            int headerCount = 0;
            int nextValuePos = 0;
            while (nextValuePos < lfPos)
            {
                headerCount++;
                var length = span.Slice(nextValuePos).IndexOf(Delimiter) + 1;
                nextValuePos += length;
                if (length == 0) break; // case for single line csv
            }

            return headerCount;
        }

        public bool Read()
        {
            if (_currentRecordPos >= _data.Length) return false;
            ReadOnlySpan<byte> span = _data.Span.Slice(_currentRecordPos);

            if (_recordLength == 0)
            {
                _recordLength = CalculateRecordLength(span);
                if (_currentValueIndices == null) _currentValueIndices = new int[_recordLength * 2];
            }
            
            int currentValuePosition = 0;
            for (int i = 0; i < _recordLength; i++)
            {
                ReadOnlySpan<byte> delim = i == _recordLength - 1 ? LineBreak.AsSpan() : Delimiter.AsSpan();
                int nextValuePosition = span.Slice(currentValuePosition).IndexOf(delim);
                if (nextValuePosition == -1)
                {
                    if (i == _recordLength - 1)
                    {
                        // the rest of the string is the last value
                        var recordValue = span.Slice(currentValuePosition);
                        nextValuePosition = recordValue.Length;
                    }
                    else
                    {
                        throw new InvalidOperationException("CSV was invalid, unexpected EOF");
                    }
                }
                _currentValueIndices[i * 2] = _currentRecordPos + currentValuePosition;
                _currentValueIndices[i * 2 + 1] = nextValuePosition;
                currentValuePosition += nextValuePosition + delim.Length; // skip over the delimiter
            }

            _currentRecordPos += currentValuePosition;
            return true;
        }

        public T GetCurrentRecordDynamic()
        {
            var obj = TCreatorFunc();
            _mapping.Map(obj, _data.Span, _currentValueIndices);
            return obj;
        }

        public static Utf8CsvReader<T> CreateFromString(string data, Utf8CsvMapping1<T> mapping)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return new Utf8CsvReader<T>(bytes, mapping);
        }

        public static Utf8CsvReader<T> CreateFromBytes(ReadOnlyMemory<byte> data, Utf8CsvMapping1<T> mapping)
        {
            return new Utf8CsvReader<T>(data, mapping);
        }
    }
}
