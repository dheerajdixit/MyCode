﻿namespace BAL
{
    using Model;
    using System;

    public interface IMovingAverage
    {
        void AddSample(float val);
        void ClearSamples();
        void InitializeSamples(float val);

        float Average { get; }

        BollingerBand BollingerBand {get;}
    }
}

