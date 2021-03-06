﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Voodoo.Validation
{
    public class EnumIsRequiredAttribute : SafeValidationAttribute
    {
        public EnumIsRequiredAttribute()
        {
            ErrorMessage = "is required";
        }

        public override bool IsValueValid(object value)
        {
            if (value == null)
                return false;

            var enumType = value.GetType();

            if (enumType.BaseType != typeof (Enum))
                return false;

            if (Enum.IsDefined(enumType, value) == false)
                return false;

            Enum.Parse(enumType, value.ToString());

            return true;
        }
    }
}