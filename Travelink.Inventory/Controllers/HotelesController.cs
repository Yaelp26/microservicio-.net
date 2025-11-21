using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travelink.Inventory.Data;
using Travelink.Inventory.Models;

[Route("api/[controller]")]
[ApiController]
public class HotelesController : ControllerBase
{
    private readonly InventoryContext _context;

    public HotelesController(InventoryContext context)
    {
        _context = context;
    }

    // GET: api/Hoteles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Hotel>>> GetHoteles()
    {
        return await _context.Hoteles.ToListAsync();
    }

    // POST: api/Hoteles
    [HttpPost]
    public async Task<ActionResult<Hotel>> PostHotel(Hotel hotel)
    {
        _context.Hoteles.Add(hotel);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetHoteles), new { id = hotel.Id }, hotel);
    }
}