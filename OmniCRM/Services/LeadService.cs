using Microsoft.EntityFrameworkCore;
using OmniCRM.Data;
using OmniCRM.Models;

namespace OmniCRM.Services;

/// <summary>
/// Service providing lead lifecycle management and pipeline filtering.
/// Uses short-lived DbContext instances to guarantee thread-safety.
/// </summary>
public class LeadService
{
    public LeadService(OmniCrmDbContext? context = null)
    {
    }

    public async Task<List<Lead>> GetAllAsync(LeadStatus? statusFilter = null, string? searchTerm = null)
    {
        await using var context = new OmniCrmDbContext();
        IQueryable<Lead> query = context.Leads
            .Include(l => l.Customer)
            .AsNoTracking();

        if (statusFilter.HasValue)
        {
            query = query.Where(l => l.Status == statusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = searchTerm.Trim().ToLower();
            query = query.Where(l =>
                l.Title.ToLower().Contains(term) ||
                (l.Customer != null && l.Customer.Name.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
    }

    public async Task<Lead?> GetByIdAsync(int id)
    {
        await using var context = new OmniCrmDbContext();
        return await context.Leads
            .Include(l => l.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Lead> CreateAsync(Lead lead)
    {
        await using var context = new OmniCrmDbContext();
        lead.CreatedAt = DateTime.UtcNow;
        context.Leads.Add(lead);
        await context.SaveChangesAsync();
        return lead;
    }

    public async Task<bool> UpdateAsync(Lead lead)
    {
        await using var context = new OmniCrmDbContext();
        var existing = await context.Leads.FindAsync(lead.Id);
        if (existing == null) return false;

        existing.Title = lead.Title;
        existing.Status = lead.Status;
        existing.EstimatedValue = lead.EstimatedValue;
        existing.Notes = lead.Notes;
        existing.CustomerId = lead.CustomerId;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = new OmniCrmDbContext();
        var lead = await context.Leads.FindAsync(id);
        if (lead == null) return false;

        context.Leads.Remove(lead);
        await context.SaveChangesAsync();
        return true;
    }
}
