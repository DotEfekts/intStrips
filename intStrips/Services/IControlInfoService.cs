using System;
using intStripsShared.Models;

namespace intStrips.Services
{
    public interface IControlInfoService
    {
        ControlInfoModel LastKnownInfo();
        event EventHandler<ControlInfoModel> ControlInfoChanged;
    }
}