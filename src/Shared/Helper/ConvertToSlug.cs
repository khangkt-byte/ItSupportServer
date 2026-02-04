namespace ItSupportServer.src.Shared.Helper
{
    public class ConvertToSlug
    {
        public static string GetSlug(string s)
        {
            s = s.ToLowerInvariant();

            // Chuẩn hóa Unicode
            s = s.Normalize(System.Text.NormalizationForm.FormD);

            // Loại bỏ dấu (accents)
            var chars = s.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                                      != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
            s = new string(chars).Normalize(System.Text.NormalizationForm.FormC);

            // Thay 'đ' và 'Đ'
            s = s.Replace('đ', 'd').Replace('Đ', 'd');

            // Xóa ký tự không hợp lệ
            s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9\s-]", "");

            // Thay khoảng trắng thành gạch ngang
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", "-").Trim();

            // Gộp nhiều gạch ngang liên tiếp
            s = System.Text.RegularExpressions.Regex.Replace(s, @"-+", "-");

            return s;
        }
    }
}
