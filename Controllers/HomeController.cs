using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Login_and_Registration.Models;
using System.Collections.Generic;

namespace Login_and_Registration
{
    public class HomeController : Controller
    {
        private readonly DbConnector _dbConnector;
 
        public HomeController(DbConnector connect)
        {
            _dbConnector = connect;
        }


        [HttpGet]
        [Route("")]
        public IActionResult Index(User user)
        {
            ViewBag.Error = TempData["error"];
            return View(user);
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid)
            {
                string testQuery = $"SELECT * FROM Users WHERE Email='{user.Email}'";
                List<Dictionary<string, object>> CheckUser = _dbConnector.Query(testQuery);
                if(CheckUser.Count > 0)
                {
                    TempData["error"] = "This email already exists. Please Log In";
                    return RedirectToAction("Index");
                }
                else
                {
                    string query = $"INSERT INTO Users (FirstName, LastName, Email, Password) VALUES ('{user.FirstName}', '{user.LastName}', '{user.Email}', '{user.Password}')";
                    _dbConnector.Execute(query);
                    List<Dictionary<string, object>> User = _dbConnector.Query($"SELECT * FROM users WHERE email='{user.Email}'");
                    System.Console.WriteLine(User[0]["id"]);
                    HttpContext.Session.SetInt32("id", (int)User[0]["id"]);
                }
            }
            
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(string email, string password)
        {
            string query = $"SELECT * FROM Users WHERE email='{email}' AND password='{password}'";
            List<Dictionary<string, object>> User = _dbConnector.Query(query);
            if(User.Count == 0)
            {
                TempData["error"] = "Username/Password Combination is incorrect.";
            }
            else
            {
                HttpContext.Session.SetInt32("id", (int)User[0]["id"]);
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetInt32("id") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                List<Dictionary<string, object>> User = _dbConnector.Query($"SELECT * FROM users WHERE id='{HttpContext.Session.GetInt32("id")}'");
                ViewBag.id = User[0]["id"];
                ViewBag.FirstName = User[0]["FirstName"];
                return View();
            }
        }
        [HttpGet]
        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}