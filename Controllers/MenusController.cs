using ERP.Api.Data;
using ERP.Api.DTOs.Menu;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenusController : ControllerBase
{
    private readonly AppDbContext _db;

    public MenusController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] long? parentId = null,
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

        var query = _db.Menus
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Title.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        var totalData =
            await query.CountAsync();

        var totalPage =
            (int)Math.Ceiling(
                totalData / (double)pageSize
            );

        var data = await query
            .OrderBy(x => x.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MenuDto
            {
                Id = x.Id,
                Uid = x.Uid,
                ParentId = x.ParentId,
                Title = x.Title,
                UrlSlug = x.UrlSlug,
                Icon = x.Icon,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            data,
            page,
            pageSize,
            totalData,
            totalPage
        });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<MenuDto>> GetById(long id)
    {
        var menu = await _db.Menus
            .AsNoTracking()
            //.Where(x =>
            //    x.Id == id &&
            //    x.DeletedAt == null)
            .Select(x => new MenuDto
            {
                Id = x.Id,
                Uid = x.Uid,
                ParentId = x.ParentId,
                Title = x.Title,
                UrlSlug = x.UrlSlug,
                Icon = x.Icon,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return menu is null
            ? NotFound()
            : Ok(menu);
    }

    //[HttpPost("create")]
    //public async Task<IActionResult> Create(
    //    [FromBody] CreateMenuRequest request)
    //{
    //    if (await _db.Menus.AnyAsync(x =>
    //        x.Title == request.Title))
    //    //x.DeletedAt == null))
    //    {
    //        return Conflict(new
    //        {
    //            message = "Judul Menu sudah digunakan."
    //        });
    //    }

    //    var menu = new Menu
    //    {
    //        Uid = Guid.NewGuid().ToString(),
    //        ParentId = request.ParentId,
    //        Title = request.Title.Trim(),
    //        UrlSlug = request.UrlSlug.Trim(),
    //        Icon = request.Icon.Trim(),
    //        SortOrder = request.SortOrder,
    //        IsActive = request.IsActive,
    //        CreatedAt = DateTime.Now
    //    };

    //    _db.Menus.Add(menu);

    //    await _db.SaveChangesAsync();

    //    return Ok(new
    //    {
    //        message = "Menu berhasil ditambahkan.",
    //        id = menu.Id,
    //        uid = menu.Uid
    //    });
    //}

    [HttpPost("create")]
    public async Task<IActionResult> Create(
    [FromBody] CreateMenuRequest request)
    {
        var title = request.Title.Trim();

        // Cari data berdasarkan nama, termasuk yang sudah soft delete
        var existingMenu = await _db.Menus
            .FirstOrDefaultAsync(x =>
                x.Title == title);
         
        // DATA SUDAH ADA DAN MASIH AKTIF 
        if (existingMenu != null &&
            existingMenu.DeletedAt == null)
        {
            return Conflict(new
            {
                message = "Judul Menu sudah digunakan."
            });
        }
         
        // DATA PERNAH ADA TAPI SUDAH SOFT DELETE 
        if (existingMenu != null &&
            existingMenu.DeletedAt != null)
        {
            existingMenu.ParentId = request.ParentId;
            existingMenu.Title = title;
            existingMenu.UrlSlug = request.UrlSlug.Trim();
            existingMenu.Icon = request.Icon.Trim();
            existingMenu.SortOrder = request.SortOrder;
            existingMenu.IsActive = request.IsActive;

            // Restore data
            existingMenu.DeletedAt = null;
            existingMenu.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Menu sebelumnya ditemukan dan berhasil dipulihkan.",
                id = existingMenu.Id,
                uid = existingMenu.Uid
            });
        }
         
        // DATA BELUM PERNAH ADA 
        var menu = new Menu
        {
            Uid = Guid.NewGuid().ToString(),
            ParentId = request.ParentId,
            Title = title,
            UrlSlug = request.UrlSlug.Trim(),
            Icon = request.Icon.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        _db.Menus.Add(menu); 
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Menu berhasil ditambahkan.",
            id = menu.Id,
            uid = menu.Uid
        });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateMenuRequest request)
    {
        var menu =
            await _db.Menus.FirstOrDefaultAsync(x =>
                x.Id == request.Id);
        //&& x.DeletedAt == null);

        if (menu is null)
        {
            return NotFound(new
            {
                message = "Menu tidak ditemukan."
            });
        }

        //if (await _db.Menus.AnyAsync(x =>     
        //    x.Id != request.Id &&
        //    x.Title == request.Title))
        ////&& x.DeletedAt == null))
        //{
        //    return Conflict(new
        //    {
        //        message = "Judul Menu sudah digunakan."
        //    });
        //}

        menu.ParentId = request.ParentId;
        menu.Title = request.Title.Trim();
        menu.UrlSlug = request.UrlSlug.Trim();
        menu.Icon = request.Icon.Trim();
        menu.SortOrder = request.SortOrder;
        menu.IsActive = request.IsActive;
        menu.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Menu berhasil diubah."
        });
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder(
    [FromBody] List<OrderMenuRequest> request)
    {
        if (request == null || request.Count == 0)
        {
            return BadRequest("Data urutan menu kosong.");
        }

        var menuIds = request
            .Select(x => x.Id)
            .ToList();

        var menus = await _db.Menus
            .Where(x => menuIds.Contains(x.Id))
            .ToListAsync();

        if (menus.Count != menuIds.Distinct().Count())
        {
            return BadRequest("Terdapat menu yang tidak ditemukan.");
        }

        foreach (var item in request)
        {
            var menu = menus.First(x => x.Id == item.Id);
            menu.ParentId = item.ParentId;
            menu.SortOrder = item.SortOrder;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Urutan menu berhasil disimpan."
        });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] DeleteMenuRequest request)
    {
        var menu = await _db.Menus
            .FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.DeletedAt == null);

        if (menu == null)
        {
            
            return NotFound("Menu tidak ditemukan.");
        }

        var children = await _db.Menus
        .Where(x =>
            x.ParentId == menu.Id &&
            x.DeletedAt == null)
        .OrderBy(x => x.SortOrder)
        .ToListAsync();

        if (children.Count > 0)
        {
            var newParent = children.First();

            newParent.ParentId = null;
            newParent.SortOrder = menu.SortOrder;
            
            for (int i = 1; i < children.Count; i++)
            {
                children[i].ParentId = newParent.Id;
                children[i].SortOrder = i;
            }
        }

        // Soft delete menu induk
        menu.DeletedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Menu berhasil dihapus dan struktur anak menu diperbarui."
        });

        //hard-delete
        //_db.Menus.Remove(menu);
        //await _db.SaveChangesAsync();
        //return Ok(new
        //{
        //    message = "Menu berhasil dihapus."
        //});
    }
}
    

