using ERP.Api.Data;
using ERP.Api.DTOs.Customer;
using ERP.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public CustomersController(
        AppDbContext db,
        IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    // =========================================================
    // GET ALL CUSTOMERS
    // GET: api/customers
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? search = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 10)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _db.Customers
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        // SEARCH
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.FullName.Contains(search) ||
                x.Email.Contains(search) ||
                x.Phone.Contains(search) ||
                (x.Npwp != null && x.Npwp.Contains(search)) ||
                (x.Nik != null && x.Nik.Contains(search)));
        }

        // FILTER STATUS
        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        // TOTAL DATA
        var totalData = await query.CountAsync();

        // DATA
        var data = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                Uid = x.Uid,
                FullName = x.FullName,
                Email = x.Email,
                Phone = x.Phone,
                Npwp = x.Npwp,
                Nik = x.Nik,
                Address = x.Address,
                Image = x.Image,

                CustomerGroupId = x.CustomerGroupId,
                CustomerGroupName = x.CustomerGroup != null
                    ? x.CustomerGroup.Name
                    : null,

                SalesForceId = x.SalesForceId,
                SalesForceName = x.SalesForce != null
                    ? x.SalesForce.Name
                    : null,

                PriceListId = x.PriceListId,
                PriceListName = x.PriceList != null
                    ? x.PriceList.Name
                    : null,

                ShippingAreaId = x.ShippingAddresses
                    .Where(sa => sa.ShippingAreaId != null)
                    .Select(sa => sa.ShippingAreaId)
                    .FirstOrDefault(),

                ShippingAreaName = x.ShippingAddresses
                    .Where(sa =>
                        sa.ShippingAreaId != null &&
                        sa.ShippingArea != null)
                    .Select(sa => sa.ShippingArea!.Name)
                    .FirstOrDefault(),

                RegisteredOnline = x.RegisteredOnline,
                ShopStatus = x.ShopStatus,
                UserId = x.UserId,

                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt
            })
            .ToListAsync();

        return Ok(new
        {
            data,
            page,
            pageSize,
            totalCount = totalData,
            totalData
        });
    }

    // =========================================================
    // CREATE CUSTOMER
    // POST: api/customers
    // =========================================================
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // VALIDASI CUSTOMER GROUP
        if (dto.CustomerGroupId.HasValue)
        {
            var customerGroupExists =
                await _db.CustomerGroups.AnyAsync(x =>
                    x.Id == dto.CustomerGroupId.Value &&
                    x.IsActive &&
                    x.DeletedAt == null);

            if (!customerGroupExists)
            {
                return BadRequest(new
                {
                    message =
                        "Customer Group tidak ditemukan atau tidak aktif."
                });
            }
        }

        // VALIDASI SALES FORCE
        if (dto.SalesForceId.HasValue)
        {
            var salesForceExists =
                await _db.SalesForces.AnyAsync(x =>
                    x.Id == dto.SalesForceId.Value &&
                    x.IsActive &&
                    x.DeletedAt == null);

            if (!salesForceExists)
            {
                return BadRequest(new
                {
                    message =
                        "Sales Force tidak ditemukan atau tidak aktif."
                });
            }
        }

        // VALIDASI PRICE LIST
        if (dto.PriceListId.HasValue)
        {
            var priceListExists =
                await _db.PriceLists.AnyAsync(x =>
                    x.Id == dto.PriceListId.Value &&
                    x.IsActive &&
                    x.DeletedAt == null);

            if (!priceListExists)
            {
                return BadRequest(new
                {
                    message =
                        "Price List tidak ditemukan atau tidak aktif."
                });
            }
        }

        var customer = new Customer
        {
            Uid = Guid.NewGuid().ToString(),
            FullName = dto.FullName,
            Email = dto.Email,
            Password = dto.Password,
            Phone = dto.Phone,
            Npwp = dto.Npwp,
            Nik = dto.Nik,
            Address = dto.Address,
            CustomerGroupId = dto.CustomerGroupId,
            SalesForceId = dto.SalesForceId,
            PriceListId = dto.PriceListId,
            RegisteredOnline = dto.RegisteredOnline,
            ShopStatus = dto.ShopStatus,
            UserId = dto.UserId,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            DeletedAt = null
        };

        _db.Customers.Add(customer);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Customer berhasil ditambahkan.",

            data = new CustomerDto
            {
                Id = customer.Id,
                Uid = customer.Uid,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                Npwp = customer.Npwp,
                Nik = customer.Nik,
                Address = customer.Address,
                Image = customer.Image,

                CustomerGroupId = customer.CustomerGroupId,
                SalesForceId = customer.SalesForceId,
                PriceListId = customer.PriceListId,

                RegisteredOnline = customer.RegisteredOnline,
                ShopStatus = customer.ShopStatus,
                UserId = customer.UserId,

                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                DeletedAt = customer.DeletedAt
            }
        });
    }

    // =========================================================
    // UPDATE CUSTOMER
    // POST: api/customers/update
    // =========================================================
    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var customer = await _db.Customers
            .FirstOrDefaultAsync(x =>
                x.Id == dto.Id &&
                x.DeletedAt == null);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer tidak ditemukan."
            });
        }

        // VALIDASI CUSTOMER GROUP
        if (dto.CustomerGroupId.HasValue)
        {
            var customerGroupExists =
                await _db.CustomerGroups.AnyAsync(x =>
                    x.Id == dto.CustomerGroupId.Value &&
                    x.IsActive &&
                    x.DeletedAt == null);

            if (!customerGroupExists)
            {
                return BadRequest(new
                {
                    message =
                        "Customer Group tidak ditemukan atau tidak aktif."
                });
            }
        }

        // VALIDASI SALES FORCE
        if (dto.SalesForceId.HasValue)
        {
            var salesForceExists =
                await _db.SalesForces.AnyAsync(x =>
                    x.Id == dto.SalesForceId.Value &&
                    x.IsActive &&
                    x.DeletedAt == null);

            if (!salesForceExists)
            {
                return BadRequest(new
                {
                    message =
                        "Sales Force tidak ditemukan atau tidak aktif."
                });
            }
        }

        // VALIDASI PRICE LIST
        if (dto.PriceListId.HasValue)
        {
            var priceListExists =
                await _db.PriceLists.AnyAsync(x =>
                    x.Id == dto.PriceListId.Value &&
                    x.IsActive &&
                    x.DeletedAt == null);

            if (!priceListExists)
            {
                return BadRequest(new
                {
                    message =
                        "Price List tidak ditemukan atau tidak aktif."
                });
            }
        }

        customer.FullName = dto.FullName;
        customer.Email = dto.Email;
        customer.Password = dto.Password;
        customer.Phone = dto.Phone;
        customer.Npwp = dto.Npwp;
        customer.Nik = dto.Nik;
        customer.Address = dto.Address;
        customer.CustomerGroupId = dto.CustomerGroupId;
        customer.SalesForceId = dto.SalesForceId;
        customer.PriceListId = dto.PriceListId;
        customer.IsActive = dto.IsActive;
        customer.RegisteredOnline = dto.RegisteredOnline;

        customer.ShopStatus = dto.ShopStatus;
        customer.UserId = dto.UserId;
        customer.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Customer berhasil diperbarui.",

            data = new CustomerDto
            {
                Id = customer.Id,
                Uid = customer.Uid,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                Npwp = customer.Npwp,
                Nik = customer.Nik,
                Address = customer.Address,
                Image = customer.Image,

                CustomerGroupId = customer.CustomerGroupId,
                SalesForceId = customer.SalesForceId,
                PriceListId = customer.PriceListId,

                RegisteredOnline = customer.RegisteredOnline,
                ShopStatus = customer.ShopStatus,
                UserId = customer.UserId,

                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                DeletedAt = customer.DeletedAt
            }
        });
    }


    // =========================================================
    // EXPORT ALL CUSTOMERS
    // GET: api/customers/export
    // =========================================================
    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport(
        string? search = null,
        bool? isActive = null)
    {
        var query = _db.Customers
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        // SEARCH
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.FullName.Contains(search) ||
                x.Email.Contains(search) ||
                x.Phone.Contains(search) ||
                (x.Npwp != null && x.Npwp.Contains(search)) ||
                (x.Nik != null && x.Nik.Contains(search)));
        }

        // FILTER STATUS
        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                Uid = x.Uid,
                FullName = x.FullName,
                Email = x.Email,
                Phone = x.Phone,
                Npwp = x.Npwp,
                Nik = x.Nik,
                Address = x.Address,
                Image = x.Image,

                CustomerGroupId = x.CustomerGroupId,
                CustomerGroupName = x.CustomerGroup != null
                    ? x.CustomerGroup.Name
                    : null,

                SalesForceId = x.SalesForceId,
                SalesForceName = x.SalesForce != null
                    ? x.SalesForce.Name
                    : null,

                PriceListId = x.PriceListId,
                PriceListName = x.PriceList != null
                    ? x.PriceList.Name
                    : null,

                ShippingAreaId = x.ShippingAddresses
                    .Where(sa => sa.ShippingAreaId != null)
                    .Select(sa => sa.ShippingAreaId)
                    .FirstOrDefault(),

                ShippingAreaName = x.ShippingAddresses
                    .Where(sa =>
                        sa.ShippingAreaId != null &&
                        sa.ShippingArea != null)
                    .Select(sa => sa.ShippingArea!.Name)
                    .FirstOrDefault(),

                RegisteredOnline = x.RegisteredOnline,
                ShopStatus = x.ShopStatus,
                UserId = x.UserId,

                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt
            })
            .ToListAsync();

        return Ok(data);
    }

    // =========================================================
    // CUSTOMER GROUP DROPDOWN
    // GET: api/customers/customer-groups
    // =========================================================
    [HttpGet("customer-groups")]
    public async Task<IActionResult> GetCustomerGroups()
    {
        var data = await _db.CustomerGroups
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name
            })
            .ToListAsync();

        return Ok(data);
    }

    // =========================================================
    // SALES FORCE DROPDOWN
    // GET: api/customers/sales-forces
    // =========================================================
    [HttpGet("sales-forces")]
    public async Task<IActionResult> GetSalesForces()
    {
        var data = await _db.SalesForces
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name
            })
            .ToListAsync();

        return Ok(data);
    }

    // =========================================================
    // PRICE LIST DROPDOWN
    // GET: api/customers/price-lists
    // =========================================================
    [HttpGet("price-lists")]
    public async Task<IActionResult> GetPriceLists()
    {
        var data = await _db.PriceLists
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name
            })
            .ToListAsync();

        return Ok(data);
    }

    // =========================================================
    // DELETE CUSTOMER
    // POST: api/customers/delete
    // =========================================================
    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var customer = await _db.Customers
            .FirstOrDefaultAsync(x =>
                x.Id == dto.Id &&
                x.DeletedAt == null);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer tidak ditemukan."
            });
        }

        customer.IsActive = false;
        customer.DeletedAt = DateTime.Now;
        customer.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Customer berhasil dihapus."
        });
    }

    // =========================================================
    // UPLOAD IMAGE
    // POST: api/customers/upload-image
    //
    // FILE:
    // ERP.Api
    // └── wwwroot
    //     └── uploads
    //         └── customers
    //             └── foto.jpg
    // =========================================================
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(
        IFormFile file,
        [FromForm] long customerId)
    {
        // =====================================================
        // VALIDASI FILE
        // =====================================================

        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "File gambar wajib dipilih."
            });
        }

        // =====================================================
        // CEK CUSTOMER
        // =====================================================

        var customer = await _db.Customers
            .FirstOrDefaultAsync(x =>
                x.Id == customerId &&
                x.DeletedAt == null);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer tidak ditemukan."
            });
        }

        // =====================================================
        // VALIDASI EXTENSION
        // =====================================================

        var allowedExtensions = new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        var extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message =
                    "Format gambar harus JPG, JPEG, PNG, atau WEBP."
            });
        }

        // =====================================================
        // VALIDASI UKURAN
        // =====================================================

        const long maxFileSize =
            5 * 1024 * 1024;

        if (file.Length > maxFileSize)
        {
            return BadRequest(new
            {
                message =
                    "Ukuran gambar maksimal 5 MB."
            });
        }

        // =====================================================
        // AMBIL WWWROOT ERP.API
        // =====================================================

        var webRootPath =
            _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot");
        }

        // =====================================================
        // FOLDER:
        // ERP.Api/wwwroot/uploads/customers
        // =====================================================

        var customersFolder =
            Path.Combine(
                webRootPath,
                "uploads",
                "customers");

        // =====================================================
        // BUAT FOLDER
        // =====================================================

        if (!Directory.Exists(customersFolder))
        {
            Directory.CreateDirectory(
                customersFolder);
        }

        // =====================================================
        // GENERATE NAMA FILE UNIK
        // =====================================================

        var fileName =
            $"{Guid.NewGuid()}{extension}";

        var filePath =
            Path.Combine(
                customersFolder,
                fileName);

        // =====================================================
        // SIMPAN FILE
        // =====================================================

        await using (var stream =
            new FileStream(
                filePath,
                FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // =====================================================
        // SIMPAN PATH KE DATABASE
        // =====================================================

        customer.Image =
            $"/uploads/customers/{fileName}";

        customer.UpdatedAt =
            DateTime.Now;

        await _db.SaveChangesAsync();

        // =====================================================
        // RESPONSE
        // =====================================================

        return Ok(new
        {
            message =
                "Foto customer berhasil diupload.",

            image =
                customer.Image
        });
    }
}