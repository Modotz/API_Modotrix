using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkCentersController : ControllerBase
{
    private readonly AppDbContext _db;

    public WorkCentersController(AppDbContext db)
    {
        _db = db;
    }

    // =========================
    // GET ALL
    // =========================

    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? search = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 10)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var query = _db.WorkCenters
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Code.Contains(search) ||
                x.Name.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        var totalItems = await query.CountAsync();

        var data = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Uid,
                x.Code,
                x.Name,
                x.CostPerHour,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            Data = data,
            Page = page,
            PageSize = pageSize,
            TotalData = totalItems,
            TotalCount = totalItems,
            TotalPage = (int)Math.Ceiling(
                totalItems / (double)pageSize)
        });
    }

    // =========================
    // GET BY ID
    // =========================

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var workCenter = await _db.WorkCenters
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.DeletedAt == null);

        if (workCenter is null)
            return NotFound();

        return Ok(workCenter);
    }

    // =========================
    // CREATE
    // =========================

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateWorkCenterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Kode wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Nama wajib diisi.");
        }

        var code = request.Code.Trim();
        var name = request.Name.Trim();

        var exists = await _db.WorkCenters
            .AnyAsync(x =>
                x.Code == code &&
                x.DeletedAt == null);

        if (exists)
        {
            return BadRequest(
                "Kode Work Center sudah digunakan.");
        }

        // ===== PULIHKAN DATA YANG SUDAH DIHAPUS (SOFT DELETE) =====
        var deleted = await _db.WorkCenters
            .Where(x =>
                x.DeletedAt != null &&
                (x.Name == name || x.Code == code))
            .OrderByDescending(x => x.DeletedAt)
            .FirstOrDefaultAsync();

        if (deleted != null)
        {
            deleted.Code = code;
            deleted.Name = name;
            deleted.CostPerHour = request.CostPerHour;
            deleted.IsActive = request.IsActive;
            deleted.DeletedAt = null;
            deleted.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Work Center berhasil ditambahkan (data lama dipulihkan).",

                id = deleted.Id,

                uid = deleted.Uid,

                code = deleted.Code
            });
        }


        var workCenter = new WorkCenter
        {
            Uid = Guid.NewGuid().ToString(),
            Code = code,
            Name = name,
            CostPerHour = request.CostPerHour,
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };
        _db.WorkCenters.Add(workCenter);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Work Center berhasil ditambahkan.",

            id = workCenter.Id,

            uid = workCenter.Uid,

            code = workCenter.Code
        });
    }

    // =========================
    // UPDATE
    // =========================

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateWorkCenterRequest request)
    {
        var workCenter = await _db.WorkCenters
            .FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (workCenter is null)
            return NotFound("Work Center tidak ditemukan.");

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Kode wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Nama wajib diisi.");
        }

        var code = request.Code.Trim();

        var duplicate = await _db.WorkCenters
            .AnyAsync(x =>
                x.Id != request.Id &&
                x.Code == code &&
                x.DeletedAt == null);

        if (duplicate)
        {
            return BadRequest(
                "Kode Work Center sudah digunakan.");
        }

        workCenter.Code = code;
        workCenter.Name = request.Name.Trim();
        workCenter.CostPerHour = request.CostPerHour;
        workCenter.IsActive = request.IsActive;
        workCenter.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(workCenter);
    }

    // =========================
    // DELETE
    // =========================

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteWorkCenterRequest request)
    {
        var workCenter = await _db.WorkCenters
            .FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (workCenter is null)
            return NotFound(
                "Work Center tidak ditemukan.");

        workCenter.DeletedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport()
    {
        var data = await _db.WorkCenters
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Code)
            .Select(x => new WorkCenterDto
            {
               Id = x.Id,
               Code = x.Code,
               Name = x.Name,
               CostPerHour = x.CostPerHour,
               IsActive = x.IsActive
            })
            .ToListAsync();

        return Ok(data);
    }
}