using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FaultIndicator_MainIPConfig.UI
{
    public class FieldValidationRule : ValidationRule
    {
        private int _min;
        private int _max;
        //private string _type;

        public int Min
        {
            get => _min;
            set => _min = value;
        }

        public int Max
        {
            get => _max;
            set => _max = value;
        }

        /*public string Type
        {
            get => _type; 
            set => _type = value;
        }*/

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int val = 0;
            var strVal = value as string;
            try
            {
                if (strVal != null)
                {
                    if (strVal.Length > 0)
                    {
                        return CheckRanges(strVal);
                    }
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Кал" + e.Message);
            }

            if ((val < Min) || (val > Max))
            {
                return new ValidationResult(false,
                  "Пожалуйста, введите число в диапазоне от " + Min + " до " + Max + ".");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
            //throw new NotImplementedException();
        }

        private ValidationResult CheckRanges(string strVal)
        {
            if (int.TryParse(strVal, out var res))
            {
                if ((res < Min) || (res > Max))
                {
                    return new ValidationResult(false,
                      "Пожалуйста, введите число в диапазоне от " + Min + " до " + Max + ".");
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }
            else
            {
                return new ValidationResult(false, "Illegal characters entered");
            }
        }
    }
}
