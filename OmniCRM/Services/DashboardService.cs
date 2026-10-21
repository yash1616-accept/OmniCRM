using Microsoft.EntityFrameworkCore;
using OmniCRM.Data;
using OmniCRM.Models;

namespace OmniCRM.Services;

public class DashboardMetrics
{
    public int TotalCustomers { get; set; }
    public int ActiveLeads { get; set; }
    public int ConvertedLeads { get; set; }
    public decimal TotalPipelineValue { get; set; }
    public int PendingFollowUps { get; set; }
    public List<Customer> RecentCustomers { get; set; } = new();
    public List<Lead> RecentLeads { get; set; } = new();
    public List<FollowUp> UpcomingFollowUps { get; set; } = new();
}

/// <summary>
/// Service calculating real-time dashboard KPIs and recent activity feeds.
/// Uses short-lived DbContext instances to guarantee thread-safety in WinForms.
/// </summary>
public class DashboardService
{
    public DashboardService(OmniCrmDbContext? context = null)
    {
    }

    public async Task<DashboardMetrics> GetMetricsAsync()
    {
        var metrics = new DashboardMetrics();

        try
        {
            await using var context = new OmniCrmDbContext();

            metrics.TotalCustomers = await context.Customers.CountAsync();

            metrics.ActiveLeads = await context.Leads
                .CountAsync(l => l.Status == LeadStatus.New ||
                                 l.Status == LeadStatus.Contacted ||
                                 l.Status == LeadStatus.Qualified);

            metrics.ConvertedLeads = await context.Leads
                .CountAsync(l => l.Status == LeadStatus.Converted);

            metrics.TotalPipelineValue = await context.Leads
                .Where(l => l.Status != LeadStatus.Lost)
                .SumAsync(l => l.EstimatedValue);

            metrics.PendingFollowUps = await context.FollowUps
                .CountAsync(f => !f.IsCompleted);

            metrics.RecentCustomers = await context.Customers
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            metrics.RecentLeads = await context.Leads
                .Include(l => l.Customer)
                .OrderByDescending(l => l.CreatedAt)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            metrics.UpcomingFollowUps = await context.FollowUps
                .Include(f => f.Customer)
                .Where(f => !f.IsCompleted)
                .OrderBy(f => f.ScheduledDate)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error calculating metrics: {ex.Message}");
        }

        return metrics;
    }
}
