using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
//using test_app2.FaultIndicators;

namespace FaultIndicator_MainIPConfig.UI
{
    internal class FaultIndicatorValuesValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value,
            System.Globalization.CultureInfo cultureInfo)
        {
            //FaultIndicatorViewModel faultIndicator = (value as BindingGroup).Items[0] as FaultIndicatorViewModel;
            string numbersPattern = @"^\d$";
            /*if (!Regex.IsMatch(Convert.ToString(faultIndicator._callFrequency), numbersPattern))
            {
                return new ValidationResult(false,
                    "Start Date must be earlier than End Date.");
            }
            else
            {
                return ValidationResult.ValidResult;
            }*/
            return ValidationResult.ValidResult;
        }
    }
}
