using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/pricelists")]
public class PriceListItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PriceListItemsController(AppDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET ITEMS BY PRICE LIST
    // =========================================================
    [HttpGet("{priceListId:long}/items")]
    public async Task<IActionResult> GetItems(long priceListId)
    {
        var priceListExists = await _context.PriceLists
            .AnyAsync(x =>
                x.Id == priceListId &&
                x.DeletedAt == null);

        if (!priceListExists)
        {
            return NotFound(new
            {
                message = "Price list tidak ditemukan."
            });
        }

        var items = await _context.PriceListItems
            .Where(x => x.PriceListId == priceListId)
            .OrderBy(x => x.Id)
            .Select(x => new PriceListItemDto
            {
                Id = x.Id,
                PriceListId = x.PriceListId,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                Price = x.Price,
                TaxInclusive = x.TaxInclusive
            })
            .ToListAsync();

        return Ok(items);
    }


    // =========================================================
    // CREATE
    // =========================================================
    [HttpPost("items/create")]
    public async Task<IActionResult> Create(CreatePriceListItemRequest dto)
    {
        var priceListExists = await _context.PriceLists
            .AnyAsync(x => x.Id == dto.PriceListId && x.DeletedAt == null);

        if (!priceListExists)
            return BadRequest(new
            {
                message = "Price list tidak ditemukan."
            });

        // Ambil data product
        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == dto.ProductId &&
                x.DeletedAt == null);

        if (product == null)
            return BadRequest(new
            {
                message = "Produk tidak ditemukan."
            });

        var alreadyExists = await _context.PriceListItems
            .AnyAsync(x =>
                x.PriceListId == dto.PriceListId &&
                x.ProductId == dto.ProductId);

        if (alreadyExists)
            return Conflict(new
            {
                message = "Produk tersebut sudah ada di Price List."
            });

        var item = new PriceListItem
        {
            Uid = Guid.NewGuid().ToString(),
            PriceListId = dto.PriceListId,
            ProductId = dto.ProductId,

            // Ambil UnitId dari Product
            UnitId = product.UnitId,

            Price = dto.Price,
            TaxInclusive = dto.TaxInclusive ?? false,

            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.PriceListItems.Add(item);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            id = item.Id,
            message = "Produk berhasil ditambahkan ke Price List."
        });
    }

    // =========================================================
    // UPDATE
    // =========================================================
    [HttpPost("items/update")]
    public async Task<IActionResult> Update(
        UpdatePriceListItemRequest dto)
    {
        var item = await _context.PriceListItems
            .FirstOrDefaultAsync(x => x.Id == dto.Id);

        if (item == null)
        {
            return NotFound(new
            {
                message = "Item Price List tidak ditemukan."
            });
        }

        item.Price = dto.Price;
        item.TaxInclusive = dto.TaxInclusive;
        item.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Harga Price List berhasil diubah."
        });
    }


    // =========================================================
    // DELETE
    // =========================================================
    [HttpPost("items/delete")]
    public async Task<IActionResult> Delete(
        DeletePriceListItemRequest dto)
    {
        var item = await _context.PriceListItems
            .FirstOrDefaultAsync(x => x.Id == dto.Id);

        if (item == null)
        {
            return NotFound(new
            {
                message = "Item Price List tidak ditemukan."
            });
        }

        _context.PriceListItems.Remove(item);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Produk berhasil dihapus dari Price List."
        });
    }
}