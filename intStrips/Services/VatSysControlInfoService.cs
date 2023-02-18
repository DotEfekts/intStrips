using System;
using intStripsShared.Models;

namespace intStrips.Services
{
    public class VatSysControlInfoService : IControlInfoService
    {
        private ControlInfoModel _lastKnown = new ControlInfoModel()
        {
            AerodromeSource = Array.Empty<AerodromeModel>(),
            ControlPosition = ControlPosition.TOWER
        };
        
        public VatSysControlInfoService(VatSysConnector vatSysConnector)
        {
            vatSysConnector.ControlInfoChanged += HandleInfoChange;
            vatSysConnector.SendControlInfoRequest();
        }

        private void HandleInfoChange(object sender, ControlInfoModel newInfo)
        {
            _lastKnown = newInfo;
            ControlInfoChanged?.Invoke(this, newInfo);
        }

        public ControlInfoModel LastKnownInfo() => _lastKnown;

        public event EventHandler<ControlInfoModel> ControlInfoChanged;
    }
}