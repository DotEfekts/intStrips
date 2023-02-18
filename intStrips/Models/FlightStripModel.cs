using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace intStrips.Models
{
    public class FlightStripModel : INotifyPropertyChanged
    {
        public StripType StripType { get; set; } = StripType.DEPARTURE;
        public string BackgroundColor => StripType == StripType.DEPARTURE ? "#ff9fb4b5" : StripType == StripType.LOCAL ? "#ffbc618c" : "#ffd7bb94";
        public bool Active { get; set; } = true;

        public bool Selected { get; set; }
        public string ElementBackground => Active ? "#00ffffff" : "#ffab9584";
        public string CallsignBackground => Selected ? "#ffcfcfe3" : "#00ffffff";

        public string Callsign { get; set; }

        public string Gate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string AdCode => StripType == StripType.ARRIVAL ? Origin : Destination;

        public ApproachCategory ApproachCategory { get; set; }
        public string ApproachCategoryCode
        {
            get
            {
                switch (ApproachCategory)
                {
                    case ApproachCategory.NONE:
                        return "";
                    case ApproachCategory.A:
                        return "A";
                    case ApproachCategory.B:
                        return "B";
                    case ApproachCategory.C:
                        return "C";
                    case ApproachCategory.D:
                        return "D";
                    default:
                        return "E";
                }
            }
        }
        public string AircraftType { get; set; }
        public WakeClass WakeClass { get; set; }
        public string WakeClassCode
        {
            get
            {
                switch (WakeClass)
                {
                    case WakeClass.LIGHT:
                        return "L";
                    case WakeClass.MEDIUM:
                        return "M";
                    case WakeClass.HEAVY:
                        return "H";
                    case WakeClass.SUPER:
                        return "J";
                    default:
                        return "?";
                }
            }
        }
        public string WakeClassBackground
        {
            get
            {
                if (WakeClass == WakeClass.HEAVY || WakeClass == WakeClass.SUPER)
                    return "#ffda6326";
                return ElementBackground;
            }
        }

        public FlightType FlightType { get; set; }
        public string FlightTypeCode
        {
            get
            {
                switch (FlightType)
                {
                    case FlightType.SCHEDULED:
                        return "S";
                    case FlightType.NON_SCHEDULED:
                        return "N";
                    case FlightType.GENERAL_AVIATION:
                        return "G";
                    case FlightType.MILITARY:
                        return "M";
                    default:
                        return "X";
                }
            }
        }
        public FlightRules FlightRules { get; set; }
        public string FlightRulesCode
        {
            get
            {
                switch (FlightRules)
                {
                    case FlightRules.INSTRUMENT:
                        return "I";
                    case FlightRules.VISUAL:
                        return "V";
                    case FlightRules.INST_TO_VISUAL:
                        return "Y";
                    case FlightRules.VISUAL_TO_INST:
                        return "Z";
                    default:
                        return "?";
                }
            }
        }
        public bool SquawkingCode { get; set; }
        public string SquawkingCodeMark => SsrCode.HasValue ? SquawkingCode ? "*" : "" : "#";
        public int? SsrCode { get; set; }

        public string Runway { get; set; }
        public string HoldingPoint { get; set; }

        public string SelectedSid { get; set; }
        public string SelectedSidBackground
        {
            get
            {
                if(FlightStage != FlightStage.CLEARANCE && !string.IsNullOrWhiteSpace(SelectedSid))
                {
                    return "#ff02b113";
                }

                return "#00ffffff";
            }
        }
        public string SidTransition { get; set; }
        public string AssignedHeading { get; set; }

        public int RequestedAltitude { get; set; }
        public int? AssignedAltitude { get; set; }

        public FlightStage FlightStage { get; set; } = FlightStage.TAXI;

        public string FlightStageCode
        {
            get
            {
                switch (FlightStage)
                {
                    case FlightStage.READY:
                        return "RDY";
                    case FlightStage.LINE_UP:
                        return "L/UP";
                    case FlightStage.TAKEOFF:
                        return "T/O";
                    default:
                        return "";
                }
            }
        }
        public bool InTowerStage => FlightStage == FlightStage.TAXI || !string.IsNullOrWhiteSpace(FlightStageCode);

        public string AssignedFrequency { get; set; }

        public string FlightRoute { get; set; }
        public string FlightRemarks { get; set; }
        public string LevelRemarks { get; set; }
        public string GlobalRemarks { get; set; }

        public bool OnGround { get; set; }
        public DateTime? StripTime { get; set; }
        public string TimeBackground
        {
            get
            {
                if (!Active)
                    return ElementBackground;

                if ((StripType == StripType.DEPARTURE && !OnGround) ||
                   (StripType == StripType.ARRIVAL && OnGround))
                    return "#ff02b113";

                return ElementBackground;
            }
        }

        public string TimeDisplay
        {
            get
            {
                if (!StripTime.HasValue)
                    return "";
                
                if (StripType == StripType.ARRIVAL)
                    return StripTime.Value.ToString("mm:ss");
                
                return StripTime.Value.ToString("hh:mm");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            var caller = new StackFrame(2).GetMethod().DeclaringType?.Name;
            PropertyChanged?.Invoke(this, new PropertyChangedCallerEventArgs(propertyName, caller));
        }
    }

    public enum StripType
    {
        ARRIVAL, DEPARTURE, LOCAL
    }

    public enum ApproachCategory
    {
        NONE, A, B, C, D, E
    }

    public enum WakeClass
    {
        LIGHT, MEDIUM, HEAVY, SUPER, UNKNOWN
    }

    public enum FlightType
    {
        SCHEDULED, NON_SCHEDULED, GENERAL_AVIATION, MILITARY, OTHER
    }

    public enum FlightRules
    {
        VISUAL, INSTRUMENT, VISUAL_TO_INST, INST_TO_VISUAL, UNKNOWN
    }

    public enum FlightStage
    {
        CLEARANCE, TAXI, READY, LINE_UP, TAKEOFF, AIRBORNE, LANDED
    }

    internal class DesignTimeStripModel : FlightStripModel
    {
        public DesignTimeStripModel()
        {
            StripType = StripType.ARRIVAL;
            Active = false;
            Callsign = "QF551";
            Gate = "D12";
            Origin = "YMML";
            Destination = "YPPH";
            ApproachCategory = ApproachCategory.C;
            AircraftType = "A20N";
            WakeClass = WakeClass.MEDIUM;
            FlightType = FlightType.SCHEDULED;
            FlightRules = FlightRules.INSTRUMENT;
            SquawkingCode = true;
            SsrCode = 2453;
            Runway = "27";
            HoldingPoint = "P";
            SelectedSid = "ML6";
            SidTransition = "DOTPA";
            AssignedHeading = "R290";
            RequestedAltitude = 50;
            AssignedAltitude = 50;
            FlightStage = FlightStage.READY;
            AssignedFrequency = "122.8";
            FlightRoute = "KADOM H44 AD Q33 ESP Q158 BEVLY Q33 ESP Q158 BEVLY";
            FlightRemarks = "";
            LevelRemarks = "";
            GlobalRemarks = "R290";
            OnGround = true;
            StripTime = DateTime.UtcNow.AddMinutes(5);
        }
    }
}
