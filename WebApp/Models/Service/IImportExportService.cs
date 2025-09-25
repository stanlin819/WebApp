using WebApp.Models.UserModel;
using WebApp.Models.TransactionModel;
public interface IImportExportService
{
    Task<IEnumerable<User>> ImportUserFile(IFormFile file);
    Task<IEnumerable<TransactionViewModel>> ImportTransactionFile(IFormFile file);
    Task<MemoryStream> ConvertExcelToPdf(IFormFile file, string package);
    MemoryStream ExportUserToExcel(IEnumerable<UserViewModel> users);
    Task<MemoryStream> ExportTransactionToExcel(IEnumerable<Transaction> transactions);
    MemoryStream ExportUserToPDF(IEnumerable<UserViewModel> users);
    Task<MemoryStream> ExportTransactionToPDF(IEnumerable<TransactionViewModel> transactions);
    Task<MemoryStream> GenerateUserReportAsync(User user, List<Transaction> transactions);
    Task<MemoryStream> GenerateUserReportFromTemplate(string templatePath, User user, List<Transaction> transactions);
    Task<MemoryStream> UserReportPdf(User user, List<Transaction> transactions, bool watermark);
    Task<MemoryStream> UserReportFromTemplatPdf(string template_path, User user, List<Transaction> transactions, bool watermark);
    Task SendUserReport(User user, List<Transaction> transactions);
}