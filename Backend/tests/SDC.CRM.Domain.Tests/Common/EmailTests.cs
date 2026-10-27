using SDC.CRM.Domain.Common;

namespace SDC.CRM.Domain.Tests.Common;

public sealed class EmailTests
{
    [Test]
    [Arguments("")]
    [Arguments("   ")]
    [Arguments("jan.example.test")]
    [Arguments("@example.test")]
    [Arguments("jan@")]
    public async Task Create__When_email_is_missing_or_has_no_local_or_domain_part__Should_reject(string value)
    {
        await Assert.That(() => Email.Create(value)).Throws<DomainException>();
    }

    [Test]
    public async Task Create__When_whitespace_surrounds_address__Should_preserve_normalized_value_semantics()
    {
        var email = Email.Create("  jan@example.test  ");
        await Assert.That(email.Value).IsEqualTo("jan@example.test");
        await Assert.That(email).IsEqualTo(Email.Create("jan@example.test"));
    }
}
