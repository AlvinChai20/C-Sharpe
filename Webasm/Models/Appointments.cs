using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;  // ✅ Needed for [Precision]

namespace UserApp.Models
{
    public class Appointments
    {
        public int Id { get; set; }

        public string? ClientName { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
        [StringLength(10, MinimumLength = 10)]
        public string? ContactNumber { get; set; }

        [Required]
        public string? Service { get; set; }

        [Required]
        [Display(Name = "Appointment Date")]
        [FutureDate] // ✅ your custom attribute
        public DateTime Date { get; set; }

        public string Status { get; set; } = "Booked";

        public string? PaymentMethod { get; set; }

        public string PurchaseStatus { get; set; } = "Pending";

        public string? PaidBy { get; set; }

        public string? Doctors { get; set; }

        // ✅ New fields for pricing
        [Required]
        [Precision(18, 2)]   // ensures always 2 decimal places in DB
        public decimal Price { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal Deposit { get; set; }
    }
}
