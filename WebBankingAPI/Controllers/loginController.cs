using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using WebBankingAPI.Models;

namespace WebBankingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class loginController : ControllerBase
    {
        [HttpGet("")]
        public ActionResult Login([FromBody]User usercredential)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                User candidate = model.Users.FirstOrDefault(fod => fod.Username == usercredential.Username && fod.Password == usercredential.Password);
                if (candidate == null) return Ok("Login non effettuato, controlla username o password");

                var TokenHandler = new JwtSecurityTokenHandler();
                var TokenDescriptor = new SecurityTokenDescriptor
                {
                    SigningCredentials = new SigningCredentials(SecurityKeyGenerator.GetSecurityKey(), SecurityAlgorithms.HmacSha256Signature),
                    Expires = DateTime.UtcNow.AddDays(1),
                    Subject = new ClaimsIdentity(
                        new Claim[]
                        {
                            new Claim("Id", candidate.Id.ToString()),
                            new Claim("Username", candidate.Username),
                        }
                    )
                };

                SecurityToken token = TokenHandler.CreateToken(TokenDescriptor);
                candidate.LastLogin = DateTime.Now;
                model.SaveChanges();
                return Ok(TokenHandler.WriteToken(token));

            }
        }
    }
}
