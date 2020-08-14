using System;
using Restaurant_Website.Models;
using Restaurant_Website.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant_Website
{
    public class AccountLogic : IAccountLogic
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        public AccountLogic(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async void ChangeEmailForID(string id, string email)
        {
            IdentityUser user = await this.userManager.FindByIdAsync(id);
            user.Email = email;
            await this.userManager.UpdateNormalizedEmailAsync(user);
            await userManager.UpdateAsync(user);
        }

        public async void ChangePasswordForID(string id, string password)
        {
            IdentityUser user = await this.userManager.FindByIdAsync(id);
            await this.userManager.RemovePasswordAsync(user);
            await this.userManager.AddPasswordAsync(user, password);
            await this.userManager.UpdateAsync(user);
        }

        public async void ChangeUsernameForID(string id, string username)
        {
            IdentityUser user = await this.userManager.FindByIdAsync(id);
            user.UserName = username;
            await this.userManager.UpdateNormalizedUserNameAsync(user);
            await userManager.UpdateAsync(user);
        }

        public async void Logout()
        {
            await this.signInManager.SignOutAsync();
        }
    }

    public interface IAccountLogic
    {
        void ChangeUsernameForID(string id, string username);
        void ChangePasswordForID(string id, string password);
        void ChangeEmailForID(string id, string email);
        void Logout();
    }
}