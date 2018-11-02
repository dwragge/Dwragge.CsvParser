using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;
using CsvHelper.Configuration;
using CSharpx;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace Dwragge.CsvParser.Benchmarks
{
    [CoreJob]
    [MemoryDiagnoser]
    public class CsvBenchmarks
    {
        public string Data;
        public byte[] DataBytes;

        [Params(10000
            //,100000
        )]
        public int NumRows;

        public CsvMapping1<HalfNumbers> mapping = new HalfNumbersTestMapping1();
        public Utf8CsvMapping1<HalfNumbers> utfMapping = new Utf8HalfNumbersMapping();
        public byte[] _numberBytes;
        public char[] _numberString = "12345678".ToCharArray();

        [GlobalSetup]
        public void Setup()
        {
            _numberBytes  = Encoding.UTF8.GetBytes("12345678");
            var random = new Random();
            int numHeaders = 10;
            var builder = new StringBuilder();

            for (int i = 0; i < numHeaders; i++)
            {
                //builder.Append((char)(97 + i));
                //if (i != numHeaders - 1) builder.Append(",");
            }

            //builder.Append("\r\n");

            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < numHeaders; j++)
                {
                    if (j % 2 == 0)
                    {
                        int wordLength = random.Next(5, 30);
                        for (int k = 0; k < wordLength; k++)
                        {
                            builder.Append((char) random.Next(48, 122));
                        }
                    }
                    else
                    {
                        int numLength = random.Next(3, 10);
                        for (int k = 0; k < numLength; k++)
                        {
                            builder.Append((char) random.Next(48, 57));
                        }
                    }
                    
                    if (j != numHeaders - 1) builder.Append(",");
                }
                builder.Append("\r\n");
            }

            Data = builder.ToString();
            DataBytes = Encoding.UTF8.GetBytes(Data);
        }

        //[Benchmark]
        public int CsvParserActionEachRecord()
        {
            var reader = CsvReader<HalfNumbers>.CreateFromString(Data, mapping);
            int totalLength = 0;
            while (reader.Read())
            {
                var record = reader.GetHalfNumbersRecord();
                totalLength += record.B;
                totalLength += record.D;
                totalLength += record.F;
                totalLength += record.H;
                totalLength += record.J;
            }

            return totalLength;
            //return reader.ReadToEnd();
        }

        //[Benchmark]
        public int CsvParserActionEachRecord_DynamicIL()
        {
            var reader = CsvReader<HalfNumbers>.CreateFromString(Data, mapping);
            int totalLength = 0;
        
            while (reader.Read())
            {
                var record = new HalfNumbers();
                reader.GetCurrentRecordDynamic(record);
                totalLength += record.B;
                totalLength += record.D;
                totalLength += record.F;
                totalLength += record.H;
                totalLength += record.J;
            }

            return totalLength;
            //return reader.ReadToEnd();
        }

        //[Benchmark]
        public int CsvParserSumNoMaterialize()
        {
            var reader = CsvReader<HalfNumbers>.CreateFromString(Data, mapping);
            int totalLength = 0;
            while (reader.Read())
            {
                totalLength += reader.GetIntColumn(1);
                totalLength += reader.GetIntColumn(3);
                totalLength += reader.GetIntColumn(5);
                totalLength += reader.GetIntColumn(7);
                totalLength += reader.GetIntColumn(9);
            }

            return totalLength;
            //return reader.ReadToEnd();
        }

        //[Benchmark]
        public List<HalfNumbers> CsvParserReadAll()
        {
            var reader = CsvReader<HalfNumbers>.CreateFromString(Data, mapping);
            int totalLength = 0;
            List<HalfNumbers> records = new List<HalfNumbers>();
            while (reader.Read())
            {
                var record = reader.GetHalfNumbersRecord();
                records.Add(record);
            }

            return records;
        }

        [Benchmark]
        public List<HalfNumbers> CsvReader_MapDynamicIL()
        {
            var reader = CsvReader<HalfNumbers>.CreateFromString(Data, mapping);
            var list = new List<HalfNumbers>();
            while (reader.Read())
            {
                var record = new HalfNumbers();
                reader.GetCurrentRecordDynamic(record);
                list.Add(record);
            }

            return list;
        }

        [Benchmark]
        public List<HalfNumbers> CsvReader_MapDynamicIL_Utf8()
        {
            var reader = Utf8CsvReader<HalfNumbers>.CreateFromBytes(DataBytes, utfMapping);
            var list = new List<HalfNumbers>();
            while (reader.Read())
            {
                var record = reader.GetCurrentRecordDynamic();
                list.Add(record);
            }

            return list;
        }

        //[Benchmark]
        public int CsvHelperActionEachRecord()
        {
            var reader = new CsvHelper.CsvReader(new StringReader(Data), new Configuration() {HeaderValidated = null});
            int sum = 0;
            while (reader.Read())
            {
                sum += reader.GetField<int>(1);
                sum += reader.GetField<int>(3);
                sum += reader.GetField<int>(5);
                sum += reader.GetField<int>(7);
                sum += reader.GetField<int>(9);
            }

            return sum;
        }

        //[Benchmark]
        // have to read each field individually
        public List<HalfNumbers> CsvHelperReadAll()
        {
            var reader = new CsvHelper.CsvReader(new StringReader(Data), new Configuration() {HeaderValidated = null});
            return reader.GetRecords<HalfNumbers>().ToList();
        }

        public class TestData
        {
            public string a { get; set; }
            public string b { get; set; }
            public string c { get; set; }
            public string d { get; set; }
            public string e { get; set; }
            public string f { get; set; }
            public string g { get; set; }
            public string h { get; set; }
            public string i { get; set; }
            public string j { get; set; }
        }

        public class CsvTestMapping : CsvMapping<TestData>
        {
            public CsvTestMapping() : base()
            {
                MapProperty(0, x => x.a);
                MapProperty(1, x => x.b);
                MapProperty(2, x => x.c);
                MapProperty(3, x => x.d);
                MapProperty(4, x => x.e);
                MapProperty(5, x => x.f);
                MapProperty(6, x => x.g);
                MapProperty(7, x => x.h);
                MapProperty(8, x => x.i);
                MapProperty(9, x => x.j);
            }
        }

        public class HalfNumbersTestMapping : CsvMapping<HalfNumbers>
        {
            public HalfNumbersTestMapping()
            {
                MapProperty(0, x => x.A);
                MapProperty(1, x => x.B);
                MapProperty(2, x => x.C);
                MapProperty(3, x => x.D);
                MapProperty(4, x => x.E);
                MapProperty(5, x => x.F);
                MapProperty(6, x => x.G);
                MapProperty(7, x => x.H);
                MapProperty(8, x => x.I);
                MapProperty(9, x => x.J);
            }
        }

        public class HalfNumbersTestMapping1 : CsvMapping1<HalfNumbers>
        {
            public HalfNumbersTestMapping1()
            {
                MapProperty(0, x => x.A);
                MapProperty(1, x => x.B);
                MapProperty(2, x => x.C);
                MapProperty(3, x => x.D);
                MapProperty(4, x => x.E);
                MapProperty(5, x => x.F);
                MapProperty(6, x => x.G);
                MapProperty(7, x => x.H);
                MapProperty(8, x => x.I);
                MapProperty(9, x => x.J);
            }
        }

        public class Utf8HalfNumbersMapping : Utf8CsvMapping1<HalfNumbers>
        {
            public Utf8HalfNumbersMapping()
            {
                MapProperty(0, x => x.A);
                MapProperty(1, x => x.B);
                MapProperty(2, x => x.C);
                MapProperty(3, x => x.D);
                MapProperty(4, x => x.E);
                MapProperty(5, x => x.F);
                MapProperty(6, x => x.G);
                MapProperty(7, x => x.H);
                MapProperty(8, x => x.I);
                MapProperty(9, x => x.J);
            }
        }
        

        //[Benchmark]
        public int TinyCsvParserActionPerRow()
        {
            var parser = new CsvParser<HalfNumbers>(new CsvParserOptions(true, ','), new HalfNumbersTestMapping());
            int sum = 0;
            parser.ReadFromString(new CsvReaderOptions(new[] {"\r\n"}), Data).ForEach(x =>
            {
                sum += x.Result.B;
                sum += x.Result.D;
                sum += x.Result.F;
                sum += x.Result.H;
                sum += x.Result.J;
            });
            return sum;
        }

        //[Benchmark]
        public List<CsvMappingResult<HalfNumbers>> TinyCsvParserReadAll()
        {
            var parser = new CsvParser<HalfNumbers>(new CsvParserOptions(true, ','), new HalfNumbersTestMapping());
            return parser.ReadFromString(new CsvReaderOptions(new[] {"\r\n"}), Data).ToList();
        }
    }

    [CoreJob]
    public class ILGenBenchmarks
    {
        public class HalfNumbersTestMapping1 : CsvMapping1<HalfNumbers>
        {
            public HalfNumbersTestMapping1()
            {
                MapProperty(0, x => x.A);
                MapProperty(1, x => x.B);
                MapProperty(2, x => x.C);
                MapProperty(3, x => x.D);
                MapProperty(4, x => x.E);
                MapProperty(5, x => x.F);
                MapProperty(6, x => x.G);
                MapProperty(7, x => x.H);
                MapProperty(8, x => x.I);
                MapProperty(9, x => x.J);
            }
        }

        [Benchmark]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void CreateType()
        {
            // MEMORY LEAK??
            var mapping = new HalfNumbersTestMapping1();
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            #if DEBUG
            
                var runner = new CsvBenchmarks();
            runner.NumRows = 100000;
            runner.Setup();
            Console.ReadLine();
            List<HalfNumbers> list = new List<HalfNumbers>();
            for (int i = 0; i < 1; i++)
            {
                list.AddRange(runner.CsvReader_MapDynamicIL_Utf8());
                Console.WriteLine(list.Count);
            }
            
            
#else

                var summary = BenchmarkRunner.Run<CsvBenchmarks>();
                //var summary = BenchmarkRunner.Run<ILGenBenchmarks>();
            
        #endif
        }
    }
}
