using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Main
{
    class TextBoxValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult(true, "Empty link");
            else if (!value.ToString().Contains("https://www.youtube.com/watch?v="))
                return new ValidationResult(false, "YouTube link not valid");

            return new ValidationResult(true, "YouTube link valid");
        }
    }
}
