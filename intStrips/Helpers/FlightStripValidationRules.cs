using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace intStrips.Helpers
{
    public class HeadingValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return Regex.IsMatch(value?.ToString() ?? "",
                "^[RL](0?[0-9]?[1-9]|0?[1-9][0-9]?|[1-2][0-9]{1,2}|3[0-5][0-9]|360)$")
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Invalid heading.");
        }
    }

    public class FrequencyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return Regex.IsMatch(value?.ToString() ?? "",
                "^1(1[89]|2[0-9]|3[0-6])\\.[0-9]{1,3}$")
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Invalid heading.");
        }
    }
    
    public class RunwayValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return Regex.IsMatch(value?.ToString() ?? "",
                "^(0?[1-9]|[12][0-9]|3[0-6])[LRC]?$")
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Invalid runway.");
        }
    }
}