using System.Diagnostics;

namespace Veeam_GZiper
{
    class Monitoring
    {
        private const ushort CpuUpdateTime = 1000;
        private const ushort RamUpdateTime = 500;

        private static readonly PerformanceCounter RamCounter =
            new PerformanceCounter("Memory", "Available MBytes");

        private static readonly PerformanceCounter CpuCounter =
            new PerformanceCounter("Processor Information", "% Processor Time", "_Total");

        private static readonly Stopwatch CpuTimer = new Stopwatch();
        private static readonly Stopwatch RamTimer = new Stopwatch();

        private static float _cpu;

        /// <summary>
        /// Get the CPU information
        /// </summary>
        public static float Cpu
        {
            get
            {
                if (!CpuTimer.IsRunning)
                {
                    _cpu = CpuCounter.NextValue();
                    CpuTimer.Start();
                }

                if (CpuTimer.ElapsedMilliseconds <= CpuUpdateTime)
                {
                    return _cpu;
                }
                _cpu = CpuCounter.NextValue();
                CpuTimer.Restart();
                return _cpu;
            }
        }

        private static float _ram;

        /// <summary>
        /// Get the RAM information
        /// </summary>
        public static float Ram
        {
            get
            {
                if (!RamTimer.IsRunning)
                {
                    _ram = RamCounter.NextValue();
                    RamTimer.Start();
                }

                if (RamTimer.ElapsedMilliseconds <= RamUpdateTime)
                {
                    return _ram;
                }
                _ram = RamCounter.NextValue();
                RamTimer.Restart();
                return _ram;
            }
        }
    }
}
