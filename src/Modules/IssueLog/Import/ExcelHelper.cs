using ClosedXML.Excel;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Excel utility helper
    /// Pattern: Static utility class
    /// Purpose: Reusable Excel operations
    /// </summary>
    public static class ExcelHelper
    {
        /// <summary>
        /// Get cell string safely (handles empty cells)
        /// </summary>
        public static string GetCellString(IXLRow row, int columnNumber)
        {
            try
            {
                return row.Cell(columnNumber).GetString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Try to get DateTime from cell
        /// </summary>
        public static DateTime? TryGetDateTime(IXLCell cell)
        {
            try
            {
                return cell.GetDateTime();
            }
            catch
            {
                if (DateTime.TryParse(cell.GetString(), out var date))
                    return date;
                return null;
            }
        }

        /// <summary>
        /// Set standard header row for issue log Excel
        /// </summary>
        public static void SetHeaderRow(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "Người thực hiện";
            worksheet.Cell(1, 2).Value = "Người yêu cầu";
            worksheet.Cell(1, 3).Value = "Bộ phận";
            worksheet.Cell(1, 4).Value = "Công ty";
            worksheet.Cell(1, 5).Value = "Chi nhánh";
            worksheet.Cell(1, 6).Value = "Mô tả sự cố";
            worksheet.Cell(1, 7).Value = "Nguyên nhân";
            worksheet.Cell(1, 8).Value = "Cách xử lý";
            worksheet.Cell(1, 9).Value = "Giải pháp lâu dài";
            worksheet.Cell(1, 10).Value = "Ngày báo cáo";
            worksheet.Cell(1, 11).Value = "Trạng thái";

            var headerRange = worksheet.Range(1, 1, 1, 11);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        /// <summary>
        /// Remove Vietnamese accents for fuzzy matching
        /// Pattern: Unicode normalization (W3C standard)
        /// </summary>
        public static string RemoveVietnameseAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var result = new string(normalized
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                    System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray());
            return result.Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}