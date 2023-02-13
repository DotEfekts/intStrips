using intStrips.Models;

namespace intStrips.Helpers
{
    public static class Utilities
    {
        public static ApproachCategory ParseApproachCategory(string category)
        {
            switch (category)
            {
                case "A":
                    return ApproachCategory.A;
                case "B":
                    return ApproachCategory.B;
                case "C":
                    return ApproachCategory.C;
                case "D":
                    return ApproachCategory.D;
                case "E":
                    return ApproachCategory.E;
                default:
                    return ApproachCategory.NONE;
            }
        }
        
        public static WakeClass ParseWakeClass(string wakeClass)
        {
            switch (wakeClass)
            {
                case "L":
                    return WakeClass.LIGHT;
                case "M":
                    return WakeClass.MEDIUM;
                case "H":
                    return WakeClass.HEAVY;
                case "J":
                    return WakeClass.SUPER;
                default:
                    return WakeClass.UNKNOWN;
            }
        }
        
        public static FlightType ParseFlightType(string flightType)
        {
            switch (flightType)
            {
                case "S":
                    return FlightType.SCHEDULED;
                case "N":
                    return FlightType.NON_SCHEDULED;
                case "G":
                    return FlightType.GENERAL_AVIATION;
                case "M":
                    return FlightType.MILITARY;
                default:
                    return FlightType.OTHER;
            }
        }
        
        public static FlightRules ParseFlightRules(string flightRules)
        {
            switch (flightRules)
            {
                case "I":
                    return FlightRules.INSTRUMENT;
                case "V":
                    return FlightRules.VISUAL;
                case "Y":
                    return FlightRules.INST_TO_VISUAL;
                case "Z":
                    return FlightRules.VISUAL_TO_INST;
                default:
                    return FlightRules.UNKNOWN;
            }
        }
        
        public static FlightStage ParseFlightStage(string stage)
        {
            switch (stage)
            {
                case "TAXI":
                    return FlightStage.TAXI;
                case "READY":
                    return FlightStage.READY;
                case "LINE_UP":
                    return FlightStage.LINE_UP;
                case "TAKEOFF":
                    return FlightStage.TAKEOFF;
                case "AIRBORNE":
                    return FlightStage.AIRBORNE;
                case "LANDED":
                    return FlightStage.LANDED;
                default:
                    return FlightStage.CLEARANCE;
            }
        }
    }
}