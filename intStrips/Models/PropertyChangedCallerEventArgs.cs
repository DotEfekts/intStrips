using System.ComponentModel;

namespace intStrips.Models
{
    public class PropertyChangedCallerEventArgs : PropertyChangedEventArgs
    {
        public string Caller { get; }
        
        public PropertyChangedCallerEventArgs(string propertyName, string caller) : base(propertyName)
        {
            Caller = caller;
        }
    }
}