using SDC.CRM.Application.Abstractions;
using SDC.CRM.Application.Abstractions.Persistence;
using SDC.CRM.Domain.Common;
using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Application.Leads.RegisterLead;

public sealed class RegisterLeadHandler(ILeadRepository leads, IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(RegisterLeadCommand command, CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.ContactEmail);

        var lead = Lead.Register(
            command.CompanyName,
            command.ContactName,
            email,
            command.ContactPhone,
            command.Source,
            command.AssignedSalespersonId);

        await leads.AddAsync(lead, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.Id;
    }
}
