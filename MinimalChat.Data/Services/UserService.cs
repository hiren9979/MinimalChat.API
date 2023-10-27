using Microsoft.AspNetCore.Identity;
using Minimal_chat_application.Context;
using Minimal_chat_application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChat.Data.Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IdentityResult> RegisterUser(RegisterModel userRegisterModel)
        {
            // Check if a user with the same email already exists
            var existingUser = await _userManager.FindByEmailAsync(userRegisterModel.Email);
            if (existingUser != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User already registered with this email" });
            }

            var user = new User
            {
                Email = userRegisterModel.Email,
                UserName = userRegisterModel.FirstName,
                FirstName = userRegisterModel.FirstName,
                LastName = userRegisterModel.LastName
            };

            return await _userManager.CreateAsync(user, userRegisterModel.Password);
        }


    }

}
