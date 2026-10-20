using ERP.Api.Data;
using ERP.Api.DTOs.Supplier;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly AppDbContext _db;

    public SuppliersController(AppDbContext db)
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

        var query = _db.Suppliers
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search) || x.Code.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalData =
            await query.CountAsync();

        var totalPage =
            (int)Math.Ceiling(
                totalData / (double)pageSize
            );

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SupplierDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
                ContactPerson = x.ContactPerson,
                Phone = x.Phone,
                Email = x.Email,
                Npwp = x.Npwp,
                Address = x.Address,
                IsActive = x.IsActive,
                //CreatedAt = x.CreatedAt,
                //UpdatedAt = x.UpdatedAt,
                //DeletedAt = x.DeletedAt
            })
            .ToListAsync();

        return Ok(new
        {
            data,
            page,
            pageSize,
            totalCount,
            totalData,
            totalPage
        });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SupplierDto>> GetById(long id)
    {
        var supplier = await _db.Suppliers
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.DeletedAt == null)
            .Select(x => new SupplierDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
                ContactPerson = x.ContactPerson,
                Phone = x.Phone,
                Email = x.Email,
                Npwp = x.Npwp,
                Address = x.Address,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return supplier is null
            ? NotFound()
            : Ok(supplier);
    }

    //[HttpPost("create")]
    //public async Task<IActionResult> Create(
    //    [FromBody] CreateSupplierRequest request)
    //{
    //    if (await _db.Suppliers.AnyAsync(x =>
    //        x.Code == request.Code &&
    //        x.DeletedAt == null))
    //    {
    //        return Conflict(new
    //        {
    //            message = "Kode Supplier sudah digunakan."
    //        });
    //    }

    //    var supplier = new Supplier 
    //    {
    //        Uid = Guid.NewGuid().ToString(),
    //        Code = request.Code.Trim(),
    //        Name = request.Name.Trim(),
    //        ContactPerson = request.ContactPerson?.Trim(),
    //        Phone = request.Phone?.Trim(),
    //        Email = request.Email?.Trim(),
    //        Npwp = request.Npwp?.Trim(),
    //        Address = request.Address?.Trim(),
    //        IsActive = request.IsActive,
    //        CreatedAt = DateTime.Now
    //    };

    //    _db.Suppliers.Add(supplier);

    //    await _db.SaveChangesAsync();

    //    return Ok(new
    //    {
    //        message = "Supplier berhasil ditambahkan.",
    //        id = supplier.Id,
    //        uid = supplier.Uid
    //    });
    //}

    [HttpPost("create")]
    public async Task<IActionResult> Create(
    [FromBody] CreateSupplierRequest request)
    {
        var code = request.Code.Trim();
         
        // CARI SUPPLIER BERDASARKAN CODE  TERMASUK DATA YANG SUDAH SOFT DELETE 
        var existingSupplier = await _db.Suppliers
            .FirstOrDefaultAsync(x =>
                x.Code == code);
         
        // CODE SUDAH ADA DAN SUPPLIER MASIH AKTIF 
        if (existingSupplier != null &&
            existingSupplier.DeletedAt == null)
        {
            return Conflict(new
            {
                message = "Kode Supplier sudah digunakan."
            });
        }
         
        // RESTORE DATA LAMA 
        if (existingSupplier != null &&
            existingSupplier.DeletedAt != null)
        {
            existingSupplier.Code = code;
            existingSupplier.Name = request.Name.Trim();
            existingSupplier.ContactPerson = request.ContactPerson?.Trim();
            existingSupplier.Phone = request.Phone?.Trim();
            existingSupplier.Email = request.Email?.Trim();
            existingSupplier.Npwp = request.Npwp?.Trim();
            existingSupplier.Address = request.Address?.Trim();

            // Aktifkan kembali supplier
            existingSupplier.IsActive = true;

            // Hapus status soft delete
            existingSupplier.DeletedAt = null;

            existingSupplier.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Supplier sebelumnya ditemukan dan berhasil dipulihkan.",
                id = existingSupplier.Id,
                uid = existingSupplier.Uid
            });
        }
         
        // CODE BELUM PERNAH ADA  
        var supplier = new Supplier
        {
            Uid = Guid.NewGuid().ToString(),
            Code = code,
            Name = request.Name.Trim(),
            ContactPerson = request.ContactPerson?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Npwp = request.Npwp?.Trim(),
            Address = request.Address?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        _db.Suppliers.Add(supplier);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Supplier berhasil ditambahkan.",
            id = supplier.Id,
            uid = supplier.Uid
        });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateSupplierRequest request)
    {
        var supplier =
            await _db.Suppliers.FirstOrDefaultAsync(x =>
                x.Id == request.Id 
                && x.DeletedAt == null);

        if (supplier is null)
        {
            return NotFound(new
            {
                message = "Supplier tidak ditemukan."
            });
        }

        if (await _db.Suppliers.AnyAsync(x =>
            x.Id != request.Id &&
            x.Code == request.Code))
        //&& x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "Kode Supplier sudah digunakan."
            });
        }

        supplier.Code = request.Code.Trim();
        supplier.Name = request.Name.Trim();
        supplier.ContactPerson = request.ContactPerson?.Trim();
        supplier.Phone = request.Phone?.Trim();
        supplier.Email = request.Email?.Trim();
        supplier.Npwp = request.Npwp?.Trim();
        supplier.Address = request.Address?.Trim();
        supplier.IsActive = request.IsActive;
        supplier.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Supplier berhasil diubah."
        });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
       [FromBody] DeleteSupplierRequest request)
    {
        var supplier =
            await _db.Suppliers.FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (supplier is null)
        {
            return NotFound(new
            {
                message = "Supplier tidak ditemukan."
            });
        }

        supplier.IsActive = false;
        supplier.DeletedAt = DateTime.Now;
        supplier.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Supplier berhasil dihapus."
        });
    }

    //hard-delete
    //_db.Menus.Remove(menu);
    //await _db.SaveChangesAsync();
    //return Ok(new
    //{
    //    message = "Menu berhasil dihapus."
    //});

    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport()
    {
        var data = await _db.Suppliers
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SupplierDto
            {
                Id = x.Id,
                Uid = x.Uid,
                Code = x.Code,
                Name = x.Name,
                ContactPerson = x.ContactPerson,
                Phone = x.Phone,
                Email = x.Email,
                Npwp = x.Npwp,
                Address = x.Address,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(data);
    }
}
    

