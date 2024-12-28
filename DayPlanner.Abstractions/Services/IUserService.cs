using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Stores;
using FirebaseAdmin.Auth;

namespace DayPlanner.Abstractions.Services
{
    public interface IUserService
    {
        Task<UserRecord> GetUserByIdAsync(string uid);
        Task<UserRecord> CreateUserAsync(UserRecordArgs args);
    }
    public class UserService : IUserService
    {
        private readonly IUserStore _userRepository;
        public UserService(IUserStore userRepository) =>
            _userRepository = userRepository;

        public async Task<UserRecord> GetUserByIdAsync(string uid) => await _userRepository.GetByIdAsync(uid);
        public async Task<UserRecord> CreateUserAsync(UserRecordArgs args) => await _userRepository.CreateAsync(args);
    }
    
}