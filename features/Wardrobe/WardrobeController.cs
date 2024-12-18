using Microsoft.AspNetCore.Mvc;
using Features.Wardrobe.DTOs;
using Features.Wardrobe.Services;
using Features.User.DTOs;

[ApiController]
public class WardrobeController : ControllerBase
{
    private readonly IWardrobeService _wardrobeService;

    public WardrobeController(IWardrobeService wardrobeService)
    {
        _wardrobeService = wardrobeService;
    }

    [HttpGet("api/wardrobe")]
    public async Task<ActionResult<IEnumerable<WardrobeItemDTO>>> GetAllItems()
    {
        var items = await _wardrobeService.GetAllWardrobeItemsAsync();
        return Ok(items);
    }

    [HttpGet("api/wardrobe/{itemId}/eligibility/{userId}")]
    public async Task<ActionResult<PurchaseEligibilityDTO>> CheckPurchaseEligibility(string itemId, string userId)
    {
        if (!Guid.TryParse(itemId, out Guid itemGuid) || !Guid.TryParse(userId, out Guid userGuid))
        {
            return BadRequest("Invalid ID format");
        }

        var eligibility = await _wardrobeService.CheckPurchaseEligibilityAsync(userGuid, itemGuid);
        return Ok(eligibility);
    }

    [HttpPost("api/wardrobe")]
    public async Task<ActionResult<WardrobeItemDTO>> CreateWardrobeItem([FromBody] CreateWardrobeItemDTO itemDto)
    {
        if (itemDto.Price < 0)
        {
            return BadRequest("Price cannot be negative");
        }

        try
        {
            var createdItem = await _wardrobeService.CreateWardrobeItemAsync(itemDto);
            return CreatedAtAction(nameof(GetAllItems), new { id = createdItem.Id }, createdItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while creating the wardrobe item: {ex.Message}");
        }
    }
}