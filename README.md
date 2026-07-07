# Applitools Visual Testing Example (C# / Playwright)

A sample project demonstrating AI-powered visual regression testing with
[Applitools Eyes](https://applitools.com/) and Playwright in C#/NUnit,
testing [saucedemo.com](https://www.saucedemo.com/) — a public demo store
built for test automation practice.

## Why visual testing?

Functional tests (like the ones in the other repos in this portfolio) check
*behavior* — did the login succeed, did the cart count update. Visual
testing catches things functional assertions miss entirely: a broken
layout, an overlapping element, a missing image, incorrect fonts or colors.
Applitools' Visual AI compares full-page screenshots against an approved
baseline and flags meaningful differences while ignoring noise like
anti-aliasing.

## Prerequisites

- .NET 8 SDK
- A free [Applitools account](https://auth.applitools.com/users/register) and API key

## Setup

1. Restore and build:

   ```bash
   dotnet restore
   dotnet build
   pwsh bin/Debug/net8.0/playwright.ps1 install --with-deps chromium
   ```

2. Set your API key as an environment variable (don't hardcode it):

   ```bash
   export APPLITOOLS_API_KEY="your-api-key-here"      # macOS/Linux
   $env:APPLITOOLS_API_KEY = "your-api-key-here"       # Windows PowerShell
   ```

3. Run the tests:

   ```bash
   dotnet test
   ```

4. Open the [Applitools Test Manager](https://eyes.applitools.com/) to
   review results. **The first run creates the baseline** — every
   checkpoint will show as "New." Run the suite again without changing
   anything and everything should show as "Passed." Intentionally tweak
   something (or just wait for the site to change) and you'll see a
   "Unresolved" diff waiting for your review.

## What's being tested

`Tests/SauceDemoVisualTests.cs` checks three full-page snapshots:

- The login page
- The inventory page after logging in
- The inventory page sorted "high to low"

Each uses `Target.Window().Fully()` for a full-page (not just viewport)
screenshot.

## Classic runner vs. Ultrafast Grid

The `UseUltrafastGrid` constant at the top of the test class switches
between two execution modes:

| Mode | What happens |
|---|---|
| **Classic runner** (`false`) | Screenshots are captured by the local Playwright browser you configured — one browser, one viewport, exactly what ran locally |
| **Ultrafast Grid** (`true`, the default here) | The page is captured once locally, then rendered against every browser/viewport listed in `config.AddBrowser(...)` / `AddDeviceEmulation(...)` in Applitools' cloud — no need to install Firefox, Safari, or maintain a device lab locally |

This example's Ultrafast Grid config renders each checkpoint against
Chrome and Firefox at 1200×800, Safari at 1024×768, and an emulated
iPhone X — four results per checkpoint from a single local page load.

## CI/CD

`.github/workflows/applitools-visual-tests.yml` runs on every push/PR to
`main`. It needs an `APPLITOOLS_API_KEY` repository secret (Settings →
Secrets and variables → Actions) to authenticate against the Applitools
server. Each CI run is tagged with a batch ID from the GitHub Actions run
ID, so results from the same pipeline run group together in the Test
Manager dashboard.

## Possible extensions

- Add ignore/layout regions for dynamic content (e.g. timestamps, ads) that shouldn't fail a visual diff
- Add `eyes.CheckAsync(Target.Region(...))` calls for component-level checks instead of full-page only
- Combine with the functional Playwright suites elsewhere in this portfolio — Applitools works well as an additional check layered onto existing functional tests, not a replacement for them
- Explore Applitools' root cause analysis, which highlights the DOM/CSS change that likely caused a visual diff
