using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationTask.Pages
{
    public class WikipediaPlaywrightPage
    {
        private readonly IPage _page;
        private readonly string _htmlElement = "html";

        public WikipediaPlaywrightPage(IPage page)
        {
            _page = page;
        }

        public async Task GotoAsync()
        {
            await _page.SetViewportSizeAsync(1920, 1080);
            await _page.GotoAsync(
                "https://en.wikipedia.org/wiki/Playwright_(software)?useskin=vector-2022",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }
            );
        }

        // Task 1: חילוץ טקסט של סקשן "Debugging features" מה-UI
        // מחזיר רק את התוכן הישיר של הסקשן (עד ה-sub-section הראשון) — בדיוק כמו ה-API
        public async Task<string> GetDebuggingFeaturesTextAsync()
        {
            var text = await _page.EvaluateAsync<string>(@"() => {
                let heading = document.getElementById('Debugging_features');
                if (!heading) return '';

                // ה-H3 נמצא בתוך div.mw-heading.mw-heading3
                let sectionHeadingDiv = heading.parentElement;
                let textContent = '';
                let nextElem = sectionHeadingDiv.nextElementSibling;

                while (nextElem) {
                    // עצור ב-sub-section הבא (mw-heading3) או בסקשן ראשי (mw-heading2)
                    if (nextElem.classList.contains('mw-heading3') ||
                        nextElem.classList.contains('mw-heading2')) break;

                    // דלג על navbox, reflist, noprint, toc
                    if (
                        nextElem.classList.contains('navbox') ||
                        nextElem.classList.contains('reflist') ||
                        nextElem.classList.contains('noprint') ||
                        nextElem.classList.contains('toc')
                    ) {
                        nextElem = nextElem.nextElementSibling;
                        continue;
                    }

                    textContent += nextElem.innerText + ' ';
                    nextElem = nextElem.nextElementSibling;
                }

                return textContent.trim();
            }");
            return text ?? "";
        }

        // Task 2: שליפת כל הטכנולוגיות תחת "Testing and debugging" ב-navbox של Microsoft development tools
        public ILocator GetMicrosoftDevToolsElements()
        {
            return _page.Locator("[id*='Microsoft_development_tools']")
                        .Locator("xpath=ancestor::table")
                        .Locator("th.navbox-group:has-text('debugging') ~ td li");
        }

        // Task 3: פתיחת תפריט ה-Appearance על ידי לחיצה על ה-checkbox
        public async Task OpenAppearanceMenuAsync()
        {
            var checkbox = _page.Locator("#vector-appearance-dropdown-checkbox");
            await checkbox.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
            await checkbox.EvaluateAsync("el => el.click()");
            await _page.WaitForTimeoutAsync(300);
        }

        // Task 3: בחירה ב-Dark mode
        public async Task SetDarkModeAsync()
        {
            var radio = _page.Locator("#skin-client-pref-skin-theme-value-night");
            await radio.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });
            await radio.EvaluateAsync("el => el.click()");
            await _page.WaitForTimeoutAsync(500);
        }

        public async Task<string> GetHtmlThemeClassAsync()
        {
            await _page.WaitForTimeoutAsync(1500);
            return await _page.GetAttributeAsync(_htmlElement, "class") ?? "";
        }
    }
}