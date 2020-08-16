﻿namespace Iit.Fibertest.Dto
{
    public class RftsEventsDto
    {
        public ReturnCode ReturnCode;
        public string ErrorMessage;

        public bool IsNoFiber;
        public RftsLevelDto[] LevelArray;
        public RftsEventsFooterDto Footer;
    }

    public class RftsLevelDto
    {
        public string Title;
        public bool IsFailed;
        public string FirstProblemLocation;
        public RftsEventDto[] EventArray;
        public TotalFiberLossDto TotalFiberLoss;
    }

    public class RftsEventDto
    {
        public int Ordinal;
        public bool IsNew;
        public bool IsFailed;

        public string LandmarkTitle;
        public string LandmarkType;
        public string State;
        public string DamageType;
        public string DistanceKm;
        public string Enabled;
        public string EventType;

        public string reflectanceCoeff;
        public string attenuationInClosure;
        public string attenuationCoeff;

        public MonitoringThreshold reflectanceCoeffThreshold;
        public MonitoringThreshold attenuationInClosureThreshold;
        public MonitoringThreshold attenuationCoeffThreshold;

        public string reflectanceCoeffDeviation;
        public string attenuationInClosureDeviation;
        public string attenuationCoeffDeviation;
    }

    public class TotalFiberLossDto
    {
        public double Value;
        public MonitoringThreshold Threshold;
        public double Deviation;
        public bool IsPassed;
    }

    public class MonitoringThreshold
    {
        public double Value;
        public bool IsAbsolute;
    }

    public class RftsEventsFooterDto
    {
        public string TraceState;
        public double Orl;
        public LevelState[] LevelStates;
    }

    public class LevelState
    {
        public string LevelTitle;
        public string State;
    }
}
