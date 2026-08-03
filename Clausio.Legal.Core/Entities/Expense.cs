namespace Clausio.Legal.Core.Entities;

public class Expense : BaseEntity
{
    public Guid    CaseId          { get; set; }
    public Case?   Case            { get; set; }
    public Guid    CreatedByUserId { get; set; }

    public string  Title           { get; set; } = string.Empty;
    public string  Category        { get; set; } = "Filing Fee"; // Filing Fee | Travel | Printing | Courier | Misc
    public decimal Amount          { get; set; }
    public DateTime Date           { get; set; } = DateTime.UtcNow;
    public string?  Receipt        { get; set; }
    public string?  Notes          { get; set; }
    public bool     Billable       { get; set; } = true;
}
