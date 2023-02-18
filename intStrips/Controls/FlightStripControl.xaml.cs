using System;
using System.Text.RegularExpressions;
using intStrips.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using intStrips.Services;

namespace intStrips.Controls
{
    public partial class FlightStripControl
    {
        private readonly IFlightStripService _stripService = FlightStripServiceProvider.Service;

        public event EventHandler<StripSelectedEventArgs> StripSelected;
        
        public FlightStripControl()
        {
            InitializeComponent();
        }

        public bool EnableGroundFields
        {
            get
            {
                if(DataContext is FlightStripModel strip)
                    return strip.Active && 
                           (strip.StripType == StripType.ARRIVAL || 
                            (strip.StripType == StripType.DEPARTURE && 
                             (strip.FlightStage == FlightStage.CLEARANCE || strip.FlightStage == FlightStage.TAXI)));
                return false;
            }
        }

        private void CycleStage(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_stripService != null && DataContext is FlightStripModel strip)
            {
                switch (strip.FlightStage)
                {
                    case FlightStage.TAXI:
                        _stripService.UpdateStripData(strip.Callsign, "FlightStage", "READY");
                        break;
                    case FlightStage.READY:
                        _stripService.UpdateStripData(strip.Callsign, "FlightStage", "LINE_UP");
                        break;
                    case FlightStage.LINE_UP:
                        _stripService.UpdateStripData(strip.Callsign, "FlightStage", "TAKEOFF");
                        break;
                    case FlightStage.TAKEOFF:
                        _stripService.UpdateStripData(strip.Callsign, "FlightStage", "TAXI");
                        break;
                }
            }
        }
        
        private void KeyHandler(object sender, KeyEventArgs e)
        {
            if (sender is TextBox text && (e.Key == Key.Enter || e.Key == Key.Escape))
            {
                e.Handled = true;

                if (e.Key == Key.Escape)
                    text.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                        
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(text), null);
                Keyboard.ClearFocus();
            }
        }

        private void KeyHandlerNoSpace(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
            if (sender is TextBox text && (e.Key == Key.Enter || e.Key == Key.Escape))
            {
                e.Handled = true;

                if (e.Key == Key.Escape)
                    text.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                        
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(text), null);
                Keyboard.ClearFocus();
            }
        }

        private void AlphanumericChecker(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textbox)) return;
            e.Handled = true;

            var preSelectText = textbox.Text.Substring(0, textbox.SelectionStart);
            var postSelectText = textbox.Text.Substring(textbox.SelectionStart + textbox.SelectionLength);
                                   
            if (preSelectText.Length + postSelectText.Length >= textbox.MaxLength)
                return;
            
            if (Regex.IsMatch(e.Text.ToUpper(), "^[A-Z0-9]$"))
            {
                textbox.Text = preSelectText + e.Text.ToUpper() + postSelectText;
                textbox.CaretIndex = preSelectText.Length + 1;
            }
        }

        private void RunwayChecker(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textbox)) return;
            e.Handled = true;

            var preSelectText = textbox.Text.Substring(0, textbox.SelectionStart);
            var postSelectText = textbox.Text.Substring(textbox.SelectionStart + textbox.SelectionLength);

            if (preSelectText.Length + postSelectText.Length >= textbox.MaxLength)
                return;
            
            var postChangeText = preSelectText + e.Text.ToUpper() + postSelectText;
                
            if (Regex.IsMatch(postChangeText, "^([0-9]?|0[1-9]?|[12][0-9]|3[0-6])[LRC]?$"))
            {
                textbox.Text = postChangeText;
                textbox.CaretIndex = preSelectText.Length + 1;
            }
        }

        private void RadioFreqCheck(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textbox)) return;
            e.Handled = true;

            var preSelectText = textbox.Text.Substring(0, textbox.SelectionStart);
            var postSelectText = textbox.Text.Substring(textbox.SelectionStart + textbox.SelectionLength);

            if (preSelectText.Length + postSelectText.Length >= textbox.MaxLength)
                return;

            var postChangeText = preSelectText + e.Text + postSelectText;
            
            if (Regex.IsMatch(postChangeText, "^(1[0-9]{0,2}\\.?[0-9]{0,3})?$"))
            {
                textbox.Text = preSelectText + e.Text.ToUpper() + postSelectText;
                textbox.CaretIndex = preSelectText.Length + 1;
            }
        }

        private void HeadingChecker(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textbox)) return;
            e.Handled = true;

            var preSelectText = textbox.Text.Substring(0, textbox.SelectionStart);
            var postSelectText = textbox.Text.Substring(textbox.SelectionStart + textbox.SelectionLength);
            
            if (textbox.MaxLength > 0 && preSelectText.Length + postSelectText.Length >= textbox.MaxLength)
                return;

            var postChangeText = preSelectText + e.Text.ToUpper() + postSelectText;
            
            if (Regex.IsMatch(postChangeText, "^[RL]?[0-3]?[0-9]{0,2}$"))
            {
                textbox.Text = postChangeText;
                textbox.CaretIndex = preSelectText.Length + 1;
            }
        }

        private void NumericCheck(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textbox)) return;
            e.Handled = true;

            var preSelectText = textbox.Text.Substring(0, textbox.SelectionStart);
            var postSelectText = textbox.Text.Substring(textbox.SelectionStart + textbox.SelectionLength);
            
            if (textbox.MaxLength > 0 && preSelectText.Length + postSelectText.Length >= textbox.MaxLength)
                return;

            var postChangeText = preSelectText + e.Text.ToUpper() + postSelectText;
            
            if (Regex.IsMatch(postChangeText, "^[0-9]*$"))
            {
                textbox.Text = preSelectText + e.Text.ToUpper() + postSelectText;
                textbox.CaretIndex = preSelectText.Length + 1;
            }
        }

        private void OpenFlightPlan(object sender, RoutedEventArgs e)
        {
            if (DataContext is FlightStripModel strip)
                VatSysConnector.Instance.SendOpenFlightPlanCommand(strip.Callsign);
        }

        private void SelectStrip(object sender, RoutedEventArgs e)
        {
            if (DataContext is FlightStripModel strip)
            {
                StripSelected?.Invoke(this, new StripSelectedEventArgs
                {
                    Strip = strip,
                    Callsign = strip.Callsign
                });
            }
        }

        private void AutoAssignSSR(object sender, RoutedEventArgs e)
        {
            if (DataContext is FlightStripModel strip && !strip.SsrCode.HasValue)
            {
                VatSysConnector.Instance.RequestSsrAutoAssign(strip.Callsign);
            }
        }
    }
}