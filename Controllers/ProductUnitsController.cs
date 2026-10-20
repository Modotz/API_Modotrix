
using ERP.Api.Data;
using ERP.Api.DTOs.Unit;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/product-units")]
public class ProductUnitsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductUnitsController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/product-units/product/10
    [HttpGet("product/{productId:long}")]
    public async Task<IActionResult> GetByProduct(long productId)
    {
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == productId
                && x.DeletedAt == null);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Produk tidak ditemukan."
            });
        }

        var units = await (
            from pu in _db.ProductUnits.AsNoTracking()
            join u in _db.Units.AsNoTracking()
                on pu.UnitId equals u.Id
            where pu.ProductId == productId
            select new
            {
                pu.Id,
                pu.ProductId,
                pu.UnitId,
                UnitCode = u.Code,
                UnitName = u.Name,
                UnitSymbol = u.Symbol,
                pu.Conversion
            }
        ).ToListAsync();

        return Ok(new
        {
            productId = product.Id,
            productName = product.Name,
            baseUnitId = product.UnitId,
            baseUnitName = await _db.Units
                .Where(x => x.Id == product.UnitId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(),
            units
        });
    }

    public class CreateProductUnitRequest
    {
        public long ProductId { get; set; }
        public long UnitId { get; set; }
        public decimal Conversion { get; set; }
    }

    // POST: api/product-units/create
    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductUnitRequest request)
    {
        if (request.ProductId <= 0 || request.UnitId <= 0 || request.Conversion <= 0)
        {
            return BadRequest(new
            {
                message = "Produk, satuan, dan konversi harus valid."
            });
        }

        var product = await _db.Products.FirstOrDefaultAsync(
            x => x.Id == request.ProductId &&
                 x.DeletedAt == null);

        if (product == null)
        {
            return NotFound(new { message = "Produk tidak ditemukan." });
        }

        var unitExists = await _db.Units.AnyAsync(
            x => x.Id == request.UnitId &&
                 x.DeletedAt == null);

        if (!unitExists)
        {
            return BadRequest(new { message = "Satuan tidak ditemukan." });
        }

        if (product.UnitId == request.UnitId)
        {
            return BadRequest(new
            {
                message = "Satuan tersebut sudah menjadi satuan dasar produk."
            });
        }

        var duplicate = await _db.ProductUnits.AnyAsync(
            x => x.ProductId == request.ProductId &&
                 x.UnitId == request.UnitId);

        if (duplicate)
        {
            return Conflict(new
            {
                message = "Satuan konversi tersebut sudah ditambahkan."
            });
        }

        var item = new ProductUnit
        {
            Uid = Guid.NewGuid().ToString(),
            ProductId = request.ProductId,
            UnitId = request.UnitId,
            Conversion = request.Conversion
        };

        _db.ProductUnits.Add(item);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Satuan konversi berhasil ditambahkan.",
            id = item.Id
        });
    }

    // POST: api/product-units/delete
    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteProductUnitRequest request)
    {
        var item = await _db.ProductUnits.FirstOrDefaultAsync(
            x => x.Id == request.Id);

        if (item == null)
        {
            return NotFound(new
            {
                message = "Satuan konversi tidak ditemukan."
            });
        }

        _db.ProductUnits.Remove(item);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Satuan konversi berhasil dihapus."
        });
    }
}