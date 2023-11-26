using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using StudentCrud.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace StudentCrud.WebUI.Controllers
{
    public class StudentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public StudentController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseAddress"]);
        }

        //INDEX
        public IActionResult Index()
        {
            ViewBag.States = GetStates();
            ViewBag.message = TempData["message"];

            return View();
        }

        //DATATABLE SEARCHING SORTING PAGINATION
        [HttpPost]
        public async Task<IActionResult> FetchAllStudent(StudentViewModel student)
        {
            try
            {
                student.PageDraw = Request.Form["draw"];
                student.StartPage = Request.Form["start"];
                student.PageLength = Request.Form["length"];
                student.SortColumn = Request.Form[$"columns[{Request.Form["order[0][column]"]}][name]"];
                student.SortOrder = Request.Form["order[0][dir]"];

                string json = JsonConvert.SerializeObject(student);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(_httpClient.BaseAddress + "/FetchAllStudent", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<List<StudentViewModel>>(responseData);
                    var studentFirst = result.FirstOrDefault();
                    var studentRecords = studentFirst == null ? 0 : studentFirst.RecordsTotal;
                    var jsonData = new { draw = student.PageDraw, recordsFiltered = studentRecords, recordsTotal = studentRecords, data = result };
                    return Ok(jsonData);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //CREATE FORM
        [HttpGet]
        public IActionResult Create()
        {
            Student student = new Student();
            student.DateOfBirth = DateTime.Today;
            ViewBag.title = "Add Student";
            ViewBag.button = "Submit";
            ViewBag.States = GetStates();
            ViewBag.Hobbies = GetHobbies();
            return View(student);
        }

        //STATES
        private List<State> GetStates()
        {
            List<State> states = new List<State>();
            HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "/GetStates").Result;
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                states = JsonConvert.DeserializeObject<List<State>>(json);
            }
            return states;
        }

        //CITIES JSON FORMAT 
        [HttpGet]
        public JsonResult GetCities(int StateId)
        {
            List<City> cities = new List<City>();
            HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "/GetCities" + "/" + StateId).Result;
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                cities = JsonConvert.DeserializeObject<List<City>>(json);
            }

            return Json(cities);
        }

        //HOBBIES
        private List<Hobbies> GetHobbies()
        {
            List<Hobbies> hobbies = new List<Hobbies>();

            HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "/GetHobbies").Result;
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                hobbies = JsonConvert.DeserializeObject<List<Hobbies>>(json);
            }
            return hobbies;
        }

        //CITIES ENUMBERABLE FORMAT FOR EDIT
        private List<City> GetCitiesById(int StateId)
        {
            List<City> cities = new List<City>();
            HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "/GetCities" + "/" + StateId).Result;
            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                cities = JsonConvert.DeserializeObject<List<City>>(json);
            }
            return cities;
        }

        //CREATE OR EDIT POST METHOD 
        [HttpPost]
        public IActionResult Create(Student student)
        {
            try
            {
                if (student.ProfileImage == null)
                {
                    string uniqueFileName = ProcessUploadedFile(student.ImageFile);
                    student.ProfileImage = uniqueFileName;
                }
                student.ImageFile = null;
                student.HobbiesId = string.Join(",", student.Hobbies);
                string json = JsonConvert.SerializeObject(student);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _httpClient.PostAsync(_httpClient.BaseAddress + "/AddStudent", content).Result;
                if (student.StudentId > 0)
                {
                    TempData["message"] = "Student Updated Successfully";
                }
                else
                {
                    TempData["message"] = "Student Added Successfully";
                }
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }

        //IMAGE INTO STRING
        private string ProcessUploadedFile(IFormFile image)
        {
            string uniqueFileName = null;
            if (image != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        //GET DATA FOR EDIT FORM FOR PARTICULAR STUDENT
        [HttpGet]
        public IActionResult Edit(int? StudentId)
        {
            try
            {
                ViewBag.title = "Edit Student";
                ViewBag.button = "Update";
                HttpResponseMessage response = _httpClient.GetAsync(_httpClient.BaseAddress + "/GetStudentDetails/" + StudentId).Result;
                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;
                    var student = JsonConvert.DeserializeObject<Student>(json);
                    student.HobbiesId = string.Join(",", student.Hobbies);
                    ViewBag.SelectedHobbies = GetHobbies();
                    // Set the IsChecked property for each hobby checkbox based on the selected hobbies of the student
                    foreach (var hobby in ViewBag.SelectedHobbies)
                    {
                        hobby.IsChecked = student.HobbiesId.Contains(hobby.HobbyId.ToString());
                    }
                    ViewBag.SelectedCities = GetCitiesById(student.StateId);
                    ViewBag.States = GetStates();
                    return View("Create", student);
                }

            }
            catch
            {
                return View("Error");
            }
            return RedirectToAction("Index");
        }

        //DELETE STUDENT
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
                HttpResponseMessage response = _httpClient.DeleteAsync(_httpClient.BaseAddress + "/Delete/" + id).Result;
                TempData["message"] = "Student Deleted Successfully";
                if (response.IsSuccessStatusCode)
                {

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();

        }

    }
}
