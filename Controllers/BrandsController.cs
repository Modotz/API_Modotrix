using ERP.Api.Data;
using ERP.Api.DTOs.Brand;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public BrandsController(
        AppDbContext db,
        IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    // =========================================================
    // GET ALL BRANDS
    // =========================================================

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

        // HANYA DATA YANG BELUM DI-SOFT DELETE
        var query = _db.Brands
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        // SEARCH
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search));
        }

        // FILTER IS ACTIVE
        // IsActive hanya untuk status Aktif / Nonaktif,
        // bukan sebagai penanda soft delete.
        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        var totalCount =
            await query.CountAsync();

        var totalPage =
            (int)Math.Ceiling(
                totalCount / (double)pageSize);

        var data =
            await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BrandDto
                {
                    Id = x.Id,
                    Uid = x.Uid,
                    Name = x.Name,
                    Image = x.Image,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

        return Ok(new
        {
            data,
            page,
            pageSize,
            totalCount,
            totalPage
        });
    }


    // =========================================================
    // GET BRAND BY ID
    // =========================================================

    [HttpGet("{id:long}")]
    public async Task<ActionResult<BrandDto>> GetById(long id)
    {
        var brand =
            await _db.Brands
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.DeletedAt == null)
                .Select(x => new BrandDto
                {
                    Id = x.Id,
                    Uid = x.Uid,
                    Name = x.Name,
                    Image = x.Image,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync();

        return brand is null
            ? NotFound(new
            {
                message = "Brand tidak ditemukan."
            })
            : Ok(brand);
    }


    // =========================================================
    // UPLOAD IMAGE
    // =========================================================

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest(new
            {
                message = "Gambar belum dipilih."
            });
        }

        var allowedExtensions = new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        var extension =
            Path
                .GetExtension(image.FileName)
                .ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message =
                    "Format gambar harus JPG, JPEG, PNG, atau WEBP."
            });
        }

        // Pastikan wwwroot tersedia
        var webRootPath =
            _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath =
                Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");
        }

        // Folder penyimpanan image brand
        var uploadsFolder =
            Path.Combine(
                webRootPath,
                "uploads",
                "brands");

        Directory.CreateDirectory(
            uploadsFolder);

        // Nama file unik
        var fileName =
            $"{Guid.NewGuid()}{extension}";

        var filePath =
            Path.Combine(
                uploadsFolder,
                fileName);

        // Simpan file
        using (var stream =
               new FileStream(
                   filePath,
                   FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        // Path yang disimpan ke database
        var imagePath =
            $"/uploads/brands/{fileName}";

        return Ok(new
        {
            image = imagePath
        });
    }


    // =========================================================
    // CREATE BRAND
    // =========================================================

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromForm] CreateBrands request)
    {
        // Normalisasi nama:
        // - hapus spasi di awal
        // - hapus spasi di akhir
        var name =
            request.Name.Trim();

        // Normalisasi untuk pengecekan
        // tidak membedakan huruf besar-kecil.
        var normalizedName =
            name.ToLower();

        // =====================================================
        // 1. CEK BRAND AKTIF / BELUM DIHAPUS
        // =====================================================

        var activeBrand =
            await _db.Brands
                .FirstOrDefaultAsync(x =>
                    x.DeletedAt == null &&
                    x.Name.Trim().ToLower() == normalizedName);

        if (activeBrand is not null)
        {
            return Conflict(new
            {
                message =
                    "Nama brand sudah digunakan."
            });
        }


        // =====================================================
        // 2. CEK BRAND YANG SUDAH DI-SOFT DELETE
        // =====================================================

        var deletedBrand =
            await _db.Brands
                .Where(x =>
                    x.DeletedAt != null &&
                    x.Name.Trim().ToLower() == normalizedName)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

        // =====================================================
        // 3. JIKA DITEMUKAN -> PULIHKAN DATA LAMA
        // =====================================================

        if (deletedBrand is not null)
        {
            // ID tetap
            // UID tetap

            deletedBrand.Name =
                name;

            deletedBrand.Image =
                request.Image;

            deletedBrand.IsActive =
                request.IsActive;

            deletedBrand.DeletedAt =
                null;

            deletedBrand.UpdatedAt =
                DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Brand berhasil dipulihkan.",

                id =
                    deletedBrand.Id,

                uid =
                    deletedBrand.Uid
            });
        }


        // =====================================================
        // 4. JIKA TIDAK ADA -> INSERT BARU
        // =====================================================

        var brand = new Brand
        {
            Uid =
                Guid.NewGuid().ToString(),

            Name =
                name,

            Image =
                request.Image,

            IsActive =
                request.IsActive,

            CreatedAt =
                DateTime.Now,

            UpdatedAt =
                null,

            DeletedAt =
                null
        };

        _db.Brands.Add(brand);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Brand berhasil ditambahkan.",

            id =
                brand.Id,

            uid =
                brand.Uid
        });
    }


    // =========================================================
    // UPDATE BRAND
    // =========================================================

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromForm] UpdateBrands request)
    {
        // Hanya boleh update brand yang belum dihapus
        var brand =
            await _db.Brands
                .FirstOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.DeletedAt == null);

        if (brand is null)
        {
            return NotFound(new
            {
                message =
                    "Brand tidak ditemukan."
            });
        }

        // Normalisasi nama
        var name =
            request.Name.Trim();

        var normalizedName =
            name.ToLower();

        // =====================================================
        // CEK NAMA KEMBAR
        // HANYA TERHADAP DATA deleted_at IS NULL
        // DAN ID BERBEDA
        // =====================================================

        var duplicateBrand =
            await _db.Brands
                .AnyAsync(x =>
                    x.Id != request.Id &&
                    x.DeletedAt == null &&
                    x.Name.Trim().ToLower() == normalizedName);

        if (duplicateBrand)
        {
            return Conflict(new
            {
                message =
                    "Nama brand sudah digunakan."
            });
        }

        // =====================================================
        // UPDATE DATA
        // =====================================================

        brand.Name =
            name;

        brand.Image =
            request.Image;

        // IsActive hanya menyimpan status
        // Aktif / Nonaktif.
        brand.IsActive =
            request.IsActive;

        brand.UpdatedAt =
            DateTime.Now;

        // DeletedAt tetap NULL
        // karena brand sedang aktif sebagai data.
        brand.DeletedAt =
            null;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Brand berhasil diubah."
        });
    }


    // =========================================================
    // SOFT DELETE BRAND
    // =========================================================

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteBrands request)
    {
        // Hanya cari data yang belum dihapus
        var brand =
            await _db.Brands
                .FirstOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.DeletedAt == null);

        if (brand is null)
        {
            return NotFound(new
            {
                message =
                    "Brand tidak ditemukan."
            });
        }

        // =====================================================
        // SOFT DELETE
        // =====================================================

        // JANGAN mengubah IsActive.
        // IsActive hanya digunakan untuk status
        // Aktif / Nonaktif.
        brand.DeletedAt =
            DateTime.Now;

        brand.UpdatedAt =
            DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Brand berhasil dihapus."
        });
    }
}