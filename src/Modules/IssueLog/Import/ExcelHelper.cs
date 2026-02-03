using ClosedXML.Excel;
using System.Globalization;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Excel utility helper
    /// Pattern: Static utility class (Helper pattern)
    /// Purpose: Reusable Excel operations
    /// Security: Safe cell reading, error handling
    /// References:
    /// - ClosedXML Documentation
    /// - Microsoft Office Open XML SDK
    /// </summary>
    public static class ExcelHelper
    {
        // ✅ Danh sách các format date được hỗ trợ
        private static readonly string[] SupportedDateFormats =
        {
            // ISO 8601 (chuẩn quốc tế)
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy/MM/dd",
            
            // Vietnamese format (DD/MM/YYYY)
            "dd/MM/yyyy",
            "d/M/yyyy",
            "dd-MM-yyyy",
            "d-M-yyyy",
            
            // US format (MM/DD/YYYY)
            "MM/dd/yyyy",
            "M/d/yyyy",
            "MM-dd-yyyy",
            
            // Short formats
            "dd/MM/yy",
            "d/M/yy",
            "MM/dd/yy",
            
            // Excel text formats
            "dd.MM.yyyy",
            "yyyy.MM.dd"
        };

        /// <summary>
        /// Get cell string safely (handles empty cells, errors)
        /// Pattern: Null Object pattern
        /// Security: Prevents null reference exceptions
        /// </summary>
        public static string GetCellString(IXLRow row, int columnNumber)
        {
            try
            {
                return row.Cell(columnNumber).GetString().Trim();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Try to get DateTime from cell with flexible parsing
        /// Pattern: Try-Parse pattern (C# convention)
        /// Security: Multiple format support, prevents parsing errors
        /// Reference: Microsoft Framework Design Guidelines
        /// 
        /// Supports:
        /// - Excel DateTime (OADate) - Priority 1
        /// - ISO 8601: YYYY-MM-DD
        /// - Vietnamese: DD/MM/YYYY, DD-MM-YYYY
        /// - US: MM/DD/YYYY
        /// - Short: DD/MM/YY
        /// </summary>
        public static DateTime? TryGetDateTime(IXLCell cell)
        {
            try
            {
                // ✅ PRIORITY 1: Excel native DateTime (most reliable)
                if (cell.DataType == XLDataType.DateTime)
                {
                    return cell.GetDateTime();
                }

                // ✅ PRIORITY 2: Excel stored as number (OLE Automation Date)
                if (cell.DataType == XLDataType.Number)
                {
                    try
                    {
                        double oaDate = cell.GetDouble();
                        
                        // Validate range (Excel dates: 1900-01-01 to 9999-12-31)
                        if (oaDate >= 0 && oaDate <= 2958465.99999) // Max OADate
                        {
                            return DateTime.FromOADate(oaDate);
                        }
                    }
                    catch
                    {
                        // Not a valid OADate, continue to string parsing
                    }
                }

                // ✅ PRIORITY 3: String parsing with multiple formats
                var cellValue = cell.GetString()?.Trim();
                
                if (string.IsNullOrWhiteSpace(cellValue))
                    return null;

                return TryParseFlexibleDate(cellValue);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parse date string với nhiều format được hỗ trợ
        /// Pattern: Strategy pattern (try multiple parsers)
        /// </summary>
        public static DateTime? TryParseFlexibleDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            dateString = dateString.Trim();

            // Strategy 1: Try exact format matching
            if (DateTime.TryParseExact(
                dateString,
                SupportedDateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var exactDate))
            {
                return exactDate;
            }

            // Strategy 2: Try smart parsing (Vietnamese culture priority)
            if (DateTime.TryParse(dateString, new CultureInfo("vi-VN"), DateTimeStyles.None, out var vnDate))
            {
                return vnDate;
            }

            // Strategy 3: Try invariant culture parsing
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var invDate))
            {
                return invDate;
            }

            // Strategy 4: Handle special cases (e.g., "03022025" -> "03/02/2025")
            if (dateString.Length == 8 && long.TryParse(dateString, out _))
            {
                // Try DDMMYYYY format
                if (DateTime.TryParseExact(
                    dateString,
                    "ddMMyyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var compactDate))
                {
                    return compactDate;
                }
            }

            return null;
        }

        /// <summary>
        /// Try to get DateOnly from cell (for .NET 6+)
        /// </summary>
        public static DateOnly? TryGetDateOnly(IXLCell cell)
        {
            var dateTime = TryGetDateTime(cell);
            return dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
        }

        /// <summary>
        /// Validate date và trả về error message nếu invalid
        /// </summary>
        public static (DateTime? date, string? error) ValidateAndParseDate(IXLCell cell, int rowNumber, string fieldName = "Ngày báo cáo")
        {
            var date = TryGetDateTime(cell);
            
            if (!date.HasValue)
            {
                var cellValue = cell.GetString()?.Trim();
                
                if (string.IsNullOrWhiteSpace(cellValue))
                {
                    return (null, $"Row {rowNumber}: {fieldName} là bắt buộc");
                }
                
                return (null, $"Row {rowNumber}: {fieldName} không hợp lệ: '{cellValue}'. Hỗ trợ: DD/MM/YYYY, MM/DD/YYYY, YYYY-MM-DD");
            }

            // Validate date range
            if (date.Value.Year < 1900 || date.Value.Year > 9999)
            {
                return (null, $"Row {rowNumber}: {fieldName} năm không hợp lệ: {date.Value.Year}");
            }

            // Warning for future dates
            if (date.Value > DateTime.UtcNow.AddDays(1))
            {
                return (date, $"Row {rowNumber}: Cảnh báo - {fieldName} trong tương lai: {date.Value:dd/MM/yyyy}");
            }

            return (date, null);
        }

        /// <summary>
        /// Try to get integer from cell
        /// </summary>
        public static int? TryGetInt(IXLCell cell)
        {
            try
            {
                if (cell.DataType == XLDataType.Number)
                    return (int)cell.GetDouble();

                return int.TryParse(cell.GetString(), out var number) ? number : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Set standard header row for issue log Excel
        /// Pattern: Template method
        /// Purpose: Consistent Excel format
        /// </summary>
        public static void SetHeaderRow(IXLWorksheet worksheet)
        {
            var headers = new[]
            {
                "Người thực hiện",
                "Người yêu cầu",
                "Bộ phận",
                "Công ty",
                "Chi nhánh",
                "Mô tả sự cố",
                "Nguyên nhân",
                "Cách xử lý",
                "Giải pháp lâu dài",
                "Ngày báo cáo",
                "Trạng thái"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // ✅ Add data validation comment for date column
            var dateHeaderCell = worksheet.Cell(1, 10); // Column 10 = Ngày báo cáo
            var comment = dateHeaderCell.CreateComment();
            comment.AddText("Định dạng hỗ trợ:\n" +
                           "- DD/MM/YYYY (ví dụ: 03/02/2025)\n" +
                           "- MM/DD/YYYY (ví dụ: 02/03/2025)\n" +
                           "- YYYY-MM-DD (ví dụ: 2025-02-03)");
            comment.Style.Alignment.SetAutomaticSize();
        }
    }
}