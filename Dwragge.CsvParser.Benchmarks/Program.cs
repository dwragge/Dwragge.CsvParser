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
           // ,100000
        )]
        public int NumRows;

        public CsvMapper<HalfNumbers> mapping = new HalfNumbersTestMapper();
        public Utf8CsvMapping1<HalfNumbers> utfMapping = new Utf8HalfNumbersMapping();

        public CsvMapper<AllNumbersTestClass> allNumbersMapper = new AllNumbersTestMapper();
        public Utf8CsvMapping1<AllNumbersTestClass> allNumbersUtfMapper = new AllNumbersTestMapperUtf8();
        

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random();
            int numHeaders = 10;
            var builder = new StringBuilder();

            //GenerateHeader(builder, numHeaders);
            GenerateHalfNumbersData(builder, random);

            //GenerateAllNumbersData(builder, random);

            Data = builder.ToString();
            DataBytes = Encoding.UTF8.GetBytes(Data);
        }

        private void GenerateHeader(StringBuilder builder, int numHeaders)
        {
            for (int i = 0; i < numHeaders; i++)
            {
                builder.Append((char)(97 + i));
                if (i != numHeaders - 1) builder.Append(",");
            }
            builder.Append("\r\n");
        }

        private void GenerateHalfNumbersData(StringBuilder builder, Random random)
        {
   
            int numHeaders = 10; // Fixed by class HalfNumbers
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < numHeaders; j++)
                {
                    if (j % 2 == 0)
                    {
                        int wordLength = random.Next(5, 30);
                        for (int k = 0; k < wordLength; k++)
                        {
                            builder.Append((char)random.Next(48, 122));
                        }
                    }
                    else
                    {
                        int numLength = random.Next(3, 10);
                        for (int k = 0; k < numLength; k++)
                        {
                            builder.Append((char)random.Next(48, 57));
                        }
                    }

                    if (j != numHeaders - 1) builder.Append(",");
                }
                builder.Append("\r\n");
            }
        }

        private void GenerateAllNumbersData(StringBuilder builder, Random random)
        {
            int numHeaders = 10; // Fixed By Class
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < numHeaders; j++)
                {
                    int numLength = random.Next(5, 20);
                    if (j % 2 == 0)
                    {
                        int decimalPointLocation = random.Next(numLength - 1);
                        for (int k = 0; k < decimalPointLocation; k++)
                        {
                            builder.Append((char)random.Next(48, 57));
                        }

                        builder.Append('.');

                        for (int k = 0; k < numLength - decimalPointLocation; k++)
                        {
                            builder.Append((char) random.Next(48, 57));
                        }
                    }
                    else
                    {
                        for (int k = 0; k < numLength; k++)
                        {
                            builder.Append((char)random.Next(48, 57));
                        }
                    }

                    if (j != numHeaders - 1) builder.Append(",");
                }
                builder.Append("\r\n");
            }
        }

        //[Benchmark]
        public List<HalfNumbers> CsvReader_MapDynamicIL()
        {
            var reader = CsvReader<HalfNumbers>.CreateFromString(Data, mapping);
            var list = new List<HalfNumbers>();
            while (reader.Read())
            {
                var record = reader.GetRecord();
                list.Add(record);
            }

            return list;
        }

        //[Benchmark]
        public List<HalfNumbers> CsvReader_MapDynamicIL_Utf8()
        {
            var reader = new FileCsvReader<HalfNumbers>(utfMapping);
            using (var stream = new MemoryStream(DataBytes))
            {
                var list = reader.ReadAll(stream);
                return list;
            }
        }

        //[Benchmark]
        public List<AllNumbersTestClass> CsvReader_AllNumbers_MapDynamicIL()
        {
            var reader = CsvReader<AllNumbersTestClass>.CreateFromString(Data, allNumbersMapper);
            var list = new List<AllNumbersTestClass>();
            while (reader.Read())
            {
                var record = reader.GetRecord();
                list.Add(record);
            }

            return list;
        }

        //[Benchmark]
        public List<AllNumbersTestClass> CsvReader_AllNumbers_MapDynamicIL_Utf8()
        {
            var reader = Utf8CsvReader<AllNumbersTestClass>.CreateFromBytes(DataBytes, allNumbersUtfMapper);
            var list = new List<AllNumbersTestClass>();
            while (reader.Read())
            {
                var record = reader.GetCurrentRecordDynamic();
                list.Add(record);
            }

            return list;
        }

        //[Benchmark]
        public List<HalfNumbers> CsvHelperReadAll()
        {
            var reader = new CsvHelper.CsvReader(new StringReader(Data), new Configuration() {HeaderValidated = null});
            while (reader.Read())
            {
                var record = new HalfNumbers();
                record.A = reader.GetField<string>(0);
                record.C = reader.GetField<string>(2);
                record.E = reader.GetField<string>(4);
                record.G = reader.GetField<string>(6);
                record.I = reader.GetField<string>(8);

                record.B = reader.GetField<int>(1);
                record.D = reader.GetField<int>(3);
                record.F = reader.GetField<int>(5);
                record.H = reader.GetField<int>(7);
                record.J = reader.GetField<int>(9);
            }
            return reader.GetRecords<HalfNumbers>().ToList();
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

        public class HalfNumbersTestMapper : CsvMapper<HalfNumbers>
        {
            public HalfNumbersTestMapper()
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
        public List<CsvMappingResult<HalfNumbers>> TinyCsvParserReadAll()
        {
            var parser = new CsvParser<HalfNumbers>(new CsvParserOptions(true, ','), new HalfNumbersTestMapping());
            return parser.ReadFromString(new CsvReaderOptions(new[] {"\r\n"}), Data).ToList();
        }

        //[Benchmark]
        public List<CsvMappingResult<AllNumbersTestClass>> TinyCsvParserReadAllNumbers()
        {
            var parser = new CsvParser<AllNumbersTestClass>(new CsvParserOptions(true, ','), new AllNumbersTestMapping());
            return parser.ReadFromString(new CsvReaderOptions(new[] { "\r\n" }), Data).ToList();
        }

        [Benchmark]
        public List<HalfNumbers> CsvParser_ParseFile()
        {
            using (var stream = File.OpenRead(@"C:\Users\dylan\Desktop\csvtest_100000.csv"))
            {
                var parser = new FileCsvReader<HalfNumbers>(utfMapping);
                var list = parser.ReadAll(stream);
                return list;
            }
            
        }

        [Benchmark]
        public List<CsvMappingResult<HalfNumbers>> TinyCsvParser_ReadFromFile()
        {
            var parser = new CsvParser<HalfNumbers>(new CsvParserOptions(true, ','), new HalfNumbersTestMapping());
            return parser.ReadFromFile(@"C:\Users\dylan\Desktop\csvtest_100000.csv", Encoding.UTF8).ToList();
        }

        [Benchmark]
        public List<HalfNumbers> CsvHelper_ReadFromFile()
        {
            var reader = new CsvHelper.CsvReader(File.OpenText(@"C:\Users\dylan\Desktop\csvtest_headers_100000.csv"));
            var records = reader.GetRecords<HalfNumbers>().ToList();
            return records;
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
            var list = runner.CsvParser_ParseFile();
            Console.WriteLine(list.Count);
            Console.ReadLine();
#else

            var summary = BenchmarkRunner.Run<CsvBenchmarks>();
            //var summary = BenchmarkRunner.Run<ILGenBenchmarks>();    
#endif
        }
    }
}
