using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutomationTask.Helpers
{
    public static class TextHelper
    {
        public static int CountUniqueWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;

            // נרמול הטקסט (הורדת סימני פיסוק ואותיות קטנות)
            string normalized = text.ToLowerInvariant();
            
            // Remove special characters but keep spaces
            normalized = Regex.Replace(normalized, @"[^\w\s\-']", " ");
            
            // Replace multiple spaces with single space
            normalized = Regex.Replace(normalized, @"\s+", " ");
            
            // Remove common Wikipedia elements and references
            normalized = Regex.Replace(normalized, @"\[\d+\]", " "); // Remove [123] references
            normalized = Regex.Replace(normalized, @"citation needed", "", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\(.*?edit.*?\)", " ", RegexOptions.IgnoreCase);
            
            // Split into words and remove empty entries
            var words = normalized.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 1) // Remove single characters
                .ToList();
            
            // Get unique words
            return words.Distinct(StringComparer.OrdinalIgnoreCase).Count();
        }
    }
}