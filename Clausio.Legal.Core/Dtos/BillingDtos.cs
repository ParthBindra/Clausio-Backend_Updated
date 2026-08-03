namespace Clausio.Legal.Core.Dtos;

// ── Invoice DTOs ──────────────────────────────────────────────────

public class CreateInvoiceDto
{
    public Guid     CaseId      { get; set; }
    public Guid     ClientId    { get; set; }
    public string   ClientName  { get; set; } = string.Empty;
    public string   CaseName    { get; set; } = string.Empty;
    public string   Description { get; set; } = string.Empty;
    public decimal  FeeAgreed   { get; set; }
    public decimal  AmountDue   { get; set; }
    public decimal  TaxAmount   { get; set; }
    public DateTime DueDate     { get; set; }
    public string?  Notes       { get; set; }
}

public class UpdateInvoiceStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class InvoiceDto
{
    public Guid     Id            { get; set; }
    public string   InvoiceNumber { get; set; } = string.Empty;
    public Guid     CaseId        { get; set; }
    public Guid     ClientId      { get; set; }
    public string   ClientName    { get; set; } = string.Empty;
    public string   CaseName      { get; set; } = string.Empty;
    public string   Description   { get; set; } = string.Empty;
    public decimal  FeeAgreed     { get; set; }
    public decimal  AmountDue     { get; set; }
    public decimal  TaxAmount     { get; set; }
    public decimal  TotalAmount   { get; set; }
    public decimal  AmountPaid    { get; set; }
    public decimal  AmountPending { get; set; }
    public string   Status        { get; set; } = string.Empty;
    public DateTime IssuedDate    { get; set; }
    public DateTime DueDate       { get; set; }
    public string?  Notes         { get; set; }
}

// ── Payment DTOs ──────────────────────────────────────────────────

public class CreatePaymentDto
{
    public Guid     InvoiceId { get; set; }
    public Guid     CaseId    { get; set; }
    public decimal  Amount    { get; set; }
    public DateTime PaidOn    { get; set; } = DateTime.UtcNow;
    public string   Mode      { get; set; } = "Cash";
    public string?  Reference { get; set; }
    public string?  Notes     { get; set; }
}

public class PaymentDto
{
    public Guid     Id            { get; set; }
    public Guid     InvoiceId     { get; set; }
    public Guid     CaseId        { get; set; }
    public decimal  Amount        { get; set; }
    public DateTime PaidOn        { get; set; }
    public string   Mode          { get; set; } = string.Empty;
    public string?  Reference     { get; set; }
    public string?  Notes         { get; set; }
    public string   InvoiceNumber { get; set; } = string.Empty;
}

// ── Expense DTOs ──────────────────────────────────────────────────

public class CreateExpenseDto
{
    public Guid     CaseId   { get; set; }
    public string   Title    { get; set; } = string.Empty;
    public string   Category { get; set; } = "Filing Fee";
    public decimal  Amount   { get; set; }
    public DateTime Date     { get; set; } = DateTime.UtcNow;
    public string?  Notes    { get; set; }
    public bool     Billable { get; set; } = true;
}

public class ExpenseDto
{
    public Guid     Id       { get; set; }
    public Guid     CaseId   { get; set; }
    public string   Title    { get; set; } = string.Empty;
    public string   Category { get; set; } = string.Empty;
    public decimal  Amount   { get; set; }
    public DateTime Date     { get; set; }
    public string?  Notes    { get; set; }
    public bool     Billable { get; set; }
}

// ── Stats DTO ─────────────────────────────────────────────────────

public class BillingStatsDto
{
    public decimal TotalBilled    { get; set; }
    public decimal TotalPaid      { get; set; }
    public decimal TotalPending   { get; set; }
    public decimal TotalExpenses  { get; set; }
    public int     InvoiceCount   { get; set; }
    public int     PaidCount      { get; set; }
    public int     UnpaidCount    { get; set; }
}
