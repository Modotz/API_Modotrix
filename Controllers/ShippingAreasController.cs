using ERP.Api.Data;
using ERP.Api.DTOs.ShippingArea;
using ERP.Api.DTOs.Unit;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/shipping-areas")]
public class ShippingAreas : ControllerBase
{
    private readonly AppDbContext _db;

    public ShippingAreas(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var query = _db.ShippingAreas
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search) || x.Code.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalData =
            await query.CountAsync();

        var totalPage =
            (int)Math.Ceiling(
                totalData / (double)pageSize
            );

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ShippingAreaDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
                Price = x.Price,
                Description = x.Description,
                IsActive = x.IsActive,
                //CreatedAt = x.CreatedAt,
                //UpdatedAt = x.UpdatedAt,
                //DeletedAt = x.DeletedAt
            })
            .ToListAsync();

        return Ok(new
        {
            data,
            page,
            pageSize,
            totalCount,
            totalData,
            totalPage
        });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ShippingAreaDto>> GetById(long id)
    {
        var shippingArea = await _db.ShippingAreas
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.DeletedAt == null)
            .Select(x => new ShippingAreaDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
                Price = x.Price,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return shippingArea is null
            ? NotFound()
            : Ok(shippingArea);
    }

    //[HttpPost("create")]
    //public async Task<IActionResult> Create(
    //    [FromBody] CreateShippingAreaRequest request)
    //{
    //    if (await _db.ShippingAreas.AnyAsync(x =>
    //        x.Code == request.Code &&
    //        x.DeletedAt == null))
    //    {
    //        return Conflict(new
    //        {
    //            message = "Kode Area Pengiriman sudah digunakan."
    //        });
    //    }

    //    var shippingArea = new ShippingArea 
    //    {
    //        Uid = Guid.NewGuid().ToString(),
    //        Code = request.Code.Trim(),
    //        Name = request.Name.Trim(),
    //        Price = request.Price,
    //        Description = request.Description?.Trim(),
    //        IsActive = request.IsActive,
    //        CreatedAt = DateTime.Now
    //    };

    //    _db.ShippingAreas.Add(shippingArea);

    //    await _db.SaveChangesAsync();

    //    return Ok(new
    //    {
    //        message = "Area Pengiriman berhasil ditambahkan.",
    //        id = shippingArea.Id,
    //        uid = shippingArea.Uid
    //    });
    //}

    [HttpPost("create")]
    public async Task<IActionResult> Create(
    [FromBody] CreateShippingAreaRequest request)
    {
        var code = request.Code.Trim();
         
        // CARI AREA PENGIRIMAN BERDASARKAN CODE 
        var existingShippingArea = await _db.ShippingAreas
            .FirstOrDefaultAsync(x =>
                x.Code == code);
         
        // CODE SUDAH ADA DAN MASIH AKTIF 
        if (existingShippingArea != null &&
            existingShippingArea.DeletedAt == null)
        {
            return Conflict(new
            {
                message = "Kode Area Pengiriman sudah digunakan."
            });
        }
         
        //  RESTORE DATA LAMA 
        if (existingShippingArea != null &&
            existingShippingArea.DeletedAt != null)
        {
            existingShippingArea.Code = code;
            existingShippingArea.Name = request.Name.Trim();
            existingShippingArea.Price = request.Price;
            existingShippingArea.Description =
                request.Description?.Trim();

            // Aktifkan kembali
            existingShippingArea.IsActive = true;

            // Hapus status soft delete
            existingShippingArea.DeletedAt = null;

            existingShippingArea.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Area Pengiriman sebelumnya ditemukan dan berhasil dipulihkan.",
                id = existingShippingArea.Id,
                uid = existingShippingArea.Uid
            });
        }
         
        //  CODE BELUM PERNAH ADA 
        var shippingArea = new ShippingArea
        {
            Uid = Guid.NewGuid().ToString(),
            Code = code,
            Name = request.Name.Trim(),
            Price = request.Price,
            Description = request.Description?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        _db.ShippingAreas.Add(shippingArea);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Area Pengiriman berhasil ditambahkan.",
            id = shippingArea.Id,
            uid = shippingArea.Uid
        });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateShippingAreaRequest request)
    {
        var shippingArea =
            await _db.ShippingAreas.FirstOrDefaultAsync(x =>
                x.Id == request.Id 
                && x.DeletedAt == null);

        if (shippingArea is null)
        {
            return NotFound(new
            {
                message = "Area Pengiriman tidak ditemukan."
            });
        }

        if (await _db.ShippingAreas.AnyAsync(x =>
            x.Id != request.Id &&
            x.Code == request.Code))
        //&& x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Kode Area Pengiriman sudah digunakan."
            });
        }

        shippingArea.Code = request.Code.Trim();
        shippingArea.Name = request.Name.Trim();
        shippingArea.Price = request.Price;
        shippingArea.Description = request.Description?.Trim();
        shippingArea.IsActive = request.IsActive;
        shippingArea.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Area Pengiriman berhasil diubah."
        });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
       [FromBody] DeleteShippingAreaRequest request)
    {
        var shippingArea =
            await _db.ShippingAreas.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (shippingArea is null)
        {
            return NotFound(new
            {
                message = "Area Pengiriman tidak ditemukan."
            });
        }

        shippingArea.IsActive = false;
        shippingArea.DeletedAt = DateTime.Now;
        shippingArea.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Area Pengiriman berhasil dihapus."
        });
    }

    //hard-delete
    //_db.Menus.Remove(menu);
    //await _db.SaveChangesAsync();
    //return Ok(new
    //{
    //    message = "Menu berhasil dihapus."
    //});

    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport()
    {
        var data = await _db.ShippingAreas
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ShippingAreaDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
                Price = x.Price,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return Ok(data);
    }
}
    

