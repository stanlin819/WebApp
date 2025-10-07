using WebApp.Models.Service;
using WebApp.Models.TransactionModel;
using WebApp.Models.Repositories;
namespace WebApp.Models.Service;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repo;
    private readonly IUserRepository _userRepo;
    public TransactionService(ITransactionRepository repo, IUserRepository userRepo)
    {
        _repo = repo;
        _userRepo = userRepo;
    }
    public async Task<IEnumerable<Transaction>> GetUserAllTransactions(int userId)
    {
        var transactions = await _repo.GetUserTransactions(userId);
        return transactions;
    }
    public async Task<decimal> GetMonthlyExpense(int userId, int year, int month)
    {
        var transactions = await _repo.GetUserTransactions(userId);
        decimal sum = 0;
        foreach (var transaction in transactions)
        {
            if (transaction.Date.Year == year && transaction.Date.Month == month && !transaction.IsIncome)
            {
                sum = sum + transaction.Amount;
            }
        }

        return sum;

    }
    public async Task<Dictionary<string, decimal>> GetCategoryDistribution(int userId)
    {
        var transactions = await _repo.GetUserTransactions(userId);
        return transactions
            .GroupBy(t => t.Category.ToString())  // 將列舉轉換為字串
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
    }

    public async Task<IEnumerable<Transaction>> GetAllTransaction()
    {
        return await _repo.GetAll();
    }
    public async Task<String> AddTransaction(Transaction transaction, int userId)
    {
        transaction.SetUserId(userId);
        transaction.SetIncome();
        return await _repo.Add(transaction);
    }
    public async Task<Dictionary<string, string>> AddTransactions(IEnumerable<TransactionViewModel> transactions)
    {
        var Mess = new Dictionary<string, string>();
        var existingTransactions = await _repo.GetAll();

        var existingTransactionKeys = existingTransactions
            .Select(et => new { et.Title, et.UserId })
            .ToHashSet();

        var allUsers = await _userRepo.GetAll();

        var groupedTransactions = transactions
            .GroupBy(t => new { t.Title, t.UserName })
            .Select(g => g.First())
            .ToList();

        var joinedTransactions = from t in groupedTransactions
                                 join u in allUsers
                                 on t.UserName equals u.Username
                                 select new
                                 {
                                     Transaction = t,
                                     UserId = u.Id
                                 };
        var duplicateTransactions = joinedTransactions
            .Where(jt => existingTransactionKeys.Contains(new { jt.Transaction.Title, jt.UserId }))
            .Select(jt => new
            {
                jt.Transaction.Title,
                allUsers.Where(u => u.Id == jt.UserId).First().Username
            }).ToList();

        var newTransactions = joinedTransactions
            .Where(jt => !existingTransactionKeys.Contains(new { jt.Transaction.Title, jt.UserId }))
            .Select(jt => new Transaction
            {
                Title = jt.Transaction.Title,
                Amount = jt.Transaction.Amount,
                Date = jt.Transaction.Date,
                Category = jt.Transaction.Category,
                IsIncome = jt.Transaction.Category == Category.Work,
                UserId = jt.UserId
            }).ToList();

        if (newTransactions.Any())
        {
            await _repo.AddTransactions(newTransactions);
        }

        if (duplicateTransactions.Any())
        {
            Mess["Warning"] = $"Found {duplicateTransactions.Count} duplicate transactions: {string.Join(", ", duplicateTransactions)}";
        }

        if (newTransactions.Count() > 0)
            Mess["Message"] = $"{newTransactions.Count()} transactions imported successfully.";

        return Mess;
    }


    public async Task<String> DeleteTransaction(int id)
    {
        return await _repo.Delete(id);
    }

    public async Task DeleteTransactions(IEnumerable<Transaction> transactions)
    {
        await _repo.DeleteTransactions(transactions);
    }

    public async Task DeleteUserTransactions(int userId)
    {
        var transactions = await GetUserAllTransactions(userId);
        await DeleteTransactions(transactions);
    }

    public async Task<String> EditTransaction(Transaction transaction)
    {
        return await _repo.Update(transaction);
    }

}