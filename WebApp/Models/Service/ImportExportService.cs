using OfficeOpenXml;
using WebApp.Models.UserModel;
using WebApp.Models.TransactionModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using Syncfusion.Pdf;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using iLovePdf;
using iLovePdf.Core;
using iLovePdf.Model.Task;
using GemBox.Spreadsheet;
using Aspose.Cells;
using Spire.Xls;

namespace WebApp.Models.Service;

public class ImportExportService : IImportExportService
{
    private readonly IUserService _userservice;
    private readonly ITransactionService _transactionservice;

    private readonly IMailService _mailservice;

    public ImportExportService(IUserService userservice, ITransactionService transactionservice, IMailService mailservice)
    {
        _userservice = userservice;
        _transactionservice = transactionservice;
        _mailservice = mailservice;
    }

    public MemoryStream ExportUserToExcel(IEnumerable<UserViewModel> users)
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Users");

            // 標題列
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Transaction Count";
            worksheet.Cells[1, 3].Value = "Total Income";
            worksheet.Cells[1, 4].Value = "Total Expense";
            worksheet.Cells[1, 5].Value = "Last Transaction Date";
            worksheet.Cells[1, 6].Value = "Email";

            // 填入資料列
            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cells[row, 1].Value = user.UserName;
                worksheet.Cells[row, 2].Value = user.TransactionCount;
                worksheet.Cells[row, 3].Value = user.TotalIncome;
                worksheet.Cells[row, 4].Value = user.TotalExpense;
                string formattedDate = user.LastTransactionDate?.ToString("yyyy-MM-dd") ?? "N/A";
                worksheet.Cells[row, 5].Value = formattedDate;
                worksheet.Cells[row, 6].Value = user.Email;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }
    }

    public MemoryStream ExportUserToPDF(IEnumerable<UserViewModel> users)
    {
        using var pdfStream = new MemoryStream();

        // 建立 PDF 文件
        iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 25, 25);
        PdfWriter writer = PdfWriter.GetInstance(document, pdfStream);
        document.Open();

        // 建立 PDF 表格
        PdfPTable table = new PdfPTable(6);
        table.WidthPercentage = 100;

        // 標題列
        table.AddCell("Name");
        table.AddCell("Transaction Count");
        table.AddCell("Total Income");
        table.AddCell("Total Expense");
        table.AddCell("Last Transaction Date");
        table.AddCell("Email");

        // 資料列
        foreach (var user in users)
        {
            table.AddCell(user.UserName);
            table.AddCell(user.TransactionCount.ToString());
            table.AddCell(user.TotalIncome.ToString());
            table.AddCell(user.TotalExpense.ToString());
            string formattedDate = user.LastTransactionDate?.ToString("yyyy-MM-dd") ?? "N/A";
            table.AddCell(formattedDate);
            table.AddCell(user.Email);
        }

        document.Add(table);
        document.Close();

        pdfStream.Position = 0;
        return pdfStream;
    }

    public async Task<MemoryStream> ExportTransactionToExcel(IEnumerable<Transaction> transactions)
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Transaction");

            // 標題列
            worksheet.Cells[1, 1].Value = "Title";
            worksheet.Cells[1, 2].Value = "Amount";
            worksheet.Cells[1, 3].Value = "Date";
            worksheet.Cells[1, 4].Value = "Category";
            worksheet.Cells[1, 5].Value = "User";

            // 填入資料列
            int row = 2;
            foreach (var transaction in transactions)
            {
                worksheet.Cells[row, 1].Value = transaction.Title;
                worksheet.Cells[row, 2].Value = transaction.Amount;
                worksheet.Cells[row, 3].Value = transaction.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 4].Value = transaction.Category.ToString();
                var user = await _userservice.GetUser(transaction.UserId);
                worksheet.Cells[row, 5].Value = user.Username;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return stream;
        }
    }

    public async Task<MemoryStream> ExportTransactionToPDF(IEnumerable<TransactionViewModel> transactions)
    {
        using var pdfStream = new MemoryStream();

        // 建立 PDF 文件
        iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 25, 25);
        PdfWriter writer = PdfWriter.GetInstance(document, pdfStream);
        document.Open();

        // 建立 PDF 表格
        PdfPTable table = new PdfPTable(5);
        table.WidthPercentage = 100;

        // 標題列
        table.AddCell("Title");
        table.AddCell("Amount");
        table.AddCell("Date");
        table.AddCell("Category");
        table.AddCell("User");

        // 資料列
        foreach (var transaction in transactions)
        {
            table.AddCell(transaction.Title);
            table.AddCell(transaction.Amount.ToString());
            table.AddCell(transaction.Date.ToString("yyyy-MM-dd"));
            table.AddCell(transaction.Category.ToString());
            table.AddCell(transaction.UserName);
        }

        document.Add(table);
        document.Close();

        pdfStream.Position = 0;
        return pdfStream;
    }

    public async Task<MemoryStream> ConvertExcelToPdf(IFormFile file, string package)
    {
        
        string tempExcelPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(file.FileName));
        string tempPdfPath = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(file.FileName)}.pdf");
        var pdfStream = new MemoryStream();
        try
        {
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                // 儲存 Excel 暫存檔
                using (var excelFile = new FileStream(tempExcelPath, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(excelFile);
                }
                switch (package)
                {
                    case "Aspose":
                        var asp_workbook = new Aspose.Cells.Workbook(tempExcelPath);
                        asp_workbook.Save(tempPdfPath, SaveFormat.Pdf);
                        break;
                    case "GameBox":
                        SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
                        ExcelFile gm_workbook = ExcelFile.Load(tempExcelPath);
                        gm_workbook.Save(tempPdfPath, new GemBox.Spreadsheet.PdfSaveOptions() { SelectionType = GemBox.Spreadsheet.SelectionType.EntireFile });

                        break;
                    case "Syncfusion":
                        IApplication application = excelEngine.Excel;
                        application.DefaultVersion = Syncfusion.XlsIO.ExcelVersion.Xlsx;
                        using (var inputStream = new FileStream(tempExcelPath, FileMode.Open, FileAccess.Read))
                        {
                            IWorkbook workbook = application.Workbooks.Open(inputStream);

                            XlsIORenderer renderer = new XlsIORenderer();
                            using (Syncfusion.Pdf.PdfDocument pdfDocument = renderer.ConvertToPDF(workbook))
                            {
                                using (var outputStream = new FileStream(tempPdfPath, FileMode.Create, FileAccess.Write))
                                {
                                    pdfDocument.Save(outputStream);
                                }
                            }

                            workbook.Close();
                        }
                        break;
                    case "Spire":
                        Spire.Xls.Workbook sp_workbook = new Spire.Xls.Workbook();

                        sp_workbook.LoadFromFile(tempExcelPath);


                        sp_workbook.SaveToFile(tempPdfPath, FileFormat.PDF);

                        sp_workbook.Dispose();
                        break;
                    case "iLovePdf":

                        string PUBLIC_KEY = "project_public_042fee95ec4cfa54e6615bfee85bbf1a_PWc1D71b0db93b2e7813646344804281ede61";
                        string SECRET_KEY = "secret_key_92a3294ffa587254e322c7aaa2117478_Yr9esbd5822f6bb3cce90159960fc076e453e";
                        var api = new iLovePdfApi(PUBLIC_KEY, SECRET_KEY);
                        var taskConvertOffice = api.CreateTask<OfficeToPdfTask>();
                        var file1 = taskConvertOffice.AddFile(tempExcelPath);

                        taskConvertOffice.Process();
                        taskConvertOffice.DownloadFile(Path.GetTempPath());
                        break;

                }

                // 讀取 PDF 進記憶體
                byte[] pdfBytes = File.ReadAllBytes(tempPdfPath);
                pdfStream = new MemoryStream(pdfBytes);
            }            
        }
        finally
        {
            // 刪除暫存檔案
            if (File.Exists(tempExcelPath)) File.Delete(tempExcelPath);
            if (File.Exists(tempPdfPath)) File.Delete(tempPdfPath);
        }
        
        pdfStream.Position = 0;

        return pdfStream;
    }

    public async Task<IEnumerable<User>> ImportUserFile(IFormFile file)
    {
        var importedData = new List<User>();
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    return null;
                }

                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                var columnMap = new Dictionary<string, int>();
                for (int col = 1; col <= colCount; col++)
                {
                    var header = worksheet.Cells[1, col].Text.Trim();
                    if (!string.IsNullOrEmpty(header))
                        columnMap[header.ToLower()] = col;
                }

                for (int row = 2; row <= rowCount; row++) // 資料列從第2列開始
                {
                    string username = columnMap.ContainsKey("name") ? worksheet.Cells[row, columnMap["name"]].Text.Trim() : null;
                    string email = columnMap.ContainsKey("email") ? worksheet.Cells[row, columnMap["email"]].Text.Trim() : null;

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
                        continue;

                    importedData.Add(new User
                    {
                        Username = username,
                        Email = email
                    });
                }

            }
        }

        return importedData;
    }

    public async Task<IEnumerable<TransactionViewModel>> ImportTransactionFile(IFormFile file)
    {
        var importedData = new List<TransactionViewModel>();
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    return null;
                }

                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                var columnMap = new Dictionary<string, int>();
                for (int col = 1; col <= colCount; col++)
                {
                    var header = worksheet.Cells[1, col].Text.Trim().ToLower();
                    if (!string.IsNullOrEmpty(header))
                        columnMap[header] = col;
                }
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        string title = columnMap.ContainsKey("title") ? worksheet.Cells[row, columnMap["title"]].Text.Trim() : null;
                        decimal amount = columnMap.ContainsKey("amount") ? decimal.Parse(worksheet.Cells[row, columnMap["amount"]].Text.Trim()) : 0;
                        DateTime date = columnMap.ContainsKey("date") ? DateTime.Parse(worksheet.Cells[row, columnMap["date"]].Text.Trim()) : DateTime.Now;
                        string username = columnMap.ContainsKey("user") ? worksheet.Cells[row, columnMap["user"]].Text.Trim() : null;

                        if (string.IsNullOrEmpty(title) || amount == 0 || string.IsNullOrEmpty(username))
                            continue;
                        WebApp.Models.TransactionModel.Category category = WebApp.Models.TransactionModel.Category.Others; // 預設值
                        if (columnMap.ContainsKey("category"))
                        {
                            string categoryStr = worksheet.Cells[row, columnMap["category"]].Text.Trim();
                            if (Enum.TryParse<WebApp.Models.TransactionModel.Category>(categoryStr, true, out WebApp.Models.TransactionModel.Category parsedCategory))
                            {
                                category = parsedCategory;
                            }
                        }

                        var isIncome = category == WebApp.Models.TransactionModel.Category.Work;


                        var user = await _userservice.GetUserByUsername(username);
                        if (user == null) continue;

                        importedData.Add(new TransactionViewModel
                        {
                            Title = title,
                            Amount = amount,
                            Date = date,
                            Category = category,
                            UserName = username
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                }
            }
        }
        return importedData;
    }

    public async Task<MemoryStream> GenerateUserReportAsync(User user, List<Transaction> transactions)
    {
        using var ms = new MemoryStream();
        using (var wordDocument = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            // 建立主要文件部分
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            Body body = new Body();

            // 標題
            DocumentFormat.OpenXml.Wordprocessing.Paragraph title = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Center }),
                new Run(new RunProperties(new Bold(), new DocumentFormat.OpenXml.Wordprocessing.FontSize() { Val = "40" }),
                    new Text($"User Report - {user.Username}"))
            );
            body.Append(title);

            // Email
            DocumentFormat.OpenXml.Wordprocessing.Paragraph email = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new Run(new Text($"Email: {user.Email}"))
            );
            body.Append(email);

            // Transaction Summary
            body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new Run(new RunProperties(new Bold(), new DocumentFormat.OpenXml.Wordprocessing.FontSize() { Val = "28" }),
                    new Text("Transaction Summary"))
            ));

            decimal income = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            decimal expense = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
            decimal net = income - expense;

            body.Append(CreateParagraph($"Income: {income.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}"));
            body.Append(CreateParagraph($"Expense: {expense.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}"));
            body.Append(CreateParagraph($"Net: {net.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}", bold: true));

            body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new Run(new RunProperties(new Bold(), new DocumentFormat.OpenXml.Wordprocessing.FontSize() { Val = "28" }),
                    new Text("Transaction List"))
            ));

            // 建立表格
            DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table(
                new TableProperties(
                    new TableBorders(
                        new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                    )
                )
            );

            // 表頭
            TableRow header = new TableRow();
            header.Append(CreateTableCell("Title", true));
            header.Append(CreateTableCell("Date", true));
            header.Append(CreateTableCell("Category", true));
            header.Append(CreateTableCell("Amount", true));
            header.Append(CreateTableCell("Type", true));
            table.Append(header);

            // 資料列
            foreach (var t in transactions)
            {
                TableRow row = new TableRow();
                row.Append(CreateTableCell(t.Title ?? ""));
                row.Append(CreateTableCell(t.Date.ToString("yyyy-MM-dd")));
                row.Append(CreateTableCell(t.Category.ToString()));
                row.Append(CreateTableCell(t.Amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"))));
                row.Append(CreateTableCell(t.IsIncome ? "Income" : "Expense"));
                table.Append(row);
            }

            body.Append(table);

            // 頁尾 (使用 SectionProperties)
            SectionProperties sectionProps = new SectionProperties(
                new FooterReference() { Type = HeaderFooterValues.Default, Id = "rIdFooter" }
            );
            body.Append(sectionProps);

            // 建立頁尾
            FooterPart footerPart = mainPart.AddNewPart<FooterPart>("rIdFooter");
            Footer footer = new Footer(
                new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                    new ParagraphProperties(new Justification() { Val = JustificationValues.Right }),
                    new Run(new Text($"Generated at {DateTime.Now:yyyy-MM-dd HH:mm}"))
                )
            );
            footerPart.Footer = footer;
            footerPart.Footer.Save();

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }

        ms.Position = 0;
        return await Task.FromResult(ms);
    }

    private DocumentFormat.OpenXml.Wordprocessing.Paragraph CreateParagraph(string text, bool bold = false)
    {
        RunProperties props = new RunProperties();
        if (bold) props.Append(new Bold());
        return new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new Run(props, new Text(text)));
    }

    private TableCell CreateTableCell(string text, bool bold = false)
    {
        RunProperties props = new RunProperties();
        if (bold) props.Append(new Bold());
        return new TableCell(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new Run(props, new Text(text))));
    }

    public async Task<MemoryStream> GenerateUserReportFromTemplate(string templatePath, User user, List<Transaction> transactions)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            // 將模板複製到記憶體，避免修改原檔
            using (FileStream fileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(memoryStream);
            }

            decimal income = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            decimal expense = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
            decimal net = income - expense;

            var placeholders = new Dictionary<string, string>
            {
                {"{USERNAME}", user.Username},
                {"{EMAIL}", user.Email},
                {"{TOTAL_INCOME}", income.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"))},
                {"{TOTAL_EXPENSE}", expense.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"))},
                {"{NET}", net.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}
            };

            var ms = ReplacePlaceholder(placeholders, memoryStream);
            var ms_table = GenerateTable(transactions, ms);


            return ms;
        }
    }

    private MemoryStream ReplacePlaceholder(Dictionary<string, string> placeholders, MemoryStream ms)
    {
        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(ms, true))
        {
            var body = wordDoc.MainDocumentPart.Document.Body;


            foreach (var paragraph in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                // 將段落中所有 Run 的文字連成一個完整字串
                var runs = paragraph.Elements<Run>().ToList();
                var fullText = string.Join("", runs.Select(r => r.GetFirstChild<Text>()?.Text ?? ""));

                // 搜尋佔位符
                foreach (var placeholder in placeholders)
                {
                    runs = paragraph.Elements<Run>().ToList();
                    fullText = string.Join("", runs.Select(r => r.GetFirstChild<Text>()?.Text ?? ""));
                    int index = fullText.IndexOf(placeholder.Key);
                    if (index >= 0)
                    {
                        // 記錄每個 Run 的範圍
                        int currentPos = 0;
                        foreach (var run in runs)
                        {
                            var textElement = run.GetFirstChild<Text>();
                            if (textElement == null) continue;

                            string runText = textElement.Text;
                            int runStart = currentPos;
                            int runEnd = currentPos + runText.Length;

                            // 檢查佔位符是否在這個 Run 中
                            int placeholderStartInRun = index - runStart;
                            int placeholderEndInRun = index + placeholder.Key.Length - runStart;

                            if (index + placeholder.Key.Length < runStart || index >= runEnd)
                            {
                                currentPos += runText.Length;
                                continue; // 佔位符不在這個 Run
                            }
                            if (placeholderStartInRun < 0 && index + placeholder.Key.Length <= runEnd)
                            {
                                textElement.Text = runText.Substring(placeholderEndInRun);//佔位符在這個 Run 結束
                            }
                            else if (placeholderStartInRun < 0 && index + placeholder.Key.Length > runEnd)
                            {
                                textElement.Text = "";//整個 Run 都是佔位符
                            }
                            else if (placeholderEndInRun >= runText.Length)
                            {
                                string before = runText.Substring(0, placeholderStartInRun);
                                textElement.Text = before + placeholder.Value;//佔位符從這個 Run 開始且沒結束
                            }
                            else if (placeholderEndInRun < runText.Length)
                            {
                                string before = runText.Substring(0, placeholderStartInRun);
                                string after = runText.Substring(placeholderEndInRun);

                                textElement.Text = before + placeholder.Value + after;//佔位符從這個 Run 開始並結束
                            }

                            currentPos += runText.Length;
                        }
                    }
                }
            }
            wordDoc.MainDocumentPart.Document.Save();
        }
        return ms;
    }

    private MemoryStream GenerateTable(List<Transaction> transactions, MemoryStream ms)
    {
        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(ms, true))
        {
            MainDocumentPart mainPart = wordDoc.MainDocumentPart;

            // 找到第一個表格（可依需求更精準定位）
            DocumentFormat.OpenXml.Wordprocessing.Table table = mainPart.Document.Body.Elements<DocumentFormat.OpenXml.Wordprocessing.Table>().FirstOrDefault();
            if (table != null)
            {
                // 找到範例資料列 (假設表格第二列為範本列)
                TableRow templateRow = table.Elements<TableRow>().Skip(1).FirstOrDefault();
                if (templateRow != null)
                {
                    foreach (var transaction in transactions)
                    {
                        TableRow newRow = (TableRow)templateRow.Clone();

                        var cells = newRow.Elements<TableCell>().ToList();

                        SetCellText(cells[0], transaction.Title);
                        SetCellText(cells[1], transaction.Date.ToString("yyyy-MM-dd"));
                        SetCellText(cells[2], transaction.Category.ToString());
                        SetCellText(cells[3], transaction.Amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US")));
                        SetCellText(cells[4], transaction.IsIncome ? "Income" : "Expense");

                        table.AppendChild(newRow);
                    }

                    templateRow.Remove();
                }
            }
            mainPart.Document.Save();
        }

        return ms;
    }

    private void SetCellText(TableCell cell, string text)
    {
        var paragraph = cell.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().FirstOrDefault();
        if (paragraph == null)
        {
            paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            cell.Append(paragraph);
        }

        var runs = paragraph.Elements<Run>().Skip(1).ToList();
        foreach (var run in runs)
        {
            run.Remove();
        }

        var frun = paragraph.Elements<Run>().FirstOrDefault();
        if (frun == null)
        {
            frun = new Run();
            paragraph.Append(frun);
        }


        var textElement = frun.Elements<Text>().FirstOrDefault();
        if (textElement == null)
        {
            textElement = new Text();
            frun.Append(textElement);
        }
        textElement.Text = text;
    }

    public async Task<MemoryStream> UserReportPdf(User user, List<Transaction> transactions, bool watermark)
    {
        var word_ms = await GenerateUserReportAsync(user, transactions);
        if (watermark)
        {
            var ms = await ConvertPDF(word_ms);
            return ms;
        }
        else
        {
            var ms = ConvertWordToPdf(word_ms);
            return ms;
        }

    }

    public async Task<MemoryStream> UserReportFromTemplatPdf(string template_path, User user, List<Transaction> transactions, bool watermark)
    {
        var word_ms = await GenerateUserReportFromTemplate(template_path, user, transactions);
        if (watermark)
        {
            var ms = await ConvertPDF(word_ms);
            return ms;
        }
        else
        {
            var ms = ConvertWordToPdf(word_ms);
            return ms;
        }
    }

    private MemoryStream ConvertWordToPdf(MemoryStream ms)
    {
        using (var word_ms = new MemoryStream(ms.ToArray()))
        {
            word_ms.Position = 0;

            var outputStream = new MemoryStream();
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(word_ms, false))
            {
                iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, outputStream);
                pdfDoc.Open();

                var body = wordDoc.MainDocumentPart.Document.Body;

                foreach (var element in body.Elements())
                {
                    if (element is DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph)
                    {
                        // 原本的段落處理
                        iTextSharp.text.Paragraph pdfParagraph = new iTextSharp.text.Paragraph();

                        foreach (var run in paragraph.Elements<Run>())
                        {
                            string text = run.InnerText;
                            if (string.IsNullOrEmpty(text))
                                continue;

                            RunProperties runProps = run.RunProperties;
                            iTextSharp.text.Font font = GetFontFromRun(runProps);

                            Chunk chunk = new Chunk(text, font);
                            pdfParagraph.Add(chunk);
                        }

                        pdfDoc.Add(pdfParagraph);
                    }
                    else if (element is DocumentFormat.OpenXml.Wordprocessing.Table table)
                    {
                        // 表格處理
                        var pdfTable = new PdfPTable(table.Elements<TableRow>().First()
                                                                        .Elements<TableCell>().Count());

                        foreach (var row in table.Elements<TableRow>())
                        {
                            foreach (var cell in row.Elements<TableCell>())
                            {
                                string cellText = string.Join("\n", cell.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
                                                                        .Select(p => p.InnerText));

                                var pdfCell = new PdfPCell(new Phrase(cellText));
                                pdfTable.AddCell(pdfCell);
                            }
                        }

                        pdfDoc.Add(pdfTable);
                    }
                }
                pdfDoc.Close();
            }
            outputStream.Position = 0;
            return outputStream;
        }
    }

    private iTextSharp.text.Font GetFontFromRun(RunProperties runProps)
    {
        // 預設字型
        string fontName = "Times-Roman";
        float fontSize = 12;
        BaseColor fontColor = BaseColor.Black;
        int style = iTextSharp.text.Font.NORMAL;

        if (runProps != null)
        {
            // 粗體
            if (runProps.Bold != null)
                style |= iTextSharp.text.Font.BOLD;

            // 斜體
            if (runProps.Italic != null)
                style |= iTextSharp.text.Font.ITALIC;

            // 字型
            var runFont = runProps.RunFonts?.Ascii?.Value;
            if (!string.IsNullOrEmpty(runFont))
                fontName = runFont;

            // 顏色
            var color = runProps.Color?.Val?.Value;
            if (!string.IsNullOrEmpty(color))
            {
                try
                {
                    fontColor = new BaseColor(
                        Convert.ToInt32(color.Substring(0, 2), 16),
                        Convert.ToInt32(color.Substring(2, 2), 16),
                        Convert.ToInt32(color.Substring(4, 2), 16)
                    );
                }
                catch { }
            }

            // 字號
            var size = runProps.FontSize?.Val?.Value;
            if (!string.IsNullOrEmpty(size))
                fontSize = Convert.ToSingle(size) / 2; // Word 字號是 2 倍
        }

        return FontFactory.GetFont(fontName, fontSize, style, fontColor);
    }

    private async Task<MemoryStream> ConvertPDF(MemoryStream wordStream)
    {
        string PUBLIC_KEY = "project_public_042fee95ec4cfa54e6615bfee85bbf1a_PWc1D71b0db93b2e7813646344804281ede61";
        string SECRET_KEY = "secret_key_92a3294ffa587254e322c7aaa2117478_Yr9esbd5822f6bb3cce90159960fc076e453e";
        var api = new iLovePdfApi(PUBLIC_KEY, SECRET_KEY);

        // 建立暫存檔案路徑
        string tempWordPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.docx");
        string tempPdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

        // 將 MemoryStream 寫入暫存 Word 檔案
        File.WriteAllBytes(tempWordPath, wordStream.ToArray());

        MemoryStream pdfStream = new MemoryStream();

        try
        {

            var taskConvertOffice = api.CreateTask<OfficeToPdfTask>();
            var file1 = taskConvertOffice.AddFile(tempWordPath);

            taskConvertOffice.Process();
            byte[] pdfBytes = await taskConvertOffice.DownloadFileAsByteArrayAsync();

            pdfStream = new MemoryStream(pdfBytes);
            pdfStream.Position = 0;
        }
        finally
        {

            // 刪除暫存檔案
            if (File.Exists(tempWordPath)) File.Delete(tempWordPath);
            if (File.Exists(tempPdfPath)) File.Delete(tempPdfPath);
        }

        return pdfStream;
    }

    public async Task SendUserReport(User user, List<Transaction> transactions)
    {
        var word_report = await GenerateUserReportAsync(user, transactions);
        var pdf_report = await UserReportPdf(user, transactions, false);

        string tempWordPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.docx");
        string tempPdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

        File.WriteAllBytes(tempWordPath, word_report.ToArray());
        File.WriteAllBytes(tempPdfPath, pdf_report.ToArray());

        var files = new List<string>();
        files.Add(tempWordPath);
        files.Add(tempPdfPath);

        await _mailservice.SendEmailAsync(files, user.Email, user.Username);
    }
}