using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Booking_Store.Models
{
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [Display(Name = "Email Address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [Display(Name = "Subject")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [Display(Name = "Message")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        [Display(Name = "Submitted On")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}