using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieSystem.Areas.Identity.Data;
using MovieSystem.Data;
using MovieSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MovieSystem.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly UserManager<AppUser> userManager;

        public CustomersController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        // GET: Customers
        public ViewResult Index()
        {
            ListUsers();
            return View();
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }
    }
}