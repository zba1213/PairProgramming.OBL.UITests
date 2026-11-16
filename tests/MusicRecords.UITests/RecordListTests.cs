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
        var search = driver.FindElement(By.CssSelector("button[type='submit']"));
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
                || d.FindElements(By.CssSelector("[role='alert']")).Count > 0);
            Assert.AreEqual(0, driver.FindElements(By.CssSelector("[role='alert']")).Count, "API must be running and accessible through CORS.");
            return driver;
        }
        catch
        {
            driver.Dispose();
            throw;
        }
    }
}
