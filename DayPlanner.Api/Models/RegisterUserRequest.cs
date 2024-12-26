namespace DayPlanner.Api.Models
{
    public partial class AccountEndpoints
    {
        public class RegisterUserRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string DisplayName { get; set; }
        }
    }
}