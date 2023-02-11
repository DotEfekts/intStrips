using intStrips.Models;
using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace intStrips
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            var model = new MainWindowModel
            {
                Strips = new List<FlightStripModel>()
                {
                    new FlightStripModel
                    {
                        StripType = StripType.DEPARTURE,
                        Callsign = "QLK2124",
                        Gate = "D22",
                        Origin = "YMML",
                        Destination = "YMER",
                        ApproachCategory = ApproachCategory.B,
                        AircraftType = "DH8C",
                        WakeClass = WakeClass.MEDIUM,
                        FlightType = FlightType.SCHEDULED,
                        FlightRules = FlightRules.INSTRUMENT,
                        SquawkingCode = true,
                        SsrCode = 5342,
                        Runway = "27",
                        HoldingPoint = "Q",
                        SelectedSid = "DOSEL1",
                        SidTransition = "DOSEL",
                        AssignedHeading = "",
                        RequestedAltitude = 50,
                        AssignedAltitude = 50,
                        FlightStage = FlightStage.CLEARANCE,
                        AssignedFrequency = "122.8",
                        FlightRoute = "P90",
                        FlightRemarks = "",
                        TakeoffRemarks = "",
                        GlobalRemarks = "",
                        OnGround = true,
                        StripTime = DateTime.UtcNow.AddMinutes(15)
                    },
                    new FlightStripModel
                    {
                        StripType = StripType.DEPARTURE,
                        Callsign = "QFA551",
                        Gate = "D12",
                        Origin = "YMML",
                        Destination = "YPPH",
                        ApproachCategory = ApproachCategory.C,
                        AircraftType = "A30N",
                        WakeClass = WakeClass.MEDIUM,
                        FlightType = FlightType.SCHEDULED,
                        FlightRules = FlightRules.INSTRUMENT,
                        SquawkingCode = true,
                        SsrCode = 2453,
                        Runway = "27",
                        HoldingPoint = "P",
                        SelectedSid = "ML6",
                        SidTransition = "DOTPA",
                        AssignedHeading = "R290",
                        RequestedAltitude = 50,
                        AssignedAltitude = 50,
                        FlightStage = FlightStage.READY,
                        AssignedFrequency = "122.8",
                        FlightRoute = "P90",
                        FlightRemarks = "",
                        TakeoffRemarks = "",
                        GlobalRemarks = "R290",
                        OnGround = true,
                        StripTime = DateTime.UtcNow.AddMinutes(5)
                    },
                    new FlightStripModel
                    {
                        StripType = StripType.ARRIVAL,
                        Callsign = "JST142",
                        Gate = "A19",
                        Origin = "YSSY",
                        Destination = "YMML",
                        ApproachCategory = ApproachCategory.D,
                        AircraftType = "B773",
                        WakeClass = WakeClass.HEAVY,
                        FlightType = FlightType.SCHEDULED,
                        FlightRules = FlightRules.INSTRUMENT,
                        SquawkingCode = true,
                        SsrCode = 2468,
                        Runway = "27",
                        HoldingPoint = "",
                        SelectedSid = "",
                        SidTransition = "",
                        AssignedHeading = "",
                        RequestedAltitude = 7,
                        AssignedAltitude = null,
                        FlightStage = FlightStage.LANDED,
                        AssignedFrequency = "125.5",
                        FlightRoute = "P90",
                        FlightRemarks = "",
                        TakeoffRemarks = "",
                        GlobalRemarks = "L50",
                        OnGround = true,
                        StripTime = DateTime.UtcNow.AddMinutes(-1)
                    },
                    new FlightStripModel
                    {
                        StripType = StripType.ARRIVAL,
                        Active = false,
                        Callsign = "AM223",
                        Gate = "G29",
                        Origin = "YHML",
                        Destination = "YMML",
                        ApproachCategory = ApproachCategory.B,
                        AircraftType = "B773",
                        WakeClass = WakeClass.LIGHT,
                        FlightType = FlightType.NON_SCHEDULED,
                        FlightRules = FlightRules.INSTRUMENT,
                        SquawkingCode = true,
                        SsrCode = 3789,
                        Runway = "16",
                        HoldingPoint = "",
                        SelectedSid = "",
                        SidTransition = "",
                        AssignedHeading = "",
                        RequestedAltitude = 7,
                        AssignedAltitude = null,
                        FlightStage = FlightStage.AIRBORNE,
                        AssignedFrequency = "122.8",
                        FlightRoute = "P90",
                        FlightRemarks = "MEDIVAC",
                        TakeoffRemarks = "",
                        GlobalRemarks = "",
                        OnGround = false,
                        StripTime = DateTime.UtcNow.AddMinutes(3)
                    },
                    new FlightStripModel
                    {
                        StripType = StripType.LOCAL,
                        Active = false,
                        Callsign = "SDT",
                        Gate = "",
                        Origin = "",
                        Destination = "YMMB",
                        ApproachCategory = ApproachCategory.NONE,
                        AircraftType = "C172",
                        WakeClass = WakeClass.LIGHT,
                        FlightType = FlightType.GENERAL_AVIATION,
                        FlightRules = FlightRules.VISUAL,
                        SquawkingCode = false,
                        SsrCode = 5336,
                        Runway = "",
                        HoldingPoint = "",
                        SelectedSid = "",
                        SidTransition = "",
                        AssignedHeading = "",
                        RequestedAltitude = 7,
                        AssignedAltitude = null,
                        FlightStage = FlightStage.AIRBORNE,
                        AssignedFrequency = "",
                        FlightRoute = "",
                        FlightRemarks = "",
                        TakeoffRemarks = "",
                        GlobalRemarks = "",
                        OnGround = false,
                        StripTime = null
                    },
                }
            };

            InitializeComponent();
            DataContext = model;

        }
    }
}