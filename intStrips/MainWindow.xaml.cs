using intStrips.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using intStrips.Services;

namespace intStrips
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            DataContext = new MainWindowModel
            {
                Strips = new ObservableCollection<FlightStripModel>()
            };

            Closing += Cleanup;
            
            FlightStripServiceProvider.Service.FlightStripsRefreshed += RefreshDataContext;
            FlightStripServiceProvider.Service.FlightStripChanged += StripChanged;
            FlightStripServiceProvider.Service.FlightStripRemoved += StripRemoved;
            InitializeComponent();
            
            Task.Run(() => FlightStripServiceProvider.Service.RequestAllFlightData());
        }

        private void StripChanged(object sender, FlightStripChangedArgs e)
        {
            var stripList = ((MainWindowModel)DataContext).Strips;
            if(!stripList.Contains(e.Strip))
                stripList.Add(e.Strip);
        }

        private void StripRemoved(object sender, FlightStripRemovedArgs e)
        {
            var stripList = ((MainWindowModel)DataContext).Strips;
            var strip = stripList.FirstOrDefault(s => s.Callsign == e.Callsign);
            if (strip == null) return;
            stripList.Remove(strip);
        }

        private void Cleanup(object sender, CancelEventArgs e)
        {
            FlightStripServiceProvider.Service.Dispose();
        }

        private void RefreshDataContext(object sender, FlightStripsRefreshedArgs e)
        {
            ((MainWindowModel)DataContext).Strips = new ObservableCollection<FlightStripModel>(e.Strips);
        }

        private void window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var el = Keyboard.FocusedElement;
            if (el is TextBox text)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(text), null);
                Keyboard.ClearFocus();
            }
        }

        private void StripSelected(object sender, StripSelectedEventArgs e)
        {
            e.Strip.Selected = !e.Strip.Selected;
        }
    }
}