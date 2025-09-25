using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.TransactionModel;
using WebApp.Models.Service;
using WebApp.Models.Repositories;
using System.Threading.Tasks;
using Azure;

namespace WebApp.Controllers;

public class TransactionController : Controller
{
    private readonly ITransactionService _service;
    private readonly IUserService _userservice;
    public TransactionController(ITransactionService service, IUserService userservice)
    {
        _service = service;
        _userservice = userservice;
    }

    public async Task<IActionResult> Index()
    {
        var transactions = await _service.GetAllTransaction();
        var viewModels = new List<TransactionViewModel>();
        foreach (var trans in transactions)
        {

            var user = await _userservice.GetUser(trans.UserId);
            var transaction = new TransactionViewModel
            {
                Title = trans.Title,
                Amount = trans.Amount,
                Date = trans.Date,
                Category = trans.Category,
                IsIncome = trans.IsIncome,
                UserName = user.Username,
                Id = trans.Id
            };

            viewModels.Add(transaction);

        }
        return View(viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int uid = 0, int page = 1)
    {
        if (uid != 0)
        {
            var user = await _userservice.GetUser(uid);
            ViewBag.UserName = user.Username;
        }
        else
        {
            ViewBag.UserName = null;
        }
        ViewBag.UserId = uid;
        var tran = new Transaction
        {
            Date = DateTime.Today
        };
        ViewBag.CurrentPage = page;
        return View(tran);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Transaction transaction, string UserName, int userId = 0, int p =1)
    {
        Console.WriteLine("Transaction Id: " + transaction.Id);
        var user = await _userservice.GetUserByUsername(UserName);
        if (user == null)
        {
            TempData["Error"] = $"User {UserName} not found.";
            return RedirectToAction("Create", new { uid = userId, page = p});
        }
        if (transaction.Title == null)
        {
            TempData["Error"] = "Please input Title";
            return RedirectToAction("Create", new { uid = userId, page = p});
        }
        if (transaction.Category == null)
        {
            TempData["Error"] = "Please select Category";
            return RedirectToAction("Create", new { uid = userId, page = p});
        }
        Console.WriteLine("Category: " + transaction.Category);
        var title = await _service.AddTransaction(transaction, user.Id);
        TempData["Message"] = $"{title} created successfully.";
        
        return RedirectToAction("Details", "User", new { id = user.Id, currentPage = p});
    }

    public async Task<IActionResult> Delete(int Id, int userId, int page)
    {
        await _service.DeleteTransaction(Id);

        return RedirectToAction("Details", "User", new { id = userId, currentPage = page });
    }

}