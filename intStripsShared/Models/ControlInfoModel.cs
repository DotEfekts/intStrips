using System;

namespace intStripsShared.Models
{
    [Serializable]
    public class ControlInfoModel
    {
        public AerodromeModel[] AerodromeSource;
        public ControlPosition ControlPosition;
    }

    [Serializable]
    public class AerodromeModel
    {
        public string AerodromeCode;
        public RunwaysModel[] Runways;
    }
    
    [Serializable]
    public class RunwaysModel
    {
        public string Runway;
        public SidStarsModel[] SidStars;
    }
    
    [Serializable]
    public class SidStarsModel
    {
        public string Code;
        public string[] Transitions;
        public bool Star;
    }

    public enum ControlPosition
    {
        TOWER, TMA, ENROUTE
    }
}