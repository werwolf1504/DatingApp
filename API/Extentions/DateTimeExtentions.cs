using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace API.Extentions
{
    public static class DateTimeExtentions
    {
        public static int CalculateAge(this DateOnly dob)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            int age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;
            return age;
        }
    }
}
