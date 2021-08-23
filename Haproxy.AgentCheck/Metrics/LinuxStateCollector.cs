using System;
using System.IO;
using Haproxy.AgentCheck.Config;
using Microsoft.Extensions.Options;

namespace Haproxy.AgentCheck.Metrics
{
    public sealed class LinuxStateCollector : IStateCollector
    {
        private readonly State _state;
        private readonly SimpleMovingAverage _movingAverage;
        private ProcStat _lastStat = ProcStat.Empty;

        public LinuxStateCollector(IOptionsMonitor<AgentCheckConfig> options, State state)
        {
            _state = state;

            if (options == null)
                throw new ArgumentNullException(nameof(options), "Must be not null");

            _movingAverage = new SimpleMovingAverage(options.CurrentValue.MovingAverageSamples);
        }

        public void Collect()
        {
            using var reader = new StreamReader("/proc/stat");
            var stat = ProcStat.FromLine(reader.ReadLine());

            if (_lastStat != ProcStat.Empty)
                _state.CpuPercent = (int)_movingAverage.Update(_lastStat.AverageCpuWith(stat));

            _lastStat = stat;
        }

        public void Dispose()
        {
            // nothing to dispose
        }
    }

    internal class ProcStat
    {
        private readonly long[] _stats;

        private ProcStat(long[] stats)
        {
            _stats = stats;
        }

        internal static ProcStat FromLine(string? firstProcStatLine)
        {
            if (firstProcStatLine == null)
                throw new ArgumentNullException(nameof(firstProcStatLine), "/proc/stat is returning invalid data");

            if (!firstProcStatLine.StartsWith("cpu ", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("/proc/stat is returning invalid first line", nameof(firstProcStatLine));

            var span = firstProcStatLine.AsSpan();
            span = span.Slice(3).TrimStart(' '); // "cpu"
            var stats = new long[7];

            for (int i = 0; i < 7; i++)
            {
                var nextSpace = span.IndexOf(' ');
                if (nextSpace == -1)
                    throw new ArgumentException("/proc/stat structure is invalid", nameof(firstProcStatLine));
                stats[i] = long.Parse(span.Slice(0, nextSpace));
                span = span.Slice(nextSpace + 1);
            }

            return new ProcStat(stats);
        }

        internal int AverageCpuWith(ProcStat with) => 100 - (int)Math.Floor((Idle - with.Idle) * 100 / (double)(Total - with.Total));

        public static ProcStat Empty { get; } = new ProcStat(new long[7]);

        /// <summary>
        /// normal processes executing in user mode
        /// </summary>
        public long User => _stats[0];
        /// <summary>
        /// niced processes executing in user mode
        /// </summary>
        public long Nice => _stats[1];
        /// <summary>
        /// processes executing in kernel mode
        /// </summary>
        public long System => _stats[2];
        /// <summary>
        /// twiddling thumbs
        /// </summary>
        public long Idle => _stats[3];
        /// <summary>
        /// waiting for I/O to complete
        /// </summary>
        public long Iowait => _stats[4];
        /// <summary>
        /// servicing interrupts
        /// </summary>
        public long Irq => _stats[5];
        /// <summary>
        /// servicing softirqs
        /// </summary>
        public long Softirq => _stats[6];

        private long Total =>
            _stats[0] +
            _stats[1] +
            _stats[2] +
            _stats[3] +
            _stats[4] +
            _stats[5] +
            _stats[6];
    }
}
