using WebApp.Models.TransactionModel;

namespace WebApp.Models.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetUserTransactions(int userId);
    Task AddTransactions(IEnumerable<Transaction> transactions);
    Task DeleteTransactions(IEnumerable<Transaction> transactions);
}