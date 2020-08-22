using HomeServing.SSO.Models;
using System.ComponentModel.DataAnnotations;

namespace HomeServing.SSO.Modules.User
{
    public class UserEditViewModel
    {
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string NickName { get; set; }
        [Required]
        public string Bio { get; set; }
        [Required]
        public Gender Gender { get; set; }
    }
}
