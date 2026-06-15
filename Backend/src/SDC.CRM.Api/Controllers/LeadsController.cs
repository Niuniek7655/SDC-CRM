using Microsoft.AspNetCore.Mvc;
using SDC.CRM.Api.Contracts.Leads;
using SDC.CRM.Application.Leads;
using SDC.CRM.Application.Leads.GetMyLeads;
using SDC.CRM.Application.Leads.RegisterLead;

namespace SDC.CRM.Api.Controllers;

[ApiController]
[Route("api/leads")]
public sealed class LeadsController(
    RegisterLeadHandler registerLeadHandler,
    GetMyLeadsHandler getMyLeadsHandler) : ControllerBase
{
    /// <summary>Register a new lead with minimal customer and contact data.</summary>
    [HttpPost]
    public async Task<IActionResult> RegisterLead(RegisterLeadRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterLeadCommand(
            request.CompanyName,
            request.ContactName,
            request.ContactEmail,
            request.ContactPhone,
            request.Source,
            request.AssignedSalespersonId);

        var leadId = await registerLeadHandler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetMyLeads),
            new { salespersonId = request.AssignedSalespersonId },
            new { id = leadId });
    }

    /// <summary>Show the leads owned by a salesperson.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<LeadSummaryDto>>> GetMyLeads(
        [FromQuery] Guid salespersonId,
        CancellationToken cancellationToken)
    {
        var leads = await getMyLeadsHandler.HandleAsync(new GetMyLeadsQuery(salespersonId), cancellationToken);
        return Ok(leads);
    }
}
