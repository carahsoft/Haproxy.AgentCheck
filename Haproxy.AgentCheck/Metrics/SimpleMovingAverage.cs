using System;

namespace Haproxy.AgentCheck.Metrics
{
    public class SimpleMovingAverage
    {
        private readonly int _k;
        private readonly int[] _values;

        private int _index = 0;
        private int _sum = 0;

        public SimpleMovingAverage(int k)
        {
            if (k <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "Must be greater than 0");

            _k = k;
            _values = new int[k];
        }

        public double Update(int nextInput)
        {
            // calculate the new sum
            _sum = _sum - _values[_index] + nextInput;

            // overwrite the old value with the new one
            _values[_index] = nextInput;

            // increment the index (wrapping back to 0)
            _index = (_index + 1) % _k;

            // calculate the average
            return ((double)_sum) / _k;
        }
    }
}
