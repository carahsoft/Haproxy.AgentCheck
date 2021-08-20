using Haproxy.AgentCheck.Config;
using Haproxy.AgentCheck.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Haproxy.AgentCheck.Tests
{
    public class CollectorTests
    {
        [Fact]
        public void CollectTest()
        {
            ServiceCollection sc = new ServiceCollection();

            sc.AddSingleton<IOptionsMonitor<AgentCheckConfig>, TestOptionsMonitor<AgentCheckConfig>>(
                provider => new TestOptionsMonitor<AgentCheckConfig>(new AgentCheckConfig
                {
                    MovingAverageSamples = 1
                }));

            sc.AddMetricCollector();
            var collector = sc.BuildServiceProvider().GetRequiredService<IStateCollector>();

            collector.Collect();
        }
    }
}
