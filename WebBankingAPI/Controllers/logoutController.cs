using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WebBankingAPI.Models;

namespace WebBankingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class logoutController : ControllerBase
    {
        [HttpPost("")]
        public ActionResult Logout()
        {
            var username = HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Username").Value;
            var id = Convert.ToInt32(HttpContext.User.Claims.FirstOrDefault(fod => fod.Type == "Id").Value);
            using (WebBankingContext model = new WebBankingContext())
            {
                var candidate = model.Users.FirstOrDefault(fod => fod.Username == username && fod.Id == id);
                candidate.LastLogout = DateTime.Now;
                model.SaveChanges();
                return Ok("Logout effettuato con successo");
            }
        }
    }
}
