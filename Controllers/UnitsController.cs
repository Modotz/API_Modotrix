using ERP.Api.Data;
using ERP.Api.DTOs.Unit;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/units")]
public class Units : ControllerBase
{
    private readonly AppDbContext _db;

    public Units(AppDbContext db)
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

        var query = _db.Units
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search) || x.Code.Contains(search));
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
            .Select(x => new UnitDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
                Symbol = x.Symbol,
                Description = x.Description
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
    public async Task<ActionResult<UnitDto>> GetById(long id)
    {
        var unit = await _db.Units
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.DeletedAt == null)
            .Select(x => new UnitDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
                Symbol = x.Symbol,
                Description = x.Description,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return unit is null
            ? NotFound()
            : Ok(unit);
    }

    //[HttpPost("create")]
    //public async Task<IActionResult> Create(
    //    [FromBody] CreateUnitRequest request)
    //{
    //    if (await _db.Units.AnyAsync(x =>
    //        x.Code == request.Code &&
    //        x.DeletedAt == null))
    //    {
    //        return Conflict(new
    //        {
    //            message = "Kode Satuan sudah digunakan."
    //        });
    //    }

    //    var unit = new Unit 
    //    {
    //        Uid = Guid.NewGuid().ToString(),
    //        Code = request.Code.Trim(),
    //        Name = request.Name.Trim(),
    //        Symbol = request.Symbol,
    //        Description = request.Description?.Trim(),
    //        CreatedAt = DateTime.Now
    //    };

    //    _db.Units.Add(unit);

    //    await _db.SaveChangesAsync();

    //    return Ok(new
    //    {
    //        message = "Satuan berhasil ditambahkan.",
    //        id = unit.Id,
    //        uid = unit.Uid
    //    });
    //}

    [HttpPost("create")]
    public async Task<IActionResult> Create(
    [FromBody] CreateUnitRequest request)
    {
        var code = request.Code.Trim();
          
        var existingUnit = await _db.Units
            .FirstOrDefaultAsync(x =>
                x.Code == code);
         
        // CODE SUDAH ADA DAN UNIT MASIH AKTIF 
        if (existingUnit != null &&
            existingUnit.DeletedAt == null)
        {
            return Conflict( "Kode Satuan sudah digunakan.");
        }
         
        //  RESTORE DATA LAMA 
        if (existingUnit != null &&
            existingUnit.DeletedAt != null)
        {
            existingUnit.Code = code;
            existingUnit.Name = request.Name.Trim();
            existingUnit.Symbol = request.Symbol;
            existingUnit.Description = request.Description?.Trim();

            // Hapus status soft delete
            existingUnit.DeletedAt = null;

            existingUnit.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok("Satuan sebelumnya berhasil dipulihkan dan dapat digunakan kembali.");
        }
         
        // CODE BELUM PERNAH ADA 
        var unit = new Unit
        {
            Uid = Guid.NewGuid().ToString(),
            Code = code,
            Name = request.Name.Trim(),
            Symbol = request.Symbol,
            Description = request.Description?.Trim(),
            CreatedAt = DateTime.Now
        };

        _db.Units.Add(unit);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Satuan berhasil ditambahkan.",
            id = unit.Id,
            uid = unit.Uid
        });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateUnitRequest request)
    {
        var unit =
            await _db.Units.FirstOrDefaultAsync(x =>
                x.Id == request.Id 
                && x.DeletedAt == null);

        if (unit is null)
        {
            return NotFound( "Satuan tidak ditemukan." );
        }

        if (await _db.Units.AnyAsync(x =>
            x.Id != request.Id &&
            x.Code == request.Code))
        //&& x.DeletedAt == null))
        {
            return Conflict ("Kode Satuan sudah digunakan.");
        }

        unit.Code = request.Code.Trim();
        unit.Name = request.Name.Trim();
        unit.Symbol = request.Symbol;
        unit.Description = request.Description?.Trim();
        unit.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok( "Satuan berhasil diubah." );
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
       [FromBody] DeleteUnitRequest request)
    {
        var unit =
            await _db.Units.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (unit is null)
        {
            return NotFound(new
            {
                message = "Satuan tidak ditemukan."
            });
        }

        unit.DeletedAt = DateTime.Now;
        unit.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok( "Satuan berhasil dihapus." );
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
        var data = await _db.Units
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new UnitDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
            })
            .ToListAsync();

        return Ok(data);
    }
}
    

