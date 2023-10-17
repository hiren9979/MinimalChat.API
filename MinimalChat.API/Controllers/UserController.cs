using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Google.Apis.Auth;
using MinimalChat.API;
using Microsoft.Extensions.Options;

namespace Minimal_chat_application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private static List<User> UserList = new List<User>();
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;


        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context,
            IConfiguration configuration, IOptions<AppSettings> applicationSetting
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _appSettings = applicationSetting.Value;

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromQuery] RegisterModel userRegisterModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            // Check if a user with the same email already exists
            var existingUser = await _userManager.FindByEmailAsync(userRegisterModel.Email);
            if (existingUser != null)
            {
                return Conflict(new { error = "User already registered with this email" });
            }

            var user = new User
            {
                Email = userRegisterModel.Email,
                UserName = userRegisterModel.FirstName,
                FirstName = userRegisterModel.FirstName,
                LastName = userRegisterModel.LastName
            };

            var result = await _userManager.CreateAsync(user, userRegisterModel.Password);

            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    userId = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email
                });
            }
            else
            {
                // Handle other registration errors here
                return BadRequest(new { error = "User registration failed", errors = result.Errors.Select(e => e.Description) });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Unauthorized(new { error = "User not found" });
                }
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

                if (result.Succeeded)
                {

                    var token = GenerateJwtToken(user);
                    var profile = new
                    {
                        id = user.Id,
                        name = user.UserName,
                        email = user.Email
                    };
                    return Ok(new { token, profile });
                }
                else
                {
                    return Unauthorized(new { error = "Invalid credentials" });
                }

                // Rest of your code
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                // You can also return a generic error message
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
         
        }

        [HttpPost("LoginWithGoogle")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] string credential)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { this._appSettings.GoogleClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

            //var user = UserList.Where(x => x.UserName == payload.Name).FirstOrDefault();

            var user = await _userManager.FindByEmailAsync(payload.Email);

                
            if (user != null)
            {
                var token = GenerateJwtToken(user);
                var profile = new
                {
                    id = user.Id,
                    name = user.UserName,
                    email = user.Email
                };
                return Ok(new { token, profile });
            }
            else
            {
                return BadRequest();
            }
        }



        [HttpGet("GetUsers")]
        [Authorize]
        public IActionResult GetUsers()
        {
            // Get the ID of the currently authenticated user
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var users = _context.Users
                .Where(u => u.Id != currentUserId) // Exclude the currently authenticated user
                .Select(u => new User
                {
                    Id = u.Id,
                    FirstName = u.FirstName + " " + u.LastName,
                    Email = u.Email
                })
                .ToList();

            if (users.Count == 0)
            {
                return NotFound(new { error = "No other users found" });
            }

            return Ok(new { users });
        }


        //Generate jwt token
        private string GenerateJwtToken(User user)
        {
            var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.FirstName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}