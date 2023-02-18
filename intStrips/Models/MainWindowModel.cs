using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace intStrips.Models
{
    internal class MainWindowModel : INotifyPropertyChanged
    {
        public ObservableCollection<FlightStripModel> Strips { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyStripsChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Strips"));
        }
    }

    internal class DesignMainWindowModel : MainWindowModel
    {
        public DesignMainWindowModel()
        {
            Strips = new ObservableCollection<FlightStripModel>()
            {
                new DesignTimeStripModel()
            };
        }
    }
}
