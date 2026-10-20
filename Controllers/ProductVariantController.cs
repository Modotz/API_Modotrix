using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/product-attributes/{attributeId:long}/variants")]
public class ProductVariantsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductVariantsController(AppDbContext db)
    {
        _db = db;
    }

    // =========================
    // GET ALL VARIANT
    // =========================

    [HttpGet]
    public async Task<IActionResult> GetAll(
        long attributeId,
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

        // Pastikan attribute ada
        var attributeExists = await _db.ProductAttributes
            .AnyAsync(x =>
                x.Id == attributeId &&
                x.DeletedAt == null);

        if (!attributeExists)
        {
            return NotFound(new
            {
                message = "Atribut tidak ditemukan."
            });
        }

        var query = _db.ProductVariants
            .AsNoTracking()
            .Where(x =>
                x.AttributeId == attributeId &&
                x.DeletedAt == null);

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
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
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
    public async Task<ActionResult<ProductVariant>> GetById(
        long attributeId,
        long id)
    {
        var variant = await _db.ProductVariants
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.AttributeId == attributeId &&
                x.DeletedAt == null)
            .FirstOrDefaultAsync();

        return variant is null
            ? NotFound()
            : Ok(variant);
    }

    // =========================
    // CREATE
    // =========================

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        long attributeId,
        [FromBody] CreateProductVariantRequest request)
    {
        // Pastikan attribute ada
        var attributeExists = await _db.ProductAttributes
            .AnyAsync(x =>
                x.Id == attributeId &&
                x.DeletedAt == null);

        if (!attributeExists)
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
                message = "Nama varian wajib diisi."
            });
        }

        var variantName = request.Name.Trim();

        // Cek nama variant dalam attribute yang sama
        if (await _db.ProductVariants.AnyAsync(x =>
            x.AttributeId == attributeId &&
            x.Name == variantName &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Nama varian sudah digunakan."
            });
        }

        // Generate kode otomatis VAR001, VAR002, dst.
        var existingCodes = await _db.ProductVariants
            .Where(x =>
                x.Code != null &&
                x.Code.StartsWith("VAR") &&
                x.DeletedAt == null)
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

        var variantCode = $"VAR{nextNumber:D3}";

        var variant = new ProductVariant
        {
            Uid = Guid.NewGuid().ToString(),

            // Ambil dari URL, bukan dari input user
            AttributeId = attributeId,

            Code = variantCode,
            Name = variantName,

            DefHeight = request.DefHeight,
            DefWidth = request.DefWidth,
            DefLength = request.DefLength,

            Description = request.Description,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,

            CreatedAt = DateTime.Now
        };

        _db.ProductVariants.Add(variant);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Varian berhasil ditambahkan.",
            id = variant.Id,
            uid = variant.Uid
        });
    }

    // =========================
    // UPDATE
    // =========================

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        long attributeId,
        [FromBody] UpdateProductVariantRequest request)
    {
        var variant = await _db.ProductVariants
            .FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.AttributeId == attributeId &&
                x.DeletedAt == null);

        if (variant is null)
        {
            return NotFound(new
            {
                message = "Varian tidak ditemukan."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama varian wajib diisi."
            });
        }

        var variantName = request.Name.Trim();

        // Cek nama duplikat dalam attribute yang sama
        if (await _db.ProductVariants.AnyAsync(x =>
            x.Id != request.Id &&
            x.AttributeId == attributeId &&
            x.Name == variantName &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Nama varian sudah digunakan."
            });
        }

        variant.Name = variantName;

        variant.DefHeight = request.DefHeight;
        variant.DefWidth = request.DefWidth;
        variant.DefLength = request.DefLength;

        variant.Description = request.Description;
        variant.SortOrder = request.SortOrder;
        variant.IsActive = request.IsActive;

        variant.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Varian berhasil diubah."
        });
    }

    // =========================
    // REORDER
    // =========================

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder(
        long attributeId,
        [FromBody] List<ReorderProductVariantRequest> request)
    {
        var variants = await _db.ProductVariants
            .Where(x =>
                x.AttributeId == attributeId &&
                x.DeletedAt == null)
            .ToListAsync();

        foreach (var item in request)
        {
            var variant = variants
                .FirstOrDefault(x => x.Id == item.Id);

            if (variant != null)
            {
                variant.SortOrder = item.SortOrder;
                variant.UpdatedAt = DateTime.Now;
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Urutan variant berhasil disimpan."
        });
    }

    // =========================
    // DELETE
    // =========================

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        long attributeId,
        [FromBody] DeleteProductVariantRequest request)
    {
        var variant = await _db.ProductVariants
            .FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.AttributeId == attributeId &&
                x.DeletedAt == null);

        if (variant is null)
        {
            return NotFound(new
            {
                message = "Varian tidak ditemukan."
            });
        }

        // Soft Delete
        variant.IsActive = false;
        variant.DeletedAt = DateTime.Now;
        variant.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Varian berhasil dihapus."
        });
    }

    [HttpGet("{parentId:long}/variants")]
    public async Task<IActionResult> GetVariants(long parentId)
    {
        var variants = await _db.Products
            .Where(x =>
                x.ParentId == parentId &&
                x.DeletedAt == null)
            .ToListAsync();

        return Ok(variants);
    }
}