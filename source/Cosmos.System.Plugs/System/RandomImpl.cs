using System;

using Cosmos.IL2CPU.Plugs;
using Cosmos.HAL;

using static System.Math;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(Random))]
    public class RandomImpl
    {
        internal static PseudoRandom rand;

        
        internal static void init()
        {
            if (rand != null)
                Ctor(null);
        }

        public static void Ctor(Random aThis) => rand = new PseudoRandom((RTC.Hour << 11) ^ (RTC.Second << 7) ^ RTC.Minute);

        public static void Ctor(Random aThis, int seed) => rand = new PseudoRandom(seed);

        public static int Next(Random aThis)
        {
            init();
            return rand.Next();
        }

        public static int Next(Random aThis, int maxValue)
        {
            init();
            return rand.Next(maxValue);
        }

        public static int Next(Random aThis, int minValue, int maxValue)
        {
            init();
            return rand.Next(minValue, maxValue);
        }

        public static double NextDouble()
        {
            init();
            return rand.NextDoublePositive();
        }

        public static void NextBytes(byte[] buffer)
        {
            for (int i = 0, l = buffer.Length; i < l; i++)
                buffer[i] = (byte)(NextDouble() * 0x100);
        }

        //static double GetUniform()
        //{
        //    uint seed = (uint)RTC.Second;
        //    uint m_w = (uint)(seed >> 16);
        //    uint m_z = (uint)(seed % 4294967296);
        //    
        //    m_z = 36969 * (m_z & 65535) + (m_z >> 16);
        //    m_w = 18000 * (m_w & 65535) + (m_w >> 16);
        //    
        //    uint u = (m_z << 16) + m_w;
        //    
        //    return (u + 1.0) * 2.328306435454494e-10;
        //}
    }

    /// <summary>
    /// Represents a pseudo-random number generator, which is based on the mersenne algorithm
    /// </summary>
    public unsafe sealed class PseudoRandom
    {
        internal const int N = 624;
        internal const int M = 397;
        internal const uint MATRIX_A = 0x9908b0df;
        internal const int MAX_RAND_INT = 0x7fffffff;
        internal const uint LOWER_MASK = MAX_RAND_INT;
        internal const uint UPPER_MASK = 0x80000000;

        internal uint[] mag01 = { 0, MATRIX_A };
        internal uint[] mt = new uint[N];
        internal int mti = N + 1;

        /// <summary>
        /// The maximum random integer value
        /// </summary>
        public static int MaxRandomInt => MAX_RAND_INT;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public PseudoRandom()
            : this(DateTime.Now.GetHashCode())
        {
        }

        /// <summary>
        /// Creates a new instance using the given seed
        /// </summary>
        /// <param name="seed">Initial random seed</param>
        public PseudoRandom(int seed)
        {
            __init((uint)seed);
        }

        /// <summary>
        /// Creates a new instance using the given initialization values
        /// </summary>
        /// <param name="arr">Initialization values</param>
        public PseudoRandom(int[] arr)
        {
            fixed (int* ptr = arr)
                __init((uint*)ptr, (uint)arr.Length);
        }

        /// <summary>
        /// Generates the next pseudo-random integer number
        /// </summary>
        /// <returns>Pseudo-random integer number</returns>
        public int Next() => Abs(__nexti() >> 1);

        /// <summary>
        /// Generates the next pseudo-random integer number
        /// </summary>
        /// <param name="max">Maximum inclusive value</param>
        /// <returns>Pseudo-random integer number</returns>
        public int Next(int max) => Next(0, max);

        /// <summary>
        /// Generates the next pseudo-random integer number
        /// </summary>
        /// <param name="min">Minimum inclusive value</param>
        /// <param name="max">Maximum inclusive value</param>
        /// <returns>Pseudo-random integer number</returns>
        public int Next(int min, int max) => (int)(__nextd2() * Abs(max - min)) + min;

        /// <summary>
        /// Generates the next pseudo-random single precision floating-point number between -0.5 and 0.5
        /// </summary>
        /// <returns>Pseudo-random single precision floating-point number</returns>
        public float NextFloat() => (float)NextDouble();

        /// <summary>
        /// Generates the next pseudo-random positive single precision floating-point number
        /// </summary>
        /// <returns>Pseudo-random single precision floating-point number</returns>
        public float NextFloatPositive() => (float)NextDoublePositive();

        /// <summary>
        /// Generates the next pseudo-random double precision floating-point number between -0.5 and 0.5
        /// </summary>
        /// <returns>Pseudo-random double precision floating-point number</returns>
        public double NextDouble() => __nextd1();

        /// <summary>
        /// Generates the next pseudo-random positive double precision floating-point number
        /// </summary>
        /// <returns>Pseudo-random double precision floating-point number</returns>
        public double NextDoublePositive() => __nextd2();


        internal void __init(uint seed)
        {
            mt[0] = seed & 0xffffffffU;

            for (mti = 1; mti < N; mti++)
                mt[mti] = (uint)(18123433252L * (mt[mti - 1] ^ (long)(mt[mti - 1] >> 30)) + mti) & 0xffffffffU;
        }

        internal void __init(uint* ptr, uint len)
        {
            __init(19650218);

            int i = 1;
            int j = 0;

            for (int k = (int)Max(N, len); k > 0; k--)
            {
                mt[i] = (uint)((mt[i] ^ ((mt[i - 1] ^ mt[i - 1] >> 30) * 1664525U)) + ptr[j] + j) & 0xffffffffU;
                ++i;
                ++j;

                if (i >= N)
                {
                    mt[0] = mt[N - 1];
                    i = 1;
                }

                j %= (int)len;
            }

            for (int k = N - 1; k > 0; k--)
            {
                mt[i] = (uint)((mt[i] ^ ((mt[i - 1] ^ mt[i - 1] >> 30) * 1566083941U)) - i) & 0xffffffffU;
                ++i;

                if (i >= N)
                {
                    mt[0] = mt[N - 1];
                    i = 1;
                }
            }

            mt[0] = UPPER_MASK;
        }

        internal int __nexti()
        {
            uint y;
            int kk;

            if (mti >= N)
            {
                if (mti == N + 1)
                    __init(5489U);

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y % 2];
                }

                for (; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M - N] ^ (y >> 1) ^ mag01[y % 2];
                }

                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y % 2];
                mti = 0;
            }

            y = mt[mti++];
            y ^= y >> 11;
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= y >> 18;
            //y &= MAX_RAND_INT;

            return (int)y;
        }

        internal double __nextd1() => __nexti() / __nextd2() - .5;

        internal double __nextd2() => Abs(__nexti() / (double)MAX_RAND_INT);
    }
}
