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
        private string _type;

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

        public string Type
        {
            get => _type;
            set => _type = value;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int val = 0;
            var strVal = value as string;
            try
            {
                if (Type == "Port")
                {
                    if (strVal != null)
                    {
                        if (strVal.Length > 0)
                        {
                            return CheckRanges(strVal);
                        }
                    }
                }
                if (Type == "IP")
                {
                    if (strVal == null)
                    {
                        return new ValidationResult(false, "Неккоректно введён IP");
                    }
                    if (strVal.Split('.').Length != 4)
                    {
                        return new ValidationResult(false, "Неккоректно введён IP");
                    }

                    //проверка что при записи вида '1.1.1.' после крайней точки не пустота
                    if (strVal.Split('.')[3] == "")
                    {
                        return new ValidationResult(false, "Неккоректно введён IP");
                    }


                    return CheckIP(strVal);
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Введенны неккоректные символы: " + e.Message);
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


        //TODO: Убрать/Изменить CheckRanges из-за использования маски в поле ввода
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
                return new ValidationResult(false, "Введенны неккоректные символы");
            }
        }


        private ValidationResult CheckIP(string strVal)
        {
            var msg = strVal.Split(".");

            if (msg.Length > 0)
            {
                foreach (string str in msg)
                {
                    if (!int.TryParse(str, out var val))
                    {
                        return new ValidationResult(false, "В IP должны содержаться только цифры");
                    }
                    if ((val < Min) || (val > Max))
                    {
                        return new ValidationResult(false,
                          "Одно из чисел не находится в диапазоне от " + Min + " до " + Max + ".");
                    }
                }
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "Неккоректно введён IP");
        }
    }
}
