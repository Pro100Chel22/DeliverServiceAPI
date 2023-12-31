﻿using ASPDotNetWebAPI.CustomValidationAttributes;
using ASPDotNetWebAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASPDotNetWebAPI.Models.DTO
{
    public class RegistrationRequestDTO
    {
        [Required]
        [MinLength(1)]
        public string FullName { get; set; }
        [Required]
        [MinLength(6)]
        [CustomPassword(Nullable = false)]
        public string Password { get; set; }
        [Required]
        [MinLength(1)]
        [EmailAddress]
        public string Email { get; set; }
        public Guid? AddressId { get; set; }
        public DateTime? BirthDate { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [CustomPhone(Nullable = true)]
        public string? PhoneNumber { get; set; }
    }
}
