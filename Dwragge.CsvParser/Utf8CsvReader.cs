using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Dwragge.CsvParser
{
    public class Utf8CsvReader<T> where T : class, new()
    {
        private ReadOnlyMemory<byte> _data;
        private int _currentRecordPos;
        private int _recordLength;
        private static byte[] Delimiter = Encoding.UTF8.GetBytes(",");
        private static byte[] LineBreak = Encoding.UTF8.GetBytes("\r\n");

        private Utf8CsvMapping1<T> _mapping;
        //index then length
        private int[] _currentValueIndices;
        private Func<T> TCreatorFunc;

        private Utf8CsvReader(ReadOnlyMemory<byte> data, Utf8CsvMapping1<T> mapping)
        {
            _data = data;
            _mapping = mapping;

            CreateTCreator();
        }

        private void CreateTCreator()
        {
            var type = typeof(T);
            if (TCreatorFunc != null) return;
            ConstructorInfo emptyConstructor = type.GetConstructor(Type.EmptyTypes);
            var dynamicMethod = new DynamicMethod("CreateInstance", type, Type.EmptyTypes, true);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
            ilGenerator.Emit(OpCodes.Ret);
            TCreatorFunc = (Func<T>)dynamicMethod.CreateDelegate(typeof(Func<>).MakeGenericType(typeof(T)));
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
            //if (_currentValue != null)
            //{
            //    ArrayPool<string>.Shared.Return(_currentValue);
            //    _currentValue = null;
            //}

            //_currentValue = ArrayPool<string>.Shared.Rent(_recordLength);
            for (int i = 0; i < _recordLength; i++)
            {
                ReadOnlySpan<byte> delim = i == _recordLength - 1 ? LineBreak.AsSpan() : Delimiter.AsSpan();
                int nextValuePosition = span.Slice(currentValuePosition).IndexOf(delim);
                ReadOnlySpan<byte> recordValue;
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

        public T GetCurrentRecordDynamic()
        {
            var obj = TCreatorFunc();
            _mapping.Map(obj, _data.Span, ref _currentValueIndices);
            return obj;
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
                var str = string.Create(_currentValueIndices[i * 2 + 1], i,
                    (chars, index) =>
                    {
                        var bytes = _data.Span.Slice(_currentValueIndices[index * 2], _currentValueIndices[index * 2 + 1]);
                        Encoding.UTF8.GetChars(bytes, chars);
                    });
                ret[i] = str;
            }

            return ret;
        }
        

        public int GetIntColumn(int index)
        {
            var span = _data.Span;
            return ParseUtils.ParseInt(span.Slice(_currentValueIndices[index * 2], _currentValueIndices[index * 2 + 1]));
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
