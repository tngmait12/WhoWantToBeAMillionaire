using Microsoft.AspNetCore.Identity;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Data
{
    public static class UserRoleInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUserModel>>();

            string[] roleNames = { "Admin", "User" };

            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);

                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var email = "admin@gmail.com";
            var password = "1111";

            if (userManager.FindByEmailAsync(email).Result == null)
            {
                AppUserModel user = new()
                {
                    Email = email,
                    UserName = "admin"
                };

                IdentityResult result = userManager.CreateAsync(user, password).Result;

                if (result.Succeeded)
                {
                    var adminRole = await roleManager.FindByNameAsync("Admin");
                    user.RoleId = adminRole.Id;
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }
            }


        }


    }
}
