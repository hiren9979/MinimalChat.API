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
using MinimalChat.Data.Services;
using MinimalChat.Domain.DTO;

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
        private readonly UserService _userService;


        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context,
            IConfiguration configuration, IOptions<AppSettings> applicationSetting,UserService userService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _appSettings = applicationSetting.Value;
            _userService = userService;

        }

        [HttpPost("Register")]
        public async Task<ActionResult<RegisterResponseDTO>> Register([FromBody] RegisterModel userRegisterModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            var registeredUser = await _userService.RegisterUser(userRegisterModel);

            if (registeredUser != null)
            {
                // Registration was successful, return the user data
                var responseDTO = new RegisterResponseDTO
                {
                    Id = registeredUser.Id,
                    FirstName = registeredUser.FirstName,
                    LastName = registeredUser.LastName,
                    Email = registeredUser.Email
                };

                return responseDTO;
            }
            else
            {
                // Handle registration errors here
                return BadRequest(new { error = "Registration of user failed" });
            }
        }



        [HttpPost("Login")]
        public async Task<LoginResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return new LoginResult { Succeeded = false, Error = "Validation failed" };
            }

            try
            {
                LoginResult loginResult = (LoginResult)await _userService.Login(model);

                if (loginResult.Succeeded)
                {
                    // Authentication was successful, return the token and user profile
                    return loginResult;
                }
                else if (loginResult.Error == "User not found")
                {
                    return new LoginResult { Succeeded = false, Error = "User not found" };
                }
                else if (loginResult.Error == "Invalid credentials")
                {
                    return new LoginResult { Succeeded = false, Error = "Invalid credentials" };
                }
                else
                {
                    // Handle other errors
                    return new LoginResult { Succeeded = false, Error = "An error occurred" };
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                // You can also return a generic error message
                return new LoginResult { Succeeded = false, Error = "An error occurred" };
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
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var users = _userService.GetUsersExcludingCurrentUser(currentUserId);

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