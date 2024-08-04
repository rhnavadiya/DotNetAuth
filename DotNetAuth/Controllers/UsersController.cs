using Azure;
using DotNetAuth.Data;
using DotNetAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;



namespace DotNetAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase   
    {
        private readonly MyDbContext dbContext;
        private readonly IConfiguration configuration;

        public UsersController(MyDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("Registration")]

        public IActionResult Registration(UserDTO userDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = dbContext.Users.FirstOrDefault(x => x.Email == userDTO.Email);
            if (user == null)
            {
                dbContext.Users.Add(new User
                {
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Email = userDTO.Email,
                    Password = userDTO.Password
                });
                dbContext.SaveChanges();
                return Ok("Account successfully Registerd");

            }else
                return BadRequest("Email Address Already Exist");
        }


        [HttpPost]
        [Route("Login")] /* it is called as Attribute based routing */
        public IActionResult Login(Login login)
        {
          

            var user = dbContext.Users.FirstOrDefault(x=> x.Email == login.Email && x.Password == login.Password );
            if(user == null)
            {

                return BadRequest("Email or Password is Invalid");
            }
            else
            {
                var claims = new[]
                {
                    //new Claim(JwtRegisteredClaimNames.Sub,configuration["Jwt:Subject"]),
                    //new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, "user_id"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("userId",user.UserId.ToString()),
                    new Claim("Email",user.Email.ToString())
                     
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("dhjncnasnanvasncvnasncnascn45366323#@$%^$$@@##$%^$$%efesdfsdvnsdhvnsdvl"));
                var signIn = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
               
                var token = new JwtSecurityToken(
                    issuer: "JwtIssuer",
                    audience: "JwtAudience",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: signIn);

                string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { Token = tokenValue, UserDetails = user });

                //return Ok("Account successfully login");
            }
        }

        [HttpGet]
        [Route("GetUsers")]

        public IActionResult GetUsers()
        {
            var user = dbContext.Users.ToList();
            if(user == null)
            {
                return NoContent();
            }
            else
            {
                return Ok(user);
            }
        }


        [Authorize]
        [HttpGet]
        [Route("GetUser")]
        

        public IActionResult GetUser(int id)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.UserId == id);
            if (user == null)
            {
                return NotFound("This User is not Exist");
            }
            else
            {
                return Ok(user);
            }
        }

        [Authorize]
        [HttpPatch("{id}")]
        

        public IActionResult UpdateUser(int id, [FromBody] JsonPatchDocument<User> user)
        {
            var userData = dbContext.Users.FirstOrDefault(x => x.UserId == id);
            if (userData == null)
            {
                return NotFound("user not exist");
            }
            else
            {

                user.ApplyTo(userData);

                dbContext.SaveChanges();

                return Ok(new {message = "sucessfully updated", userUpdatedData = userData });

            }
        }

        //[Authorize]
        //[HttpPatch("{id}")]
        //[Route("UpdateUserAllProperty")]

        //public IActionResult UpdateUserAllProperty(int id, [FromBody] User user)
        //{
        //    var userData = dbContext.Users.FirstOrDefault(x => x.UserId == id);
        //    if (userData == null)
        //    {
        //        return NotFound("user not exist");
        //    }
        //    else
        //    {

        //        userData.FirstName = user.FirstName;
        //        userData.LastName = user.LastName;
        //        userData.Email = user.Email;
        //        userData.Password = user.Password;
        //        userData.IsActive = user.IsActive;
        //        userData.CreatedAt = user.CreatedAt;

        //        dbContext.SaveChanges();

        //        return Ok(new { message = "sucessfully updated", userUpdatedData = userData });

        //    }
        //}

        [Authorize]
        [HttpDelete("{id}")]

        public IActionResult removeUser(int id)
        {
            var findUser = dbContext.Users.FirstOrDefault(x => x.UserId == id);
            if(findUser != null)
            {
                dbContext.Users.Remove(findUser);

                dbContext.SaveChanges();

                return Ok(new {message = "user Successfully removed",removedUser = findUser });
            }

            return NotFound(new { message = "user does not found in the system" });
        }



    }
}
