using System;
using System.IO;
using System.Threading;

namespace Veeam_GZiper
{
    class GZiper
    {
        private const ushort UpdateProgressTime = 100;
        private const ushort MinFileLength = 10;
        private const uint Mega = 1024 * 1024;
        private const uint MaxPercentageOfFileSize = 110;

        public static ushort BlocksCount { get; private set; }
        private const string ArchiveExtension = ".gzz";


        /// <summary>
        /// Compress current file
        /// </summary>
        /// <param name="inFile">Input file</param>
        /// <param name="outFile">Output file</param>
        public static void Compress(string inFile, string outFile)
        {
            ValidateOperation(inFile, outFile, true);
            Console.WriteLine("Compressing successfully started!");

            using (var writeStream = new FileStream(outFile, FileMode.Append))
            {
                Output.WriteKey(writeStream);
                Output.WriteBlocksCount(writeStream, BlocksCount);
                var threadWriter = new Thread(delegate () { Output.Write(writeStream, true); });
                using (var readStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
                {
                    var reader = new Input(Mega);
                    var threadReader = new Thread(delegate () { reader.Read(readStream); });
                    threadReader.Start();
                    var archiveManagerThread = new Thread(delegate () { ThreadManager.Start(true); });
                    archiveManagerThread.Start();
                    threadWriter.Start();
                    while (threadReader.IsAlive)
                    {
                        WriteProgress();
                    }
                }

                while (threadWriter.IsAlive)
                {
                    WriteProgress();
                }
            }

            Console.WriteLine("Done!");
        }

        /// <summary>
        /// Decompress current file
        /// </summary>
        /// <param name="inFile">Input File</param>
        /// <param name="outFile">Output File</param>
        public static void Decompress(string inFile, string outFile)
        {
            ValidateOperation(inFile, outFile, false);
            Console.WriteLine("Decompressing successfully started!");

            using (var writeStream = new FileStream(outFile, FileMode.Append))
            {
                var threadWriter = new Thread(delegate () { Output.Write(writeStream, false); });
                using (var readStream = new FileStream(inFile, FileMode.Open))
                {
                    var reader = new Input();
                    BlocksCount = Input.ReadBlocksCount(readStream);
                    var threadReader = new Thread(delegate () { reader.Read(readStream); });
                    threadReader.Start();
                    var archiveManagerThread = new Thread(delegate () { ThreadManager.Start(false); });
                    archiveManagerThread.Start();
                    threadWriter.Start();
                    while (threadReader.IsAlive)
                    {
                        WriteProgress();
                    }
                }

                while (threadWriter.IsAlive)
                {
                    WriteProgress();
                }
            }

            Console.WriteLine("Done!");
        }

        /// <summary>
        /// Writes the progress of compressing or decompressing file
        /// </summary>
        private static void WriteProgress()
        {
            Thread.Sleep(UpdateProgressTime);
            Console.Clear();
            Console.CursorVisible = false;

            Console.WriteLine("{0}% reading done          ",
                Math.Round((double)100 * Input.OrderId / BlocksCount, 2));
            Console.WriteLine("{0}% compressing done      ",
                Math.Round((double)100 * ThreadManager.OrderId / BlocksCount, 2));
            Console.WriteLine("{0}% writing done          ",
                Math.Round((double)100 * Output.OrderId / BlocksCount, 2));
            Console.WriteLine("{0}% total done            ",
                Math.Round(
                    (double)100 * (ThreadManager.OrderId + Output.OrderId + Input.OrderId) / (3 * BlocksCount),
                    2));
        }

        /// <summary>
        /// Do validation for current operation
        /// </summary>
        /// <param name="inFile">Input file</param>
        /// <param name="outFile">Output file</param>
        /// <param name="isCompress">Check for compress state</param>
        private static void ValidateOperation(string inFile, string outFile, bool isCompress)
        {
            if (!File.Exists(inFile))
            {
                throw new FileNotFoundException("Incoming file not found!");
            }

            var fileInfo = new FileInfo(inFile);
            if (fileInfo.Extension.Equals(ArchiveExtension) && isCompress)
            {
                throw new ArgumentException("Incoming file already compressed!");
            }

            if (!fileInfo.Extension.Equals(ArchiveExtension) && !isCompress)
            {
                throw new ArgumentException("Incoming file was not compressed!");
            }

            var currentDrive = new DriveInfo(Path.GetPathRoot(fileInfo.FullName));
            if (currentDrive.AvailableFreeSpace < fileInfo.Length / 100 * MaxPercentageOfFileSize)
            {
                throw new Exception("Not enough space for writing file.");
            }

            if (isCompress)
            {
                BlocksCount = (ushort)Math.Ceiling((double)fileInfo.Length / (Mega));
            }

            if (outFile.Equals(""))
            {
                throw new ArgumentException("Wrong outgoing file name!");
            }
            if (File.Exists(outFile))
            {
                throw new FileNotFoundException("Outgoing file already exists!");
            }

            fileInfo = new FileInfo(outFile);
            if (!fileInfo.Extension.Equals(ArchiveExtension) && isCompress)
            {
                throw new ArgumentException("Wrong outgoing file extension! Use \"" + ArchiveExtension + "\" .");
            }

            if (fileInfo.Extension.Equals(ArchiveExtension) && !isCompress)
            {
                throw new ArgumentException("Wrong outgoing file extension!");
            }

            using (var readStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                if (!isCompress && (readStream.Length <= MinFileLength || !Input.ReadAndCheckKey(readStream)))
                {
                    throw new ArgumentException("Wrong incoming file format!");
                }
            }

            using (var writeStream = new FileStream(outFile, FileMode.CreateNew))
            {
            }
        }

        /// <summary>
        /// Cancel process of compressing file
        /// </summary>
        public static void Cancel()
        {
            Console.WriteLine("Program was canceled.");
            Environment.Exit(0);
        }
    }
}
