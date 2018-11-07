using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Dwragge.CsvParser
{
    public class FileCsvReader<T> where T : class, new()
    {
        private List<T> _list;
        private int _recordLength;
        private int _lastRecordLocation;
        private readonly Utf8CsvMapping1<T> _mapper;

        public FileCsvReader(Utf8CsvMapping1<T> mapper)
        {
            _mapper = mapper;
        }

        public List<T> ReadAll(Stream s)
        {
            var pipe = new Pipe();
            _list = new List<T>();
            Task writing = FillPipeAsync(s, pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);

            Task.WaitAll(writing, reading);
            

            return _list;
        }

        private async Task FillPipeAsync(Stream s, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    int bytesRead = await s.ReadAsync(memory);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    writer.Advance(bytesRead);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            writer.Complete();
        }

        private async Task ReadPipeAsync(PipeReader reader)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;
                SequencePosition? position = null;
                var shouldBreak = false;

                do
                {
                    position = buffer.PositionOf((byte) '\n');

                    // Account for the last line not including a \n
                    if (position == null && result.IsCompleted)
                    {
                        var lastSlice = buffer.Slice(0, buffer.End);
                        // Even though the 'RFC' says the file shouldn't have a blank line at the end
                        // We should still account for it as it'd be a quite common problem
                        // And if we don't account it crashes
                        if (lastSlice.Length != 0)
                        {
                            ProcessLine(lastSlice);
                        }
                    }

                    if (position != null)
                    {
                        ProcessLine(buffer.Slice(0, position.Value));
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                }
                while (position != null);

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            reader.Complete();
        }

        private static int CalculateRecordLength(ReadOnlySequence<byte> span)
        {
            // need to figure out how long the header is
            //var position = span.PositionOf((byte) '\n');
            //long lfPos;
            //if (position == null)
            //{
            //    lfPos = span.Length - 1;
            //}
            //else
            //{
            //    lfPos = position.Value.GetInteger();
            //}
            //while (nextValuePos )
            //{
            //    headerCount++;
            //    var nextPos = span.Slice(nextValuePos).PositionOf((byte)',');
            //    var length = nextPos?.GetInteger() + 1 ?? 0;
            //    nextValuePos += length;
            //    if (length == 0) break; // case for single line csv
            //}

            int headerCount = 1;
            SequencePosition? nextPostion = span.PositionOf((byte) ',');
            while (nextPostion != null)
            {
                headerCount++;
                nextPostion = span.Slice(span.GetPosition(1, nextPostion.Value)).PositionOf((byte) ',');
            }

            return headerCount;
        }

        // TODO Use GetPosition instead of nextpositionrelative
        private void ProcessLine(ReadOnlySequence<byte> readOnlySequence)
        {
            if (_recordLength == 0)
            {
                _recordLength = CalculateRecordLength(readOnlySequence);
            }

            Span<int> valueIndices = stackalloc int[_recordLength * 2];
            SequencePosition[] positions = ArrayPool<SequencePosition>.Shared.Rent(_recordLength + 1);

            SequencePosition currentValuePosition = readOnlySequence.Start;
            for (int i = 0; i < _recordLength; i++)
            {
                byte delim = i == _recordLength - 1 ? (byte)'\r' : (byte)',';
                var nextValuePosition = readOnlySequence.Slice(readOnlySequence.GetPosition(1, currentValuePosition)).PositionOf(delim);
                if (nextValuePosition == null)
                {
                    if (i == _recordLength - 1)
                    {
                        // the rest of the string is the last value
                        //var recordValue = readOnlySequence.Slice(currentValuePosition);
                        nextValuePosition = currentValuePosition;
                    }
                    else
                    {
                        throw new InvalidOperationException("CSV was invalid, unexpected EOF");
                    }
                }

                positions[i] = currentValuePosition;
                currentValuePosition = readOnlySequence.GetPosition(1, nextValuePosition.Value);
            }

            positions[_recordLength] = readOnlySequence.End;

            var currentPos = 0;
            for (int i = 0; i < _recordLength; i++)
            {
                // Because we already skipped over the delimiter, need to 'unskip' over it so length doesn't include it
                var segmentLength =  (int)readOnlySequence.Slice(positions[i], positions[i + 1]).Length - 1; 
                valueIndices[i * 2] = currentPos;
                valueIndices[i * 2 + 1] = segmentLength;
                currentPos += segmentLength + 1; // skip over the delim
            }

            // This is disgusting, find a better way to do this
            // this happens because .End doesn't have a delim to skip over
            // and we assume that we do in the for loop
            valueIndices[(_recordLength - 1) * 2 + 1] += 1;

            var entity = _mapper.Map(readOnlySequence, valueIndices);
            _list.Add(entity);
        }
    }
}
