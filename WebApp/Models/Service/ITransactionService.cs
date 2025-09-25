using WebApp.Models.TransactionModel;

namespace WebApp.Models.Service;

public interface ITransactionService
{
    Task<IEnumerable<Transaction>> GetUserAllTransactions(int userId);
    Task<decimal> GetMonthlyExpense(int userId, int year, int month);
    Task<Dictionary<string, decimal>> GetCategoryDistribution(int userId);

    Task<IEnumerable<Transaction>> GetAllTransaction();
    Task<String> AddTransaction(Transaction transaction, int userId);
    Task<Dictionary<string, string>> AddTransactions(IEnumerable<TransactionViewModel> transactions);
    Task<String> DeleteTransaction(int id);
    Task DeleteTransactions(IEnumerable<Transaction> transactions);
    Task DeleteUserTransactions(int userId);
    Task<String> EditTransaction(Transaction transaction);
}