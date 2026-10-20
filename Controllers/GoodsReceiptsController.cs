
using ERP.Api.Data; 
using ERP.Api.DTOs.GoodsReceipt;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/goods-receipts")]
public class GoodsReceipts : ControllerBase
{
    private readonly AppDbContext _db;

    public GoodsReceipts(AppDbContext db)
    {
        _db = db;
    }

    private static readonly string[] AllowedStatuses =
    {
        "draft",
        "posted",
        "cancelled"
    };

     
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] long? supplierId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.GoodsReceipts
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.ReceiptNo.Contains(search) ||
                (x.Note != null && x.Note.Contains(search)));
        }

        if (supplierId.HasValue && supplierId.Value > 0)
        {
            query = query.Where(x =>
                x.SupplierId == supplierId.Value);
        }

        var totalData = await query.CountAsync();

        var totalPage = (int)Math.Ceiling(
            totalData / (double)pageSize);

        var data = await query
            .OrderByDescending(x => x.ReceiptDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new GoodsReceiptDto
            {
                Id = x.Id,
                Uid = x.Uid,
                ReceiptNo = x.ReceiptNo,
                ReceiptDate = x.ReceiptDate,

                WarehouseId = x.WarehouseId,
                WarehouseName = _db.Warehouses
                    .Where(w =>
                        w.id == x.WarehouseId &&
                        w.deleted_at == null)
                    .Select(w => w.name)
                    .FirstOrDefault(),

                SupplierId = x.SupplierId,
                SupplierName = _db.Suppliers
                    .Where(s =>
                        s.Id == x.SupplierId &&
                        s.DeletedAt == null)
                    .Select(s => s.Name)
                    .FirstOrDefault(),

                POId = x.POId,
                RefNo = x.RefNo,
                Note = x.Note,
                Status = x.Status,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            data,
            page,
            pageSize,
            totalCount = totalData,
            totalData,
            totalPage
        });
    }
     
    private async Task<string> GenerateReceiptNumber(
        DateOnly receiptDate)
    {
        var dailyPrefix =
            $"GR-{receiptDate:yyMMdd}";

        var existingNumbers = await _db.GoodsReceipts
            .AsNoTracking()
            .Where(x => x.ReceiptNo.StartsWith(dailyPrefix))
            .Select(x => x.ReceiptNo)
            .ToListAsync();

        var maxNumber = existingNumbers
            .Select(number =>
            {
                var suffix = number.Length > dailyPrefix.Length
                    ? number[dailyPrefix.Length..]
                    : string.Empty;

                return int.TryParse(suffix, out var value)
                    ? value
                    : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"{dailyPrefix}{maxNumber + 1:D3}";
    }
 
    [HttpGet("next-number")]
    public async Task<IActionResult> GetNextReceiptNumber(
        [FromQuery] DateOnly? receiptDate = null)
    {
        var date = receiptDate ??
            DateOnly.FromDateTime(DateTime.Today);

        var receiptNo = await GenerateReceiptNumber(date);

        return Ok(new
        {
            receiptNo
        });
    }

     
    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses()
    {
        var warehouses = await _db.Warehouses
            .AsNoTracking()
            .Where(x => x.deleted_at == null)
            .OrderBy(x => x.name)
            .Select(x => new
            {
                id = x.id,
                name = x.name
            })
            .ToListAsync();

        return Ok(warehouses);
    }
 
    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSuppliers()
    {
        var suppliers = await _db.Suppliers
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "ID Penerimaan Barang tidak valid."
            });
        }

        var item = await _db.GoodsReceipts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new GoodsReceiptDto
            {
                Id = x.Id,
                Uid = x.Uid,
                ReceiptNo = x.ReceiptNo,
                ReceiptDate = x.ReceiptDate,

                WarehouseId = x.WarehouseId,
                WarehouseName = _db.Warehouses
                    .Where(w =>
                        w.id == x.WarehouseId &&
                        w.deleted_at == null)
                    .Select(w => w.name)
                    .FirstOrDefault(),

                SupplierId = x.SupplierId,
                SupplierName = _db.Suppliers
                    .Where(s =>
                        s.Id == x.SupplierId &&
                        s.DeletedAt == null)
                    .Select(s => s.Name)
                    .FirstOrDefault(),

                POId = x.POId,
                RefNo = x.RefNo,
                Note = x.Note,
                Status = x.Status,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                Details = _db.GoodsReceiptItems
                    .Where(d => d.ReceiptId == x.Id)
                    .Select(d => new GoodsReceiptItemDto
                    {
                        ReceiptId = d.ReceiptId,
                        ProductId = d.ProductId,

                        ProductName = _db.Products
                            .Where(p => p.Id == d.ProductId)
                            .Select(p => p.Name)
                            .FirstOrDefault(),

                        UnitId = d.UnitId,

                        UnitCode = _db.Units
                            .Where(u => u.Id == d.UnitId)
                            .Select(u => u.Code)
                            .FirstOrDefault(),

                        UnitQty = d.UnitQty,
                        Qty = d.Qty,
                        UnitCost = d.UnitCost,
                        Subtotal = d.Subtotal,

                        DimLength = d.DimLength,
                        DimWidth = d.DimWidth,
                        DimHeight = d.DimHeight,
                        DimCount = d.DimCount
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound(new
            {
                message = "Penerimaan Barang tidak ditemukan."
            });
        }

        return Ok(item);
    }
     
    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] GoodsReceiptDto request)
    {
        if (request == null)
        {
            return BadRequest(new
            {
                message = "Data Penerimaan Barang wajib diisi."
            });
        }

        if (request.ReceiptDate == default)
        {
            return BadRequest(new
            {
                message = "Tanggal penerimaan wajib diisi."
            });
        }

        if (request.SupplierId <= 0 ||
            request.WarehouseId <= 0)
        {
            return BadRequest(new
            {
                message = "Supplier dan gudang wajib dipilih."
            });
        }

        if (!AllowedStatuses.Contains(
                request.Status,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Status Penerimaan Barang tidak valid."
            });
        }

        if (request.Details == null ||
            request.Details.Count == 0)
        {
            return BadRequest(new
            {
                message = "Minimal satu barang harus ditambahkan."
            });
        }

        for (var i = 0; i < request.Details.Count; i++)
        {
            var detail = request.Details[i];

            if (detail.ProductId <= 0)
            {
                return BadRequest(new
                {
                    message = $"Produk pada baris {i + 1} wajib dipilih."
                });
            }

            if (detail.Qty <= 0)
            {
                return BadRequest(new
                {
                    message = $"Kuantitas pada baris {i + 1} harus lebih dari 0."
                });
            }

            if (detail.UnitCost < 0 ||
                detail.Subtotal < 0 ||
                (detail.UnitQty.HasValue &&
                 detail.UnitQty.Value <= 0) ||
                (detail.DimLength.HasValue &&
                 detail.DimLength.Value < 0) ||
                (detail.DimWidth.HasValue &&
                 detail.DimWidth.Value < 0) ||
                (detail.DimHeight.HasValue &&
                 detail.DimHeight.Value < 0) ||
                (detail.DimCount.HasValue &&
                 detail.DimCount.Value < 0))
            {
                return BadRequest(new
                {
                    message = $"Nilai detail barang pada baris {i + 1} tidak valid."
                });
            }
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            var receipt = new GoodsReceipt
            {
                Uid = Guid.NewGuid().ToString(),
                ReceiptNo = await GenerateReceiptNumber(
                    request.ReceiptDate),

                ReceiptDate = request.ReceiptDate,
                WarehouseId = request.WarehouseId,
                SupplierId = request.SupplierId,
                POId = request.POId,
                RefNo = request.RefNo,
                Note = request.Note,
                Status = request.Status.Trim().ToLowerInvariant(),
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _db.GoodsReceipts.Add(receipt);

            await _db.SaveChangesAsync();

            var details = request.Details.Select(detail =>
                new GoodsReceiptItem
                {
                    Uid = Guid.NewGuid().ToString(),

                    ReceiptId = receipt.Id,
                    ProductId = detail.ProductId,
                    UnitId = detail.UnitId,

                    UnitQty = detail.UnitQty ?? detail.Qty,
                    Qty = detail.Qty,

                    UnitCost = detail.UnitCost,
                    Subtotal = detail.Subtotal,

                    DimLength = detail.DimLength ?? 0m,
                    DimWidth = detail.DimWidth ?? 0m,
                    DimHeight = detail.DimHeight ?? 0m,
                    DimCount = detail.DimCount ?? 0m
                })
                .ToList();

            _db.GoodsReceiptItems.AddRange(details);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = receipt.Id },
                new
                {
                    message = "Penerimaan Barang berhasil disimpan.",
                    id = receipt.Id,
                    receiptNo = receipt.ReceiptNo,
                    status = receipt.Status
                });
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();

            return Conflict(new
            {
                message = "Penerimaan Barang gagal disimpan. Periksa data dan relasi database.",
                detail = ex.InnerException?.Message ?? ex.Message
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                message = "Terjadi kesalahan saat menyimpan Penerimaan Barang.",
                detail = ex.Message
            });
        }
    } 

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] GoodsReceiptDto request)
    {
        if (request == null || request.Id <= 0)
        {
            return BadRequest(new
            {
                message = "Data Penerimaan Barang tidak valid."
            });
        }

        var receipt = await _db.GoodsReceipts
            .FirstOrDefaultAsync(x => x.Id == request.Id);

        if (receipt == null)
        {
            return NotFound(new
            {
                message = "Penerimaan Barang tidak ditemukan."
            });
        }

        if (!string.Equals(
                receipt.Status,
                "draft",
                StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new
            {
                message = "Hanya Penerimaan Barang berstatus draft yang dapat diedit."
            });
        }

        if (request.ReceiptDate == default)
        {
            return BadRequest(new
            {
                message = "Tanggal penerimaan wajib diisi."
            });
        }

        if (request.SupplierId <= 0 ||
            request.WarehouseId <= 0)
        {
            return BadRequest(new
            {
                message = "Supplier dan gudang wajib dipilih."
            });
        }

        if (!AllowedStatuses.Contains(
                request.Status,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Status Penerimaan Barang tidak valid."
            });
        }

        if (request.Details == null ||
            request.Details.Count == 0)
        {
            return BadRequest(new
            {
                message = "Minimal satu barang harus ditambahkan."
            });
        }

        for (var i = 0; i < request.Details.Count; i++)
        {
            var detail = request.Details[i];

            if (detail.ProductId <= 0 ||
                detail.Qty <= 0 ||
                detail.UnitCost < 0 ||
                detail.Subtotal < 0 ||
                (detail.UnitQty.HasValue &&
                 detail.UnitQty.Value <= 0))
            {
                return BadRequest(new
                {
                    message = $"Detail barang pada baris {i + 1} tidak valid."
                });
            }
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            var oldDetails = await _db.GoodsReceiptItems
                .Where(x => x.ReceiptId == receipt.Id)
                .ToListAsync();

            _db.GoodsReceiptItems.RemoveRange(oldDetails);

            receipt.ReceiptDate = request.ReceiptDate;
            receipt.SupplierId = request.SupplierId;
            receipt.WarehouseId = request.WarehouseId;
            receipt.POId = request.POId;
            receipt.RefNo = request.RefNo;
            receipt.Note = request.Note;
            receipt.Status = request.Status.Trim().ToLowerInvariant();
            receipt.UpdatedAt = DateTime.UtcNow;

            var newDetails = request.Details.Select(detail =>
                new GoodsReceiptItem
                {
                    Uid = Guid.NewGuid().ToString(),

                    ReceiptId = receipt.Id,
                    ProductId = detail.ProductId,
                    UnitId = detail.UnitId,

                    UnitQty = detail.UnitQty ?? detail.Qty,
                    Qty = detail.Qty,

                    UnitCost = detail.UnitCost,
                    Subtotal = detail.Subtotal,

                    DimLength = detail.DimLength ?? 0m,
                    DimWidth = detail.DimWidth ?? 0m,
                    DimHeight = detail.DimHeight ?? 0m,
                    DimCount = detail.DimCount ?? 0m
                })
                .ToList();

            _db.GoodsReceiptItems.AddRange(newDetails);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Penerimaan Barang berhasil diubah.",
                id = receipt.Id,
                receiptNo = receipt.ReceiptNo,
                status = receipt.Status
            });
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();

            return Conflict(new
            {
                message = "Penerimaan Barang gagal diedit. Periksa data dan relasi database.",
                detail = ex.InnerException?.Message ?? ex.Message
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                message = "Terjadi kesalahan saat mengedit Penerimaan Barang.",
                detail = ex.Message
            });
        }
    }
     
    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteGoodsReceiptRequest request)
    {
        if (request == null || request.Id <= 0)
        {
            return BadRequest(new
            {
                message = "ID Penerimaan Barang tidak valid."
            });
        }

        var receipt = await _db.GoodsReceipts
            .FirstOrDefaultAsync(x => x.Id == request.Id);

        if (receipt == null)
        {
            return NotFound(new
            {
                message = "Penerimaan Barang tidak ditemukan."
            });
        }

        if (!string.Equals(
                receipt.Status,
                "draft",
                StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new
            {
                message = "Hanya Penerimaan Barang berstatus draft yang dapat dihapus."
            });
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            var details = await _db.GoodsReceiptItems
                .Where(x => x.ReceiptId == receipt.Id)
                .ToListAsync();

            _db.GoodsReceiptItems.RemoveRange(details);
            _db.GoodsReceipts.Remove(receipt);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Penerimaan Barang beserta detailnya berhasil dihapus."
            });
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();

            return Conflict(new
            {
                message = "Penerimaan Barang gagal dihapus karena terkait dengan data lain.",
                detail = ex.InnerException?.Message ?? ex.Message
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                message = "Terjadi kesalahan saat menghapus Penerimaan Barang.",
                detail = ex.Message
            });
        }
    }
     
    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> Approve(long id)
    {
        var receipt = await _db.GoodsReceipts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (receipt == null)
        {
            return NotFound(new
            {
                message = "Penerimaan Barang tidak ditemukan."
            });
        }

        if (!string.Equals(
                receipt.Status,
                "draft",
                StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new
            {
                message = "Hanya Penerimaan Barang berstatus draft yang dapat diposting."
            });
        }

        receipt.Status = "posted";
        receipt.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Penerimaan Barang berhasil diposting.",
            id = receipt.Id,
            receiptNo = receipt.ReceiptNo,
            status = receipt.Status
        });
    }
}
