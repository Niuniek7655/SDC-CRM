using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDC.CRM.Domain.Common;
using SDC.CRM.Domain.Leads;

namespace SDC.CRM.Infrastructure.Persistence.Configurations;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");

        builder.HasKey(lead => lead.Id);

        builder.Property(lead => lead.CompanyName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(lead => lead.ContactName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(lead => lead.ContactEmail)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(lead => lead.ContactPhone)
            .HasMaxLength(50);

        builder.Property(lead => lead.Source)
            .HasMaxLength(100);

        builder.Property(lead => lead.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(lead => lead.RejectionReason)
            .HasMaxLength(500);

        builder.Property(lead => lead.AssignedSalespersonId)
            .IsRequired();

        builder.Property(lead => lead.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(lead => lead.AssignedSalespersonId);

        // Domain events are not persisted.
        builder.Ignore(lead => lead.DomainEvents);
    }
}
