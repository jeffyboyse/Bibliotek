using System;

namespace bibliotek.Services
{
    public interface IUserService
    {
        Task<(bool Success, string Message, User?)> LoginAsync(string)
    }
}
   
