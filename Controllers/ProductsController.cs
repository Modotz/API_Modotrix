using ERP.Api.Data;
using ERP.Api.DTOs.Product;
using ERP.Api.DTOs.Unit;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks.Dataflow;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/products")]
public class Products : ControllerBase
{
    private readonly AppDbContext _db;

    public Products(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] string? search = null,
    [FromQuery] long? brandId = null,
    [FromQuery] long? categoryId = null,
    [FromQuery] long? subCategoryId = null,
    [FromQuery] string? productType = null,
    [FromQuery] long? unitId = null,
    [FromQuery] bool? trackStock = null,
    [FromQuery] bool? isActive = null,
    [FromQuery] bool showVariant = false,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    { 
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

         
        var query = _db.Products
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

         
        if (!showVariant)
        {
            // Hanya produk induk
            query = query.Where(x => !x.ParentId.HasValue);
        }
        else
        { 
            query = query.Where(x =>
                !x.ParentId.HasValue ||
                _db.Products.Any(parent =>
                    parent.Id == x.ParentId.Value &&
                    parent.HasVariants == true &&
                    parent.DeletedAt == null
                )
            );
        } 
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.ProductNumber.Contains(search) ||
                x.Name.Contains(search) ||
                x.Sku.Contains(search));
        }
         
        if (brandId.HasValue)
        {
            query = query.Where(x =>
                x.BrandId == brandId.Value);
        }
         
        if (categoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == categoryId.Value);
        }
         
        if (subCategoryId.HasValue)
        {
            query = query.Where(x =>
                x.SubCategoryId == subCategoryId.Value);
        }

         
        if (!string.IsNullOrWhiteSpace(productType))
        {
            query = query.Where(x =>
                x.ProductType == productType);
        }

         
        if (unitId.HasValue)
        {
            query = query.Where(x =>
                x.UnitId == unitId.Value);
        } 


        if (trackStock.HasValue)
        {
            query = query.Where(x =>
                x.TrackStock == trackStock.Value);
        }

         
        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }
         
        query = query
            .OrderBy(x =>
                x.ParentId.HasValue
                    ? x.ParentId
                    : x.Id)
            .ThenBy(x =>
                x.ParentId.HasValue ? 1 : 0)
            .ThenBy(x => x.Id);

         
        var totalData = await query.CountAsync(); 
        var totalPage =
            (int)Math.Ceiling(
                totalData / (double)pageSize
            );
         
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Uid = x.Uid,

                ProductNumber = x.ProductNumber,

                Sku = x.Sku,
                Barcode = x.Barcode,

                InternalCode = x.InternalCode,
                ExternalCode1 = x.ExternalCode1,
                ExternalCode2 = x.ExternalCode2,
                ExternalCode3 = x.ExternalCode3,

                Name = x.Name,
                 
                CategoryId = x.CategoryId,
                SubCategoryId = x.SubCategoryId,

                CategoryName = _db.ProductCategories
                    .Where(c =>
                        c.Id == x.CategoryId &&
                        c.DeletedAt == null)
                    .Select(c => c.Name)
                    .FirstOrDefault(),

                SubCategoryName = _db.ProductCategories
                    .Where(c =>
                        c.Id == x.SubCategoryId &&
                        c.DeletedAt == null)
                    .Select(c => c.Name)
                    .FirstOrDefault(),
                 
                BrandId = x.BrandId,

                BrandName = _db.Brands
                    .Where(c =>
                        c.Id == x.BrandId &&
                        c.DeletedAt == null)
                    .Select(c => c.Name)
                    .FirstOrDefault(),
                 
                ProductType = x.ProductType, 
                ParentId = x.ParentId, 
                HasVariants = x.HasVariants, 
                VariantCount = _db.Products
                    .Count(v =>
                        v.ParentId == x.Id &&
                        v.DeletedAt == null),
                 
                UnitId = x.UnitId,

                UnitName = _db.Units
                    .Where(c =>
                        c.Id == x.UnitId &&
                        c.DeletedAt == null)
                    .Select(c => c.Name)
                    .FirstOrDefault(),
                 
                DimMode = x.DimMode,

                DefLength = x.DefLength,
                DefWidth = x.DefWidth,
                DefHeight = x.DefHeight,
                 
                PriceMode = x.PriceMode,

                CostPerM3 = x.CostPerM3,
                SellPerM3 = x.SellPerM3,

                CostPrice = x.CostPrice,
                SellPrice = x.SellPrice,
                 
                MinStock = x.MinStock,

                TrackStock = x.TrackStock,
                TrackSerial = x.TrackSerial,
                 
                WarrantyMonths = x.WarrantyMonths,

                Description = x.Description,

                Image = x.Image,

                IsActive = x.IsActive
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

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductDto>> GetById(long id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.DeletedAt == null)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Uid = x.Uid,
                ProductNumber = x.ProductNumber,
                Sku = x.Sku,
                Barcode = x.Barcode,
                InternalCode = x.InternalCode,
                ExternalCode1 = x.ExternalCode1,
                ExternalCode2 = x.ExternalCode2,
                ExternalCode3 = x.ExternalCode3,
                Name = x.Name,
                CategoryId = x.CategoryId,
                SubCategoryId = x.SubCategoryId,
                BrandId = x.BrandId,
                ProductType = x.ProductType,
                ParentId = x.ParentId,

                HasVariants = x.HasVariants,
                VariantCount = _db.Products
                    .Count(v =>
                        v.ParentId == x.Id &&
                        v.DeletedAt == null),

                UnitId = x.UnitId,
                DimMode = x.DimMode,
                DefLength = x.DefLength,
                DefWidth = x.DefWidth,
                DefHeight = x.DefHeight,
                PriceMode = x.PriceMode,
                CostPerM3 = x.CostPerM3,
                SellPerM3 = x.SellPerM3,
                Description = x.Description,
                CostPrice = x.CostPrice,
                SellPrice = x.SellPrice,
                MinStock = x.MinStock,
                TrackStock = x.TrackStock,
                TrackSerial = x.TrackSerial,
                WarrantyMonths = x.WarrantyMonths,
                Image = x.Image,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return product is null
            ? NotFound()
            : Ok(product);
    }

    private async Task<string> GenerateProductNumber()
    {
        var lastProductNumber = await _db.Products
            .Where(x =>
                x.ProductNumber != null &&
                x.ProductNumber.StartsWith("PRD-"))
            .OrderByDescending(x => x.Id)
            .Select(x => x.ProductNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (!string.IsNullOrWhiteSpace(lastProductNumber))
        {
            var numberPart = lastProductNumber.Replace("PRD-", "");

            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"PRD-{nextNumber:D5}";
    }

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands()
    {
        var brands = await _db.Brands
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

        return Ok(brands);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _db.ProductCategories
            .AsNoTracking()
            .Where(x =>
                x.DeletedAt == null &&
                x.ParentId == null)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.ParentId
            })
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("subcategories/{subCategory}")]
    public async Task<IActionResult> GetSubCategories(long subCategory)
    {
        var data = await _db.ProductCategories
            .AsNoTracking()
            .Where(x =>
                x.ParentId == subCategory &&
                x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.ParentId
            })
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("units")]
    public async Task<IActionResult> GetUnits()
    {
        var units = await _db.Units
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

        return Ok(units);
    }

    //[HttpPost("create")]
    //public async Task<IActionResult> Create(
    // [FromBody] CreateProductRequest request)
    //{
    //    if (string.IsNullOrWhiteSpace(request.Sku))
    //    {
    //        return BadRequest(new
    //        {
    //            message = "SKU wajib diisi."
    //        });
    //    }

    //    if (string.IsNullOrWhiteSpace(request.Name))
    //    {
    //        return BadRequest(new
    //        {
    //            message = "Nama Produk wajib diisi."
    //        });
    //    }

    //    if (string.IsNullOrWhiteSpace(request.ProductType))
    //    {
    //        return BadRequest(new
    //        {
    //            message = "Jenis Produk wajib dipilih."
    //        });
    //    }

    //    if (request.UnitId <= 0)
    //    {
    //        return BadRequest(new
    //        {
    //            message = "Unit wajib dipilih."
    //        });
    //    }

    //    var skuExists = await _db.Products
    //        .AnyAsync(x =>
    //            x.Sku == request.Sku &&
    //            x.DeletedAt == null);

    //    if (skuExists)
    //    {
    //        return Conflict(new
    //        {
    //            message = "SKU sudah digunakan."
    //        });
    //    }

    //    var productNumber = await GenerateProductNumber();

    //    var product = new Product
    //    {
    //        Uid = Guid.NewGuid().ToString(),

    //        // Produk utama, belum memiliki parent
    //        ParentId = request.ParentId,
    //        ProductNumber = productNumber,
    //        Sku = request.Sku.Trim(),
    //        Name = request.Name.Trim(),
    //        Barcode = request.Barcode?.Trim(),
    //        InternalCode = request.InternalCode?.Trim(),
    //        ExternalCode1 = request.ExternalCode1?.Trim(),
    //        ExternalCode2 = request.ExternalCode2?.Trim(),
    //        ExternalCode3 = request.ExternalCode3?.Trim(),
    //        BrandId = request.BrandId,
    //        CategoryId = request.CategoryId,
    //        SubCategoryId = request.SubCategoryId,
    //        ProductType = request.ProductType.Trim(),
    //        UnitId = request.UnitId,
    //        HasVariants = request.HasVariants ?? false,
    //        DimMode = request.DimMode,
    //        DefLength = request.DefLength,
    //        DefWidth = request.DefWidth,
    //        DefHeight = request.DefHeight,
    //        PriceMode = request.PriceMode,
    //        CostPerM3 = request.CostPerM3,
    //        SellPerM3 = request.SellPerM3,
    //        CostPrice = request.CostPrice ?? 0,
    //        SellPrice = request.SellPrice ?? 0,
    //        MinStock = request.MinStock ?? 0,
    //        Description = request.Description?.Trim(),
    //        TrackStock = request.TrackStock ?? false,
    //        TrackSerial = request.TrackSerial ?? false,
    //        WarrantyMonths = request.WarrantyMonths ?? 0,
    //        IsActive = request.IsActive ?? true,
    //        Image = request.Image?.Trim(),
    //        CreatedAt = DateTime.Now
    //    };

    //    _db.Products.Add(product);
    //    if (request.ParentId.HasValue)
    //    {
    //        var parentProduct = await _db.Products.FirstOrDefaultAsync(x =>
    //            x.Id == request.ParentId.Value &&
    //            x.DeletedAt == null);

    //        if (parentProduct != null)
    //        {
    //            parentProduct.HasVariants = true; 
    //        }
    //    }

    //    await _db.SaveChangesAsync();

    //    return Ok(new
    //    {
    //        success = true,
    //        message = "Produk berhasil ditambahkan.",
    //        id = product.Id,
    //        productNumber = product.ProductNumber
    //    });
    //}

    [HttpPost("create")]
    public async Task<IActionResult> Create(
    [FromBody] CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            return BadRequest(new
            {
                message = "SKU wajib diisi."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "Nama Produk wajib diisi."
            });
        }

        if (string.IsNullOrWhiteSpace(request.ProductType))
        {
            return BadRequest(new
            {
                message = "Jenis Produk wajib dipilih."
            });
        }

        if (request.UnitId <= 0)
        {
            return BadRequest(new
            {
                message = "Unit wajib dipilih."
            });
        }

        var sku = request.Sku.Trim(); 
        var existingProduct = await _db.Products
            .FirstOrDefaultAsync(x => x.Sku == sku);
         
        //  SKU MASIH AKTIF 
        if (existingProduct != null &&
            existingProduct.DeletedAt == null)
        {
            return Conflict(new
            {
                message = "SKU sudah digunakan."
            });
        }
         
        // RESTORE PRODUCT LAMA 
        if (existingProduct != null &&
            existingProduct.DeletedAt != null)
        {
            existingProduct.Sku = sku;
            existingProduct.Name = request.Name.Trim();
            existingProduct.Barcode = request.Barcode?.Trim();
            existingProduct.InternalCode = request.InternalCode?.Trim();
            existingProduct.ExternalCode1 = request.ExternalCode1?.Trim();
            existingProduct.ExternalCode2 = request.ExternalCode2?.Trim();
            existingProduct.ExternalCode3 = request.ExternalCode3?.Trim();

            existingProduct.BrandId = request.BrandId;
            existingProduct.CategoryId = request.CategoryId;
            existingProduct.SubCategoryId = request.SubCategoryId;
            existingProduct.ProductType = request.ProductType.Trim();

            existingProduct.ParentId = request.ParentId;
            existingProduct.HasVariants = request.HasVariants ?? false;

            existingProduct.UnitId = request.UnitId;

            existingProduct.DimMode = request.DimMode;
            existingProduct.DefLength = request.DefLength;
            existingProduct.DefWidth = request.DefWidth;
            existingProduct.DefHeight = request.DefHeight;

            existingProduct.PriceMode = request.PriceMode;
            existingProduct.CostPerM3 = request.CostPerM3;
            existingProduct.SellPerM3 = request.SellPerM3;

            existingProduct.CostPrice = request.CostPrice ?? 0;
            existingProduct.SellPrice = request.SellPrice ?? 0;
            existingProduct.MinStock = request.MinStock ?? 0;

            existingProduct.Description = request.Description?.Trim();

            existingProduct.TrackStock = request.TrackStock ?? false;
            existingProduct.TrackSerial = request.TrackSerial ?? false;
            existingProduct.WarrantyMonths = request.WarrantyMonths ?? 0;

            existingProduct.IsActive = request.IsActive ?? true;
            existingProduct.Image = request.Image?.Trim();

            // RESTORE
            existingProduct.DeletedAt = null;
            existingProduct.UpdatedAt = DateTime.Now;

            // Jika product ini adalah VARIAN,
            // pastikan parent memiliki HasVariants = true
            if (request.ParentId.HasValue)
            {
                var parentProduct = await _db.Products
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.ParentId.Value &&
                        x.DeletedAt == null);

                if (parentProduct != null)
                {
                    parentProduct.HasVariants = true;
                    parentProduct.UpdatedAt = DateTime.Now;
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Produk sebelumnya ditemukan dan berhasil dipulihkan.",
                id = existingProduct.Id,
                uid = existingProduct.Uid,
                productNumber = existingProduct.ProductNumber
            });
        }
         
        // SKU BELUM PERNAH ADA BUAT PRODUCT BARU 
        var productNumber = await GenerateProductNumber();

        var product = new Product
        {
            Uid = Guid.NewGuid().ToString(),

            ParentId = request.ParentId,

            ProductNumber = productNumber,

            Sku = sku,
            Name = request.Name.Trim(),

            Barcode = request.Barcode?.Trim(),
            InternalCode = request.InternalCode?.Trim(),
            ExternalCode1 = request.ExternalCode1?.Trim(),
            ExternalCode2 = request.ExternalCode2?.Trim(),
            ExternalCode3 = request.ExternalCode3?.Trim(),

            BrandId = request.BrandId,
            CategoryId = request.CategoryId,
            SubCategoryId = request.SubCategoryId,

            ProductType = request.ProductType.Trim(),

            UnitId = request.UnitId,

            HasVariants = request.HasVariants ?? false,

            DimMode = request.DimMode,

            DefLength = request.DefLength,
            DefWidth = request.DefWidth,
            DefHeight = request.DefHeight,

            PriceMode = request.PriceMode,

            CostPerM3 = request.CostPerM3,
            SellPerM3 = request.SellPerM3,

            CostPrice = request.CostPrice ?? 0,
            SellPrice = request.SellPrice ?? 0,
            MinStock = request.MinStock ?? 0,

            Description = request.Description?.Trim(),

            TrackStock = request.TrackStock ?? false,
            TrackSerial = request.TrackSerial ?? false,

            WarrantyMonths = request.WarrantyMonths ?? 0,

            IsActive = request.IsActive ?? true,

            Image = request.Image?.Trim(),

            CreatedAt = DateTime.Now
        };

        _db.Products.Add(product);
         
        // JIKA PRODUCT ADALAH VARIAN
        // -> PARENT OTOMATIS HasVariants = true  
        if (request.ParentId.HasValue)
        {
            var parentProduct = await _db.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ParentId.Value &&
                    x.DeletedAt == null);

            if (parentProduct != null)
            {
                parentProduct.HasVariants = true;
                parentProduct.UpdatedAt = DateTime.Now;
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Produk berhasil ditambahkan.",
            id = product.Id,
            uid = product.Uid,
            productNumber = product.ProductNumber
        });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateProductRequest request)
    {
        var product =
            await _db.Products.FirstOrDefaultAsync(x =>
                x.Id == request.Id 
                && x.DeletedAt == null);

        if (product is null)
        {
            return NotFound(new
            {
                message = "Produk tidak ditemukan."
            });
        }

        if (await _db.Products.AnyAsync(x =>
            x.Id != request.Id &&
            x.Sku == request.Sku &&
            x.DeletedAt == null))
        {
            return Conflict(new
            {
                message = "SKU sudah digunakan."
            });
        }

        product.Sku = request.Sku.Trim();
        product.Name = request.Name?.Trim();
        product.InternalCode = request.InternalCode?.Trim();
        product.ExternalCode1 = request.ExternalCode1?.Trim();
        product.ExternalCode2 = request.ExternalCode2?.Trim();
        product.ExternalCode3 = request.ExternalCode3?.Trim();
        product.Name = request.Name?.Trim();
        product.BrandId = request.BrandId;
        product.CategoryId = request.CategoryId;
        product.SubCategoryId = request.SubCategoryId;
        product.ProductType = request.ProductType?.Trim();
        //product.HasVariants = request.HasVariants ?? false;
        var hasVariants = await _db.Products.AnyAsync(x =>
            x.ParentId == product.Id &&
            x.DeletedAt == null);

        product.HasVariants = hasVariants || (request.HasVariants ?? false);
        product.UnitId = request.UnitId;
        product.DimMode = request.DimMode;
        product.DefLength = request.DefLength;
        product.DefWidth = request.DefWidth;
        product.DefHeight = request.DefHeight;
        product.PriceMode = request.PriceMode?.Trim();
        product.CostPerM3 = request.CostPerM3;
        product.SellPerM3 = request.SellPerM3;
        product.Description = request.Description?.Trim();
        product.CostPrice = request.CostPrice;
        product.SellPrice = request.SellPrice;
        product.MinStock = request.MinStock;
        product.TrackStock = request.TrackStock;
        product.TrackSerial = request.TrackSerial ?? false;
        product.WarrantyMonths = request.WarrantyMonths;
        product.IsActive = request.IsActive;
        product.Image = request.Image?.Trim();
        product.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Produk berhasil diubah."
        });
    }

    [HttpPost("{id:long}/sync-prices-to-variants")]
    public async Task<IActionResult> SyncPricesToVariants(long id)
    {
        var parent = await _db.Products
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.DeletedAt == null);

        if (parent == null)
        {
            return NotFound(new
            {
                message = "Produk induk tidak ditemukan."
            });
        }

        if (parent.HasVariants != true)
        {
            return BadRequest(new
            {
                message = "Produk ini tidak memiliki varian."
            });
        }

        var variants = await _db.Products
            .Where(x =>
                x.ParentId == parent.Id &&
                x.DeletedAt == null)
            .ToListAsync();

        if (variants.Count == 0)
        {
            return BadRequest(new
            {
                message = "Produk induk belum memiliki varian aktif."
            });
        }

        var priceMode = parent.PriceMode?
            .Trim()
            .ToLowerInvariant();

        var isVolumePrice =
            priceMode == "volume" ||
            priceMode == "kubikasi";

        foreach (var variant in variants)
        {
            if (isVolumePrice)
            {
                // Harga berdasarkan kubikasi.
                variant.CostPerM3 = parent.CostPerM3;
                variant.SellPerM3 = parent.SellPerM3;
            }
            else
            {
                // Harga tetap.
                variant.CostPrice = parent.CostPrice;
                variant.SellPrice = parent.SellPrice;
            }

            variant.UpdatedAt = DateTime.Now;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = isVolumePrice
                ? "Tarif beli dan jual per m³ berhasil disamakan ke seluruh varian."
                : "Harga beli dan jual berhasil disamakan ke seluruh varian.",
            parentId = parent.Id,
            priceMode = parent.PriceMode,
            updatedCount = variants.Count
        });
    }

    //[HttpPost("delete")]
    //public async Task<IActionResult> Delete(
    //   [FromBody] DeleteProductRequest request)
    //{
    //    var product =
    //        await _db.Products.FirstOrDefaultAsync(x =>
    //            x.Id == request.Id &&
    //            x.DeletedAt == null);

    //    if (product is null)
    //    {
    //        return NotFound(new
    //        {
    //            message = "Produk tidak ditemukan."
    //        });
    //    }

    //    product.DeletedAt = DateTime.Now;
    //    product.UpdatedAt = DateTime.Now;

    //    await _db.SaveChangesAsync();

    //    return Ok(new
    //    {
    //        message = "Produk berhasil dihapus."
    //    });
    //}

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
    [FromBody] DeleteProductUnitRequest request)
    {
        var unit = await _db.Units
            .FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (unit is null)
        {
            return NotFound(new
            {
                message = "Satuan tidak ditemukan."
            });
        }
         
        var hasProducts = await _db.Products
            .AnyAsync(x => x.UnitId == request.Id);
         
        unit.DeletedAt = DateTime.Now;
        unit.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = hasProducts
                ? "Satuan berhasil dinonaktifkan. Data terkait produk tetap tersimpan."
                : "Satuan berhasil dihapus."
        });
    }

    [HttpGet("delete-check/{id:long}")]
    public async Task<IActionResult> DeleteCheck(long id)
    {
        var unit = await _db.Units
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.DeletedAt == null);

        if (unit is null)
        {
            return NotFound(new
            {
                message = "Satuan tidak ditemukan."
            });
        }

        var relations = new List<string>(); 
        var hasProducts = await _db.Products
            .AnyAsync(x => x.UnitId == id);

        if (hasProducts)
        {
            relations.Add("Produk");
        }

        return Ok(new
        {
            hasRelation = relations.Count > 0,
            message = relations.Count > 0
                ? "Satuan masih digunakan oleh data lain."
                : "",
            relations
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
        var data = await _db.Products
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Uid = x.Uid,
                ProductNumber = x.ProductNumber,
                Sku = x.Sku,
                Barcode = x.Barcode,
                Name = x.Name,
                CategoryId = x.CategoryId,
                UnitId = x.UnitId,
                CostPrice = x.CostPrice,
                SellPrice = x.SellPrice,
                MinStock = x.MinStock,
                TrackStock = x.TrackStock,
                Image = x.Image,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return Ok(data);
    }

    //kelola-variant
    [HttpGet("variants/{parentId:long}")]
    public async Task<IActionResult> GetVariants(long parentId)
    {
        var variants = await _db.Products
            .AsNoTracking()
            .Where(x =>
                x.ParentId == parentId &&
                x.DeletedAt == null)
            .OrderBy(x => x.Id)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Uid = x.Uid,
                ProductNumber = x.ProductNumber,
                Sku = x.Sku,
                Barcode = x.Barcode,
                InternalCode = x.InternalCode,
                ExternalCode1 = x.ExternalCode1,
                ExternalCode2 = x.ExternalCode2,
                ExternalCode3 = x.ExternalCode3,

                Name = x.Name,

                CategoryId = x.CategoryId,
                SubCategoryId = x.SubCategoryId,
                BrandId = x.BrandId,

                CategoryName = _db.ProductCategories
                    .Where(c => c.Id == x.CategoryId &&
                                c.DeletedAt == null)
                    .Select(c => c.Name)
                    .FirstOrDefault(),

                BrandName = _db.Brands
                    .Where(b => b.Id == x.BrandId &&
                                b.DeletedAt == null)
                    .Select(b => b.Name)
                    .FirstOrDefault(),

                ProductType = x.ProductType,
                ParentId = x.ParentId,
                HasVariants = x.HasVariants,

                UnitId = x.UnitId,
                UnitName = _db.Units
                    .Where(u => u.Id == x.UnitId &&
                                u.DeletedAt == null)
                    .Select(u => u.Name)
                    .FirstOrDefault(),

                DimMode = x.DimMode,
                DefLength = x.DefLength,
                DefWidth = x.DefWidth,
                DefHeight = x.DefHeight,

                PriceMode = x.PriceMode,
                CostPerM3 = x.CostPerM3,
                SellPerM3 = x.SellPerM3,

                Description = x.Description,
                CostPrice = x.CostPrice,
                SellPrice = x.SellPrice,
                MinStock = x.MinStock,

                TrackStock = x.TrackStock,
                TrackSerial = x.TrackSerial,
                WarrantyMonths = x.WarrantyMonths,

                Image = x.Image,
                IsActive = x.IsActive,

                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(variants);
    }

    //qr-code
    [HttpGet("uid/{uid}")]
    public async Task<IActionResult> GetByUid(string uid)
    {
        var product = await _db.Products
            .Where(x =>
                x.Uid == uid &&
                x.DeletedAt == null)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Uid = x.Uid,
                ProductNumber = x.ProductNumber,
                Sku = x.Sku,
                Name = x.Name,

                BrandId = x.BrandId,
                CategoryId = x.CategoryId,
                SubCategoryId = x.SubCategoryId,

                ProductType = x.ProductType,

                ParentId = x.ParentId,
                HasVariants = x.HasVariants,

                UnitId = x.UnitId,

                DimMode = x.DimMode,
                DefLength = x.DefLength,
                DefWidth = x.DefWidth,
                DefHeight = x.DefHeight,

                PriceMode = x.PriceMode,

                CostPerM3 = x.CostPerM3,
                SellPerM3 = x.SellPerM3,

                CostPrice = x.CostPrice,
                SellPrice = x.SellPrice,
                MinStock = x.MinStock,

                TrackStock = x.TrackStock,
                TrackSerial = x.TrackSerial,

                WarrantyMonths = x.WarrantyMonths,

                IsActive = x.IsActive,

                Image = x.Image
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}
    

