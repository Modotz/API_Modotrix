using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.DTOs.Product;
using ERP.Api.DTOs.Unit;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesForcesController : ControllerBase
{
    private readonly AppDbContext _db;

    public SalesForcesController(AppDbContext db)
    {
        _db = db;
    }

    // =========================
    // GET ALL
    // =========================

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

        var query = _db.SalesForces
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        // Search kode / nama / phone / email / area
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Code.Contains(search) ||
                x.Name.Contains(search) ||
                (x.Phone != null && x.Phone.Contains(search)) ||
                (x.Email != null && x.Email.Contains(search)) ||
                (x.Area != null && x.Area.Contains(search))
            );
        }

        // Filter status
        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        var totalData =
            await query.CountAsync();

        var totalPage =
            (int)Math.Ceiling(
                totalData / (double)pageSize
            );

        var data =
            await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        return Ok(new
        {
            data,
            page,
            pageSize,
            totalData,
            totalPage
        });
    }


    // =========================
    // GET BY ID
    // =========================

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SalesForce>> GetById(
        long id)
    {
        var salesForce =
            await _db.SalesForces
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.DeletedAt == null)
                .FirstOrDefaultAsync();

        return salesForce is null
            ? NotFound()
            : Ok(salesForce);
    }


    // =========================
    // CREATE
    // =========================

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesForceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new
            {
                message = "Kode sales force wajib diisi."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama sales force wajib diisi."
            });
        }

        var code = request.Code.Trim();
        var name = request.Name.Trim();

        // Cek kode
        if (await _db.SalesForces.AnyAsync(x =>
            x.Code == code &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Kode sales force sudah digunakan."
            });
        }

        // ===== PULIHKAN DATA YANG SUDAH DIHAPUS (SOFT DELETE) =====
        var deleted = await _db.SalesForces
            .Where(x =>
                x.DeletedAt != null &&
                (x.Name == name || x.Code == code))
            .OrderByDescending(x => x.DeletedAt)
            .FirstOrDefaultAsync();

        if (deleted != null)
        {
            deleted.Code = code;
            deleted.Name = name;
            deleted.Phone = string.IsNullOrWhiteSpace(request.Phone)
                ? null
                : request.Phone.Trim();
            deleted.Email = string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim();
            deleted.Area = string.IsNullOrWhiteSpace(request.Area)
                ? null
                : request.Area.Trim();
            deleted.IsActive = request.IsActive;
            deleted.DeletedAt = null;
            deleted.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Sales force berhasil ditambahkan (data lama dipulihkan).",
                id = deleted.Id,
                uid = deleted.Uid
            });
        }

        var salesForce = new SalesForce
        {
            Uid = Guid.NewGuid().ToString(),
            Code = code,
            Name = name,
            Phone = string.IsNullOrWhiteSpace(request.Phone)
                ? null
                : request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim(),
            Area = string.IsNullOrWhiteSpace(request.Area)
                ? null
                : request.Area.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        _db.SalesForces.Add(salesForce);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Sales force berhasil ditambahkan.",
            id = salesForce.Id,
            uid = salesForce.Uid
        });
    }


    // =========================
    // UPDATE
    // =========================

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateSalesForceRequest request)
    {
        var salesForce =
            await _db.SalesForces
                .FirstOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.DeletedAt == null);

        if (salesForce is null)
        {
            return NotFound(new
            {
                message = "Sales force tidak ditemukan."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new
            {
                message = "Kode sales force wajib diisi."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama sales force wajib diisi."
            });
        }

        var code = request.Code.Trim();
        var name = request.Name.Trim();

        // Cek kode duplikat
        if (await _db.SalesForces.AnyAsync(x =>
            x.Id != request.Id &&
            x.Code == code &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Kode sales force sudah digunakan."
            });
        }

        salesForce.Code = code;
        salesForce.Name = name;

        salesForce.Phone =
            string.IsNullOrWhiteSpace(request.Phone)
                ? null
                : request.Phone.Trim();

        salesForce.Email =
            string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim();

        salesForce.Area =
            string.IsNullOrWhiteSpace(request.Area)
                ? null
                : request.Area.Trim();

        salesForce.IsActive =
            request.IsActive;

        salesForce.UpdatedAt =
            DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Sales force berhasil diubah."
        });
    }


    // =========================
    // DELETE
    // =========================

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteSalesForceRequest request)
    {
        var salesForce =
            await _db.SalesForces
                .FirstOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.DeletedAt == null);

        if (salesForce is null)
        {
            return NotFound(new
            {
                message = "Sales force tidak ditemukan."
            });
        }

        // Soft delete
        salesForce.IsActive = false;
        salesForce.DeletedAt = DateTime.Now;
        salesForce.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Sales force berhasil dihapus."
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport()
    {
        var data = await _db.SalesForces
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SalesForceDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Phone = x.Phone,
                Email = x.Email,
                Area = x.Area,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return Ok(data);
    }
}