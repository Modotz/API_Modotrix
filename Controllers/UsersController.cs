using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Api.DTOs.User;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public UsersController(
        AppDbContext db,
        IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }
    //public UsersController(AppDbContext db)
    //{
    //    _db = db;
    //}

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] long? roleId = null,
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

        var query = _db.Users
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search) ||
                x.Username.Contains(search) ||
                x.Email.Contains(search));
        }

        if (roleId.HasValue)
        {
            query = query.Where(x =>
                x.RoleId == roleId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        var totalCount =
            await query.CountAsync();

        var totalPage =
            (int)Math.Ceiling(
                totalCount / (double)pageSize
            );

        var data = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserDto
            {
                Id = x.Id,
                Uid = x.Uid,
                RoleId = x.RoleId,
                RoleName = x.Role != null ? x.Role.Name : string.Empty,
                Name = x.Name,
                Username = x.Username,
                Email = x.Email,
                Photo = x.Photo,
                Signature = x.Signature,
                IsActive = x.IsActive,
                LastLogin = x.LastLogin,
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

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserDto>> GetById(long id)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.DeletedAt == null)
            .Select(x => new UserDto
            {
                Id = x.Id,
                Uid = x.Uid,
                RoleId = x.RoleId,
                RoleName = x.Role != null ? x.Role.Name : string.Empty,
                Name = x.Name,
                Username = x.Username,
                Email = x.Email,
                Photo = x.Photo,
                Signature = x.Signature,
                IsActive = x.IsActive,
                LastLogin = x.LastLogin,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return user is null
            ? NotFound()
            : Ok(user);
    }

    [HttpPost("upload-photo")]
    public async Task<IActionResult> UploadPhoto(IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
        {
            return BadRequest(new
            {
                message = "Foto belum dipilih."
            });
        }

        var allowedExtensions = new[]
        {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

        var extension = Path.GetExtension(photo.FileName)
            .ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message = "Format foto harus JPG, JPEG, PNG, atau WEBP."
            });
        }

        // Pastikan wwwroot tersedia
        var webRootPath = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot"
            );
        }

        // Lokasi penyimpanan foto
        var uploadsFolder = Path.Combine(
            webRootPath,
            "uploads",
            "users"
        );

        Directory.CreateDirectory(uploadsFolder);

        // Nama file unik
        var fileName = $"{Guid.NewGuid()}{extension}";

        var filePath = Path.Combine(
            uploadsFolder,
            fileName
        );

        // Simpan foto
        using (var stream = new FileStream(
            filePath,
            FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }

        // Path yang disimpan ke database
        var photoPath = $"/uploads/users/{fileName}";

        return Ok(new
        {
            photo = photoPath
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request)
    {
        if (await _db.Users.AnyAsync(x =>
            x.Username == request.Username &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Username sudah digunakan."
            });
        }

        if (await _db.Users.AnyAsync(x =>
            x.Email == request.Email &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Email sudah digunakan."
            });
        }

        var user = new User
        {
            Uid = Guid.NewGuid().ToString(),
            RoleId = request.RoleId,
            Name = request.Name.Trim(),
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),

            Password =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password
                ),

            Photo = request.Photo,
            Signature = request.Signature,
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "User berhasil ditambahkan.",
            id = user.Id,
            uid = user.Uid
        });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateUserRequest request)
    {
        var user =
            await _db.Users.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (user is null)
        {
            return NotFound(new
            {
                message = "User tidak ditemukan."
            });
        }

        if (await _db.Users.AnyAsync(x =>
            x.Id != request.Id &&
            x.Username == request.Username &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Username sudah digunakan."
            });
        }

        if (await _db.Users.AnyAsync(x =>
            x.Id != request.Id &&
            x.Email == request.Email &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Email sudah digunakan."
            });
        }

        user.RoleId = request.RoleId;
        user.Name = request.Name.Trim();
        user.Username = request.Username.Trim();
        user.Email = request.Email.Trim();
        user.Photo = request.Photo;
        user.Signature = request.Signature;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(
            request.Password))
        {
            user.Password =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password
                );
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "User berhasil diubah."
        });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteUserRequest request)
    {
        var user =
            await _db.Users.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (user is null)
        {
            return NotFound(new
            {
                message = "User tidak ditemukan."
            });
        }

        user.IsActive = false;
        user.DeletedAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "User berhasil dihapus."
        });
    }
}