using Microsoft.AspNetCore.Mvc;
using StudentCrud.API.Models;
using StudentCrud.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace StudentCrud.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentAPIController : ControllerBase
    {
        private readonly StudentDbContext _context;

        public StudentAPIController()
        {
            _context = new StudentDbContext();
        }

        //STUDENT WEB API
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return new ContentResult()
            {
                Content = "<h2>Student Web API</h2>",
                ContentType = "text/html",
            };
        }

        //DATA SEARCHING SORTING PAGINATION
        [HttpPost]
        [Route("FetchAllStudent")]
        public IActionResult FetchAllStudent(StudentViewModel student)
        {
            try
            {

                int draw = Convert.ToInt32(student.PageDraw);
                int startPage = Convert.ToInt32(student.StartPage);
                int pageLength = Convert.ToInt32(student.PageLength);
                string sortColumn = student.SortColumn;
                string sortOrder = student.SortOrder;

                // Get search criteria
                string fstName = student.FirstName;
                string stateId = Convert.ToString(student.StateId);
                string cityId = Convert.ToString(student.CityId);
                DateTime? dateOfBirth = student.DateOfBirth;

                var students = (from s in _context.Student
                                join city in _context.City on s.CityId equals city.CityId
                                join state in _context.State on city.StateId equals state.StateId
                                where(string.IsNullOrEmpty(fstName) || s.FirstName.Contains(fstName)) &&
                                (student.StateId == 0 || city.StateId == student.StateId) &&
                                (student.CityId == 0 || s.CityId == student.CityId) &&
                                (student.DateOfBirth == null || student.DateOfBirth == default(DateTime) || s.DateOfBirth == student.DateOfBirth)
                                select new StudentViewModel
                                {
                                    StudentId = s.StudentId,
                                    FirstName = s.FirstName,
                                    LastName = s.LastName,
                                    Email = s.Email,
                                    StateName = state.StateName,
                                    StateId = state.StateId,
                                    CityName = city.CityName,
                                    CityId = city.CityId,
                                    HobbyName = string.Join(", ", _context.Hobbies.Where(h => s.HobbiesId.Contains(h.HobbyId.ToString())).Select(h => h.HobbyName)),
                                    Gender = s.Gender,
                                    PhoneNumber = s.PhoneNumber,
                                    Address = s.Address,
                                    DateOfBirth = s.DateOfBirth.Date,
                                    ProfileImage = s.ProfileImage
                                });

                switch (sortColumn)
                {
                    case "FirstName":
                        students = sortOrder == "asc" ? students.OrderBy(s => s.FirstName) : students.OrderByDescending(s => s.FirstName);
                        break;
                    case "LastName":
                        students = sortOrder == "asc" ? students.OrderBy(s => s.LastName) : students.OrderByDescending(s => s.LastName);
                        break;
                    case "Email":
                        students = sortOrder == "asc" ? students.OrderBy(s => s.Email) : students.OrderByDescending(s => s.Email);
                        break;
                    case "State":
                        students = sortOrder == "asc" ? students.OrderBy(s => s.StateName) : students.OrderByDescending(s => s.StateName);
                        break;
                    case "City":
                        students = sortOrder == "asc" ? students.OrderBy(s => s.CityName) : students.OrderByDescending(s => s.CityName);
                        break;
                    case "Hobbies":
                        students = sortOrder == "asc" ? students.OrderBy(s => s.HobbyName) : students.OrderByDescending(s => s.HobbyName);
                        break;
                    case "Gender":
                        students = sortOrder == "asc" ? students.OrderBy(s => s.Gender) : students.OrderByDescending(s => s.Gender);
                        break;
                    case "PhoneNumber":
                        students = sortOrder == "asc" ? students.OrderBy(s => s.PhoneNumber) : students.OrderByDescending(s => s.PhoneNumber);
                        break;
                    case "Address":
                        students = sortOrder == "asc" ? students.OrderBy(s => s.Address) : students.OrderByDescending(s => s.Address);
                        break;
                    case "DateOfBirth":
                        students = sortOrder == "asc" ? students.OrderBy(s => s.DateOfBirth) : students.OrderByDescending(s => s.DateOfBirth);
                        break;
                    default:
                        students = students.OrderBy(s => s.FirstName);
                        break;
                }
                var studentList = students.ToList().Select(s =>
                {
                    s.RecordsTotal = students.Count();
                    return s;
                }).ToList();

                //pagination
                students = studentList.AsQueryable().Skip(startPage).Take(pageLength);
                return Ok(students);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //PARTICULAR STUDENT DETAILS 
        [HttpGet]
        [Route("GetStudentDetails/{StudentId}")]
        public IActionResult GetStudentDetails(int StudentId)
        {
            try
            {
                var students = _context.Student.Find(StudentId);
                if (students == null)
                {
                    return NotFound($"Student not available with this id: {StudentId} ");
                }
                Student student = new Student()
                {
                    StudentId = students.StudentId,
                    FirstName = students.FirstName,
                    LastName = students.LastName,
                    Email = students.Email,
                    StateId = _context.City.FirstOrDefault(c => c.CityId == students.CityId)?.StateId ?? 0,
                    CityId = students.CityId,
                    Gender = students.Gender,
                    PhoneNumber = students.PhoneNumber,
                    Address = students.Address,
                    DateOfBirth = students.DateOfBirth,
                    Hobbies = students.HobbiesId.Split(',').ToArray(),
                    ProfileImage = students.ProfileImage
                };
                return Ok(student);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //STATES
        [HttpGet]
        [Route("GetStates")]
        public IActionResult GetStates()
        {
            try
            {
                var states = _context.State.ToList();
                if (states.Count == 0)
                {
                    return NotFound("States not available");
                }
                return Ok(states);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // CITIES 
        [HttpGet]
        [Route("GetCities/{StateId}")]
        public IActionResult GetCities(int StateId)
        {
            try
            {
                var cities = _context.City.Where(c => c.StateId == StateId).Select(c => new { c.CityId, c.CityName }).ToList();
                if (cities.Count == 0)
                {
                    return NotFound("Cities not available for selected state");
                }
                return Ok(cities);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //HOBBIES
        [HttpGet]
        [Route("GetHobbies")]
        public IActionResult GetHobbies()
        {
            try
            {
                var hobbies = _context.Hobbies.Select(h => new { h.HobbyId, h.HobbyName }).ToList();
                if (hobbies.Count == 0)
                {
                    return NotFound("hobbies not available for selected state");
                }
                return Ok(hobbies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //POST METHOD FOR ADD OR UPDATE STUDENT
        [HttpPost]
        [Route("AddStudent")]
        public IActionResult AddStudent(Student student)
        {
            try
            {
                if (student.StudentId > 0)
                {
                    _context.Student.Update(student);
                }
                else
                {
                    _context.Student.Add(student);
                }
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //DELETE STUDENT DATA
        [HttpDelete]
        [Route("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var student = _context.Student.FirstOrDefault(s => s.StudentId == id);
                if (student == null)
                {
                    return NotFound();
                }
                _context.Student.Remove(student);
                _context.SaveChanges();
                return Ok(student);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}



