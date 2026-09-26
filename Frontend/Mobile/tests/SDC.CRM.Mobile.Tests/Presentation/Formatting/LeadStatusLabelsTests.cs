using SDC.CRM.Mobile.Presentation.Formatting;

namespace SDC.CRM.Mobile.Tests.Presentation.Formatting;

public sealed class LeadStatusLabelsTests
{
    [Test]
    [Arguments("New", "Nowy")]
    [Arguments("Qualified", "Zakwalifikowany")]
    [Arguments("Rejected", "Odrzucony")]
    public async Task For__When_status_is_known__Should_return_polish_label_from_glossary(string status, string expectedLabel)
    {
        var label = LeadStatusLabels.For(status);

        await Assert.That(label).IsEqualTo(expectedLabel);
    }

    [Test]
    public async Task For__When_status_is_unknown_to_the_app__Should_return_the_status_code()
    {
        var label = LeadStatusLabels.For("InContact");

        await Assert.That(label).IsEqualTo("InContact");
    }

    [Test]
    public async Task For__When_status_is_missing__Should_return_empty_text()
    {
        var label = LeadStatusLabels.For(null);

        await Assert.That(label).IsEqualTo(string.Empty);
    }
}

