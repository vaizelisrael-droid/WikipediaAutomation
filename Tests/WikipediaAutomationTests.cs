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

            TestContext.WriteLine("=== Task 1: Debugging Features Section ===");
            TestContext.WriteLine($"UI Text:\n{uiText}\n");
            TestContext.WriteLine($"API Text:\n{apiText}\n");
            TestContext.WriteLine($"UI Unique Word Count:  {uiWordCount}");
            TestContext.WriteLine($"API Unique Word Count: {apiWordCount}");

            Assert.That(uiWordCount, Is.EqualTo(apiWordCount),
                $"UI word count ({uiWordCount}) does not match API word count ({apiWordCount}).");

            TestContext.WriteLine("✔ Both counts are equal.");
        }

        [Test]
        public async Task Task2_VerifyTechnologyNamesAreLinks()
        {
            await _wikiPage.GotoAsync();

            var items = _wikiPage.GetMicrosoftDevToolsElements();
            await items.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

            int count = await items.CountAsync();
            Assert.That(count, Is.GreaterThan(0), "No technology items found in the section.");

            TestContext.WriteLine("=== Task 2: Microsoft Development Tools - Testing and Debugging ===");

            for (int i = 0; i < count; i++)
            {
                var item = items.Nth(i);
                var link = item.Locator("a[href]");
                int linkCount = await link.CountAsync();
                string text = await item.InnerTextAsync();

                if (linkCount > 0)
                {
                    string href = await link.First.GetAttributeAsync("href") ?? "";
                    TestContext.WriteLine($"  ✔ [{i}] '{text.Trim()}' — is a link: {href}");
                }
                else
                {
                    TestContext.WriteLine($"  ✘ [{i}] '{text.Trim()}' — is NOT a link");
                }

                Assert.That(linkCount, Is.GreaterThan(0),
                    $"Technology '{text.Trim()}' at index {i} is not a text link.");
            }
        }

        [Test]
        public async Task Task3_VerifyDarkModeActivation()
        {
            await _wikiPage.GotoAsync();

            TestContext.WriteLine("=== Task 3: Dark Mode Activation ===");
            TestContext.WriteLine("Opening appearance menu...");

            await _wikiPage.OpenAppearanceMenuAsync();
            await _wikiPage.SetDarkModeAsync();

            string htmlClass = await _wikiPage.GetHtmlThemeClassAsync();

            TestContext.WriteLine($"HTML class after change: {htmlClass}");

            Assert.That(htmlClass, Does.Contain("night"), "The page color theme did not change to dark mode.");

            TestContext.WriteLine("✔ Dark mode activated successfully.");
        }
    }
}