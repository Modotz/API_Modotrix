using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceListsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PriceListsController(AppDbContext db)
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

        var query = _db.PriceLists
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        // SEARCH
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                (x.Code != null && x.Code.Contains(search)) ||
                x.Name.Contains(search));
        }

        // STATUS
        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        var totalData = await query.CountAsync();

        var totalPage =
            (int)Math.Ceiling(
                totalData / (double)pageSize
            );

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
                x.Note,
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
            TotalData = totalData,
            TotalPage = totalPage
        });
    }


    // =========================
    // GET BY ID
    // =========================

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var priceList = await _db.PriceLists
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.Uid,
                x.Code,
                x.Name,
                x.Note,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (priceList == null)
            return NotFound();

        return Ok(priceList);
    }


    // =========================
    // CREATE
    // =========================

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePriceListRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama price list wajib diisi."
            });
        }

        var priceListName = request.Name.Trim();

        var priceListCode =
            string.IsNullOrWhiteSpace(request.Code)
                ? null
                : request.Code.Trim();


        // Cek data yang masih aktif
        if (await _db.PriceLists.AnyAsync(x =>
        x.DeletedAt == null &&
        (
            (!string.IsNullOrEmpty(priceListCode) &&
             x.Code == priceListCode)
            ||
            x.Name == priceListName
        )))
        {
            return Conflict(new
            {
                message = "Kode atau nama price list sudah digunakan."
            });
        }


        // =====================================================
        // PULIHKAN DATA YANG SUDAH DI-SOFT DELETE
        // =====================================================

        var deleted = await _db.PriceLists
            .Where(x =>
                x.DeletedAt != null &&
                (
                    (!string.IsNullOrEmpty(priceListCode) &&
                     x.Code == priceListCode)
                    ||
                    x.Name == priceListName
                ))
            .OrderByDescending(x => x.DeletedAt)
            .FirstOrDefaultAsync();

        if (deleted != null)
        {
            deleted.Code = priceListCode;

            deleted.Name = priceListName;

            deleted.Note =
                string.IsNullOrWhiteSpace(request.Note)
                    ? null
                    : request.Note.Trim();

            deleted.IsActive = request.IsActive;

            // Batalkan soft delete
            deleted.DeletedAt = null;

            deleted.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Price list berhasil ditambahkan (data lama dipulihkan).",

                data = deleted
            });
        }

        var priceList = new PriceList
        {
            Uid = Guid.NewGuid().ToString(),
            Code = request.Code,
            Name = request.Name.Trim(),
            Note = request.Note,
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        _db.PriceLists.Add(priceList);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Price list berhasil ditambahkan.",
            data = priceList
        });
    }


    // =========================
    // UPDATE
    // =========================

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdatePriceListRequest request)
    {
        var priceList = await _db.PriceLists
            .FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (priceList == null)
        {
            return NotFound(new
            {
                message = "Price list tidak ditemukan."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama price list wajib diisi."
            });
        }

        // Cek kode hanya jika diisi
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var codeExists =
                await _db.PriceLists.AnyAsync(x =>
                    x.Code == request.Code &&
                    x.Id != request.Id &&
                    x.DeletedAt == null);

            if (codeExists)
            {
                return BadRequest(new
                {
                    message = "Kode price list sudah digunakan."
                });
            }
        }

        priceList.Code = request.Code;
        priceList.Name = request.Name.Trim();
        priceList.Note = request.Note;
        priceList.IsActive = request.IsActive;
        priceList.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Price list berhasil diubah.",
            data = priceList
        });
    }


    // =========================
    // DELETE
    // =========================

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeletePriceListRequest request)
    {
        var priceList = await _db.PriceLists
            .FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (priceList == null)
        {
            return NotFound(new
            {
                message = "Price list tidak ditemukan."
            });
        }

        // SOFT DELETE
        priceList.DeletedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Price list berhasil dihapus."
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport()
    {
        var data = await _db.PriceLists
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PriceListDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
                Note = x.Note,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(data);
    }
}