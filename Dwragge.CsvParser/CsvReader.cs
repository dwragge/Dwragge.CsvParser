using System;

namespace Dwragge.CsvParser
{
    public class CsvReader<T> where T : class, new()
    {
        private readonly ReadOnlyMemory<char> _data;
        private int _currentRecordPos;
        private int _recordLength;
        private const string Delimiter = ",";
        private const string LineBreak = "\r\n";

        private readonly CsvMapper<T> _mapper;
        //index then length
        private int[] _currentValueIndices;
        private readonly Func<T> TCreatorFunc;

        private CsvReader(ReadOnlyMemory<char> data, CsvMapper<T> mapper)
        {
            _data = data;
            _mapper = mapper;
            TCreatorFunc = ReflectionUtils.CreateConstructorCallFunc<T>();
        }

        private static int CalculateRecordLength(ReadOnlySpan<char> span)
        {
            // need to figure out how long the header is
            int lfPos = span.IndexOf('\n');
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

            var span = _data.Span.Slice(_currentRecordPos);

            if (_recordLength == 0)
            {
                _recordLength = CalculateRecordLength(span);
                if (_currentValueIndices == null) _currentValueIndices = new int[_recordLength * 2];
            }
            
            int currentValuePosition = 0;
            for (int i = 0; i < _recordLength; i++)
            {
                var delim = i == _recordLength - 1 ? LineBreak.AsSpan() : Delimiter.AsSpan();
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

        public T GetRecord()
        {
            var obj = TCreatorFunc();
            return _mapper.Map(obj, _data.Span, ref _currentValueIndices);
        }

        public static CsvReader<T> CreateFromString(string data, CsvMapper<T> mapping) 
        {
            return new CsvReader<T>(data.AsMemory(), mapping);
        }
    }
}
