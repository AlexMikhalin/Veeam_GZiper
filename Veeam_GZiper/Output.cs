using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Veeam_GZiper
{
    class Output
    {
        private const ushort BlockSizeLength = 3;
        private const ushort SleepTime = 500;

        private static bool _started;
        private static readonly List<BlockOfFile> Blocks;

        public static ushort OrderId { get; private set; }

        static Output()
        {
            Blocks = new List<BlockOfFile>();
            OrderId = 0;
        }

        public static void WriteKey(FileStream writeStream)
        {
            byte[] key = { 0, 0, 0, 0 };
            writeStream.Write(key, 0, key.Length);
        }

        public static void WriteBlocksCount(FileStream writeStream, ushort blocksCount)
        {
            var count = BitConverter.GetBytes(blocksCount);
            writeStream.Write(count, 0, 2);
        }

        public static void Write(FileStream writeStream, bool compress)
        {
            try
            {
                if (_started)
                {
                    return;
                }
                _started = true;

                while (OrderId != GZiper.BlocksCount)
                {
                    var currentBlock = GetBlock(OrderId);
                    if (currentBlock == null)
                    {
                        Thread.Sleep(SleepTime);
                        continue;
                    }

                    WriteBlock(compress, writeStream, currentBlock.Value);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown error has been occured!");
                Console.WriteLine(e.Message);
                GZiper.Cancel();
            }

            _started = false;
        }

        private static void WriteBlock(bool isCompress, Stream writeStream, BlockOfFile currentBlock)
        {
            if (isCompress)
            {
                WriteBlockSize(writeStream, currentBlock.Bytes.Length);
            }
            writeStream.Write(currentBlock.Bytes, 0, currentBlock.Bytes.Length);
            OrderId++;
        }

        private static void WriteBlockSize(Stream writeStream, int size)
        {
            writeStream.Write(BitConverter.GetBytes(size), 0, BlockSizeLength);
        }

        public static void AddBlock(BlockOfFile block)
        {
            lock (Blocks)
            {
                Blocks.Add(block);
            }
        }

        private static BlockOfFile? GetBlock(ushort number)
        {
            lock (Blocks)
            {
                for (var i = 0; i < Blocks.Count; i++)
                    if (Blocks[i].Number == number)
                    {
                        var b = Blocks[i];
                        Blocks.RemoveAt(i);
                        return b;
                    }

                return null;
            }
        }
    }
}
