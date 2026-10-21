using Microsoft.EntityFrameworkCore;
using OmniCRM.Data;
using OmniCRM.Models;

namespace OmniCRM.Services;

/// <summary>
/// Service providing asynchronous CRUD operations and validation logic for Customers.
/// Uses short-lived DbContext instances to guarantee thread-safety and avoid concurrency conflicts.
/// </summary>
public class CustomerService
{
    public CustomerService(OmniCrmDbContext? context = null)
    {
    }

    public async Task<List<Customer>> GetAllAsync(string? searchTerm = null)
    {
        await using var context = new OmniCrmDbContext();
        IQueryable<Customer> query = context.Customers
            .Include(c => c.Leads)
            .Include(c => c.FollowUps)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = searchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                c.CompanyName.ToLower().Contains(term) ||
                c.Phone.Contains(term));
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        await using var context = new OmniCrmDbContext();
        return await context.Customers
            .Include(c => c.Leads)
            .Include(c => c.FollowUps)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<(bool Success, string? ErrorMessage, Customer? Customer)> CreateAsync(Customer customer)
    {
        await using var context = new OmniCrmDbContext();

        // Validation: Unique Email Check
        bool emailExists = await context.Customers
            .AnyAsync(c => c.Email.ToLower() == customer.Email.ToLower());

        if (emailExists)
        {
            return (false, $"A customer with email '{customer.Email}' already exists.", null);
        }

        customer.CreatedAt = DateTime.UtcNow;
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return (true, null, customer);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(Customer customer)
    {
        await using var context = new OmniCrmDbContext();
        var existing = await context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        if (existing == null)
        {
            return (false, "Customer not found.");
        }

        // Validate unique email excluding self
        bool emailConflict = await context.Customers
            .AnyAsync(c => c.Email.ToLower() == customer.Email.ToLower() && c.Id != customer.Id);

        if (emailConflict)
        {
            return (false, $"Another customer with email '{customer.Email}' already exists.");
        }

        existing.Name = customer.Name;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.CompanyName = customer.CompanyName;
        existing.Address = customer.Address;

        await context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        await using var context = new OmniCrmDbContext();
        var customer = await context.Customers.FindAsync(id);
        if (customer == null)
        {
            return (false, "Customer not found.");
        }

        context.Customers.Remove(customer);
        await context.SaveChangesAsync();
        return (true, null);
    }
}
