using System.IO;
using Features.User.Entities;
using Features.User.DTOs;
using Features.User.Exceptions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("api/users")]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("api/users/{userID}")]
    public async Task<ActionResult<UserDTO>> GetUser(string userID)
    {
        if (Guid.TryParse(userID, out Guid guid))
        {
            var user = await _userService.GetUserAsync(guid);

            if (user is not null)
            {
                return Ok(user);
            }
        }

        return NotFound();
    }

    [HttpGet("api/users/google-id/{googleId}")]
    public async Task<ActionResult<UserDTO>> GetUserByGoogleId(string googleId)
    {
        var user = await _userService.GetUserByGoogleIdAsync(googleId);

        if (user is not null)
        {
            return Ok(user);
        }

        return NotFound();
    }

    [HttpPost("api/users/{userID}/rewardGoldXp")]
    public async Task<ActionResult<UserDTO>> UpdateUserGoldXp(string userID, int addGold, int addXp)
    {

        if (Guid.TryParse(userID, out Guid guid))
        {
            var user = await _userService.GetUserAsync(guid);

            int beforeGold = user.Gold;
            int beforeXp = Convert.ToInt32(user.XP);

            bool action = await _userService.UpdateUserGoldXp(guid, addGold, addXp);

            // TO-DO, add rank-up check, if xp exceeds next rank-up xp then user is promoted.
            if (user is not null)
            {
                return Ok(new { message = $"Gold {beforeGold} -> {user.Gold}, XP {beforeXp} -> {user.XP}" });
            }
        }

        return NotFound();
    }

    [HttpPost("api/users/{userID}/check-rankup")]
    public async Task<ActionResult> CheckUserRankUp(string userID)
    {
        try
        {
            if (!Guid.TryParse(userID, out Guid guid))
            {
                return BadRequest("Invalid user ID format");
            }

            bool rankUpSuccess = await _userService.CheckAndApplyRankUpAsync(guid);
            return Ok(new { message = "Rank up successful!" });
        }
        catch (UserNotFoundException ex)
        {
            using (StreamWriter writer = new StreamWriter("logs/user_errors.txt", true))
            {
                await writer.WriteLineAsync($"[{DateTime.Now}] {ex.Message}");
            }

            return NotFound(new
            {
                message = ex.Message,
                userId = ex.UserId
            });
        }
        catch (InsufficientXPException ex)
        {
            using (StreamWriter writer = new StreamWriter("logs/rankup_errors.txt", true))
            {
                await writer.WriteLineAsync($"[{DateTime.Now}] {ex.Message}");
            }

            return BadRequest(new
            {
                message = "Insufficient XP for rank up",
                currentXP = ex.CurrentXP,
                requiredXP = ex.RequiredXP,
                needed = ex.RequiredXP - ex.CurrentXP
            });
        }
    }


}
