using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> UserExists(string userId);
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> CreateUser(User user);
        Task<User?> DeleteUser(string id);
    }
}