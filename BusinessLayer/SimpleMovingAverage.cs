using Clifton.Collections.Generic;
using Model;
using System;
using System.Linq;
namespace BAL
{


    public class SimpleMovingAverage : IMovingAverage
    {
        private CircularList<float> samples;
        protected float total;

        public SimpleMovingAverage(int numSamples)
        {
            if (numSamples <= 0)
            {
                throw new ArgumentOutOfRangeException("numSamples can't be negative or 0.");
            }
            this.samples = new CircularList<float>(numSamples);
            this.total = 0f;
        }

        public void AddSample(float val)
        {
            if (this.samples.Count == this.samples.Length)
            {
                if(this.total==float.NaN)
                {
                    this.total = 0f;
                }
                this.total -= this.samples.Value;
            }
            this.samples.Value = val;
            this.total += val;
            this.samples.Next();
        }

        public void ClearSamples()
        {
            this.total = 0f;
            this.samples.Clear();
        }

        public void InitializeSamples(float v)
        {
            this.samples.SetAll(v);
            this.total = v * this.samples.Length;
        }

        public float Average
        {
            get
            {
                if (this.samples.Count == 0)
                {
                    throw new ApplicationException("Number of samples is 0.");
                }
                return (this.total / ((float)this.samples.Count));
            }
        }
        public float Min
        {
            get
            {
                return this.samples.Min();
            }
        }
        public float Max
        {
            get
            {
                return this.samples.Max();
            }
        }

        public BollingerBand BollingerBand
        {
            get
            {
                if (this.samples.Count == 0)
                {
                    throw new ApplicationException("Number of samples is 0.");
                }

                var sampleData = this.samples.ToArray();
                float sumOfSquaresOfDifferences = sampleData.Select(val => (val - this.Average) * (val - this.Average)).Sum();
                double sd = Math.Sqrt(sumOfSquaresOfDifferences / this.samples.Count);
                return new BollingerBand { Upper = this.Average + 2 * sd, Middle = this.Average, Lower = this.Average - 2 * sd };
            }
        }
    }
}

