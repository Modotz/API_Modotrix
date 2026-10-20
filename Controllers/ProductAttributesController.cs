using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductAttributesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductAttributesController(AppDbContext db)
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

        var query = _db.ProductAttributes
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        // Search berdasarkan Code / Name
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                (x.Code != null &&
                 x.Code.Contains(search)) ||
                x.Name.Contains(search));
        }

        // Filter status
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
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Uid,
                x.Code,
                x.Name,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,
                x.DeletedAt,

                VariantCount = _db.ProductVariants.Count(v =>
                    v.AttributeId == x.Id &&
                    v.DeletedAt == null)

            })
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
    public async Task<ActionResult<ProductAttribute>> GetById(long id)
    {
        var attribute = await _db.ProductAttributes
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.DeletedAt == null)
            .FirstOrDefaultAsync();

        return attribute is null
            ? NotFound()
            : Ok(attribute);
    }

    // =========================
    // CREATE
    // =========================

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductAttributeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama atribut wajib diisi."
            });
        }

        var attributeName = request.Name.Trim();

        // Cek nama atribut
        if (await _db.ProductAttributes.AnyAsync(x =>
            x.Name == attributeName &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Nama atribut sudah digunakan."
            });
        }

        var deletedAttribute =
        await _db.ProductAttributes
            .IgnoreQueryFilters()
            .Where(x =>
                x.DeletedAt != null &&
                x.Name == attributeName)
            .OrderByDescending(x => x.DeletedAt)
            .FirstOrDefaultAsync();


        if (deletedAttribute != null)
        {
            deletedAttribute.IsActive = request.IsActive;
            deletedAttribute.DeletedAt = null;
            deletedAttribute.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Atribut berhasil ditambahkan (data lama dipulihkan).",
                id = deletedAttribute.Id,
                uid = deletedAttribute.Uid,
                code = deletedAttribute.Code
            });
        }



        // Generate kode otomatis ATR001, ATR002, dst.
        // Data yang sudah di-soft-delete tetap dihitung supaya kode tidak dipakai ulang.
        var existingCodes = await _db.ProductAttributes
            .IgnoreQueryFilters()
            .Where(x =>
                x.Code != null &&
                x.Code.StartsWith("ATR"))
            .Select(x => x.Code!)
            .ToListAsync();

        int nextNumber = 1;

        foreach (var code in existingCodes)
        {
            if (code.Length > 3 &&
                int.TryParse(code.Substring(3), out int number))
            {
                if (number >= nextNumber)
                    nextNumber = number + 1;
            }
        }


        var attributeCode = $"ATR{nextNumber:D3}";

        var attribute = new ProductAttribute
        {
            Uid = Guid.NewGuid().ToString(),
            Code = attributeCode,
            Name = attributeName,
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        _db.ProductAttributes.Add(attribute);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                message = ex.InnerException?.Message ?? ex.Message
            });
        }

        return Ok(new
        {
            message = "Atribut berhasil ditambahkan.",
            id = attribute.Id,
            uid = attribute.Uid
        });
    }


    // =========================
    // UPDATE
    // =========================

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateProductAttributeRequest request)
    {
        var attribute =
            await _db.ProductAttributes.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (attribute is null)
        {
            return NotFound(new
            {
                message = "Atribut tidak ditemukan."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama atribut wajib diisi."
            });
        }

        var attributeName = request.Name.Trim();

        // Cek nama duplikat
        if (await _db.ProductAttributes.AnyAsync(x =>
            x.Id != request.Id &&
            x.Name == attributeName &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Nama atribut sudah digunakan."
            });
        }






        attribute.Name = attributeName;
        attribute.IsActive = request.IsActive;
        attribute.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Atribut berhasil diubah."
        });
    }

    // =========================
    // DELETE
    // =========================

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteProductAttributeRequest request)
    {
        var attribute =
            await _db.ProductAttributes.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (attribute is null)
        {
            return NotFound(new
            {
                message = "Atribut tidak ditemukan."
            });
        }

        // Soft Delete
        attribute.IsActive = false;
        attribute.DeletedAt = DateTime.Now;
        attribute.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Atribut berhasil dihapus."
        });
    }

    // =========================
    // EXPORT
    // =========================

    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport()
    {
        var data = await _db.ProductAttributes
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Uid,
                x.Code,
                x.Name,

                VariantCount = _db.ProductVariants.Count(v =>
                    v.AttributeId == x.Id &&
                    v.DeletedAt == null),

                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(data);
    }
}