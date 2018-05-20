using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigFileSorter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var inputFilename = "generated.txt";
                var outputFilename = "sorted.txt";

                using (var inputFile = MemoryMappedFile.CreateFromFile(inputFilename, FileMode.Open))
                using (var inputStream = inputFile.CreateViewStream())
                using (var reader = new StreamReader(inputStream))
                {
                    var buffer = new char[1024 * 1024];
                    reader.ReadBlock(buffer, 0, 1024 * 1024);
                    Console.WriteLine("Reading data...");
                    var data = new List<CLine>(10000000);
                    String readData;

                    while (true)
                    {
                        readData = reader.ReadLine();
                        if (String.IsNullOrWhiteSpace(readData) || readData.Contains('\0'))
                            break;

                        data.Add(CLine.Parse(readData));
                    }

                    Console.WriteLine("Reading is complete. Sorting data...");

                    data.Sort();

                    Console.WriteLine("Sorting is complete. Writing data...");

                    using (var sortedFile = File.Create("sorted.txt"))
                    using (var writer = new StreamWriter(sortedFile))
                    {
                        foreach (var line in data)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }

                Console.WriteLine("Data successfully writed. Press Escape to quit.");
                while (Console.ReadKey().Key != ConsoleKey.Escape)
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
