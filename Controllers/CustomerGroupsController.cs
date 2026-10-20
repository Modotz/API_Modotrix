using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerGroupsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CustomerGroupsController(AppDbContext db)
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

        var query = _db.CustomerGroups
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Code.Contains(search) ||
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

        var totalPage = (int)Math.Ceiling(
            totalData / (double)pageSize);

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
                x.DiscountPercent,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,
                x.PriceListId,

                PriceList = x.PriceListId == null
                    ? null
                    : _db.PriceLists
                        .Where(p =>
                            p.Id == x.PriceListId &&
                            p.DeletedAt == null)
                        .Select(p => new
                        {
                            Id = p.Id,
                            Name = p.Name
                        })
                        .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(new
        {
            data,
            page,
            pageSize,
            totalData,
            totalCount = totalData,
            totalPage
        });
    }

    // =========================================================
    // EXPORT ALL CUSTOMER GROUPS
    // GET: api/CustomerGroups/export
    // =========================================================

    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var query = _db.CustomerGroups
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Code.Contains(search) ||
                x.Name.Contains(search) ||
                (x.Description != null &&
                 x.Description.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        var data = await query
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Uid,
                x.Code,
                x.Name,
                x.DiscountPercent,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,
                x.PriceListId,

                PriceList = x.PriceListId == null
                    ? null
                    : _db.PriceLists
                        .Where(p =>
                            p.Id == x.PriceListId &&
                            p.DeletedAt == null)
                        .Select(p => new
                        {
                            Id = p.Id,
                            Name = p.Name
                        })
                        .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(data);
    }

    // =========================
    // GET BY ID
    // =========================

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var customerGroup = await _db.CustomerGroups
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
                x.DiscountPercent,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,
                x.PriceListId,

                PriceList = x.PriceListId == null
                    ? null
                    : _db.PriceLists
                        .Where(p =>
                            p.Id == x.PriceListId &&
                            p.DeletedAt == null)
                        .Select(p => new
                        {
                            Id = p.Id,
                            Name = p.Name
                        })
                        .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (customerGroup is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer group tidak ditemukan.",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Detail customer group berhasil diambil.",
            data = customerGroup
        });
    }

    // =========================
    // GET BY UID
    // =========================

    [HttpGet("uid/{uid}")]
    public async Task<IActionResult> GetByUid(string uid)
    {
        if (string.IsNullOrWhiteSpace(uid))
        {
            return BadRequest(new
            {
                success = false,
                message = "UID customer group wajib diisi.",
                data = (object?)null
            });
        }

        uid = uid.Trim();

        var customerGroup = await _db.CustomerGroups
            .AsNoTracking()
            .Where(x =>
                x.Uid == uid &&
                x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.Uid,
                x.Code,
                x.Name,
                x.DiscountPercent,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,
                x.PriceListId,

                PriceList = x.PriceListId == null
                    ? null
                    : _db.PriceLists
                        .Where(p =>
                            p.Id == x.PriceListId &&
                            p.DeletedAt == null)
                        .Select(p => new
                        {
                            Id = p.Id,
                            Name = p.Name
                        })
                        .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (customerGroup is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer group tidak ditemukan.",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Detail customer group berhasil diambil.",
            data = customerGroup
        });
    }

    // =========================
    // CREATE / RESTORE
    // =========================

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerGroups request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new
            {
                success = false,
                message = "Kode customer group wajib diisi.",
                data = (object?)null
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                success = false,
                message = "Nama customer group wajib diisi.",
                data = (object?)null
            });
        }

        if (request.DiscountPercent < 0 ||
            request.DiscountPercent > 100)
        {
            return BadRequest(new
            {
                success = false,
                message = "Discount percent harus berada antara 0 sampai 100.",
                data = (object?)null
            });
        }

        var code = request.Code.Trim();
        var name = request.Name.Trim();

        // CEK KODE YANG BELUM DIHAPUS
        var existingGroup = await _db.CustomerGroups
            .FirstOrDefaultAsync(x =>
                x.Code == code &&
                x.DeletedAt == null);

        if (existingGroup is not null)
        {
            return Conflict(new
            {
                success = false,
                message = "Kode customer group sudah digunakan.",
                data = (object?)null
            });
        }

        // VALIDASI PRICE LIST
        if (request.PriceListId.HasValue)
        {
            var priceListExists =
                await _db.PriceLists.AnyAsync(x =>
                    x.Id == request.PriceListId.Value &&
                    x.DeletedAt == null);

            if (!priceListExists)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Price list yang dipilih tidak ditemukan atau sudah dihapus.",
                    data = (object?)null
                });
            }
        }

        // CARI KODE YANG PERNAH DIHAPUS
        var deletedGroup = await _db.CustomerGroups
            .Where(x =>
                x.Code == code &&
                x.DeletedAt != null)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        // PULIHKAN DATA LAMA
        if (deletedGroup is not null)
        {
            deletedGroup.Code = code;
            deletedGroup.Name = name;
            deletedGroup.DiscountPercent =
                request.DiscountPercent;

            deletedGroup.Description =
                string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim();

            deletedGroup.IsActive = request.IsActive;
            deletedGroup.PriceListId = request.PriceListId;

            deletedGroup.DeletedAt = null;
            deletedGroup.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Customer group lama berhasil dipulihkan.",
                data = new
                {
                    id = deletedGroup.Id,
                    uid = deletedGroup.Uid
                }
            });
        }

        // BUAT DATA BARU JIKA KODE BELUM PERNAH ADA
        var customerGroup = new CustomerGroups
        {
            Uid = Guid.NewGuid().ToString(),
            Code = code,
            Name = name,
            DiscountPercent = request.DiscountPercent,

            Description =
                string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),

            IsActive = request.IsActive,
            CreatedAt = DateTime.Now,
            UpdatedAt = null,
            DeletedAt = null,
            PriceListId = request.PriceListId
        };

        _db.CustomerGroups.Add(customerGroup);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Customer group berhasil ditambahkan.",
            data = new
            {
                id = customerGroup.Id,
                uid = customerGroup.Uid
            }
        });
    }

    // =========================
    // UPDATE
    // =========================

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateCustomerGroups request)
    {
        var customerGroup =
            await _db.CustomerGroups
                .FirstOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.DeletedAt == null);

        if (customerGroup is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer group tidak ditemukan.",
                data = (object?)null
            });
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new
            {
                success = false,
                message = "Kode customer group wajib diisi.",
                data = (object?)null
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                success = false,
                message = "Nama customer group wajib diisi.",
                data = (object?)null
            });
        }

        if (request.DiscountPercent < 0 ||
            request.DiscountPercent > 100)
        {
            return BadRequest(new
            {
                success = false,
                message = "Discount percent harus berada antara 0 sampai 100.",
                data = (object?)null
            });
        }

        var code = request.Code.Trim();
        var name = request.Name.Trim();

        if (await _db.CustomerGroups.AnyAsync(x =>
            x.Id != request.Id &&
            x.Code == code &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                success = false,
                message = "Kode customer group sudah digunakan.",
                data = (object?)null
            });
        }

        if (request.PriceListId.HasValue)
        {
            var priceListExists =
                await _db.PriceLists.AnyAsync(x =>
                    x.Id == request.PriceListId.Value &&
                    x.DeletedAt == null);

            if (!priceListExists)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Price list yang dipilih tidak ditemukan atau sudah dihapus.",
                    data = (object?)null
                });
            }
        }

        customerGroup.Code = code;
        customerGroup.Name = name;

        customerGroup.DiscountPercent =
            request.DiscountPercent;

        customerGroup.Description =
            string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

        customerGroup.IsActive = request.IsActive;
        customerGroup.PriceListId = request.PriceListId;
        customerGroup.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Customer group berhasil diubah.",
            data = new
            {
                id = customerGroup.Id,
                uid = customerGroup.Uid
            }
        });
    }

    // =========================
    // DELETE / SOFT DELETE
    // =========================

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteCustomerGroups request)
    {
        var customerGroup =
            await _db.CustomerGroups
                .FirstOrDefaultAsync(x =>
                    x.Id == request.Id &&
                    x.DeletedAt == null);

        if (customerGroup is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Customer group tidak ditemukan.",
                data = (object?)null
            });
        }

        // IsActive tidak diubah.
        customerGroup.DeletedAt = DateTime.Now;
        customerGroup.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Customer group berhasil dihapus.",
            data = new
            {
                id = customerGroup.Id,
                uid = customerGroup.Uid
            }
        });
    }
}
