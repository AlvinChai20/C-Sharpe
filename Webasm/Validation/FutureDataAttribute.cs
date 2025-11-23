using System;
using System.ComponentModel.DataAnnotations;

public class FutureDateAttribute : ValidationAttribute
{
    public FutureDateAttribute()
    {
        ErrorMessage = "Appointment date must be in the future.";
    }

    public override bool IsValid(object value)
    {
        if (value is DateTime dateTime)
        {
            // The selected date must be greater than the current date and time
            return dateTime > DateTime.Now;
        }
        return false; // Not a valid DateTime
    }
}