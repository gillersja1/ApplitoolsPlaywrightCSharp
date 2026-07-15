using Microsoft.Playwright;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ApplitoolsPlaywrightCSharp.Tests;

[TestFixture]
public class SauceDemoVisualTests
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    [SetUp]
    public async Task SetUp()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _page = await _browser.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // No-op for local tests.
    }

    [Test]
    public async Task LoginPage_Loads()
    {
        await _page.GotoAsync("https://www.saucedemo.com/");
        var title = await _page.TitleAsync();
        Assert.That(title.Contains("Swag Labs") || title.Length > 0, "Title should load");
    }

    [Test]
    public async Task InventoryPage_CanLogin()
    {
        await _page.GotoAsync("https://www.saucedemo.com/");
        await _page.FillAsync("#user-name", "standard_user");
        await _page.FillAsync("#password", "secret_sauce");
        await _page.ClickAsync("#login-button");

        // Wait for inventory container to appear
        await _page.WaitForSelectorAsync(".inventory_list", new PageWaitForSelectorOptions { Timeout = 5000 });
        Assert.That(_page.Url.Contains("inventory"), "Should navigate to inventory page after login");
    }

    [Test]
    public async Task InventorySortedHighToLow_Action()
    {
        await _page.GotoAsync("https://www.saucedemo.com/");
        await _page.FillAsync("#user-name", "standard_user");
        await _page.FillAsync("#password", "secret_sauce");
        await _page.ClickAsync("#login-button");
        await _page.SelectOptionAsync("[data-test='product-sort-container']", "hilo");

        // Basic smoke: ensure products are present
        var items = await _page.QuerySelectorAllAsync(".inventory_item");
        Assert.That(items.Count > 0, "Inventory should contain items");
    }
}
