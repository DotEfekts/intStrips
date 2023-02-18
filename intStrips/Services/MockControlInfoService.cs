using System;
using intStripsShared.Models;

namespace intStrips.Services
{
    public class MockControlInfoService : IControlInfoService
    {
        public ControlInfoModel LastKnownInfo() => new ControlInfoModel
        {
            AerodromeSource = new[] { 
                new AerodromeModel
                {
                    AerodromeCode = "YMML"
                }
            },
            ControlPosition = ControlPosition.TOWER
        };

        public event EventHandler<ControlInfoModel> ControlInfoChanged;
    }
}