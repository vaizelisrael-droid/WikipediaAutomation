using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;
using AutomationTask.Pages;
using AutomationTask.Clients;
using AutomationTask.Helpers;
using Microsoft.Playwright;

namespace AutomationTask.Tests
{
    [TestFixture]
    public class WikipediaAutomationTests : PageTest
    {
        private WikipediaPlaywrightPage _wikiPage;
        private WikipediaApiClient _apiClient;

        [SetUp]
        public void Setup()
        {
            _wikiPage = new WikipediaPlaywrightPage(Page);
            _apiClient = new WikipediaApiClient();
        }

        [Test]
        public async Task Task1_VerifyUniqueWordCountsAreEqual()
        {
            await _wikiPage.GotoAsync();
            string uiText = await _wikiPage.GetDebuggingFeaturesTextAsync();
            string apiText = await _apiClient.GetSectionTextViaApiAsync();

            int uiWordCount = TextHelper.CountUniqueWords(uiText);
            int apiWordCount = TextHelper.CountUniqueWords(apiText);

            Assert.That(uiWordCount, Is.EqualTo(apiWordCount),
                $"UI word count ({uiWordCount}) does not match API word count ({apiWordCount}).");
        }

        [Test]
        public async Task Task2_VerifyTechnologyNamesAreLinks()
        {
            await _wikiPage.GotoAsync();

            var items = _wikiPage.GetMicrosoftDevToolsElements();
            await items.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

            int count = await items.CountAsync();
            Assert.That(count, Is.GreaterThan(0), "No technology items found in the section.");

            for (int i = 0; i < count; i++)
            {
                var item = items.Nth(i);
                // כל פריט צריך להכיל קישור עם href — אם לא, הטסט נכשל
                var link = item.Locator("a[href]");
                int linkCount = await link.CountAsync();
                string text = await item.InnerTextAsync();

                Assert.That(linkCount, Is.GreaterThan(0),
                    $"Technology '{text.Trim()}' at index {i} is not a text link.");
            }
        }

        [Test]
        public async Task Task3_VerifyDarkModeActivation()
        {
            await _wikiPage.GotoAsync();

            await _wikiPage.OpenAppearanceMenuAsync();
            await _wikiPage.SetDarkModeAsync();

            string htmlClass = await _wikiPage.GetHtmlThemeClassAsync();

            Assert.That(htmlClass, Does.Contain("night"), "The page color theme did not change to dark mode.");
        }
    }
}