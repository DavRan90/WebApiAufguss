using System.Text.RegularExpressions;

namespace WebApi.Models
{
    public static class PhoneHelper
    {
        public static string NormalizeSwedishPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;

            phone = Regex.Replace(phone, @"[\s\-\(\)]", "");
            if (phone.StartsWith("00")) phone = "+" + phone[2..];
            else if (phone.StartsWith("0")) phone = "+46" + phone[1..];

            return phone;
        }
    }

}
