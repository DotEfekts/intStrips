using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace intStrips.Models
{
    internal class MainWindowModel
    {
        public ICollection<FlightStripModel> Strips { get; set; }
    }
}
