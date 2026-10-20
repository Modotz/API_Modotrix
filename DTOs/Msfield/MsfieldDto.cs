namespace ERP.Api.DTOs;

public class MsfieldDto
{
    public long Id { get; set; }

    public string? Uid { get; set; }

    public string FieldKey { get; set; } = string.Empty;

    public string FieldText { get; set; } = string.Empty;

    public string FiledValue { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int UserId { get; set; }
}