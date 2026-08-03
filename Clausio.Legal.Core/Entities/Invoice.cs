namespace Clausio.Legal.Core.Entities;

public class Invoice : BaseEntity
{
    public string   InvoiceNumber { get; set; } = string.Empty;
    public Guid     CaseId        { get; set; }
    public Case?    Case          { get; set; }
    public Guid     ClientId      { get; set; }
    public Client?  Client        { get; set; }
    public Guid     CreatedByUserId { get; set; }

    public string   ClientName    { get; set; } = string.Empty;
    public string   CaseName      { get; set; } = string.Empty;
    public string   Description   { get; set; } = string.Empty;
    public decimal  FeeAgreed     { get; set; }
    public decimal  AmountDue     { get; set; }
    public decimal  TaxAmount     { get; set; }
    public decimal  TotalAmount   { get; set; }
    public string   Status        { get; set; } = "Unpaid"; // Unpaid | Paid | Partial | Cancelled
    public DateTime IssuedDate    { get; set; } = DateTime.UtcNow;
    public DateTime DueDate       { get; set; }
    public string?  Notes         { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
