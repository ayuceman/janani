using System.ComponentModel.DataAnnotations;

namespace SimpleApp.ViewModels.Account
{
    public record UserModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
