using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.DTOs.Supplier;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[Route("api/msfields")]
[ApiController]
public class MsfieldsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MsfieldsController(AppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // GET: api/Fields
    // ==========================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MsfieldDto>>> GetFields()
    {
        var fields = await _context.Msfields
            .Where(x => x.deleted_at == null)
            .OrderByDescending(x => x.id)
            .Select(x => new MsfieldDto
            {
                Id = x.id,
                Uid = x.uid,
                FieldKey = x.field_key,
                FieldText = x.field_text,
                FiledValue = x.filed_value,
                Description = x.description,
                IsActive = x.is_active,
                CreatedAt = x.created_at,
                UpdatedAt = x.updated_at,
                DeletedAt = x.deleted_at,
                UserId = x.user_id
            })
            .ToListAsync();

        return Ok(fields);
    }


    // ==========================================
    // GET: api/Fields/1
    // ==========================================
    [HttpGet("{id}")]
    public async Task<ActionResult<MsfieldDto>> GetField(long id)
    {
        var field = await _context.Msfields
            .Where(x =>
                x.id == id &&
                x.deleted_at == null)
            .Select(x => new MsfieldDto
            {
                Id = x.id,
                Uid = x.uid,
                FieldKey = x.field_key,
                FieldText = x.field_text,
                FiledValue = x.filed_value,
                Description = x.description,
                IsActive = x.is_active,
                CreatedAt = x.created_at,
                UpdatedAt = x.updated_at,
                DeletedAt = x.deleted_at,
                UserId = x.user_id
            })
            .FirstOrDefaultAsync();

        if (field == null)
        {
            return NotFound(new
            {
                message = "Data field tidak ditemukan."
            });
        }

        return Ok(field);
    }


    // ==========================================
    // POST: api/Fields/Create
    // ==========================================
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] CreateMsfieldRequest dto)
    {
        var field = new Msfield
        {
            uid = Guid.NewGuid().ToString(),
            field_key = dto.FieldKey,
            field_text = dto.FieldText,
            filed_value = dto.FiledValue,
            description = dto.Description,
            is_active = dto.IsActive,
            user_id = dto.UserId,
            created_at = DateTime.Now
        };

        _context.Msfields.Add(field);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Data field berhasil disimpan."
        });
    }


    // ==========================================
    // POST: api/Fields/Update/1
    // ==========================================
    [HttpPost("Update")]
    public async Task<ActionResult<MsfieldDto>> UpdateField(
    UpdateMsfieldRequest dto)
    {
        var existingField = await _context.Msfields
            .FirstOrDefaultAsync(x =>
                x.id == dto.Id &&
                x.deleted_at == null);

        if (existingField == null)
        {
            return NotFound(new
            {
                message = "Data field tidak ditemukan."
            });
        }

        existingField.field_key = dto.FieldKey;
        existingField.field_text = dto.FieldText;
        existingField.filed_value = dto.FiledValue;
        existingField.description = dto.Description;
        existingField.is_active = dto.IsActive;
        existingField.user_id = dto.UserId;
        existingField.updated_at = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Data field berhasil diperbarui.",
            data = existingField
        });
    }


    // ==========================================
    // POST: api/Fields/Delete
    // ==========================================
    [HttpPost("Delete")]
    public async Task<IActionResult> DeleteField(DeleteMsfieldRequest dto)
    {
        var field = await _context.Msfields
            .FirstOrDefaultAsync(x =>
                x.id == dto.Id &&
                x.deleted_at == null);

        if (field == null)
        {
            return NotFound(new
            {
                message = "Data field tidak ditemukan."
            });
        }

        // Soft Delete
        field.deleted_at = DateTime.Now;
        field.updated_at = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Data field berhasil dihapus."
        });
    }


    [HttpGet("export")]
    public async Task<IActionResult> GetAllForExport()
    {
        var data = await _context.Msfields
            .AsNoTracking()
            .Where(x => x.deleted_at == null)
            .OrderByDescending(x => x.created_at)
            .Select(x => new MsfieldDto
            {
                Id = x.id,
                Uid = x.uid,
                FieldKey = x.field_key,
                FieldText = x.field_text,
                FiledValue = x.filed_value,
                Description = x.description,
                IsActive = x.is_active,
                CreatedAt = x.created_at,
                UpdatedAt = x.updated_at
            })
            .ToListAsync();

        return Ok(data);
    }
}