using Microsoft.EntityFrameworkCore;
using Npgsql;
using SDC.CRM.Domain.Common;
using SDC.CRM.Domain.Leads;
using SDC.CRM.Infrastructure.Persistence;

namespace SDC.CRM.Api.IntegrationTests.Persistence;

// Each test creates and drops its OWN database. Never migrates or clears appdb.
[Explicit]
[Category("PostgreSQL")]
public sealed class PostgresLeadRepositoryTests
{
    private static readonly Guid OwnerA = Guid.Parse("7d1c1a52-1b8e-4f2e-9a57-5c3f0e6a9b10");
    private static readonly Guid OwnerB = Guid.Parse("8d1c1a52-1b8e-4f2e-9a57-5c3f0e6a9b11");

    [Test]
    public async Task Migrate__When_applied_twice__Should_leave_no_pending_migrations()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using var db = database.OpenContext();
        await db.Database.MigrateAsync();
        await db.Database.MigrateAsync();
        await Assert.That((await db.Database.GetPendingMigrationsAsync()).Count()).IsEqualTo(0);
        await Assert.That((await db.Database.GetAppliedMigrationsAsync()).Any()).IsTrue();
    }

    [Test]
    public async Task Save__When_context_is_reopened__Should_preserve_normalized_values_and_nulls()
    {
        await using var database = await TestDatabase.CreateAsync();
        var full = Lead.Register(" Żółw SA ", " Jan Kowalski ", Email.Create(" jan@example.test "), " +48 123 ", " Targi ", OwnerA);
        var minimal = Lead.Register("Minimal", "Anna", Email.Create("anna@example.test"), " ", null, OwnerA);
        await using (var db = database.OpenContext())
        {
            await db.Database.MigrateAsync();
            await new LeadRepository(db).AddAsync(full);
            await new LeadRepository(db).AddAsync(minimal);
            // PostgreSQL stores microseconds: use an exactly representable, controlled UTC timestamp.
            db.Entry(full).Property(x => x.CreatedAtUtc).CurrentValue = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc).AddTicks(1234560);
            await new UnitOfWork(db).SaveChangesAsync();
        }

        await using var reopened = database.OpenContext();
        var repository = new LeadRepository(reopened);
        var saved = (await repository.GetByIdAsync(full.Id))!;
        var blank = (await repository.GetByIdAsync(minimal.Id))!;
        await Assert.That(saved.CompanyName).IsEqualTo("Żółw SA");
        await Assert.That(saved.ContactName).IsEqualTo("Jan Kowalski");
        await Assert.That(saved.ContactEmail).IsEqualTo(Email.Create("jan@example.test"));
        await Assert.That(saved.ContactPhone).IsEqualTo("+48 123");
        await Assert.That(saved.Source).IsEqualTo("Targi");
        await Assert.That(saved.AssignedSalespersonId).IsEqualTo(OwnerA);
        await Assert.That(saved.Status).IsEqualTo(LeadStatus.New);
        await Assert.That(saved.CreatedAtUtc).IsEqualTo(full.CreatedAtUtc);
        await Assert.That(saved.CreatedAtUtc.Kind).IsEqualTo(DateTimeKind.Utc);
        await Assert.That(blank.ContactPhone).IsNull();
        await Assert.That(blank.Source).IsNull();
    }

    [Test]
    public async Task ListBySalesperson__When_both_owners_have_leads__Should_isolate_both_directions_and_sort_all_rows()
    {
        await using var database = await TestDatabase.CreateAsync();
        var oldest = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var a1 = Lead.Register("A1", "Jan", Email.Create("a1@example.test"), null, null, OwnerA);
        var a2 = Lead.Register("A2", "Jan", Email.Create("a2@example.test"), null, null, OwnerA);
        var b1 = Lead.Register("B1", "Anna", Email.Create("b1@example.test"), null, null, OwnerB);
        var b2 = Lead.Register("B2", "Anna", Email.Create("b2@example.test"), null, null, OwnerB);
        await using (var db = database.OpenContext())
        {
            await db.Database.MigrateAsync();
            var rows = new[] { a1, b1, a2, b2 };
            for (var i = 0; i < rows.Length; i++)
            {
                await db.Leads.AddAsync(rows[i]);
                db.Entry(rows[i]).Property(x => x.CreatedAtUtc).CurrentValue = oldest.AddMinutes(i);
            }
            await db.SaveChangesAsync();
        }

        await using var reopened = database.OpenContext();
        var repository = new LeadRepository(reopened);
        var a = await repository.ListBySalespersonAsync(OwnerA);
        var b = await repository.ListBySalespersonAsync(OwnerB);
        await Assert.That(a.Select(x => x.Id).SequenceEqual(new[] { a2.Id, a1.Id })).IsTrue();
        await Assert.That(b.Select(x => x.Id).SequenceEqual(new[] { b2.Id, b1.Id })).IsTrue();
    }

    private sealed class TestDatabase(string adminConnection, string databaseName, string connection) : IAsyncDisposable
    {
        public CrmDbContext OpenContext() => new(new DbContextOptionsBuilder<CrmDbContext>().UseNpgsql(connection).Options);

        public static async Task<TestDatabase> CreateAsync()
        {
            var supplied = Environment.GetEnvironmentVariable("SDC_CRM_TEST_POSTGRES")
                ?? throw new InvalidOperationException("Set SDC_CRM_TEST_POSTGRES to a disposable PostgreSQL server, Database=postgres (never appdb).");
            var admin = new NpgsqlConnectionStringBuilder(supplied) { Pooling = false };
            if (admin.Database != "postgres")
                throw new InvalidOperationException("The test bootstrap connection must use Database=postgres; each test creates a separate database.");
            var name = "sdc_crm_tests_" + Guid.NewGuid().ToString("N");
            await using var connection = new NpgsqlConnection(admin.ConnectionString);
            await connection.OpenAsync();
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await create.ExecuteNonQueryAsync();
            var isolated = new NpgsqlConnectionStringBuilder(admin.ConnectionString) { Database = name };
            return new(admin.ConnectionString, name, isolated.ConnectionString);
        }

        public async ValueTask DisposeAsync()
        {
            await using var connection = new NpgsqlConnection(adminConnection);
            await connection.OpenAsync();
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{databaseName}\" WITH (FORCE)", connection);
            await drop.ExecuteNonQueryAsync();
        }
    }
}
