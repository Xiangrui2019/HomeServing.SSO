using System.ComponentModel.DataAnnotations;

namespace HomeServing.SSO.Modules.User
{
    public class UserAddViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
