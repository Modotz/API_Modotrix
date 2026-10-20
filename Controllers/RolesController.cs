using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RolesController(AppDbContext db)
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

        var query = _db.Roles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search) ||
                (x.Description != null &&
                 x.Description.Contains(search)));
        }

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
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Uid = x.Uuid,
                Name = x.Name,
                Description = x.Description,
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
            totalData,
            totalPage
        });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<RoleDto>> GetById(long id)
    {
        var role = await _db.Roles
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.DeletedAt == null)
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Uid = x.Uuid,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return role is null
            ? NotFound()
            : Ok(role);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama role wajib diisi."
            });
        }

        var roleName = request.Name.Trim();

        if (await _db.Roles.AnyAsync(x =>
            x.Name == roleName &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Nama role sudah digunakan."
            });
        }

        var role = new Role
        {
            Uuid = Guid.NewGuid().ToString(),
            Name = roleName,
            Description =
                string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        _db.Roles.Add(role);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Role berhasil ditambahkan.",
            id = role.Id,
            uid = role.Uuid
        });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateRoleRequest request)
    {
        var role =
            await _db.Roles.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (role is null)
        {
            return NotFound(new
            {
                message = "Role tidak ditemukan."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama role wajib diisi."
            });
        }

        var roleName = request.Name.Trim();

        if (await _db.Roles.AnyAsync(x =>
            x.Id != request.Id &&
            x.Name == roleName &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Nama role sudah digunakan."
            });
        }

        role.Name = roleName;

        role.Description =
            string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

        role.IsActive = request.IsActive;
        role.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Role berhasil diubah."
        });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteRoleRequest request)
    {
        var role =
            await _db.Roles.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (role is null)
        {
            return NotFound(new
            {
                message = "Role tidak ditemukan."
            });
        }

        // Soft delete
        role.IsActive = false;
        role.DeletedAt = DateTime.Now;
        role.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Role berhasil dihapus."
        });
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<RoleDto>>> GetAll()
    {
        var roles = await _db.Roles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Uid = x.Uuid,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return Ok(roles);
    }
}
