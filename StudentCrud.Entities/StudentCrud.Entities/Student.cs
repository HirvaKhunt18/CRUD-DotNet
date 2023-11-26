using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace StudentCrud.Entities
{
    public class Student
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }


        [Required(ErrorMessage = "*First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters.")]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "*Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot be longer than 50 characters.")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "*Email is required.")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }


        [Required(ErrorMessage = "*Please select State.")]
        [NotMapped]
        public int StateId { get; set; }
     

        [Required(ErrorMessage = "*Please select City.")]
        [ForeignKey("City")]
        public int CityId { get; set; }
        public City City { get; set; }


        [Required]
        public string HobbiesId { get; set; }
        [NotMapped]
        public string[] Hobbies { get; set; }


        [Required(ErrorMessage = "*Gender is required.")]
        public string Gender { get; set; }


        [Phone]
        [Required(ErrorMessage = "*Phone Number is required.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }


        [Required(ErrorMessage = "*Address is required.")]
        public string Address { get; set; }


        [Required(ErrorMessage = "*Date of Birth is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Image")]
        [Required]
        [MaxLength]
        public string ProfileImage { get; set; }

        
        [NotMapped]
        public IFormFile ImageFile { get; set; }

    }
}
