using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutomationTask.Clients
{
    public class WikipediaApiClient
    {
        private readonly HttpClient _httpClient;

        public WikipediaApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        public async Task<string> GetSectionTextViaApiAsync()
        {
            string url = "https://en.wikipedia.org/w/api.php?action=parse&page=Playwright_(software)&prop=text&section=5&format=json";

            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                var response = await _httpClient.GetAsync(url);

                if ((int)response.StatusCode == 429)
                {
                    if (attempt >= maxRetries)
                        throw new HttpRequestException("Rate limited by Wikipedia API after retries (429).");
                    int delayMs = (int)Math.Pow(2, attempt) * 1000;
                    await Task.Delay(delayMs);
                    continue;
                }

                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(body);
                var htmlContent = doc.RootElement
                    .GetProperty("parse")
                    .GetProperty("text")
                    .GetProperty("*")
                    .GetString();

                return StripHtml(htmlContent ?? "");
            }

            throw new InvalidOperationException("Failed to fetch API after retries.");
        }

        private string StripHtml(string input)
        {
            // הסר script ו-style
            input = System.Text.RegularExpressions.Regex.Replace(input,
                "<script[^>]*>.*?</script>", "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline);
            input = System.Text.RegularExpressions.Regex.Replace(input,
                "<style[^>]*>.*?</style>", "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline);

            // הסר כותרות סקשן (h2/h3) כולל edit links
            input = System.Text.RegularExpressions.Regex.Replace(input,
                "<h[23][^>]*>.*?</h[23]>", "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline);

            // הסר הערות שוליים — sup עם reference
            input = System.Text.RegularExpressions.Regex.Replace(input,
                "<sup[^>]*>.*?</sup>", "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline);

            // הסר את כל בלוק ה-references (ol, div)
            input = System.Text.RegularExpressions.Regex.Replace(input,
                "<ol[^>]*class=\"[^\"]*references[^\"]*\"[^>]*>.*?</ol>", "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline);
            input = System.Text.RegularExpressions.Regex.Replace(input,
                "<div[^>]*class=\"[^\"]*mw-references[^\"]*\"[^>]*>.*?</div>", "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline);
            input = System.Text.RegularExpressions.Regex.Replace(input,
                "<div[^>]*class=\"[^\"]*reflist[^\"]*\"[^>]*>.*?</div>", "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline);

            // הסר HTML comments
            input = System.Text.RegularExpressions.Regex.Replace(input,
                "<!--.*?-->", "",
                System.Text.RegularExpressions.RegexOptions.Singleline);

            // הסר HTML tags
            input = System.Text.RegularExpressions.Regex.Replace(input, "<[^>]+>", " ");

            // decode HTML entities
            input = System.Net.WebUtility.HtmlDecode(input);

            // הסר רווחים מיותרים
            input = System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ");

            return input.Trim();
        }
    }
}