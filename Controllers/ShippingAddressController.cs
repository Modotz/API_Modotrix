using ERP.Api.Data;
using ERP.Api.DTOs.Customer;
using ERP.Api.DTOs.ShippingAddress;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/shipping-addresses")]
public class ShippingAddressesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ShippingAddressesController(AppDbContext db)
    {
        _db = db;
    }

    // =========================================================
    // GET SHIPPING ADDRESSES BY CUSTOMER
    // GET: api/shipping-addresses/customer/{customerId}
    // =========================================================
    [HttpGet("customer/{customerId:long}")]
    public async Task<IActionResult> GetByCustomer(long customerId)
    {
        var customerExists = await _db.Customers
            .AnyAsync(x =>
                x.Id == customerId &&
                x.DeletedAt == null);

        if (!customerExists)
        {
            return NotFound(new
            {
                message = "Customer tidak ditemukan."
            });
        }

        var data = await _db.ShippingAddresses
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                id = x.Id,
                uid = x.Uid,
                customerId = x.CustomerId,
                name = x.Name,
                address = x.Address,
                shippingAreaId = x.ShippingAreaId,
                shippingAreaName = x.ShippingArea != null
                    ? x.ShippingArea.Name
                    : null
            })
            .ToListAsync();

        return Ok(data);
    }

    // =========================================================
    // SHIPPING AREA DROPDOWN
    // GET: api/shipping-addresses/shipping-areas
    // =========================================================
    [HttpGet("shipping-areas")]
    public async Task<IActionResult> GetShippingAreas()
    {
        var data = await _db.ShippingAreas
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                price = x.Price
            })
            .ToListAsync();

        return Ok(data);
    }

    // =========================================================
    // CREATE SHIPPING ADDRESS
    // POST: api/shipping-addresses
    // =========================================================
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateShippingAddressDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var customerExists = await _db.Customers
            .AnyAsync(x =>
                x.Id == dto.CustomerId &&
                x.DeletedAt == null);

        if (!customerExists)
        {
            return BadRequest(new
            {
                message = "Customer tidak ditemukan."
            });
        }

        if (dto.ShippingAreaId.HasValue)
        {
            var shippingAreaExists = await _db.ShippingAreas
                .AnyAsync(x =>
                    x.Id == dto.ShippingAreaId.Value &&
                    x.IsActive &&
                    x.DeletedAt == null);

            if (!shippingAreaExists)
            {
                return BadRequest(new
                {
                    message = "Shipping Area tidak ditemukan atau tidak aktif."
                });
            }
        }

        var shippingAddress = new ShippingAddress
        {
            Uid = Guid.NewGuid().ToString(),
            CustomerId = dto.CustomerId,
            Name = dto.Name,
            Address = dto.Address,
            ShippingAreaId = dto.ShippingAreaId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.ShippingAddresses.Add(shippingAddress);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Alamat pengiriman berhasil ditambahkan.",
            data = new
            {
                id = shippingAddress.Id,
                uid = shippingAddress.Uid,
                customerId = shippingAddress.CustomerId,
                name = shippingAddress.Name,
                address = shippingAddress.Address,
                shippingAreaId = shippingAddress.ShippingAreaId
            }
        });
    }

    // =========================================================
    // UPDATE SHIPPING ADDRESS
    // POST: api/shipping-addresses/update
    // =========================================================
    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateShippingAddressDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var shippingAddress = await _db.ShippingAddresses
            .FirstOrDefaultAsync(x =>
                x.Id == dto.Id &&
                x.CustomerId == dto.CustomerId);

        if (shippingAddress == null)
        {
            return NotFound(new
            {
                message = "Alamat pengiriman tidak ditemukan."
            });
        }

        var customerExists = await _db.Customers
            .AnyAsync(x =>
                x.Id == dto.CustomerId &&
                x.DeletedAt == null);

        if (!customerExists)
        {
            return BadRequest(new
            {
                message = "Customer tidak ditemukan."
            });
        }

        if (dto.ShippingAreaId.HasValue)
        {
            var shippingAreaExists = await _db.ShippingAreas
                .AnyAsync(x =>
                    x.Id == dto.ShippingAreaId.Value &&
                    x.IsActive &&
                    x.DeletedAt == null);

            if (!shippingAreaExists)
            {
                return BadRequest(new
                {
                    message = "Shipping Area tidak ditemukan atau tidak aktif."
                });
            }
        }

        shippingAddress.Name = dto.Name;
        shippingAddress.Address = dto.Address;
        shippingAddress.ShippingAreaId = dto.ShippingAreaId;
        shippingAddress.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Alamat pengiriman berhasil diperbarui.",
            data = new
            {
                id = shippingAddress.Id,
                uid = shippingAddress.Uid,
                customerId = shippingAddress.CustomerId,
                name = shippingAddress.Name,
                address = shippingAddress.Address,
                shippingAreaId = shippingAddress.ShippingAreaId
            }
        });
    }

    //DELETE SHIPPING ADDRESS
    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteShippingAddressDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var shippingAddress = await _db.ShippingAddresses
            .FirstOrDefaultAsync(x => x.Id == dto.Id);

        if (shippingAddress == null)
        {
            return NotFound(new
            {
                message = "Alamat pengiriman tidak ditemukan."
            });
        }

        _db.ShippingAddresses.Remove(shippingAddress);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Alamat pengiriman berhasil dihapus."
        });
    }

}