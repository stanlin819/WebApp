using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using WebApp.Models.Service;
using System.Reflection;
using WebApp.Models.UserModel;
using WebApp.Models.TransactionModel;
using iTextSharp.text;
using iTextSharp.text.pdf;



public class ImportExportController : Controller
{
    private readonly IUserService _userservice;
    private readonly ITransactionService _transactionservice;

    private readonly IImportExportService _service;

    public ImportExportController(IUserService userservice, ITransactionService transactionservice, IImportExportService service)
    {
        _userservice = userservice;
        _transactionservice = transactionservice;
        _service = service;
    }


    [HttpGet]
    public IActionResult Import(string comeFrom)
    {
        ViewBag.comeFrom = comeFrom;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ImportUserFile(IFormFile file)
    {
        if (file == null || file.Length == 0 || !Path.GetExtension(file.FileName).ToLowerInvariant().Equals(".xlsx"))
        {
            TempData["Warning"] = "Please select a valid Excel file.";
            return RedirectToAction("Import", new { comeFrom = "User" });
        }

        var importedData = await _service.ImportUserFile(file);
        return View(importedData);

    }

    public async Task<IActionResult> ImportTransactionFile(IFormFile file)
    {
        if (file == null || file.Length == 0 || !Path.GetExtension(file.FileName).ToLowerInvariant().Equals(".xlsx"))
        {
            TempData["Warning"] = "Please select a valid Excel file.";
            return RedirectToAction("Import", new { comeFrom = "Transaction" });
        }

        var importedData = await _service.ImportTransactionFile(file);
        return View(importedData);
    }

    public async Task<IActionResult> ConvertExcelToPdf(IFormFile file, string cf, string package)
    {
        if (file == null || file.Length == 0 || !Path.GetExtension(file.FileName).ToLowerInvariant().Equals(".xlsx"))
        {
            TempData["Warning"] = "Please select a valid Excel file.";
            return RedirectToAction("Import", new { comeFrom = cf });
        }
        var pdfStream = await _service.ConvertExcelToPdf(file, package);

        string pdfName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{package}.pdf";

        return File(pdfStream.ToArray(), "application/pdf", pdfName);
    }

    public async Task<IActionResult> ExportAllUserToExcel()
    {
        var users = await _userservice.GetUserOverview();
        users = users.OrderBy(u => u.UserName);

        var stream = _service.ExportUserToExcel(users);

        string excelName = $"User_{System.DateTime.Now:yyyyMMddHHmmss}.xlsx";

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);

    }

    public async Task<IActionResult> ExportAllTransactionToExcel()
    {
        var transactions = await _transactionservice.GetAllTransaction();
        transactions = transactions.OrderBy(t => t.UserId);

        var stream = await _service.ExportTransactionToExcel(transactions);

        string excelName = $"Transaction_{System.DateTime.Now:yyyyMMddHHmmss}.xlsx";

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);

    }

    public async Task<IActionResult> ExportTransactionToExcel(IEnumerable<TransactionViewModel> transactions)
    {
        var tran = new List<Transaction>();
        foreach (var trans in transactions)
        {
            var user = await _userservice.GetUserByUsername(trans.UserName);

            var transaction = new Transaction
            {
                Title = trans.Title,
                Amount = trans.Amount,
                Date = trans.Date,
                Category = trans.Category,
                UserId = user.Id
            };

            tran.Add(transaction);
        }
        var stream = await _service.ExportTransactionToExcel(tran);

        string excelName = $"Transaction_{System.DateTime.Now:yyyyMMddHHmmss}.xlsx";

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
    }
    public async Task<IActionResult> ExportToExcel(string item)
    {
        // ExcelPackage.License.SetNonCommercialPersonal("Stanlin");

        var data = null as IEnumerable<object>;
        var properties = null as PropertyInfo[];

        if (item.ToLower() == "users")
        {
            var user = await _userservice.GetAllUsers();
            data = user.OrderBy(u => u.Username);
            properties = typeof(User).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        }
        else if (item.ToLower() == "transactions")
        {
            data = await _transactionservice.GetAllTransaction();
            properties = typeof(Transaction).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        }
        else
        {
            return BadRequest("Invalid item type. Please specify 'users' or 'transactions'.");
        }

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add(item);

            // 填入標題列
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = properties[i].Name;
            }

            // 填入資料列
            for (int row = 0; row < data.Count(); row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var value = properties[col].GetValue(data.ElementAt(row));
                    worksheet.Cells[row + 2, col + 1].Value = value;
                }
            }

            // 自動調整欄寬
            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string excelName = $"Export_{System.DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
    }


    public async Task<IActionResult> ExportAllUserToPDF()
    {
        var users = await _userservice.GetUserOverview();
        users = users.OrderBy(u => u.UserName);
        var pdfStream = _service.ExportUserToPDF(users);
        string pdfName = $"User_{System.DateTime.Now:yyyyMMddHHmmss}.pdf";

        return File(pdfStream.ToArray(), "application/pdf", pdfName);
    }

    // public IActionResult ExportUserPDF(IEnumerable<User> users)
    // {
    //     var pdfStream = _service.ExportUserToPDF(users);
    //     string pdfName = $"User_{System.DateTime.Now:yyyyMMddHHmmss}.pdf";
    //     return File(pdfStream.ToArray(), "application/pdf", pdfName);
    // }

    public async Task<IActionResult> ExportAllTransactionToPDF()
    {
        var transactions = await _transactionservice.GetAllTransaction();
        var users = await _userservice.GetAllUsers();

        var transactionViewModels = transactions
            .OrderBy(t => t.UserId)
            .Select(t => new TransactionViewModel
            {
                Title = t.Title,
                Amount = t.Amount,
                Date = t.Date,
                Category = t.Category,
                IsIncome = t.IsIncome,
                UserName = users.FirstOrDefault(u => u.Id == t.UserId)?.Username ?? "Unknown",
                Id = t.Id
            })
            .ToList();

        var pdfStream = await _service.ExportTransactionToPDF(transactionViewModels);
        string pdfName = $"Transaction_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        return File(pdfStream.ToArray(), "application/pdf", pdfName);
    }

    public async Task<IActionResult> ExportTransactionPDF(IEnumerable<TransactionViewModel> transactions)
    {
        var pdfStream = await _service.ExportTransactionToPDF(transactions);
        string pdfName = $"Transaction_{System.DateTime.Now:yyyyMMddHHmmss}.pdf";
        return File(pdfStream.ToArray(), "application/pdf", pdfName);
    }


    public async Task<IActionResult> Save_User(IEnumerable<User> users)
    {
        var mes = await _userservice.AddUsers(users);
        if (mes.ContainsKey("Warning"))
            TempData["Warning"] = mes["Warning"];

        if (mes.ContainsKey("Message"))
            TempData["Message"] = mes["Message"];
            
        return RedirectToAction("Index", "User");
    }

    [HttpPost]
    public async Task<IActionResult> Save_Transaction(IEnumerable<TransactionViewModel> transactions)
    {
        var mes = await _transactionservice.AddTransactions(transactions);

        if (mes.ContainsKey("Warning"))
            TempData["Warning"] = mes["Warning"];

        if (mes.ContainsKey("Message"))
            TempData["Message"] = mes["Message"];

        return RedirectToAction("Index", "Transaction");
    }

    public IActionResult Discard_User()
    {
        TempData["Message"] = "Import operation discarded.";
        return RedirectToAction("Index", "User");
    }

    public IActionResult Discard_Transaction()
    {
        TempData["Message"] = "Import operation discarded.";
        return RedirectToAction("Index", "Transaction");
    }

    [HttpGet]
    public async Task<IActionResult> UserReport(int id)
    {
        var user = await _userservice.GetUser(id);
        if (user == null) return NotFound();

        var transactions = await _transactionservice.GetUserAllTransactions(id);
        transactions = transactions.OrderByDescending(t => t.Date);

        var ms = await _service.GenerateUserReportAsync(user, transactions.ToList());
        var fileName = $"UserReport_{user.Username}_{DateTime.Now:yyyyMMddHHmmss}.docx";

        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            fileName);
    }

    public async Task<IActionResult> UserReportFromTemplate(int id)
    {
        string templatePath = "wwwroot/template/word_template.docx";
        var user = await _userservice.GetUser(id);
        var transactions = await _transactionservice.GetUserAllTransactions(id);
        transactions = transactions.OrderByDescending(t => t.Date);

        var ms = await _service.GenerateUserReportFromTemplate(templatePath, user, transactions.ToList());
        var fileName = $"UserReport_{user.Username}_{DateTime.Now:yyyyMMddHHmmss}.docx";

        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            fileName);
    }

    public async Task<IActionResult> UserReportPdf(int id, bool watermark)
    {
        var user = await _userservice.GetUser(id);
        if (user == null) return NotFound();

        var transactions = await _transactionservice.GetUserAllTransactions(id);
        transactions = transactions.OrderByDescending(t => t.Date);

        var ms = await _service.UserReportPdf(user, transactions.ToList(), watermark);

        string pdfName = $"UserReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        return File(ms.ToArray(), "application/pdf", pdfName);
    }

    public async Task<IActionResult> UserReportFromTemplatPdf(int id, bool watermark)
    {
        string templatePath = "wwwroot/template/word_template.docx";
        var user = await _userservice.GetUser(id);
        if (user == null) return NotFound();

        var transactions = await _transactionservice.GetUserAllTransactions(id);
        transactions = transactions.OrderByDescending(t => t.Date);

        var ms = await _service.UserReportFromTemplatPdf(templatePath, user, transactions.ToList(), watermark);

        string pdfName = $"UserReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        return File(ms.ToArray(), "application/pdf", pdfName);
    }

    public async Task<IActionResult> SendUserReport(int id)
    {
        var user = await _userservice.GetUser(id);
        if (user == null) return NotFound();

        var transactions = await _transactionservice.GetUserAllTransactions(id);
        transactions = transactions.OrderByDescending(t => t.Date);

        await _service.SendUserReport(user, transactions.ToList());
        return RedirectToAction("Details", "User",new { id});

    }
}
