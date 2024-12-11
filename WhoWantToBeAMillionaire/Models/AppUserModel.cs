using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WhoWantToBeAMillionaire.Data.Validation;

namespace WhoWantToBeAMillionaire.Models
{
    public class AppUserModel : IdentityUser
    {
        public string RoleId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        //public string Token { get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; } 
    }
}
