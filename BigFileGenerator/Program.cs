using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace BigFileGenerator
{
    // ReSharper disable once InconsistentNaming
    class Program
    {
        private static readonly CBlockingQueue<String> s_fileWriteQueue = new CBlockingQueue<String>();
        private static readonly Int64 _bytesPerGigabyte = 1024L * 1024L * 1024L;

        private static UInt32 s_filesize;
        private static String s_filename = "generated.txt";
        private static Timer s_timer;

        static void Main(String[] args)
        {
            if (!CheckArgs(args))
                return;

            Console.WriteLine("Prepare for generating...");

            var stopwatch = new Stopwatch();
            StartGeneration(stopwatch);

            Console.WriteLine(
                $"File '{Path.GetFullPath(s_filename)}' is generated" + Environment.NewLine +
                $"Generation total time, s: {stopwatch.Elapsed.TotalSeconds}");
        }

        private static bool CheckArgs(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("USAGE: BigFileGenerator.exe <size> [filename]");
                Console.WriteLine("\t<size> - desired size of generating file, GB, uint32.");
                Console.WriteLine(
                    "\t[filename] - name of generating file. Default 'CURRENTDIR\\generated.txt', if not specified.");
                return false;
            }

            Console.WriteLine("Checking program parameters...");

            if (!UInt32.TryParse(args[0], out s_filesize))
            {
                Console.WriteLine($"Wrong size format: {args[0]}");
                return false;
            }

            if (args.Length > 1)
            {
                s_filename = args[1];
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(s_filename));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

            return true;
        }

        private static void StartGeneration(Stopwatch stopwatch)
        {
            using (var generator = CDataGenerator.CreateGenerator(s_fileWriteQueue))
            using (var cts = new CancellationTokenSource())
            {
                Thread t1 = new Thread(_ => generator.GenerateStringsThreadSafe(cts.Token));
                Thread t2 = new Thread(_ => generator.GenerateStringsThreadSafe(cts.Token));
                Thread t3 = new Thread(_ => generator.GenerateStringsThreadSafe(cts.Token));
                Thread t4 = new Thread(_ => generator.GenerateStringsThreadSafe(cts.Token));

                using (var bufferedFileStream = new BufferedStream(File.Create(s_filename), 4096 * 1024))
                using (var generatedFileWriter = new StreamWriter(bufferedFileStream))
                {
                    Console.WriteLine("Generating...");
                    s_timer = new Timer(_ => CheckNeedToStop(cts), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                    stopwatch.Start();

                    t1.Start();
                    t2.Start();
                    t3.Start();
                    t4.Start();

                    while (!cts.IsCancellationRequested)
                    {
                        var nextBlockToWrite = s_fileWriteQueue.Dequeue();
                        generatedFileWriter.Write(nextBlockToWrite);
                    }

                    stopwatch.Stop();
                    s_fileWriteQueue.Clear();

                    t1.Join();
                    t2.Join();
                    t3.Join();
                    t4.Join();
                }
            }
        }

        private static void CheckNeedToStop(CancellationTokenSource cts)
        {
            var fileInfo = new FileInfo(s_filename);
            
            if (fileInfo.Length >= _bytesPerGigabyte * s_filesize)
            {
                Console.WriteLine("Cancel generating...");
                cts.Cancel();
                s_timer.Dispose();
                return;
            }

            Console.Write(".");
        }
    }
}
