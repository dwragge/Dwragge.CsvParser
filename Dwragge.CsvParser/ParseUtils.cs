using System;
using System.Buffers.Text;
using System.Text;

// The code that calls this is dynamically genereated
// ReSharper disable UnusedMember.Global

namespace Dwragge.CsvParser
{
    public static class ParseUtils
    {
        public static int ParseInt(ReadOnlySpan<char> str)
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

        public static string SpanToString(ReadOnlySpan<byte> bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static int ParseInt(ReadOnlySpan<byte> str)
        {
            if (!Utf8Parser.TryParse(str, out int value, out int _))
            {
                throw new InvalidOperationException();
            }

            return value;
        }

        public static float ParseFloat(ReadOnlySpan<char> str)
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

        public static float ParseFloat(ReadOnlySpan<byte> str)
        {
            if (!Utf8Parser.TryParse(str, out float value, out int _))
            {
                throw new InvalidOperationException();
            }

            return value;
        }
    }
}
