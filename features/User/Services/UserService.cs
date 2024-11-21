using Data;
using Features.User.Entities;
using Features.User.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Features.User.Exceptions;

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
            GoogleId = user.GoogleId,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PublicRank = user.Rank
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
            XP = user.XP,
            Gold = user.Gold,
            DisplayName = user.DisplayName,
            PublicRank = user.Rank
        };
    }

    public async Task<IEnumerable<UserDTO>> GetUsersByRankAsync(Rank rank)
    {
        var users = await _context.Users.ToListAsync();
        var userList = new UserList(users);

        var filteredUsers = userList
            .Where(u => u.Rank == rank)
            .ToList();


        filteredUsers.Sort();

        return filteredUsers.Select(user => new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            XP = user.XP,
            Gold = user.Gold,
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

    // Rank formula, when even 125*x^3 and 125*x^3 + 125 when odd, x is user rank enum
    public async Task<int> CheckXPRequiredForLevelUp(Guid userID)
    {
        try
        {
            var user = await _context.Users.FindAsync(userID) ?? throw new Exception("User not found.");
            int rankVariable = (int)user.Rank;
            return (rankVariable % 2 != 0) ? 125 * (rankVariable) ^ 3 : 125 * (rankVariable) ^ 3 + 125;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return -1;
        }

    }

    public async Task<bool> CheckAndApplyRankUpAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            int requiredXP = await CheckXPRequiredForLevelUp(userId);

            if (user.XP < requiredXP)
            {
                throw new InsufficientXPException(userId, user.XP, requiredXP);
            }

            user.XP -= requiredXP;
            user.Rank = user.Rank + 1;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (UserNotFoundException ex)
        {
            using (StreamWriter writer = new StreamWriter("logs/user_errors.txt", true))
            {
                await writer.WriteLineAsync($"[{DateTime.Now}] User not found: {ex.Message}");
            }
            throw; // Re-throw to be handled by controller
        }
        catch (InsufficientXPException ex)
        {
            using (StreamWriter writer = new StreamWriter("logs/rankup_errors.txt", true))
            {
                await writer.WriteLineAsync($"[{DateTime.Now}] Insufficient XP: {ex.Message}");
            }
            throw; // Re-throw to be handled by controller
        }
        catch (Exception ex)
        {
            using (StreamWriter writer = new StreamWriter("logs/general_errors.txt", true))
            {
                await writer.WriteLineAsync($"[{DateTime.Now}] Unexpected error: {ex.Message}");
            }
            throw; // Re-throw unexpected exceptions
        }
    }


    public async Task<bool> UpdateUserGoldXp(Guid userId, int gold, int xp)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"Error, could not give rewards, because the user {userId} who earned them doesn't exist.");
            }
            if (xp < 0)
            {
                throw new KeyNotFoundException($"Error, and how exactly did we get here?");
            }

            user.Gold += gold;
            user.XP += xp;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return false;
        }
    }
    public async Task<User?> ValidateUserAsync(string googleId, string email, string displayName)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId && u.Email == email && u.DisplayName == displayName);
    }

    public async Task<bool> IsUsernameTakenAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.DisplayName == username);
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User> CreateUserAsync(UserValidationDTO userValidationDTO)
    {
        var newUser = new User
        {
            GoogleId = userValidationDTO.GoogleId,
            Email = userValidationDTO.Email,
            DisplayName = userValidationDTO.DisplayName
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return newUser;
    }

    public async Task<UserDTO?> GetUserByGoogleIdAsync(string googleId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

        if (user is null)
            return null;

        return new UserDTO
        {
            Id = user.Id,
            GoogleId = user.GoogleId,
            Email = user.Email,
            XP = user.XP,
            Gold = user.Gold,
            DisplayName = user.DisplayName,
            PublicRank = user.Rank
        };
    }


}
