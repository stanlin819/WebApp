using WebApp.Models.TransactionModel;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace WebApp.Models.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;
    public TransactionRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Transaction>> GetUserTransactions(int userId) =>
        await _context.Transactions.Where(t => t.UserId == userId).ToListAsync();

    public async Task<IEnumerable<Transaction>> GetAll()
    {
        return await _context.Transactions.ToListAsync();
    }

    public async Task<Transaction> Get(int id)
    {
        var t = await _context.Transactions.FindAsync(id);
        if (t == null)
            throw new KeyNotFoundException($"Transacion with id {id} not found.");
        else
            return t;
    }


    public async Task<String> Add(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction.Title;
    }
    public async Task AddTransactions(IEnumerable<Transaction> transactions)
    {
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();
    }

    public async Task<String> Update(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
        return transaction.Title;
    }

    public async Task<String> Delete(int id)
    {
        var t = await _context.Transactions.FindAsync(id);
        if (t != null)
        {
            _context.Transactions.Remove(t);
            await _context.SaveChangesAsync();
            return t.Title;
        }
        return "";
    }

    public async Task DeleteTransactions(IEnumerable<Transaction> transactions)
    {
        _context.Transactions.RemoveRange(transactions);
        await _context.SaveChangesAsync();

    }
}