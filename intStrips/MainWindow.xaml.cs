using intStrips.Models;
using System;
using System.Collections.Generic;
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
            InitializeComponent();
            
            DataContext = new MainWindowModel
            {
                Strips = FlightStripServiceProvider.Service.GetAllFlightStrips()
            };
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