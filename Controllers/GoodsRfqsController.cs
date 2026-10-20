
using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoodsRfqsController : ControllerBase
{
    private readonly AppDbContext _db;

    public GoodsRfqsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var query =
            from rfq in _db.GoodsRfqs.AsNoTracking()
            join supplier in _db.Suppliers.AsNoTracking()
                on rfq.SupplierId equals supplier.Id
            select new GoodsRfqDto
            {
                Id = rfq.Id,
                Uid = rfq.Uid,
                RfqNo = rfq.RfqNo,
                RfqDate = rfq.RfqDate,
                ValidUntil = rfq.ValidUntil,
                SupplierId = rfq.SupplierId,
                SupplierName = supplier.Name,
                PrId = rfq.PrId,
                Note = rfq.Note,
                Status = rfq.Status,
                SubtotalAmount = rfq.SubtotalAmount,
                CreatedAt = rfq.CreatedAt
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.RfqNo.Contains(search) ||
                x.SupplierName.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        var totalData = await query.CountAsync();

        var data = await query
            .OrderByDescending(x => x.RfqDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPage =
            (int)Math.Ceiling(totalData / (double)pageSize);

        return Ok(new
        {
            data,
            page,
            pageSize,
            totalData,
            totalPage
        });
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateGoodsRfqDto dto)
    {
        if (dto.SupplierId <= 0)
        {
            return BadRequest(new
            {
                message = "Supplier wajib dipilih."
            });
        }

        if (dto.Items == null || dto.Items.Count == 0)
        {
            return BadRequest(new
            {
                message = "Minimal harus ada satu item RFQ."
            });
        }

        if (dto.Items.Any(x =>
            x.ProductId <= 0 ||
            x.Quantity <= 0 ||
            x.UnitQty < 0 ||
            x.DimLength < 0 ||
            x.DimWidth < 0 ||
            x.DimHeight < 0 ||
            x.DimCount < 0 ||
            x.EstPrice < 0 ||
            x.QuotedPrice < 0))
        {
            return BadRequest(new
            {
                message = "Periksa produk, quantity, dimensi, dan harga item."
            });
        }

        var supplierExists = await _db.Suppliers
            .AnyAsync(x => x.Id == dto.SupplierId);

        if (!supplierExists)
        {
            return BadRequest(new
            {
                message = "Supplier tidak ditemukan."
            });
        }

        var productIds = dto.Items
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var existingProductIds = await _db.Products
            .Where(x => productIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        if (productIds.Except(existingProductIds).Any())
        {
            return BadRequest(new
            {
                message = "Ada produk yang tidak ditemukan."
            });
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            var now = DateTime.Now;
            var datePrefix = $"RFQ-{now:yyMMdd}";

            // Cari nomor RFQ terakhir pada tanggal yang sama.
            var lastRfqNo = await _db.GoodsRfqs
                .Where(x => x.RfqNo.StartsWith(datePrefix))
                .OrderByDescending(x => x.RfqNo)
                .Select(x => x.RfqNo)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrEmpty(lastRfqNo) &&
                int.TryParse(
                    lastRfqNo.Substring(datePrefix.Length),
                    out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            var newRfqNo = $"{datePrefix}{nextNumber:D3}";

            // Buat header RFQ.
            var rfq = new GoodsRfq
            {
                Uid = Guid.NewGuid().ToString(),
                RfqNo = newRfqNo,
                RfqDate = dto.RfqDate,
                ValidUntil = dto.ValidUntil,
                SupplierId = dto.SupplierId,
                PrId = dto.PrId,
                Note = dto.Note,
                Status = "draft",
                SubtotalAmount = 0,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.GoodsRfqs.Add(rfq);

            // Simpan header untuk mendapatkan ID RFQ.
            await _db.SaveChangesAsync();

            // Buat item-item RFQ.
            var items = dto.Items.Select(x => new GoodsRfqItem
            {
                RfqId = rfq.Id,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                UnitQty = x.UnitQty,
                DimLength = x.DimLength,
                DimWidth = x.DimWidth,
                DimHeight = x.DimHeight,
                DimCount = x.DimCount,
                Quantity = x.Quantity,

                // Harga perkiraan internal dari halaman Create Draft.
                EstPrice = x.EstPrice,

                // Harga penawaran belum diisi pada tahap Draft.
                QuotedPrice = 0,
                Subtotal = 0,

                Note = x.Note
            }).ToList();

            // Total penawaran belum dihitung selama masih Draft.
            rfq.SubtotalAmount = 0;

            _db.GoodsRfqItems.AddRange(items);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Goods RFQ berhasil dibuat.",
                id = rfq.Id,
                rfqNo = rfq.RfqNo,
                subtotalAmount = rfq.SubtotalAmount,
                items
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        }

        // GET: api/GoodsRfqs/1
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var rfq = await _db.GoodsRfqs   
                .FirstOrDefaultAsync(x => x.Id == id);

            if (rfq == null)
                return NotFound(new { message = "RFQ tidak ditemukan." });

            var supplier = await _db.Suppliers
                .Where(x => x.Id == rfq.SupplierId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            var items = await _db.GoodsRfqItems
                .Where(x => x.RfqId == id)
                .Select(x => new GoodsRfqItemDto
                {
                    Id = x.Id,
                    RfqId = x.RfqId,
                    ProductId = x.ProductId,
                    UnitId = x.UnitId,
                    UnitQty = x.UnitQty,
                    DimLength = x.DimLength,
                    DimWidth = x.DimWidth,
                    DimHeight = x.DimHeight,
                    DimCount = x.DimCount,
                    Quantity = x.Quantity,
                    EstPrice = x.EstPrice,
                    QuotedPrice = x.QuotedPrice,
                    Subtotal = x.Subtotal,
                    Note = x.Note
                })
                .ToListAsync();

            var result = new GoodsRfqDto
            {
                Id = rfq.Id,
                Uid = rfq.Uid,
                RfqNo = rfq.RfqNo,
                RfqDate = rfq.RfqDate,
                ValidUntil = rfq.ValidUntil,
                SupplierId = rfq.SupplierId,
                SupplierName = supplier ?? "",
                PrId = rfq.PrId,
                Note = rfq.Note,
                Status = rfq.Status,
                SubtotalAmount = rfq.SubtotalAmount,
                CreatedAt = rfq.CreatedAt,
                Items = items
            };

            return Ok(result);
        }

        // PUT: api/GoodsRfqs/1
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(
            long id,
            [FromBody] CreateGoodsRfqDto dto)
        {
            var rfq = await _db.GoodsRfqs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (rfq == null)
                return NotFound(new { message = "RFQ tidak ditemukan." });

            if (!string.Equals(
                rfq.Status, "draft",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "RFQ hanya dapat diedit selama berstatus Draft."
                });
            }

            if (dto.SupplierId <= 0)
                return BadRequest(new { message = "Supplier wajib dipilih." });

            if (dto.Items == null || !dto.Items.Any())
                return BadRequest(new { message = "Minimal satu barang harus diisi." });

            var supplierExists = await _db.Suppliers
                .AnyAsync(x => x.Id == dto.SupplierId);

            if (!supplierExists)
                return BadRequest(new { message = "Supplier tidak ditemukan." });

            var productIds = dto.Items
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            if (productIds.Any(x => x <= 0))
                return BadRequest(new { message = "Semua barang wajib dipilih." });

            var existingProductIds = await _db.Products
                .Where(x => productIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (existingProductIds.Count != productIds.Count)
                return BadRequest(new { message = "Ada barang yang tidak ditemukan." });

            if (dto.Items.Any(x => x.Quantity <= 0))
                return BadRequest(new { message = "Kuantitas barang harus lebih dari 0." });

            await using var transaction =
                await _db.Database.BeginTransactionAsync();

            try
            {
                rfq.RfqDate = dto.RfqDate;
                rfq.ValidUntil = dto.ValidUntil;
                rfq.SupplierId = dto.SupplierId;
                rfq.PrId = dto.PrId;
                rfq.Note = dto.Note;
                rfq.UpdatedAt = DateTime.Now;

                // Hapus item lama karena daftar barang akan diganti.
                var oldItems = await _db.GoodsRfqItems
                    .Where(x => x.RfqId == id)
                    .ToListAsync();

                _db.GoodsRfqItems.RemoveRange(oldItems);

                var newItems = dto.Items.Select(x => new GoodsRfqItem
                {
                    RfqId = id,
                    ProductId = x.ProductId,
                    UnitId = x.UnitId,
                    UnitQty = x.UnitQty,
                    DimLength = x.DimLength,
                    DimWidth = x.DimWidth,
                    DimHeight = x.DimHeight,
                    DimCount = x.DimCount,
                    Quantity = x.Quantity,
                    EstPrice = x.EstPrice,

                    // Belum ada penawaran ketika masih Draft.
                    QuotedPrice = 0,
                    Subtotal = 0,

                    Note = x.Note
                }).ToList();

                _db.GoodsRfqItems.AddRange(newItems);

                // Belum ada harga penawaran, sehingga subtotal tetap 0.
                rfq.SubtotalAmount = 0;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "RFQ berhasil diperbarui.",
                    id = rfq.Id
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan saat memperbarui RFQ."
                });
            }

        }

        // =========================================================
        // KIRIM KE SUPPLIER : draft -> terkirim
        // POST: api/GoodsRfqs/1/send
        // =========================================================
        [HttpPost("{id:long}/send")]
        public async Task<IActionResult> Sent(long id)
        {
            var rfq = await _db.GoodsRfqs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (rfq == null)
                return NotFound(new { message = "RFQ tidak ditemukan." });

            if (!IsStatus(rfq.Status, "draft"))
            {
                return BadRequest(new
                {
                    message = "Hanya RFQ berstatus Draft yang dapat dikirim ke supplier."
                });
            }

            var hasItems = await _db.GoodsRfqItems
                .AnyAsync(x => x.RfqId == id);

            if (!hasItems)
            {
                return BadRequest(new
                {
                    message = "RFQ belum memiliki item."
                });
            }

            rfq.Status = "sent";
            rfq.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "RFQ berhasil dikirim ke supplier.",
                id = rfq.Id,
                status = rfq.Status
            });
        }

        // =========================================================
        // INPUT PENAWARAN : terkirim / ditawar -> ditawar
        // POST: api/GoodsRfqs/1/quote
        // Body: { "items": [ { "id": 10, "quotedPrice": 150000 } ] }
        // =========================================================
        [HttpPost("{id:long}/quote")]
        public async Task<IActionResult> Quote(
            long id,
            [FromBody] QuoteGoodsRfqDto dto)
        {
            var rfq = await _db.GoodsRfqs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (rfq == null)
                return NotFound(new { message = "RFQ tidak ditemukan." });

            if (!IsStatus(rfq.Status, "sent") &&
                !IsStatus(rfq.Status, "quoted"))
            {
                return BadRequest(new
                {
                    message = "Penawaran hanya dapat diinput setelah RFQ dikirim ke supplier."
                });
            }

            if (dto.Items == null || dto.Items.Count == 0)
            {
                return BadRequest(new
                {
                    message = "Data penawaran kosong."
                });
            }

            if (dto.Items.Any(x => x.QuotedPrice < 0))
            {
                return BadRequest(new
                {
                    message = "Harga penawaran tidak boleh negatif."
                });
            }

            if (!dto.Items.Any(x => x.QuotedPrice > 0))
            {
                return BadRequest(new
                {
                    message = "Isi minimal satu harga penawaran."
                });
            }

            var items = await _db.GoodsRfqItems
                .Where(x => x.RfqId == id)
                .ToListAsync();

            foreach (var input in dto.Items)
            {
                var item = items.FirstOrDefault(x => x.Id == input.Id);

                if (item == null)
                    continue;

                item.QuotedPrice = input.QuotedPrice;
                item.Subtotal = item.Quantity * input.QuotedPrice;
            }

            // Total penawaran = jumlah subtotal semua item
            rfq.SubtotalAmount = items.Sum(x => x.Subtotal);
            rfq.Status = "quoted";
            rfq.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Penawaran supplier berhasil disimpan.",
                id = rfq.Id,
                status = rfq.Status,
                subtotalAmount = rfq.SubtotalAmount
            });
        }

        // =========================================================
        // BATALKAN RFQ : draft / terkirim / ditawar -> batal
        // POST: api/GoodsRfqs/1/cancel
        // =========================================================
        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id)
        {
            var rfq = await _db.GoodsRfqs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (rfq == null)
                return NotFound(new { message = "RFQ tidak ditemukan." });

            if (IsStatus(rfq.Status, "converted") ||
                IsStatus(rfq.Status, "cancelled"))
            {
                return BadRequest(new
                {
                    message = "RFQ ini tidak dapat dibatalkan."
                });
            }

            rfq.Status = "cancelled";
            rfq.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "RFQ berhasil dibatalkan.",
                id = rfq.Id,
                status = rfq.Status
            });
        }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var rfq = await _db.GoodsRfqs.FirstOrDefaultAsync(x => x.Id == id);

        if (rfq == null)
            return NotFound(new { message = "RFQ tidak ditemukan." });

        if (IsStatus(rfq.Status, "converted"))
        {
            return BadRequest(new
            {
                message = "RFQ yang sudah menjadi PO tidak dapat dihapus."
            });
        }

        try
        {
            var items = await _db.GoodsRfqItems
                .Where(x => x.RfqId == id)
                .ToListAsync();

            _db.GoodsRfqItems.RemoveRange(items);
            _db.GoodsRfqs.Remove(rfq);

            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { message = ex.GetBaseException().Message });
        }

        return Ok(new { message = "RFQ berhasil dihapus." });
    }

    private static bool IsStatus(string? current, string expected) =>
            string.Equals(
                (current ?? string.Empty).Trim().Replace(" ", "_"),
                expected,
                StringComparison.OrdinalIgnoreCase);


    // GET: api/GoodsRfqs/next-number
    [HttpGet("next-number")]
    public async Task<IActionResult> NextNumber()
    {
        var prefix = $"RFQ-{DateTime.Now:yyMMdd}";

        var last = await _db.GoodsRfqs
            .Where(x => x.RfqNo.StartsWith(prefix))
            .OrderByDescending(x => x.RfqNo)
            .Select(x => x.RfqNo)
            .FirstOrDefaultAsync();

        var next = 1;
        if (!string.IsNullOrEmpty(last) &&
            int.TryParse(last.Substring(prefix.Length), out var n))
            next = n + 1;

        return Ok(new { rfqNo = $"{prefix}{next:D3}" });
    }

}
    