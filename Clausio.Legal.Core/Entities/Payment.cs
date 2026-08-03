namespace Clausio.Legal.Core.Entities;

public class Payment : BaseEntity
{
    public Guid      InvoiceId     { get; set; }
    public Invoice?  Invoice       { get; set; }
    public Guid      CaseId        { get; set; }
    public Guid      CreatedByUserId { get; set; }

    public decimal   Amount        { get; set; }
    public DateTime  PaidOn        { get; set; } = DateTime.UtcNow;
    public string    Mode          { get; set; } = "Cash"; // Cash | UPI | Bank Transfer | Cheque
    public string?   Reference     { get; set; }
    public string?   Notes         { get; set; }
}
