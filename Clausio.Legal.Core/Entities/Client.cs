namespace Clausio.Legal.Core.Entities;

public class Client : BaseEntity
{
    public string? FirstName     { get; set; }
    public string? LastName      { get; set; }
    public string? Phone         { get; set; }
    public string? AltPhone      { get; set; }
    public string? Email         { get; set; }
    public string? WhatsApp      { get; set; }
    public string? Address       { get; set; }
    public string? ClientType    { get; set; }
    public string? Aadhar        { get; set; }
    public string? Pan           { get; set; }
    public string? Occupation    { get; set; }
    public double? MonthlyIncome { get; set; }
    public string? BankName      { get; set; }
    public bool    IsVip         { get; set; }
    public string? Notes         { get; set; }
    public Guid    CreatedByUserId { get; set; }
    public ICollection<Case> Cases { get; set; } = new List<Case>();
}
