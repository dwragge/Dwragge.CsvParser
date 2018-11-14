# Dwragge.CsvParser

This is a small toy program I wrote to test out what the new Memory<T>, Span<T> and Pipelines APIs could do in .NET Core 2.1
It also uses Reflection.Emit to have the fastest type mapping possible, which was fun to implement.

For reading a file from disk into memory, and mapping it to a C# object: the benchmarks are below

``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.345 (1803/April2018Update/Redstone4)
Intel Core i5-6400 CPU 2.70GHz (Skylake), 1 CPU, 4 logical and 4 physical cores
.NET Core SDK=2.1.400
  [Host] : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
  Core   : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT

Job=Core  Runtime=Core  

```
|                     Method | NumRows |     Mean |     Error |    StdDev |      Gen 0 |      Gen 1 |     Gen 2 |    Allocated |
|--------------------------- |-------- |---------:|----------:|----------:|-----------:|-----------:|----------:|-------------:|
|        CsvParser_ParseFile |  100000 | 319.7 ms |  6.278 ms |  8.381 ms | 11000.0000 |  4000.0000 |         - |       1.7 KB |
| TinyCsvParser_ReadFromFile |  100000 | 414.2 ms |  7.767 ms |  7.265 ms | 53000.0000 | 14000.0000 | 1000.0000 |  80444.99 KB |
|     CsvHelper_ReadFromFile |  100000 | 809.0 ms | 15.836 ms | 15.553 ms | 44000.0000 | 11000.0000 | 1000.0000 | 266980.64 KB |

I think the allocated table is wrong, in other tests it was only 100-1000x less memory allocated. However, it is 25 - 60 percent faster than other libraries.

