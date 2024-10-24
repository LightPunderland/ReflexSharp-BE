using Data;
using Features.User.Entities;
using Features.User.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
    {
        var users = await _context.Users.ToListAsync();
        var userList = new UserList(users);

        userList.Sort();

        return userList.Select(user => new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PublicRank = Enum.TryParse(user.Rank, out Rank rank) ? rank : Rank.None
        }).ToList();
    }

    public async Task<UserDTO?> GetUserAsync(Guid userID)
    {
        var user = await _context.Users.FindAsync(userID);

        if (user is null)
            return null;

        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PublicRank = Enum.TryParse(user.Rank, out Rank rank) ? rank : Rank.None
        };
    }

    public async Task<IEnumerable<UserDTO>> GetUsersByRankAsync(Rank rank)
    {
        var users = await _context.Users.ToListAsync();
        var userList = new UserList(users);

        var filteredUsers = userList
            .Where(u => Enum.TryParse(u.Rank, out Rank userRank) && userRank == rank)
            .ToList();


        filteredUsers.Sort();

        return filteredUsers.Select(user => new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PublicRank = rank
        }).ToList();
    }

    public async Task<bool> CheckUsernameAvailabilityAsync(string username)
    {
        var users = await _context.Users.ToListAsync();
        var userList = new UserList(users);

        return userList.All(u => u.Email != username);
    }
}
