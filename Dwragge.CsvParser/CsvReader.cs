using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;

namespace Dwragge.CsvParser
{
    public class CsvReader<T> where T : class, new()
    {
        private ReadOnlyMemory<char> _data;
        private int _currentRecordPos;
        private int _recordLength;
        private const string Delimiter = ",";
        private const string LineBreak = "\r\n";

        private CsvMapping1<T> _mapping;
        //index then length
        private int[] _currentValueIndices;

        private CsvReader(ReadOnlyMemory<char> data, CsvMapping1<T> mapping)
        {
            _data = data;
            _mapping = mapping;
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

            ReadOnlySpan<char> span = _data.Span.Slice(_currentRecordPos);

            if (_recordLength == 0)
            {
                _recordLength = CalculateRecordLength(span);
                if (_currentValueIndices == null) _currentValueIndices = new int[_recordLength * 2];
            }
            
            int currentValuePosition = 0;
            //if (_currentValue != null)
            //{
            //    ArrayPool<string>.Shared.Return(_currentValue);
            //    _currentValue = null;
            //}

            //_currentValue = ArrayPool<string>.Shared.Rent(_recordLength);
            for (int i = 0; i < _recordLength; i++)
            {
                ReadOnlySpan<char> delim = i == _recordLength - 1 ? LineBreak.AsSpan() : Delimiter.AsSpan();
                int nextValuePosition = span.Slice(currentValuePosition).IndexOf(delim);
                ReadOnlySpan<char> recordValue;
                if (nextValuePosition == -1)
                {
                    if (i == _recordLength - 1)
                    {
                        // the rest of the string is the last value
                        recordValue = span.Slice(currentValuePosition);
                        //_currentRecordPos += recordValue.Length; // so the loop ends
                        nextValuePosition = recordValue.Length;
                        //nextValuePosition = span.Length - _currentRecordPos + currentValuePosition;
                    }
                    else
                    {
                        throw new InvalidOperationException("CSV was invalid, unexpected EOF");
                    }
                }
                else
                {
                    //recordValue = span.Slice(_currentRecordPos + currentValuePosition, nextValuePosition);
                }
                //_currentValue[i] = new string(recordValue);
                _currentValueIndices[i * 2] = _currentRecordPos + currentValuePosition;
                _currentValueIndices[i * 2 + 1] = nextValuePosition;
                currentValuePosition += nextValuePosition + delim.Length; // skip over the delimiter
            }

            _currentRecordPos += currentValuePosition;
            return true;
        }

        public T GetCurrentRecordDynamic(T obj)
        {
            return _mapping.Map(obj, _data.Span, ref _currentValueIndices);
        }

        public string[] GetCurrentRecord()
        {
            //var ret = new string[_recordLength]; // allocate
            //for (int i = 0; i < _recordLength; i++)
            //{
            //    ret[i] = _currentValue[i]; // copy each item !! (probably very bad)
            //}

            //ArrayPool<string>.Shared.Return(_currentValue);
            //_currentValue = null;
            //return ret;
            var ret = new string[_recordLength];
            var span = _data.Span;
            for (int i = 0; i < _recordLength; i++)
            {
                var str = new string(span.Slice(_currentValueIndices[i * 2], _currentValueIndices[i * 2 + 1]));
                ret[i] = str;
            }

            return ret;
        }

        

        public TestRecordAllString GetCurrentPocoRecord()
        {
            var ret = new TestRecordAllString();
            var span = _data.Span;
            ret.A = new string(span.Slice(_currentValueIndices[0], _currentValueIndices[1]));
            ret.B = new string(span.Slice(_currentValueIndices[2], _currentValueIndices[3]));
            ret.C = new string(span.Slice(_currentValueIndices[4], _currentValueIndices[5]));
            ret.D = new string(span.Slice(_currentValueIndices[6], _currentValueIndices[7]));
            ret.E = new string(span.Slice(_currentValueIndices[8], _currentValueIndices[9]));
            ret.F = new string(span.Slice(_currentValueIndices[10], _currentValueIndices[11]));
            ret.G = new string(span.Slice(_currentValueIndices[12], _currentValueIndices[13]));
            ret.H = new string(span.Slice(_currentValueIndices[14], _currentValueIndices[15]));
            ret.I = new string(span.Slice(_currentValueIndices[16], _currentValueIndices[17]));
            ret.J = new string(span.Slice(_currentValueIndices[18], _currentValueIndices[19]));

            return ret;
        }
        
        private static int ParseInt(ReadOnlySpan<char> str)
        {
            Span<byte> bytes = stackalloc byte[str.Length];
            int numBytes = Encoding.UTF8.GetBytes(str, bytes);
            if (numBytes > str.Length) throw new InvalidOperationException();
            if (!Utf8Parser.TryParse(bytes, out int value, out int _))
            {
                throw new InvalidOperationException();
            }

            return value;
        }

        public int GetIntColumn(int index)
        {
            var span = _data.Span;
            return ParseInt(span.Slice(_currentValueIndices[index * 2], _currentValueIndices[index * 2 + 1]));
        }

        private float Parse(ReadOnlySpan<char> str)
        {
            Span<byte> bytes = stackalloc byte[str.Length];
            int numBytes = Encoding.UTF8.GetBytes(str, bytes);
            if (numBytes > str.Length) throw new InvalidOperationException();
            if (!Utf8Parser.TryParse(bytes, out float value, out int _))
            {
                throw new InvalidOperationException();
            }

            return value;
        }

        public HalfNumbers GetHalfNumbersRecord()
        {
            var ret = new HalfNumbers();
            var span = _data.Span;

            ret.A = new string(span.Slice(_currentValueIndices[0], _currentValueIndices[1]));
            ret.B = ParseInt(span.Slice(_currentValueIndices[2], _currentValueIndices[3]));
            ret.C = new string(span.Slice(_currentValueIndices[4], _currentValueIndices[5]));
            ret.D = ParseInt(span.Slice(_currentValueIndices[6], _currentValueIndices[7]));
            ret.E = new string(span.Slice(_currentValueIndices[8], _currentValueIndices[9]));
            ret.F = ParseInt(span.Slice(_currentValueIndices[10], _currentValueIndices[11]));
            ret.G = new string(span.Slice(_currentValueIndices[12], _currentValueIndices[13]));
            ret.H = ParseInt(span.Slice(_currentValueIndices[14], _currentValueIndices[15]));
            ret.I = new string(span.Slice(_currentValueIndices[16], _currentValueIndices[17]));
            ret.J = ParseInt(span.Slice(_currentValueIndices[18], _currentValueIndices[19]));

            return ret;
        }

        public Span<int> GetRecordDataArray()
        {
            Span<int> dataArray = new int[_recordLength * 2];
            _currentValueIndices.CopyTo(dataArray);
            return dataArray;
        }

        public List<string[]> ReadToEnd()
        {
            var list = new List<string[]>();
            while (Read())
            {
                list.Add(GetCurrentRecord());
            }

            return list;
        }

        public static CsvReader<T> CreateFromString(string data, CsvMapping1<T> mapping) 
        {
            return new CsvReader<T>(data.AsMemory(), mapping);
        }
    }
}
