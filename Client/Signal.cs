using System;

namespace Client
{
    public class Signal
    {
        private readonly double _magnitude;
        private readonly double _frequency;
        private readonly double _phase;
        private readonly double _noise;
        private readonly double _dc;
        private int _index;
        readonly Random _random = new Random();
        private double _totalNoise;

        public Signal(double magnitude, double frequency, double phase, double noise, double dc)
        {
            _magnitude = magnitude;
            _frequency = frequency;
            _phase = phase;
            _noise = noise;
            _dc = dc;
        }

        public double NextValue()
        {
            _totalNoise += (_random.NextDouble() - 0.5) * _noise;
            var value = _magnitude * Math.Sin(_frequency * _index + _phase) + _dc + _totalNoise;
            _index++;
            return value;
        }
    }
}
