using Microsoft.AspNetCore.Mvc;
using WebApp.Models.UserModel;
using WebApp.Models.TransactionModel;
using WebApp.Models.Service;
using System.Transactions;


namespace WebApp.Controllers;

public class UserController : Controller
{
    private readonly IUserService _service;
    private readonly ITransactionService _transaction_service;

    private readonly IBudgetService _budget_service;

    private readonly IFileService _file_service;

    private readonly IMailService _mail_service;

    private readonly IVideoService _video_service;

    int PageSize = 10;


    public UserController(IUserService service, ITransactionService transaction_service, IBudgetService budget_service, IFileService file_service, IMailService mail_service, IVideoService video_service)
    {
        _service = service;
        _transaction_service = transaction_service;
        _budget_service = budget_service;
        _file_service = file_service;
        _mail_service = mail_service;
        _video_service = video_service;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var users = await _service.GetUserOverview();

        var totalUsers = users.Count();
        var totalPages = (int)Math.Ceiling(totalUsers / (double)PageSize);

        users = users.OrderBy(u => u.UserName);
        users = users.Skip((page - 1) * PageSize).Take(PageSize).ToList();

        // ViewBag.Users = users;
        ViewData["CurrentPage"] = page;
        ViewBag.TotalPages = totalPages;

        return View(users);
    }


    [HttpGet]
    public IActionResult Create()
    {
        return View(new User());
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        var mes = await _service.AddUser(user);
        if (mes["isSuccess"].Equals("false"))
        {
            TempData["isSuccess"] = false;
            TempData["Message"] = mes["Message"];
            return View();
        }
        TempData["isSuccess"] = true;
        TempData["Message"] = mes["Message"];
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> UserTablePartial(int page = 1)
    {
        var users = await _service.GetUserOverview();

        var totalUsers = users.Count();
        var totalPages = (int)Math.Ceiling(totalUsers / (double)PageSize);

        users = users.OrderBy(u => u.UserName);
        users = users.Skip((page - 1) * PageSize).Take(PageSize).ToList();

        ViewData["CurrentPage"] = page;
        ViewBag.TotalPages = totalPages;
        return PartialView("_UsersTable", users);
    }

    public IActionResult CreatePartial()
    {
        return PartialView("_CreateUser");
    }

    public async Task<IActionResult> CreateAjax(User user)
    {
        if (ModelState.IsValid)
        {
            var mes = await _service.AddUser(user);

            if (mes["isSuccess"].Equals("false"))
            {
                return Json(new { success = false, message = mes["Message"] });
            }
            return Json(new { success = true, message = mes["Message"] });

        }

        return PartialView("_CreateUser", user);
    }

    public IActionResult Pagination(int page, int totalPages)
    {
        ViewData["CurrentPage"] = page;
        ViewBag.TotalPages = totalPages;
        return PartialView("_Pagination");
    }
    public async Task<IActionResult> Delete(int id, int currentPage = 1)
    {
        var name = await _service.DeleteUser(id);

        await _file_service.RemoveUserFold(id);
        await _video_service.DeleteUserVideo(id);


        TempData["Message"] = $"{name} deleted successfully.";
        ViewData["CurrentPage"] = currentPage;

        return RedirectToAction("Index", new { page = currentPage });
    }

    public async Task<IActionResult> DeleteAlluser()
    {
        var users = await _service.GetAllUsers();

        await _service.DeleteUsers(users);

        await _file_service.RemoveAllFile();

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _service.GetUser(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(User user)
    {
        var mes = await _service.UpdateUser(user);
        if (mes["isSuccess"].Equals("false"))
        {
            TempData["isSuccess"] = false;
            TempData["Message"] = mes["Message"];
        }
        else
        {
            TempData["isSuccess"] = true;
            TempData["Message"] = mes["Message"];
        }
        return RedirectToAction("Details", new { user.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id = 0, int currentPage = 1, string userName = "", string comeFrom = "User")
    {
        var user = new User();
        if (userName != "" && id == 0)
        {
            user = await _service.GetUserByUsername(userName);
        }
        else
        {
            user = await _service.GetUser(id);
        }

        var budgets = await _budget_service.GetUserBudgets(user.Id);

        if (!budgets.Any())
        {
            await _budget_service.InitUserBudget(user.Id);
            budgets = await _budget_service.GetUserBudgets(user.Id);
        }

        var transactions = await _transaction_service.GetUserAllTransactions(user.Id);
        var viewModels = new List<TransactionViewModel>();
        foreach (var trans in transactions)
        {
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

        var files = await _file_service.GetUserFiles(user.Id);
        var videos = await _video_service.GetUserVideos(user.Id);

        var categoryDistribution = await _transaction_service.GetCategoryDistribution(user.Id);

        ViewBag.ComeFrom = comeFrom;
        ViewBag.Files = files;
        ViewBag.Videos = videos;
        ViewBag.Budgets = budgets;
        ViewBag.CategoryDistribution = categoryDistribution;

        ViewBag.Transactions = viewModels;
        ViewData["CurrentPage"] = currentPage;

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> EditBudget(int id, int currentPage = 1)
    {
        var budget = await _budget_service.GetUserBudgets(id);

        ViewBag.userId = id;
        ViewBag.currentPage = currentPage;

        return View(budget);
    }

    [HttpPost]
    public async Task<IActionResult> EditBudget(Dictionary<string, decimal?> limits, int id, int currentPage = 1)
    {
        var budgets = await _budget_service.GetUserBudgets(id);
        foreach (var limit in limits)
        {
            var budget = budgets.Where(b => b.Category.Equals(limit.Key)).FirstOrDefault();
            if (budget != null)
            {
                budget.setLimit(limit.Value);
                await _budget_service.UpdateBudget(budget);   
            }
        }

        return RedirectToAction("Details", new { id, currentPage });
    }

    public async Task<IActionResult> VideoList(int userId)
    {   
        var videos = await _video_service.GetUserVideos(userId);
        ViewBag.Videos = videos;
        var user = await _service.GetUser(userId);
        return PartialView("_VideoList", user);
    }
}