using Microsoft.EntityFrameworkCore;
using OmniCRM.Models;

namespace OmniCRM.Data;

/// <summary>
/// Handles database creation, migrations, and initial seed data loading.
/// </summary>
public static class DbInitializer
{
    public static async Task<bool> InitializeAsync(OmniCrmDbContext context)
    {
        try
        {
            // Verify connection or create database if not existing
            bool created = await context.Database.EnsureCreatedAsync();

            // Seed initial data if customers table is empty
            if (!await context.Customers.AnyAsync())
            {
                await SeedSampleDataAsync(context);
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
            return false;
        }
    }

    private static async Task SeedSampleDataAsync(OmniCrmDbContext context)
    {
        var customer1 = new Customer
        {
            Name = "John Doe",
            Email = "john.doe@apexindustries.com",
            Phone = "+1 (555) 234-5678",
            CompanyName = "Apex Industries Ltd",
            Address = "100 Industrial Parkway, Chicago, IL",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var customer2 = new Customer
        {
            Name = "Sophia Martinez",
            Email = "smartinez@nexustech.io",
            Phone = "+1 (555) 876-5432",
            CompanyName = "Nexus Tech Solutions",
            Address = "450 Silicon Blvd, Austin, TX",
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var customer3 = new Customer
        {
            Name = "Vikram Halnure",
            Email = "vikram@halnuredistributors.com",
            Phone = "+91 98230 12345",
            CompanyName = "Halnure Global Distributors",
            Address = "Plot 42, Sector 18, Pune, MH",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        await context.Customers.AddRangeAsync(customer1, customer2, customer3);
        await context.SaveChangesAsync();

        // Seed Sample Leads
        var leads = new List<Lead>
        {
            new Lead
            {
                Title = "Enterprise ERP Software License",
                CustomerId = customer1.Id,
                Status = LeadStatus.Qualified,
                EstimatedValue = 45000.00m,
                Notes = "Client requested detailed quote for 50 workstation seats.",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Lead
            {
                Title = "Cloud Infrastructure Migration",
                CustomerId = customer2.Id,
                Status = LeadStatus.Contacted,
                EstimatedValue = 28000.00m,
                Notes = "Introductory technical briefing scheduled for next Monday.",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new Lead
            {
                Title = "Annual Maintenance & Support Contract",
                CustomerId = customer3.Id,
                Status = LeadStatus.Converted,
                EstimatedValue = 15000.00m,
                Notes = "Contract signed and purchase order issued.",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Lead
            {
                Title = "Custom Mobile Sales Companion App",
                CustomerId = customer1.Id,
                Status = LeadStatus.New,
                EstimatedValue = 32000.00m,
                Notes = "Inbound inquiry via website contact form.",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        await context.Leads.AddRangeAsync(leads);

        // Seed Sample Follow-ups
        var followUps = new List<FollowUp>
        {
            new FollowUp
            {
                CustomerId = customer1.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(2),
                Notes = "Deliver finalized SLA proposal document and discuss deployment timeline.",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new FollowUp
            {
                CustomerId = customer2.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(5),
                Notes = "Review AWS vs Azure cost comparison chart with VP of Engineering.",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new FollowUp
            {
                CustomerId = customer3.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(-1),
                Notes = "Kickoff onboarding meeting with operations leadership.",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            }
        };

        await context.FollowUps.AddRangeAsync(followUps);
        await context.SaveChangesAsync();
    }
}
