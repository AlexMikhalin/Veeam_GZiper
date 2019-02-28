using System;
using System.Collections.Generic;
using System.Threading;

namespace Veeam_GZiper
{
    class ThreadManager
    {
        private const ushort MaxCpuValue = 60;
        private const ushort BlocksForThread = 10;
        private const ushort SleepTime = 1000;

        public static ushort OrderId { get; private set; }

        private static readonly Queue<BlockOfFile> BlockQueue = new Queue<BlockOfFile>();
        private static readonly Stack<Compresser> Archivers = new Stack<Compresser>();

        /// <summary>
        /// Start compressing
        /// </summary>
        /// <param name="isCompress">Check for compressing state</param>
        public static void Start(bool isCompress)
        {
            try
            {
                var MaxThreads = Environment.ProcessorCount;
                OrderId = 0;
                AddArchiver(isCompress);
                while (OrderId != GZiper.BlocksCount)
                {
                    if (Archivers.Count * BlocksForThread < BlockQueue.Count 
                        && Archivers.Count < MaxThreads 
                        && Monitoring.Cpu < MaxCpuValue)
                    {
                        AddArchiver(isCompress);
                    }
                    else
                    {
                        RemoveArchiver();
                    }
                    Thread.Sleep(SleepTime);
                }

                WaitThreads();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown error has been occured!");
                Console.WriteLine(e.Message);
                GZiper.Cancel();
            }
        }


        private static void WaitThreads()
        {
            while (Archivers.Count > 0)
            {
                if (Archivers.Peek().Done)
                {
                    Archivers.Pop();
                }
            }
        }


        public static int GetBlockCount()
        {
            return BlockQueue.Count;
        }

        private static void AddArchiver(bool isCompress)
        {
            var archiver = new Compresser();
            Archivers.Push(archiver);
            if (isCompress)
            {
                new Thread(delegate () { Archivers.Peek().Start(true); }).Start();
            }
            else
            {
                new Thread(delegate () { Archivers.Peek().Start(false); }).Start();
            }
        }

        private static void RemoveArchiver()
        {
            if (Archivers.Count < 2)
            {
                return;
            }
            var archiver = Archivers.Pop();
            archiver.Interrup();
        }

        public static void EnqueueBlock(BlockOfFile block)
        {
            lock (BlockQueue)
            {
                BlockQueue.Enqueue(block);
            }
        }

        public static BlockOfFile? DequeueBlock()
        {
            lock (BlockQueue)
            {
                if (BlockQueue.Count == 0)
                {
                    return null;
                }
                OrderId++;
                return BlockQueue.Dequeue();
            }
        }
    }
}
