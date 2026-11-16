using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
namespace MusicRecords.UITests;
[TestClass]
public class RecordListTests
{
    [TestMethod]
    public void RecordList_DisplaysRecordsFromRunningApi()
    {
        using var driver = OpenRecordList();
        Assert.AreEqual(0, driver.FindElements(By.Id("record-title")).Count);
        Assert.AreEqual(0, driver.FindElements(By.CssSelector(".delete-record")).Count);
        var rows = driver.FindElements(By.CssSelector("[data-testid='record-row']"));
        Assert.AreEqual(3, rows.Count, "Run with the story 1 backend seed data.");
        StringAssert.Contains(rows[0].Text, "Kind of Blue");
        StringAssert.Contains(rows[0].Text, "Miles Davis");
        StringAssert.Contains(rows[0].Text, "45:57");
        StringAssert.Contains(rows[0].Text, "1959");
    }

    [TestMethod]
    public void RecordList_SearchesByTitleArtistAndYearAndResetsFilters()
    {
        using var driver = OpenRecordList();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        driver.FindElement(By.Id("filter-title")).SendKeys("blue");
        driver.FindElement(By.Id("filter-artist")).SendKeys("miles");
        var year = driver.FindElement(By.Id("filter-year"));
        year.SendKeys("1959");
        var search = driver.FindElement(By.CssSelector(".filters button[type='submit']"));
        search.Click();

        wait.Until(d => d.FindElements(By.CssSelector("[data-testid='record-row']")).Count == 1);
        StringAssert.Contains(driver.FindElement(By.CssSelector("[data-testid='record-row']")).Text, "Kind of Blue");

        year.Clear();
        year.SendKeys("1969");
        search.Click();
        wait.Until(d => d.FindElements(By.CssSelector("[role='status']"))
            .Any(status => status.Text.Contains("Ingen musikplader matcher")));

        driver.FindElement(By.CssSelector(".reset-filters")).Click();
        wait.Until(d => d.FindElements(By.CssSelector("[data-testid='record-row']")).Count == 3);
        Assert.AreEqual(string.Empty, driver.FindElement(By.Id("filter-title")).GetDomProperty("value"));
    }

    [TestMethod]
    public void Editor_CanAddAndDeleteARecord()
    {
        var username = Environment.GetEnvironmentVariable("UI_TEST_USERNAME");
        var password = Environment.GetEnvironmentVariable("UI_TEST_PASSWORD");
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)
            || Environment.GetEnvironmentVariable("UI_TEST_ROLE") != "Editor")
        {
            Assert.Inconclusive("Configure an Editor test account and run against a test database.");
        }

        using var driver = OpenRecordList();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        SubmitCredentials(driver, username!, password!);
        var title = $"Testplade {Guid.NewGuid():N}";
        var row = By.XPath($"//tr[@data-testid='record-row'][td[1]='{title}']");

        var titleInput = wait.Until(d => d.FindElements(By.Id("record-title")).FirstOrDefault(e => e.Enabled));
        Assert.IsNotNull(titleInput);
        titleInput.SendKeys(title);
        driver.FindElement(By.Id("record-artist")).SendKeys("Testkunstner");
        driver.FindElement(By.Id("record-duration")).SendKeys("125");
        driver.FindElement(By.Id("record-year")).SendKeys("2024");
        driver.FindElement(By.CssSelector(".record-form button[type='submit']")).Click();

        var added = wait.Until(d => d.FindElements(row).FirstOrDefault(
            element => element.FindElement(By.CssSelector(".delete-record")).Enabled));
        Assert.IsNotNull(added);
        StringAssert.Contains(added.Text, "Testkunstner");
        StringAssert.Contains(added.Text, "2:05");
        StringAssert.Contains(added.Text, "2024");

        added.FindElement(By.CssSelector(".delete-record")).Click();
        driver.SwitchTo().Alert().Accept();
        wait.Until(d => d.FindElements(By.CssSelector(".record-feedback [role='status']"))
            .Any(status => status.Text == "Musikpladen er slettet."));
        wait.Until(d => d.FindElements(row).Count == 0);
    }

    [TestMethod]
    public void Login_WithValidCredentials_ShowsAccountAndCanLogOut()
    {
        var username = Environment.GetEnvironmentVariable("UI_TEST_USERNAME");
        var password = Environment.GetEnvironmentVariable("UI_TEST_PASSWORD");
        var role = Environment.GetEnvironmentVariable("UI_TEST_ROLE");
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            Assert.Inconclusive("Set UI_TEST_USERNAME, UI_TEST_PASSWORD and UI_TEST_ROLE to match the API test account.");

        using var driver = OpenRecordList();
        SubmitCredentials(driver, username!, password!);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        var status = wait.Until(d => d.FindElements(By.CssSelector("[data-testid='login-status']")).FirstOrDefault());
        Assert.IsNotNull(status);
        Assert.AreEqual($"{username} ({role})", status.Text);

        driver.FindElement(By.CssSelector(".login-panel button")).Click();
        wait.Until(d => d.FindElements(By.Id("login-password")).Count == 1);
        Assert.AreEqual(0, driver.FindElements(By.CssSelector("[data-testid='login-status']")).Count);
        Assert.AreEqual(3, driver.FindElements(By.CssSelector("[data-testid='record-row']")).Count);
    }

    [TestMethod]
    public void Login_WithInvalidCredentials_ShowsErrorAndLeavesRecordListVisible()
    {
        using var driver = OpenRecordList();
        SubmitCredentials(driver, $"unknown-{Guid.NewGuid():N}", "invalid-password");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        var alert = wait.Until(d => d.FindElements(By.CssSelector(".login-panel [role='alert']")).FirstOrDefault());
        Assert.IsNotNull(alert);
        Assert.AreEqual("Forkert brugernavn eller adgangskode.", alert.Text);
        Assert.AreEqual(0, driver.FindElements(By.CssSelector("[data-testid='login-status']")).Count);
        Assert.AreEqual(3, driver.FindElements(By.CssSelector("[data-testid='record-row']")).Count);
    }

    private static void SubmitCredentials(IWebDriver driver, string username, string password)
    {
        driver.FindElement(By.Id("login-username")).SendKeys(username);
        driver.FindElement(By.Id("login-password")).SendKeys(password);
        driver.FindElement(By.CssSelector(".login-panel button[type='submit']")).Click();
    }

    private static ChromeDriver OpenRecordList()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        var binary = Environment.GetEnvironmentVariable("CHROME_BINARY");
        if (!string.IsNullOrEmpty(binary)) options.BinaryLocation = binary;
        var driver = new ChromeDriver(options);
        try
        {
            driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5500");
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            wait.Until(d => d.FindElements(By.CssSelector("[data-testid='record-row']")).Count > 0
                || d.FindElements(By.CssSelector("#app > [role='alert']")).Count > 0);
            Assert.AreEqual(0, driver.FindElements(By.CssSelector("#app > [role='alert']")).Count, "API must be running and accessible through CORS.");
            return driver;
        }
        catch
        {
            driver.Dispose();
            throw;
        }
    }
}
