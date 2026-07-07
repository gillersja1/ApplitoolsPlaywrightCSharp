using Applitools;
using Applitools.Playwright;
using Applitools.VisualGrid;
using Microsoft.Playwright;
using NUnit.Framework;

namespace ApplitoolsPlaywrightCSharp.Tests;

/// <summary>
/// Visual regression tests for saucedemo.com using Applitools Eyes.
///
/// Unlike pixel-diffing tools, Applitools uses AI-based visual comparison
/// (Applitools "Visual AI") to detect meaningful visual differences while
/// ignoring noise like anti-aliasing or minor rendering variance.
///
/// Set <see cref="UseUltrafastGrid"/> to true to render each checkpoint
/// against multiple browsers/viewports in Applitools' cloud (Ultrafast
/// Grid) from a single local page load, or false to use the Classic
/// runner, which only checks the exact browser/viewport under test.
/// </summary>
[TestFixture]
public class SauceDemoVisualTests
{
    private const bool UseUltrafastGrid = true;

    private static readonly BatchInfo Batch = new("SauceDemo Playwright Visual Suite");

    private static readonly EyesRunner Runner = UseUltrafastGrid
        ? new VisualGridRunner(new RunnerOptions().TestConcurrency(5))
        : new ClassicRunner();

    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private Eyes _eyes = null!;

    [SetUp]
    public async Task SetUp()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _page = await _browser.NewPageAsync();

        _eyes = new Eyes(Runner);

        var config = _eyes.GetConfiguration();
        config.SetAppName("SauceDemo Visual Tests");
        config.SetBatch(Batch);

        if (UseUltrafastGrid)
        {
            // Each of these renders the same checkpoint against a different
            // browser/viewport in the cloud, without needing a local install
            // of Firefox/Safari or a device lab.
            config.AddBrowser(1200, 800, BrowserType.CHROME);
            config.AddBrowser(1200, 800, BrowserType.FIREFOX);
            config.AddBrowser(1024, 768, BrowserType.SAFARI);
            config.AddDeviceEmulation(DeviceName.iPhone_X);
        }

        _eyes.SetConfiguration(config);

        _page = await _eyes.OpenAsync(
            _page,
            "SauceDemo Visual Tests",
            TestContext.CurrentContext.Test.Name,
            new RectangleSize(1200, 800));
    }

    [TearDown]
    public async Task TearDown()
    {
        // false = don't throw on mismatch here; results are inspected via
        // the runner in OneTimeTearDown so one failing checkpoint doesn't
        // abort collection of the others.
        await _eyes.CloseAsync(false);
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Applitools' actual pass/fail/new-baseline verdicts are computed
        // server-side (and for Ultrafast Grid, after cloud rendering
        // finishes), so results are collected once at the end rather than
        // per-test.
        var allResults = Runner.GetAllTestResults(false);
        TestContext.WriteLine(allResults);
    }

    [Test]
    public async Task LoginPage_MatchesBaseline()
    {
        await _page.GotoAsync("https://www.saucedemo.com/");

        await _eyes.CheckAsync(Target.Window().Fully().WithName("Login page"));
    }

    [Test]
    public async Task InventoryPage_MatchesBaseline_AfterLogin()
    {
        await _page.GotoAsync("https://www.saucedemo.com/");
        await _page.FillAsync("#user-name", "standard_user");
        await _page.FillAsync("#password", "secret_sauce");
        await _page.ClickAsync("#login-button");

        await _eyes.CheckAsync(Target.Window().Fully().WithName("Inventory page"));
    }

    [Test]
    public async Task InventorySortedHighToLow_MatchesBaseline()
    {
        await _page.GotoAsync("https://www.saucedemo.com/");
        await _page.FillAsync("#user-name", "standard_user");
        await _page.FillAsync("#password", "secret_sauce");
        await _page.ClickAsync("#login-button");
        await _page.SelectOptionAsync("[data-test='product-sort-container']", "hilo");

        await _eyes.CheckAsync(Target.Window().Fully().WithName("Inventory sorted high to low"));
    }
}
