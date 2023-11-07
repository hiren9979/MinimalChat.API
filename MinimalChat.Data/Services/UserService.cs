using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;
using MinimalChat.Domain.DTO;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Data.Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;


        public UserService(UserManager<User> userManager, ApplicationDbContext context, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<User> RegisterUser(RegisterModel userRegisterModel)
        {
            // Check if a user with the same email already exists
            var existingUser = await _userManager.FindByEmailAsync(userRegisterModel.Email);
            if (existingUser != null)
            {
                return null; // User with the same email already exists
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
                return user; // Return the newly registered user
            }

            return null; // Registration failed
        }

        public async Task<LoginResult> Login(LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return new LoginResult { Succeeded = false, Error = "User not found" };
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
                    return new LoginResult { Succeeded = true, Token = token, Profile = profile };
                }
                else
                {
                    return new LoginResult { Succeeded = false, Error = "Invalid credentials" };
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                // You can also return a generic error message
                return new LoginResult { Succeeded = false, Error = "An error occurred" };
            }
        }

        public List<UserDTO> GetUsersExcludingCurrentUser(string currentUserId)
        {
            var users = _context.Users
                .Where(u => u.Id != currentUserId)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    firstName = u.FirstName,
                    Email = u.Email
                })
                .ToList();

            return users;
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
