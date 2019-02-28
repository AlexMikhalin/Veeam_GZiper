using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Veeam_GZiper
{
    class Compresser
    {
        private bool _isInterupeted;
        public bool Done { get; private set; }
        private const ushort SleepTime = 500;

        public Compresser()
        {
            _isInterupeted = false;
            Done = false;
        }

        /// <summary>
        /// Starts compressinf of file
        /// </summary>
        /// <param name="isCompress">check for compress state</param>
        public void Start(bool isCompress)
        {
            try
            {
                while (!_isInterupeted && ThreadManager.OrderId != GZiper.BlocksCount)
                {
                    var currentBlock = ThreadManager.DequeueBlock();
                    if (currentBlock == null)
                    {
                        Thread.Sleep(SleepTime);
                        continue;
                    }

                    if (isCompress)
                    {
                        CompressBlock(currentBlock.Value);                        
                    }
                    else
                    {
                        DecompressBlock(currentBlock.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown error has been occured!");
                Console.WriteLine(e.Message);
                GZiper.Cancel();
            }

            Done = true;
        }

        /// <summary>
        /// Compress block of input file
        /// </summary>
        /// <param name="block">Block of file</param>
        private static void CompressBlock(BlockOfFile block)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(block.Bytes, 0, block.Bytes.Length);
                }
                Output.AddBlock(new BlockOfFile(block.Number, memoryStream.ToArray()));
            }
        }

        /// <summary>
        /// Decompress block of input file
        /// </summary>
        /// <param name="block">Block of file</param>
        private static void DecompressBlock(BlockOfFile block)
        {
            using (var memoryStreamIn = new MemoryStream(block.Bytes))
            {
                using (var zipStream = new GZipStream(memoryStreamIn, CompressionMode.Decompress))
                {
                    using (var memoryStreamOut = new MemoryStream())
                    {
                        zipStream.CopyTo(memoryStreamOut);
                        Output.AddBlock(new BlockOfFile(block.Number, memoryStreamOut.ToArray()));
                    }
                }
            }
        }

        /// <summary>
        /// Interrupts the compressing
        /// </summary>
        public void Interrup()
        {
            _isInterupeted = true;
        }
    }
}
