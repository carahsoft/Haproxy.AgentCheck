using System;
using System.Diagnostics;
using Haproxy.AgentCheck.Config;
using Microsoft.Extensions.Options;

namespace Haproxy.AgentCheck.Metrics
{
    public sealed class WindowsStateCollector : IStateCollector
    {
        private readonly State _state;
        private readonly SimpleMovingAverage _movingAverage;
        private readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private readonly PerformanceCounter _iisRequests = new PerformanceCounter(@"ASP.NET", "requests current", true);

        public WindowsStateCollector(IOptionsMonitor<AgentCheckConfig> options, State state)
        {
            _state = state;

            if(options == null)
                throw new ArgumentNullException(nameof(options), "Must be not null");

            _movingAverage = new SimpleMovingAverage(options.CurrentValue.MovingAverageSamples);
        }

        public void Collect()
        {
            _state.CpuPercent = (int)_movingAverage.Update((int)_cpuCounter.NextValue());
            _state.IisRequests = (int)_iisRequests.NextValue();
        }

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _iisRequests?.Dispose();
        }
    }
}
