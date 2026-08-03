using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clausio.Legal.API.Controllers;

[Authorize]
[ApiController]
[Route("api/billing")]
public class BillingController(IBillingService billingService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Stats ─────────────────────────────────────────────────────

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await billingService.GetStatsAsync(UserId, ct);
        return Ok(stats);
    }

    // ── Invoices ──────────────────────────────────────────────────

    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices(CancellationToken ct)
    {
        var invoices = await billingService.GetInvoicesAsync(UserId, ct);
        return Ok(invoices);
    }

    [HttpPost("invoices")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto, CancellationToken ct)
    {
        var invoice = await billingService.CreateInvoiceAsync(UserId, dto, ct);
        return Ok(invoice);
    }

    [HttpGet("invoices/{id:guid}")]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken ct)
    {
        var invoice = await billingService.GetInvoiceAsync(UserId, id, ct);
        return Ok(invoice);
    }

    [HttpPut("invoices/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateInvoiceStatusDto dto, CancellationToken ct)
    {
        var invoice = await billingService.UpdateInvoiceStatusAsync(UserId, id, dto.Status, ct);
        return Ok(invoice);
    }

    [HttpDelete("invoices/{id:guid}")]
    public async Task<IActionResult> DeleteInvoice(Guid id, CancellationToken ct)
    {
        await billingService.DeleteInvoiceAsync(UserId, id, ct);
        return Ok();
    }

    // ── Payments ──────────────────────────────────────────────────

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] Guid? caseId, CancellationToken ct)
    {
        var payments = await billingService.GetPaymentsAsync(UserId, caseId, ct);
        return Ok(payments);
    }

    [HttpPost("payments")]
    public async Task<IActionResult> RecordPayment([FromBody] CreatePaymentDto dto, CancellationToken ct)
    {
        var payment = await billingService.RecordPaymentAsync(UserId, dto, ct);
        return Ok(payment);
    }

    [HttpDelete("payments/{id:guid}")]
    public async Task<IActionResult> DeletePayment(Guid id, CancellationToken ct)
    {
        await billingService.DeletePaymentAsync(UserId, id, ct);
        return Ok();
    }

    // ── Expenses ──────────────────────────────────────────────────

    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpenses([FromQuery] Guid? caseId, CancellationToken ct)
    {
        var expenses = await billingService.GetExpensesAsync(UserId, caseId, ct);
        return Ok(expenses);
    }

    [HttpPost("expenses")]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto dto, CancellationToken ct)
    {
        var expense = await billingService.CreateExpenseAsync(UserId, dto, ct);
        return Ok(expense);
    }

    [HttpDelete("expenses/{id:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid id, CancellationToken ct)
    {
        await billingService.DeleteExpenseAsync(UserId, id, ct);
        return Ok();
    }
}
