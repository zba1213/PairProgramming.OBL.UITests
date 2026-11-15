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
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        var binary = Environment.GetEnvironmentVariable("CHROME_BINARY");
        if (!string.IsNullOrEmpty(binary)) options.BinaryLocation = binary;
        using var driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl(Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5500");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        wait.Until(d => d.FindElements(By.CssSelector("[data-testid='record-row']")).Count > 0
            || d.FindElements(By.CssSelector("[role='alert']")).Count > 0);
        Assert.AreEqual(0, driver.FindElements(By.CssSelector("[role='alert']")).Count, "API must be running and accessible through CORS.");
        var rows = driver.FindElements(By.CssSelector("[data-testid='record-row']"));
        Assert.AreEqual(3, rows.Count, "Run with the story 1 backend seed data.");
        StringAssert.Contains(rows[0].Text, "Kind of Blue");
        StringAssert.Contains(rows[0].Text, "Miles Davis");
        StringAssert.Contains(rows[0].Text, "45:57");
        StringAssert.Contains(rows[0].Text, "1959");
    }
}
