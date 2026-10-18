using System;
using System.IO;
using System.Media;
using System.Text;

namespace ElevatorApp
{
    /// <summary>
    /// Plays the lift sounds. The sounds (siren, moving hum, arrival "ding") are generated
    /// in code, so no .wav files are needed. Siren has priority over the moving sound.
    /// </summary>
    public sealed class SoundService : IDisposable
    {
        private const int SampleRate = 22050;

        private readonly byte[] _siren;
        private readonly byte[] _hum;
        private readonly byte[] _ding;
        private readonly SoundPlayer _player = new SoundPlayer();

        private bool _moving;
        private bool _emergency;

        public SoundService()
        {
            _siren = BuildSiren();
            _hum = BuildHum();
            _ding = BuildDing();
        }

        public void SetMoving(bool moving)
        {
            _moving = moving;
            UpdatePlayback();
        }

        public void SetEmergency(bool emergency)
        {
            _emergency = emergency;
            UpdatePlayback();
        }

        public void PlayDing()
        {
            if (_emergency) return;
            Play(_ding, false);
        }

        public void Dispose()
        {
            try { _player.Stop(); } catch (Exception) { }
            _player.Dispose();
        }

        private void UpdatePlayback()
        {
            if (_emergency)
                Play(_siren, true);
            else if (_moving)
                Play(_hum, true);
            else
                Stop();
        }

        private void Stop()
        {
            try { _player.Stop(); } catch (Exception) { }
        }

        private void Play(byte[] wav, bool loop)
        {
            try
            {
                _player.Stop();
                _player.Stream = new MemoryStream(wav);
                if (loop) _player.PlayLooping(); else _player.Play();
            }
            catch (Exception)
            {
                // sound is optional - never crash the app because of it
            }
        }

        // ---------------- sound generation ----------------

        /// <summary>Loud two-tone wailing alarm (500 - 1400 Hz), 2 second loop.</summary>
        private static byte[] BuildSiren()
        {
            int count = SampleRate * 2;
            var data = new short[count];
            double phase = 0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / SampleRate;
                double freq = 950 + 450 * Math.Sin(2 * Math.PI * 0.5 * t - Math.PI / 2);
                phase += 2 * Math.PI * freq / SampleRate;

                double s = Math.Sin(phase) + 0.35 * Math.Sin(3 * phase) + 0.15 * Math.Sin(5 * phase);
                data[i] = ToSample(s / 1.5 * 0.95);
            }

            return BuildWav(data);
        }

        /// <summary>Low motor / cable rumble, 1 second loop.</summary>
        private static byte[] BuildHum()
        {
            int count = SampleRate;
            var data = new short[count];
            var random = new Random(42);
            double lowPassed = 0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / SampleRate;
                double tone = 0.55 * Math.Sin(2 * Math.PI * 55 * t)
                            + 0.30 * Math.Sin(2 * Math.PI * 110 * t)
                            + 0.12 * Math.Sin(2 * Math.PI * 165 * t);

                double noise = random.NextDouble() * 2 - 1;
                lowPassed += 0.06 * (noise - lowPassed);

                double wobble = 0.85 + 0.15 * Math.Sin(2 * Math.PI * 6 * t);
                data[i] = ToSample((tone + 2.2 * lowPassed) * wobble * 0.55);
            }

            return BuildWav(data);
        }

        /// <summary>Short "ding" when the lift arrives.</summary>
        private static byte[] BuildDing()
        {
            int count = (int)(SampleRate * 0.9);
            var data = new short[count];

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / SampleRate;
                double envelope = Math.Exp(-5 * t);
                double s = Math.Sin(2 * Math.PI * 988 * t) + 0.5 * Math.Sin(2 * Math.PI * 1976 * t);
                data[i] = ToSample(s / 1.5 * envelope * 0.8);
            }

            return BuildWav(data);
        }

        private static short ToSample(double value)
        {
            value = Math.Max(-1.0, Math.Min(1.0, value));
            return (short)(value * short.MaxValue);
        }

        private static byte[] BuildWav(short[] samples)
        {
            int dataLength = samples.Length * 2;

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataLength);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);                    // chunk size
                writer.Write((short)1);              // PCM
                writer.Write((short)1);              // mono
                writer.Write(SampleRate);
                writer.Write(SampleRate * 2);        // byte rate
                writer.Write((short)2);              // block align
                writer.Write((short)16);             // bits per sample

                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataLength);
                foreach (short sample in samples)
                    writer.Write(sample);

                writer.Flush();
                return stream.ToArray();
            }
        }
    }
}
