namespace BAL
{
    using Clifton.Collections.Generic;
    using System;

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
                return (this.total / ((float) this.samples.Count));
            }
        }
    }
}

