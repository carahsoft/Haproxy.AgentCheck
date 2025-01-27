using System;
using Haproxy.AgentCheck.Metrics;
using Xunit;

namespace Haproxy.AgentCheck.Tests
{
    public class ProcStatTests
    {
        [Fact]
        public void ProcStatParsing()
        {
            string sample = "cpu  23294 0 8415 11690959 1078 0 923 0 0 0";
            var stat = ProcStat.FromLine(sample);

            Assert.Equal(23294, stat.User);
            Assert.Equal(0, stat.Nice);
            Assert.Equal(8415, stat.System);
            Assert.Equal(11690959, stat.Idle);
            Assert.Equal(1078, stat.Iowait);
            Assert.Equal(0, stat.Irq);
            Assert.Equal(923, stat.Softirq);
        }

        [Fact]
        public void ProcStatLongParsing()
        {
            string sample = "cpu  655913008 114228 308101923 8419930804 43200740 0 11210587 0 0 0";
            var stat = ProcStat.FromLine(sample);

            Assert.Equal(655913008, stat.User);
            Assert.Equal(114228, stat.Nice);
            Assert.Equal(308101923, stat.System);
            Assert.Equal(8419930804, stat.Idle);
            Assert.Equal(43200740, stat.Iowait);
            Assert.Equal(0, stat.Irq);
            Assert.Equal(11210587, stat.Softirq);
        }

        [Fact]
        public void ManPageExampleParsing()
        {
            string sample = "cpu 10132153 290696 3084719 46828483 16683 0 25195 0 175628 0";
            var stat = ProcStat.FromLine(sample);
            Assert.Equal(10132153, stat.User);
            Assert.Equal(25195, stat.Softirq);
        }

        [Fact]
        public void Average()
        {
            // real data extracted to validate the method
            // get data : head -n 1 /proc/stat
            // cpu stress on my 16 cores : stress -c 4
            // compared with htop

            string first = "cpu  41389 68 12438 19810438 1167 0 1230 0 0 0";
            string second = "cpu  44991 68 12448 19821179 1168 0 1276 0 0 0";

            var cpuUsage = ProcStat.FromLine(first).AverageCpuWith(ProcStat.FromLine(second));

            Assert.Equal(26, cpuUsage);
        }

        [Fact]
        public void ThrowOnInvalidData()
        {
            Assert.Throws<ArgumentNullException>(() => { ProcStat.FromLine(null); });

            Assert.Throws<ArgumentException>(() => { ProcStat.FromLine(string.Empty); });
            Assert.Throws<ArgumentException>(() => { ProcStat.FromLine(" "); });
            Assert.Throws<ArgumentException>(() => { ProcStat.FromLine("cpu      "); });

            Assert.Throws<FormatException>(() => { ProcStat.FromLine("cpu invalid data"); });
        }
    }
}
