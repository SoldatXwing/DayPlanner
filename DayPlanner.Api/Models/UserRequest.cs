namespace DayPlanner.Api.Endpoints
{
    public partial class AccountEndpoints
    {
        public class UserRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
