using System;
using System.Collections.Generic;
using System.Linq;
using intStrips.Models;

namespace intStrips.Services
{
    public class MockFlightDataService : IFlightDataReader, IFlightDataWriter
    {
        public FlightDataModel[] GetAllFlightData()
        {
            return _mockFlights.ToArray();
        }

        public event EventHandler<FlightDataAddedArgs> FlightDataAdded;
        public event EventHandler<FlightDataChangedArgs> FlightDataChanged;
        public event EventHandler<FlightDataRemovedArgs> FlightDataRemoved;

        public void SetFlightDataField(object sender, string callsign, string field, string value)
        {
            var data = _mockFlights.FirstOrDefault(f => f.Callsign == callsign);
            if (data == null) return;

            var prop = data.GetType().GetProperty(field);
            if (prop == null) return;
            
            //prop.SetValue(data, value);
            FlightDataChanged?.Invoke(sender, new FlightDataChangedArgs
            {
                Callsign = callsign,
                Field = field,
                Update = prop.GetValue(data).ToString()
            });
        }

        private readonly List<FlightDataModel> _mockFlights = new List<FlightDataModel>()
        {
            new FlightDataModel
            {
                Callsign = "QLK2124",
                OriginGate = "D22",
                Origin = "YMML",
                Destination = "YMER",
                ApproachCategory = "B",
                AircraftType = "DH8C",
                WakeClass = "M",
                FlightType = "S",
                FlightRules = "I",
                SsrCode = "5342",
                DepartureRunway = "27",
                DepartureHoldingPoint = "Q",
                SelectedSid = "DOSEL1",
                SidTransition = "DOSEL",
                AssignedHeading = "",
                RequestedAltitude = "50",
                AssignedAltitude = "50",
                FlightStage = "CLEARANCE",
                AssignedFrequency = "122.8",
                FlightRoute = "P90",
                EstimatedDepartureTime = DateTime.UtcNow.AddMinutes(15).ToString("o"),
                EstimatedArrivalTime = DateTime.UtcNow.AddHours(1).ToString("o")
            },
            new FlightDataModel
            {
                Callsign = "VOZ142",
                OriginGate = "D6",
                Origin = "YMML",
                Destination = "YPPH",
                ApproachCategory = "C",
                AircraftType = "B739",
                WakeClass = "M",
                FlightType = "S",
                FlightRules = "I",
                SsrCode = "6242",
                DepartureRunway = "27",
                DepartureHoldingPoint = "P",
                SelectedSid = "DOSEL1",
                SidTransition = "DOSEL",
                RequestedAltitude = "50",
                AssignedAltitude = "50",
                FlightStage = "LINE_UP",
                AssignedFrequency = "122.8",
                FlightRoute = "KADOM H44 AD Q33 ESP Q158 BEVLY ESP Q158 BEVLY",
                EstimatedDepartureTime = DateTime.UtcNow.AddMinutes(1).ToString("o"),
                EstimatedArrivalTime = DateTime.UtcNow.AddHours(5).ToString("o")
            },
            new FlightDataModel
            {
                Callsign = "QFA551",
                OriginGate = "D12",
                Origin = "YMML",
                Destination = "YPPH",
                ApproachCategory = "C",
                AircraftType = "A20N",
                WakeClass = "M",
                FlightType = "S",
                FlightRules = "I",
                SsrCode = "2453",
                DepartureRunway = "27",
                DepartureHoldingPoint = "P",
                SelectedSid = "ML6",
                SidTransition = "DOTPA",
                AssignedHeading = "R290",
                RequestedAltitude = "50",
                AssignedAltitude = "50",
                FlightStage = "AIRBORNE",
                AssignedFrequency = "122.8",
                FlightRoute = "P90",
                GlobalRemarks = "R290",
                EstimatedDepartureTime = DateTime.UtcNow.AddMinutes(-1).ToString("o"),
                ActualDepartureTime = DateTime.UtcNow.ToString("o"),
                EstimatedArrivalTime = DateTime.UtcNow.AddHours(5).ToString("o")
            },
            new FlightDataModel
            {
                Callsign = "JST142",
                DestinationGate = "A19",
                Origin = "YSSY",
                Destination = "YMML",
                ApproachCategory = "D",
                AircraftType = "B773",
                WakeClass = "H",
                FlightType = "S",
                FlightRules = "I",
                SsrCode = "2468",
                ArrivalRunway = "27",
                RequestedAltitude = "7",
                AssignedAltitude = null,
                FlightStage = "LANDED",
                AssignedFrequency = "125.5",
                FlightRoute = "P90",
                GlobalRemarks = "L50",
                EstimatedDepartureTime = DateTime.UtcNow.AddHours(-1).ToString("o"),
                ActualDepartureTime = DateTime.UtcNow.AddHours(-1).ToString("o"),
                EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(-1).ToString("o"),
                ActualArrivalTime = DateTime.UtcNow.AddMinutes(-1).ToString("o")
            },
            new FlightDataModel
            {
                Callsign = "AM223",
                DestinationGate = "G29",
                Origin = "YHML",
                Destination = "YMML",
                ApproachCategory = "B",
                AircraftType = "B773",
                WakeClass = "L",
                FlightType = "N",
                FlightRules = "I",
                SsrCode = "3789",
                ArrivalRunway = "16",
                RequestedAltitude = "7",
                AssignedAltitude = null,
                FlightStage = "AIRBORNE",
                AssignedFrequency = "122.8",
                FlightRoute = "P90",
                FlightRemarks = "MEDEVAC",
                ActualDepartureTime = DateTime.UtcNow.AddMinutes(-20).ToString("o"),
                EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(3).ToString("o"),
            },
            new FlightDataModel
            {
                Callsign = "SDT",
                Destination = "YMMB",
                AircraftType = "C172",
                WakeClass = "L",
                FlightType = "G",
                FlightRules = "V",
                SsrCode = "5336",
                RequestedAltitude = "7",
                AssignedAltitude = null,
                FlightStage = "AIRBORNE",
            },
        };
    }
}

/*private readonly List<FlightStripModel> _mockFlights = new List<FlightStripModel>()
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
                FlightStage = FlightStage.AIRBORNE,
                AssignedFrequency = "122.8",
                FlightRoute = "P90",
                FlightRemarks = "",
                TakeoffRemarks = "",
                GlobalRemarks = "R290",
                OnGround = true,
                StripTime = DateTime.UtcNow
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
                FlightRemarks = "MEDEVAC",
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
        };*/