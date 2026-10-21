using Microsoft.EntityFrameworkCore;
using OmniCRM.Data;
using OmniCRM.Models;

namespace OmniCRM.Services;

/// <summary>
/// Service providing scheduling and status updates for Customer Follow-Ups.
/// Uses short-lived DbContext instances to guarantee thread-safety.
/// </summary>
public class FollowUpService
{
    public FollowUpService(OmniCrmDbContext? context = null)
    {
    }

    public async Task<List<FollowUp>> GetAllAsync(bool? onlyPending = false)
    {
        await using var context = new OmniCrmDbContext();
        IQueryable<FollowUp> query = context.FollowUps
            .Include(f => f.Customer)
            .AsNoTracking();

        if (onlyPending == true)
        {
            query = query.Where(f => !f.IsCompleted);
        }

        return await query.OrderBy(f => f.ScheduledDate).ToListAsync();
    }

    public async Task<FollowUp> CreateAsync(FollowUp followUp)
    {
        await using var context = new OmniCrmDbContext();
        followUp.CreatedAt = DateTime.UtcNow;
        context.FollowUps.Add(followUp);
        await context.SaveChangesAsync();
        return followUp;
    }

    public async Task<bool> ToggleCompletedAsync(int id)
    {
        await using var context = new OmniCrmDbContext();
        var existing = await context.FollowUps.FindAsync(id);
        if (existing == null) return false;

        existing.IsCompleted = !existing.IsCompleted;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = new OmniCrmDbContext();
        var existing = await context.FollowUps.FindAsync(id);
        if (existing == null) return false;

        context.FollowUps.Remove(existing);
        await context.SaveChangesAsync();
        return true;
    }
}
