using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.DTOs.Product;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductCategoriesController(AppDbContext db)
    {
        _db = db;
    } 

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] long? parentId = null,
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

        var query = _db.ProductCategories
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        // Search berdasarkan code / name
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                (x.Code != null &&
                 x.Code.Contains(search)) ||
                x.Name.Contains(search));
        }

        // Filter berdasarkan induk
        if (parentId.HasValue)
        {
            query = query.Where(x =>
                x.ParentId == parentId.Value);
        }

        // Filter status
        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        // Ambil semua data yang lolos filter, urut nama
        var all = await query
            .OrderBy(x => x.Name)
            .ToListAsync();

        var allIds = all.Select(x => x.Id).ToHashSet();

        // Induk = tanpa parent, atau parent-nya tidak ikut hasil filter
        var roots = all
            .Where(x => x.ParentId == null
                     || x.ParentId == 0
                     || !allIds.Contains(x.ParentId.Value))
            .ToList();

        var childrenLookup = all
            .Where(x => x.ParentId != null
                     && x.ParentId != 0
                     && allIds.Contains(x.ParentId.Value))
            .ToLookup(x => x.ParentId!.Value);

        // Susun berurutan: induk, lalu anak-anaknya
        var ordered = new List<ProductCategory>();

        foreach (var root in roots)
        {
            ordered.Add(root);
            ordered.AddRange(childrenLookup[root.Id]);
        }

        var totalData = ordered.Count;

        var totalPage =
            (int)Math.Ceiling(totalData / (double)pageSize);

        // Paginasi per baris: tetap 10 baris per halaman
        var data = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            data,
            page,
            pageSize,
            totalData,
            totalPage
        });

    } 

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductCategory>> GetById(long id)
    {
        var category = await _db.ProductCategories
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.DeletedAt == null)
            .FirstOrDefaultAsync();

        return category is null
            ? NotFound()
            : Ok(category);
    }
     

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama kategori wajib diisi."
            });
        }

        var categoryName = request.Name.Trim();

        // Cek parent
        if (request.ParentId.HasValue)
        {
            var parentExists =
                await _db.ProductCategories.AnyAsync(x =>
                    x.Id == request.ParentId.Value &&
                    x.DeletedAt == null);

            if (!parentExists)
            {
                return BadRequest(new
                {
                    message = "Induk kategori tidak ditemukan."
                });
            }
        }

        // Cek nama kategori
        if (await _db.ProductCategories.AnyAsync(x =>
            x.Name == categoryName &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Nama kategori sudah digunakan."
            });
        }

        // Cek code jika diisi
        var categoryCode =
            string.IsNullOrWhiteSpace(request.Code)
                ? null
                : request.Code.Trim();

        if (categoryCode != null &&
            await _db.ProductCategories.AnyAsync(x =>
                x.Code == categoryCode &&
                x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Code kategori sudah digunakan."
            });
        }

        // ===== PULIHKAN DATA YANG SUDAH DIHAPUS (SOFT DELETE) =====
        var deleted = await _db.ProductCategories
            .Where(x =>
                x.DeletedAt != null &&
                (x.Name == categoryName ||
                 (categoryCode != null && x.Code == categoryCode)))
            .OrderByDescending(x => x.DeletedAt)
            .FirstOrDefaultAsync();

        if (deleted != null)
        {
            deleted.ParentId = request.ParentId;
            deleted.Code = categoryCode;
            deleted.Name = categoryName;
            deleted.IsActive = request.IsActive;
            deleted.DeletedAt = null;
            deleted.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Kategori berhasil ditambahkan (data lama dipulihkan).",
                id = deleted.Id,
                uid = deleted.Uid
            });
        }

        var category = new ProductCategory
        {
            ParentId = request.ParentId,
            Uid = Guid.NewGuid().ToString(),
            Code = categoryCode,
            Name = categoryName,
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        _db.ProductCategories.Add(category);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Kategori berhasil ditambahkan.",
            id = category.Id,
            uid = category.Uid
        });
    }
     

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateProductCategoryRequest request)
    {
        var category =
            await _db.ProductCategories.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (category is null)
        {
            return NotFound(new
            {
                message = "Kategori tidak ditemukan."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama kategori wajib diisi."
            });
        }

        // Tidak boleh menjadikan dirinya sendiri sebagai parent
        if (request.ParentId == request.Id)
        {
            return BadRequest(new
            {
                message = "Kategori tidak dapat menjadi induk dirinya sendiri."
            });
        }

        // Cek parent
        if (request.ParentId.HasValue)
        {
            var parentExists =
                await _db.ProductCategories.AnyAsync(x =>
                    x.Id == request.ParentId.Value &&
                    x.DeletedAt == null);

            if (!parentExists)
            {
                return BadRequest(new
                {
                    message = "Induk kategori tidak ditemukan."
                });
            }
        }

        var categoryName = request.Name.Trim();

        // Cek nama duplikat
        if (await _db.ProductCategories.AnyAsync(x =>
            x.Id != request.Id &&
            x.Name == categoryName &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Nama kategori sudah digunakan."
            });
        }

        var categoryCode =
            string.IsNullOrWhiteSpace(request.Code)
                ? null
                : request.Code.Trim();

        // Cek code duplikat
        if (categoryCode != null &&
            await _db.ProductCategories.AnyAsync(x =>
                x.Id != request.Id &&
                x.Code == categoryCode &&
                x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Code kategori sudah digunakan."
            });
        }

        category.ParentId = request.ParentId;
        category.Code = categoryCode;
        category.Name = categoryName;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Kategori berhasil diubah."
        });
    }
     

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteProductCategoryRequest request)
    {

        var category =
            await _db.ProductCategories.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (category is null)
        {
            return NotFound(new
            {
                message = "Kategori tidak ditemukan."
            });
        }

        // Cek apakah kategori punya subkategori
        var hasChildren =
            await _db.ProductCategories.AnyAsync(x =>
                x.ParentId == request.Id &&
                x.DeletedAt == null);

        if (hasChildren)
        {
            return BadRequest(new
            {
                message =
                    "Kategori tidak dapat dihapus karena masih memiliki subkategori."
            });
        }

        // Soft delete
        category.IsActive = false;
        category.DeletedAt = DateTime.Now;
        category.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Kategori berhasil dihapus."
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport()
    {
        var data = await _db.ProductCategories
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProductCategoryDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(data);
    }
}