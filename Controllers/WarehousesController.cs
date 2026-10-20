using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[Route("api/warehouses")]
[ApiController]
public class WarehousesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<WarehousesController> _logger;

    public WarehousesController(
        AppDbContext context,
        ILogger<WarehousesController> logger)
    {
        _context = context;
        _logger = logger;
    }


    // ==========================================
    // GET: api/warehouses
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> GetWarehouses(
        string? search = null,
        bool? isDefault = null,
        bool? isActive = null)
    {
        try
        {
            var query = _context.Warehouses
                .AsNoTracking()
                .Where(x => x.deleted_at == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.code.Contains(search) ||
                    x.name.Contains(search));
            }

            if (isDefault.HasValue)
            {
                query = query.Where(x =>
                    x.is_default == isDefault.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x =>
                    x.is_active == isActive.Value);
            }

            var warehouses = await query
                .OrderByDescending(x => x.id)
                .Select(x => new WarehouseDto
                {
                    Id = x.id,
                    Uid = x.uid,
                    Code = x.code,
                    Name = x.name,
                    Address = x.address,
                    Phone = x.phone,
                    PicName = x.pic_name,
                    IsDefault = x.is_default,
                    IsActive = x.is_active,
                    CreatedAt = x.created_at,
                    UpdatedAt = x.updated_at,
                    DeletedAt = x.deleted_at
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Data gudang berhasil diambil.",
                data = warehouses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error saat mengambil data Warehouse: {Exception}",
                ex.ToString());

            return StatusCode(500, new
            {
                success = false,
                message = "Terjadi kesalahan saat mengambil data gudang.",
                error = HttpContext.RequestServices
                    .GetRequiredService<IWebHostEnvironment>()
                    .IsDevelopment()
                    ? ex.Message
                    : null,
                data = (object?)null
            });
        }
    }


    // ==========================================
    // GET: api/warehouses/{id}
    // ==========================================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWarehouse(long id)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .Where(x =>
                x.id == id &&
                x.deleted_at == null)
            .Select(x => new WarehouseDto
            {
                Id = x.id,
                Uid = x.uid,
                Code = x.code,
                Name = x.name,
                Address = x.address,
                Phone = x.phone,
                PicName = x.pic_name,
                IsDefault = x.is_default,
                IsActive = x.is_active,
                CreatedAt = x.created_at,
                UpdatedAt = x.updated_at,
                DeletedAt = x.deleted_at
            })
            .FirstOrDefaultAsync();

        if (warehouse == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Data gudang tidak ditemukan.",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Data gudang berhasil diambil.",
            data = warehouse
        });
    }


    // ==========================================
    // GET: api/warehouses/uid/{uid}
    // ==========================================
    [HttpGet("uid/{uid}")]
    public async Task<IActionResult> GetWarehouseByUid(string uid)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .Where(x =>
                x.uid == uid &&
                x.deleted_at == null)
            .Select(x => new WarehouseDto
            {
                Id = x.id,
                Uid = x.uid,
                Code = x.code,
                Name = x.name,
                Address = x.address,
                Phone = x.phone,
                PicName = x.pic_name,
                IsDefault = x.is_default,
                IsActive = x.is_active,
                CreatedAt = x.created_at,
                UpdatedAt = x.updated_at,
                DeletedAt = x.deleted_at
            })
            .FirstOrDefaultAsync();

        if (warehouse == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Data gudang tidak ditemukan.",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Data gudang berhasil diambil.",
            data = warehouse
        });
    }


    // ==========================================
    // POST: api/warehouses/Create
    // ==========================================
    [HttpPost("Create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateWarehouse dto)
    {
        var codeExists = await _context.Warehouses
            .AnyAsync(x =>
                x.code == dto.Code &&
                x.deleted_at == null);

        if (codeExists)
        {
            return BadRequest(new
            {
                success = false,
                message = "Kode gudang sudah digunakan.",
                data = (object?)null
            });
        }

        if (dto.IsDefault)
        {
            var currentDefault = await _context.Warehouses
                .Where(x =>
                    x.is_default &&
                    x.deleted_at == null)
                .ToListAsync();

            foreach (var warehouse in currentDefault)
            {
                warehouse.is_default = false;
                warehouse.updated_at = DateTime.Now;
            }
        }

        var warehouseData = new Warehouse
        {
            uid = Guid.NewGuid().ToString(),
            code = dto.Code,
            name = dto.Name,
            address = dto.Address,
            phone = dto.Phone,
            pic_name = dto.PicName,
            is_default = dto.IsDefault,
            is_active = dto.IsActive,
            created_at = DateTime.Now
        };

        _context.Warehouses.Add(warehouseData);

        // Debug nilai sebelum disimpan
        System.Diagnostics.Debug.WriteLine("========== DEBUG WAREHOUSE ==========");
        System.Diagnostics.Debug.WriteLine($"DTO IsDefault      : {dto.IsDefault}");
        System.Diagnostics.Debug.WriteLine($"DTO IsActive       : {dto.IsActive}");
        System.Diagnostics.Debug.WriteLine($"Entity IsDefault   : {warehouseData.is_default}");
        System.Diagnostics.Debug.WriteLine($"Entity IsActive    : {warehouseData.is_active}");
        System.Diagnostics.Debug.WriteLine("=====================================");

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("========== SAVE ERROR ==========");
            System.Diagnostics.Debug.WriteLine(ex.ToString());

            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine("========== INNER EXCEPTION ==========");
                System.Diagnostics.Debug.WriteLine(ex.InnerException.ToString());
            }

            throw;
        }
        await _context.SaveChangesAsync();

        var result = new WarehouseDto
        {
            Id = warehouseData.id,
            Uid = warehouseData.uid,
            Code = warehouseData.code,
            Name = warehouseData.name,
            Address = warehouseData.address,
            Phone = warehouseData.phone,
            PicName = warehouseData.pic_name,
            IsDefault = warehouseData.is_default,
            IsActive = warehouseData.is_active,
            CreatedAt = warehouseData.created_at,
            UpdatedAt = warehouseData.updated_at,
            DeletedAt = warehouseData.deleted_at
        };

        return Ok(new
        {
            success = true,
            message = "Data gudang berhasil disimpan.",
            data = result
        });
    }


    // ==========================================
    // POST: api/warehouses/Update
    // ==========================================
    [HttpPost("Update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateWarehouse dto)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.id == dto.Id &&
                x.deleted_at == null);

        if (warehouse == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Data gudang tidak ditemukan.",
                data = (object?)null
            });
        }

        var codeExists = await _context.Warehouses
            .AnyAsync(x =>
                x.code == dto.Code &&
                x.id != dto.Id &&
                x.deleted_at == null);

        if (codeExists)
        {
            return BadRequest(new
            {
                success = false,
                message = "Kode gudang sudah digunakan.",
                data = (object?)null
            });
        }

        if (dto.IsDefault)
        {
            var currentDefaults = await _context.Warehouses
                .Where(x =>
                    x.id != dto.Id &&
                    x.is_default &&
                    x.deleted_at == null)
                .ToListAsync();

            foreach (var item in currentDefaults)
            {
                item.is_default = false;
                item.updated_at = DateTime.Now;
            }
        }

        warehouse.code = dto.Code;
        warehouse.name = dto.Name;
        warehouse.address = dto.Address;
        warehouse.phone = dto.Phone;
        warehouse.pic_name = dto.PicName;
        warehouse.is_default = dto.IsDefault;
        warehouse.is_active = dto.IsActive;
        warehouse.updated_at = DateTime.Now;

        await _context.SaveChangesAsync();

        var result = new WarehouseDto
        {
            Id = warehouse.id,
            Uid = warehouse.uid,
            Code = warehouse.code,
            Name = warehouse.name,
            Address = warehouse.address,
            Phone = warehouse.phone,
            PicName = warehouse.pic_name,
            IsDefault = warehouse.is_default,
            IsActive = warehouse.is_active,
            CreatedAt = warehouse.created_at,
            UpdatedAt = warehouse.updated_at,
            DeletedAt = warehouse.deleted_at
        };

        return Ok(new
        {
            success = true,
            message = "Data gudang berhasil diperbarui.",
            data = result
        });
    }


    // ==========================================
    // POST: api/warehouses/Delete
    // ==========================================
    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteWarehouse dto)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.id == dto.Id &&
                x.deleted_at == null);

        if (warehouse == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Data gudang tidak ditemukan.",
                data = (object?)null
            });
        }

        warehouse.deleted_at = DateTime.Now;
        warehouse.updated_at = DateTime.Now;
        warehouse.is_default = false;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Data gudang berhasil dihapus.",
            data = (object?)null
        });
    }


    // ==========================================
    // GET: api/warehouses/export
    // ==========================================
    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport()
    {
        var data = await _context.Warehouses
            .AsNoTracking()
            .Where(x => x.deleted_at == null)
            .OrderByDescending(x => x.created_at)
            .Select(x => new WarehouseDto
            {
                Code = x.code,
                Name = x.name,
                Address = x.address,
                Phone = x.phone,
                PicName = x.pic_name,
                IsDefault = x.is_default,
                IsActive = x.is_active
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Data gudang untuk export berhasil diambil.",
            data = data
        });
    }
}