using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDC.CRM.Api.Authorization;
using SDC.CRM.Api.Contracts.Leads;
using SDC.CRM.Application.Abstractions.Identity;
using SDC.CRM.Application.Leads;
using SDC.CRM.Application.Leads.GetMyLeads;
using SDC.CRM.Application.Leads.RegisterLead;

namespace SDC.CRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/leads")]
public sealed class LeadsController(
    RegisterLeadHandler registerLeadHandler,
    GetMyLeadsHandler getMyLeadsHandler,
    ICurrentUser currentUser,
    ILogger<LeadsController> logger) : ControllerBase
{
    /// <summary>Register a new lead with minimal customer and contact data.</summary>
    [HttpPost]
    [Authorize(Policy = CrmPolicies.RegisterLead)]
    public async Task<IActionResult> RegisterLead(RegisterLeadRequest request, CancellationToken cancellationToken)
    {
        // The owning salesperson is taken from the authenticated identity, never
        // from the client payload, so the UI cannot register leads for others.
        var command = new RegisterLeadCommand(
            request.CompanyName,
            request.ContactName,
            request.ContactEmail,
            request.ContactPhone,
            request.Source,
            currentUser.Id);

        var leadId = await registerLeadHandler.HandleAsync(command, cancellationToken);

        logger.LogInformation("Lead {LeadId} registered by {UserId}", leadId, currentUser.Id);

        // Location points at the caller's lead list; the body carries the new id.
        return CreatedAtAction(nameof(GetMyLeads), routeValues: null, value: new { id = leadId });
    }

    /// <summary>Show the leads owned by the authenticated salesperson.</summary>
    [HttpGet("mine")]
    [Authorize(Policy = CrmPolicies.ViewOwnLeads)]
    public async Task<ActionResult<IReadOnlyList<LeadSummaryDto>>> GetMyLeads(CancellationToken cancellationToken)
    {
        logger.LogDebug("Listing leads for {UserId}", currentUser.Id);

        var leads = await getMyLeadsHandler.HandleAsync(new GetMyLeadsQuery(currentUser.Id), cancellationToken);
        return Ok(leads);
    }
}
