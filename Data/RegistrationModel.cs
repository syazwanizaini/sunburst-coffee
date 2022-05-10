using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Data Annotation 
using System.ComponentModel.DataAnnotations;

namespace Sunburst.Data
{
    public class RegistrationModel
    {
        // Data Annotations for Email Attributes
        // Attribute should be filled
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // First Password Entry
        // Attribute should be filled 
        // A Maximum of 100 and Minimum of 6
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        // A Compare tells the Password Attribute
        // Error Message will be printed when both passwords does not match
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The Password and Confirmation Password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}
