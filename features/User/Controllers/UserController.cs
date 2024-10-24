using Features.User.Entities;
using Features.User.DTOs;
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
}
