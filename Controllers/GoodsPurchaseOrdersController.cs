using ERP.Api.Data;
using ERP.Api.DTOs.GoodsPurchaseOrder;  
using ERP.Api.DTOs.Product;
using ERP.Api.DTOs.Unit;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks.Dataflow;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/goods-purchase-orders")]
public class GoodsPurchaseOrders : ControllerBase
{
    private readonly AppDbContext _db;

    public GoodsPurchaseOrders (AppDbContext db)
    {
        _db = db;
    }

    private static readonly string[] AllowedStatuses =
    {
        "draft",
        "pending_approval",
        "ordered",
        "partial",
        "received",
        "cancelled"
    };

    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] string? search = null,
    [FromQuery] long? supplierId = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    { 
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;


        var query = _db.GoodsPurchaseOrders
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.PONo.Contains(search) ||
                (x.Note != null && x.Note.Contains(search)));
        }

        var totalData = await query.CountAsync();
        var totalPage =
            (int)Math.Ceiling(
                totalData / (double)pageSize
            );
            
        var data = await query
            .OrderByDescending(x => x.PODate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new GoodsPurchaseOrderDto
            {
                Id = x.Id,
                Uid = x.Uid,
                PONo = x.PONo,
                PODate = x.PODate,

                SupplierId = x.SupplierId,
                SupplierName = _db.Suppliers
                    .Where(c =>
                        c.Id == x.SupplierId &&
                        c.DeletedAt == null)
                    .Select(c => c.Name)
                    .FirstOrDefault(),

                PRId = x.PRId,
                RFQId = x.RFQId,

                WarehouseId = x.WarehouseId,
                WarehouseName = _db.Warehouses
                    .Where(c =>
                        c.id == x.WarehouseId &&
                        c.deleted_at == null)
                    .Select(c => c.name)
                    .FirstOrDefault(),

                ExpectedDate = x.ExpectedDate,
                Note = x.Note,
                SubtotalAmount = x.SubtotalAmount ?? 0,
                DiscountAmount = x.DiscountAmount ?? 0,
                TaxPercent = x.TaxPercent ?? 0,
                TaxAmount = x.TaxAmount ?? 0,
                Status = x.Status,
                TotalAmount = x.TotalAmount,
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

    private async Task<string> GeneratePONumber(
     DateOnly poDate,
     bool fromPRorRFQ)
    {
        var prefix = fromPRorRFQ ? "PO" : "GPO";
        var dateCode = poDate.ToString("yyMMdd");
        var dailyPrefix = $"{prefix}-{dateCode}";

        var existingNumbers = await _db.GoodsPurchaseOrders
            .AsNoTracking()
            .Where(x => x.PONo.StartsWith(dailyPrefix))
            .Select(x => x.PONo)
            .ToListAsync();

        var maxNumber = existingNumbers
            .Select(number =>
            {
                var suffix = number.Substring(dailyPrefix.Length);

                return int.TryParse(suffix, out var value)
                    ? value
                    : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"{dailyPrefix}{maxNumber + 1:D3}";
    }

    [HttpGet("next-number")]
    public async Task<IActionResult> GetNextPONumber(
    [FromQuery] DateOnly? poDate = null,
    [FromQuery] bool fromPRorRFQ = false)
    {
        var date = poDate
            ?? DateOnly.FromDateTime(DateTime.Today);

        var poNumber = await GeneratePONumber(date, fromPRorRFQ);

        return Ok(new
        {
            poNo = poNumber
        });
    }

    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses()
    {
        var warehouses = await _db.Warehouses
            .AsNoTracking()
            .Where(x => x.deleted_at == null)
            .Select(x => new
            {
                x.id,
                x.name
            })
            .OrderBy(x => x.name)
            .ToListAsync();

        return Ok(warehouses);
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSuppliers()
    {
        var suppliers = await _db.Suppliers
            .AsNoTracking()
            .Where(x =>
                x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.Name
            })
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _db.GoodsPurchaseOrders
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new GoodsPurchaseOrderDto
            {
                Id = x.Id,
                Uid = x.Uid,
                PONo = x.PONo,
                PODate = x.PODate,

                SupplierId = x.SupplierId,
                SupplierName = _db.Suppliers
                    .Where(s => s.Id == x.SupplierId && s.DeletedAt == null)
                    .Select(s => s.Name)
                    .FirstOrDefault(),

                PRId = x.PRId,
                RFQId = x.RFQId,

                WarehouseId = x.WarehouseId,
                WarehouseName = _db.Warehouses
                    .Where(w => w.id == x.WarehouseId && w.deleted_at == null)
                    .Select(w => w.name)
                    .FirstOrDefault(),

                ExpectedDate = x.ExpectedDate,
                Note = x.Note,

                SubtotalAmount = x.SubtotalAmount ?? 0,
                DiscountAmount = x.DiscountAmount ?? 0,
                TaxPercent = x.TaxPercent ?? 0,
                TaxAmount = x.TaxAmount ?? 0,

                Status = x.Status,
                TotalAmount = x.TotalAmount,

                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                Details = _db.GoodsPurchaseOrderItems
                    .Where(d => d.POId == x.Id)
                    .Select(d => new GoodsPurchaseOrderItemDto
                    {
                        POId = d.POId,
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

                        Qty = d.Qty,
                        ReceivedQty = d.ReceivedQty,
                        UnitPrice = d.UnitPrice,
                        DiscPercent = d.DiscPercent,
                        TaxPercent = d.TaxPercent,
                        Subtotal = d.Subtotal,

                        DimLength = d.DimLength,
                        DimWidth = d.DimWidth,
                        DimHeight = d.DimHeight,
                        DimCount = d.DimCount,
                        UnitQty = d.UnitQty
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound(new
            {
                message = "Purchase Order tidak ditemukan."
            });

        return Ok(item);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] GoodsPurchaseOrderDto request)
    {
        if (request == null)
            return BadRequest(new { message = "Data Purchase Order wajib diisi." });

        if (request.PODate == default)
            return BadRequest(new { message = "Tanggal PO wajib diisi." });

        if (request.SupplierId <= 0)
            return BadRequest(new { message = "Supplier wajib dipilih." });

        if (request.WarehouseId <= 0)
            return BadRequest(new { message = "Gudang wajib dipilih." });

        if (!AllowedStatuses.Contains(request.Status))
            return BadRequest(new { message = "Status PO tidak valid." });

        if (request.Details == null || request.Details.Count == 0)
            return BadRequest(new { message = "Minimal satu barang harus ditambahkan." });

        if (request.SubtotalAmount < 0 ||
            request.DiscountAmount < 0 ||
            request.TaxPercent < 0 ||
            request.TaxPercent > 100 ||
            request.TaxAmount < 0 ||
            request.TotalAmount < 0)
        {
            return BadRequest(new
            {
                message = "Nilai nominal dan pajak tidak valid."
            });
        }
         
        for (var i = 0; i < request.Details.Count; i++)
        {
            var detail = request.Details[i];

            if (detail.ProductId <= 0)
                return BadRequest(new
                {
                    message = $"Produk pada baris {i + 1} wajib dipilih."
                });

            if (detail.Qty <= 0)
                return BadRequest(new
                {
                    message = $"Kuantitas pada baris {i + 1} harus lebih dari 0."
                });

            if (detail.UnitPrice < 0 ||
                detail.DiscPercent < 0 ||
                detail.DiscPercent > 100 ||
                detail.TaxPercent < 0 ||
                detail.TaxPercent > 100 ||
                detail.Subtotal < 0)
            {
                return BadRequest(new
                {
                    message = $"Harga, diskon, pajak, atau subtotal pada baris {i + 1} tidak valid."
                });
            }
        }

        var fromPRorRFQ = request.PRId.HasValue || request.RFQId.HasValue;

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            var subtotalAmount = request.Details.Sum(x => x.Subtotal); 
            var discountAmount = request.DiscountAmount;
             
            var taxableAmount = subtotalAmount - discountAmount;

            if (taxableAmount < 0)
            {
                return BadRequest(new
                {
                    message = "Diskon PO tidak boleh melebihi subtotal."
                });
            }
             
            var taxAmount = decimal.Round(
                taxableAmount * request.TaxPercent / 100m,
                2,
                MidpointRounding.AwayFromZero
            );
             
            var totalAmount = subtotalAmount - discountAmount + taxAmount;
            var poNumber = await GeneratePONumber(
                request.PODate,
                fromPRorRFQ);

            var po = new GoodsPurchaseOrder
            {
                Uid = Guid.NewGuid().ToString(),
                PONo = poNumber,
                PODate = request.PODate,
                SupplierId = request.SupplierId,
                PRId = request.PRId,
                RFQId = request.RFQId,
                WarehouseId = request.WarehouseId,
                ExpectedDate = request.ExpectedDate ?? DateOnly.FromDateTime(DateTime.Today),
                Note = request.Note,

                // Nilai hasil perhitungan API
                SubtotalAmount = subtotalAmount,
                DiscountAmount = discountAmount,
                TaxPercent = request.TaxPercent,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,

                Status = request.Status,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _db.GoodsPurchaseOrders.Add(po);
            await _db.SaveChangesAsync();

            var details = request.Details.Select(detail =>  new GoodsPurchaseOrderItem
            {
                Uid = Guid.NewGuid().ToString(),
                POId = po.Id,
                ProductId = detail.ProductId,

                DimLength = detail.DimLength ?? 0m,
                DimWidth = detail.DimWidth ?? 0m,
                DimHeight = detail.DimHeight ?? 0m,
                DimCount = detail.DimCount ?? 0m,

                UnitId = detail.UnitId,
                UnitQty = detail.UnitQty ?? detail.Qty,
                Qty = detail.Qty,
                 
                ReceivedQty = 0m,

                UnitPrice = detail.UnitPrice,
                DiscPercent = detail.DiscPercent ?? 0m,
                TaxPercent = detail.TaxPercent ?? 0m,
                Subtotal = detail.Subtotal
            }).ToList();

            _db.GoodsPurchaseOrderItems.AddRange(details);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = po.Id },
                new
                {
                    message = "Purchase Order dan detail barang berhasil disimpan.",
                    id = po.Id,
                    poNo = po.PONo,
                    status = po.Status
                });
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            return Conflict(new
            {
                message = "Purchase Order gagal disimpan. Periksa data dan relasi database."
            });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                message = "Terjadi kesalahan saat menyimpan Purchase Order."
            });
        }
       
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(
    [FromBody] GoodsPurchaseOrderDto request)
    {
        if (request == null || request.Id <= 0)
            return BadRequest(new
            {
                message = "Data Purchase Order tidak valid."
            });

        var item = await _db.GoodsPurchaseOrders.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (item == null)
            return NotFound(new
            {
                message = "Purchase Order tidak ditemukan."
            });

        if (!string.Equals(
            item.Status,
            "draft",
            StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new
            {
                message = "Hanya Purchase Order berstatus draft yang dapat diedit."
            });
        }

        if (request.PODate == default)
            return BadRequest(new
            {
                message = "Tanggal PO wajib diisi."
            });

        if (request.SupplierId <= 0 || request.WarehouseId <= 0)
            return BadRequest(new
            {
                message = "Supplier dan gudang wajib dipilih."
            });

        if (!AllowedStatuses.Contains(request.Status))
            return BadRequest(new
            {
                message = "Status PO tidak valid."
            });
 
        if (request.DiscountAmount < 0 ||
            request.TaxPercent < 0 ||
            request.TaxPercent > 100)
        {
            return BadRequest(new
            {
                message = "Diskon atau persentase pajak tidak valid."
            });
        }

        var duplicate = await _db.GoodsPurchaseOrders
            .AnyAsync(x =>
                x.PONo == request.PONo.Trim() &&
                x.Id != request.Id);
 
        foreach (var detail in request.Details)
        {
            if (detail.ProductId <= 0 ||
                detail.Qty <= 0 ||
                detail.UnitPrice < 0 ||
                detail.DiscPercent < 0 ||
                detail.DiscPercent > 100 ||
                detail.TaxPercent < 0 ||
                detail.TaxPercent > 100)
            {
                return BadRequest(new
                {
                    message = "Terdapat detail barang dengan nilai tidak valid."
                });
            }
        }

        await using var transaction =  await _db.Database.BeginTransactionAsync();

        try
        { 
            var oldDetails = await _db.GoodsPurchaseOrderItems
                .Where(x => x.POId == item.Id)
                .ToListAsync();
             
            if (oldDetails.Any(x => x.ReceivedQty > 0))
            {
                return Conflict(new
                {
                    message = "PO tidak dapat diedit karena sudah memiliki barang yang diterima."
                });
            }
             
            _db.GoodsPurchaseOrderItems.RemoveRange(oldDetails);


            var subtotalAmount = request.Details.Sum(x => x.Subtotal);
            var discountAmount = request.DiscountAmount;

            if (subtotalAmount < 0 || discountAmount > subtotalAmount)
            {
                return BadRequest(new
                {
                    message = "Diskon nota tidak boleh melebihi subtotal."
                });
            }

            var taxableAmount = subtotalAmount - discountAmount;

            var taxAmount = decimal.Round(
                taxableAmount * request.TaxPercent / 100m, 2,
                MidpointRounding.AwayFromZero);

            var totalAmount = taxableAmount + taxAmount;
             
            item.PONo = request.PONo.Trim();
            item.PODate = request.PODate;
            item.SupplierId = request.SupplierId;
            item.PRId = request.PRId;
            item.RFQId = request.RFQId;
            item.WarehouseId = request.WarehouseId;
            item.ExpectedDate = request.ExpectedDate;
            item.Note = request.Note;
            item.SubtotalAmount = subtotalAmount;
            item.DiscountAmount = discountAmount;
            item.TaxPercent = request.TaxPercent;
            item.TaxAmount = taxAmount;
            item.TotalAmount = totalAmount;
            item.Status = request.Status;
            item.UpdatedAt = DateTime.UtcNow;
             
            var newDetails = request.Details.Select(detail =>
                new GoodsPurchaseOrderItem
                {
                    Uid = Guid.NewGuid().ToString(),
                    POId = item.Id,
                    ProductId = detail.ProductId,
                    UnitId = detail.UnitId,
                    Qty = detail.Qty,
                    UnitQty = detail.UnitQty ?? detail.Qty,

                    DimLength = detail.DimLength ?? 0m,
                    DimWidth = detail.DimWidth ?? 0m,
                    DimHeight = detail.DimHeight ?? 0m,
                    DimCount = detail.DimCount ?? 0m,

                    UnitPrice = detail.UnitPrice,
                    DiscPercent = detail.DiscPercent,
                    TaxPercent = detail.TaxPercent,
                    Subtotal = detail.Subtotal,

                    ReceivedQty = 0m
                }).ToList();

            _db.GoodsPurchaseOrderItems.AddRange(newDetails);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Purchase Order berhasil diubah.",
                id = item.Id,
                poNo = item.PONo,
                subtotalAmount,
                discountAmount,
                taxAmount,
                totalAmount
            });
        }
        
         
        catch (DbUpdateException ex)
        {
              await transaction.RollbackAsync(); 
                return Conflict(new
                    {
                        message = "Purchase Order gagal diedit. Periksa data dan relasi database.",
                        detail = ex.InnerException?.Message ?? ex.Message
                    });
                }
        catch (Exception ex)
        {
                await transaction.RollbackAsync();

                Console.WriteLine(
                    $"UPDATE PO ERROR: {ex}");

                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan saat mengedit Purchase Order.",
                    detail = ex.Message
                });
        }

    }


    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteGoodsPurchaseOrderRequest request)
    {
        if (request == null || request.Id <= 0)
        {
            return BadRequest("ID Purchase Order tidak valid.");
        }

        var id = request.Id;

        var item = await _db.GoodsPurchaseOrders
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
        {
            return NotFound("Purchase Order tidak ditemukan.");
        }
         
        var hasReceivedItems = await _db.GoodsPurchaseOrderItems
            .AnyAsync(x => x.POId == id && x.ReceivedQty > 0);

        if (hasReceivedItems)
        {
            return Conflict( "Purchase Order tidak dapat dihapus karena sudah memiliki barang yang diterima.");
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        { 
            var details = await _db.GoodsPurchaseOrderItems
                .Where(x => x.POId == id)
                .ToListAsync();
             
            _db.GoodsPurchaseOrderItems.RemoveRange(details); 
            _db.GoodsPurchaseOrders.Remove(item);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("Purchase Order beserta detailnya berhasil dihapus.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(); 
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Purchase Order gagal dihapus. Silakan periksa relasi database atau hubungi administrator.");
        }
    }


    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> Approve(long id)
    {
        var po = await _db.GoodsPurchaseOrders
            .FirstOrDefaultAsync(x => x.Id == id);

        if (po == null)
        {
            return NotFound(new
            {
                message = "Purchase Order tidak ditemukan."
            });
        }

        if (!string.Equals(
                po.Status,
                "draft",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Hanya PO berstatus draft yang dapat disetujui."
            });
        }

        po.Status = "ordered";
        po.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Purchase Order berhasil disetujui.",
            id = po.Id,
            status = po.Status
        });
    }

}
