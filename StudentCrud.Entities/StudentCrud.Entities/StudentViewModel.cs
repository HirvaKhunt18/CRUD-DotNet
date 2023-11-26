using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace StudentCrud.Entities
{
    public class StudentViewModel
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; }        
        public string LastName { get; set; }    
        public string Email { get; set; }       
        public int StateId { get; set; }
        public string StateName { get; set; }       
        public int CityId { get; set; }
        public string CityName { get; set; }       
        public List<string> HobbiesId { get; set; }
        public string[] Hobbies { get; set; }
        public string HobbyName { get; set; }       
        public string Gender { get; set; }      
        public string PhoneNumber { get; set; }     
        public string Address { get; set; }
        public string ProfileImage { get; set; }
        public IFormFile ImageFile { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PageDraw { get; set; }
        public string StartPage { get; set; }
        public string PageLength { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public int RecordsTotal { get; set; }
    }
}