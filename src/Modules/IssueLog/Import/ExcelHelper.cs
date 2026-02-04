using ClosedXML.Excel;

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
        /// Try to get DateTime from cell
        /// Pattern: Try-Parse pattern (C# convention)
        /// Reference: Microsoft Framework Design Guidelines
        /// </summary>
        public static DateTime? TryGetDateTime(IXLCell cell)
        {
            try
            {
                // Prefer Excel DateTime format
                if (cell.DataType == XLDataType.DateTime)
                    return cell.GetDateTime();

                // Fallback to string parsing
                return cell.GetString() is string str && DateTime.TryParse(str, out var date)
                    ? date
                    : null;
            }
            catch
            {
                return null;
            }
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
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }
}