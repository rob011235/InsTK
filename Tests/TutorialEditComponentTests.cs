using Bunit;
using InsTK.Server.Components.Pages.Tutorials;
using InsTK.Server.Data;
using InsTK.Shared.Models.Tutorials;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Syncfusion.Blazor;
using Xunit;

namespace Tests;

public sealed class TutorialEditComponentTests : TestContext
{
    public TutorialEditComponentTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        Services.AddSyncfusionBlazor();
        Services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());
    }

    [Fact]
    public void EditPage_UsesCurrentTutorialTitleForDefaultBrightspaceAssignmentTitle()
    {
        Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(CreateFactory());

        var cut = RenderComponent<Edit>();

        cut.Find("#tutorial-title").Change("Routing Basics");
        cut.FindAll("button").Single(button => button.TextContent.Contains("Brightspace Assignment", StringComparison.Ordinal)).Click();
        cut.WaitForAssertion(() => Assert.Contains("Assignment Title", cut.Markup));

        var assignmentTitle = cut.Find("#assignment-title");
        Assert.Equal("Tutorial - Routing Basics", assignmentTitle.GetAttribute("value"));
    }

    [Fact]
    public void EditPage_DoesNotOverwriteCustomBrightspaceAssignmentTitleWhenTutorialTitleChanges()
    {
        var tutorialId = Guid.NewGuid();
        using var seedContext = CreateContext("custom-assignment-title");
        seedContext.Tutorials.Add(new TutorialDefinition
        {
            Id = tutorialId,
            Title = "Original Title",
            BrightspaceAssignmentTitle = "Custom Assignment Name",
            BrightspaceAssignmentInstructions = "Follow the guide.",
            BrightspaceSubmissionInstructions = "Submit work."
        });
        seedContext.SaveChanges();

        Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(CreateFactory("custom-assignment-title"));

        var cut = RenderComponent<Edit>(parameters => parameters.Add(component => component.Id, tutorialId));

        cut.Find("#tutorial-title").Change("Updated Tutorial Title");
        cut.FindAll("button").Single(button => button.TextContent.Contains("Brightspace Assignment", StringComparison.Ordinal)).Click();
        cut.WaitForAssertion(() => Assert.Contains("Assignment Title", cut.Markup));

        var assignmentTitle = cut.Find("#assignment-title");
        Assert.Equal("Custom Assignment Name", assignmentTitle.GetAttribute("value"));
    }

    [Fact]
    public void TutorialExportPopup_UsesUnsavedDraftValues()
    {
        Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(CreateFactory());

        var cut = RenderComponent<Edit>();

        cut.Find("#tutorial-title").Change("Unsaved Tutorial Title");
        cut.FindAll("button").First(button => button.TextContent.Trim() == "Export").Click();
        cut.WaitForAssertion(() => Assert.Contains("Tutorial Export: Unsaved Tutorial Title", cut.Markup));

        Assert.Contains("Tutorial Export: Unsaved Tutorial Title", cut.Markup);
        Assert.Contains("&lt;h1&gt;Unsaved Tutorial Title&lt;/h1&gt;", cut.Markup);
        Assert.Contains("Download", cut.Markup);
    }

    [Fact]
    public void AssignmentExportPopup_ShowsOnlyBrightspaceAssignmentHtmlFromDraft()
    {
        Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(CreateFactory());

        var cut = RenderComponent<Edit>();
        cut.FindAll("button").Single(button => button.TextContent.Contains("Brightspace Assignment", StringComparison.Ordinal)).Click();
        cut.WaitForAssertion(() => Assert.Contains("Assignment Instructions", cut.Markup));

        cut.Find("#assignment-instructions").Change("Review the draft instructions.");
        cut.FindAll("button").First(button => button.TextContent.Trim() == "Export").Click();
        cut.WaitForAssertion(() => Assert.Contains("Brightspace Assignment Export: Tutorial", cut.Markup));

        Assert.Contains("Brightspace Assignment Export: Tutorial", cut.Markup);
        Assert.Contains("Brightspace Assignment HTML", cut.Markup);
        Assert.DoesNotContain("WordPress Markdown", cut.Markup);
        Assert.Contains("Review the draft instructions.", cut.Markup);
    }

    [Fact]
    public void EditPage_LoadsExistingHtmlIntoEditorAndPreview()
    {
        var tutorialId = Guid.NewGuid();
        using var seedContext = CreateContext("existing-html");
        seedContext.Tutorials.Add(new TutorialDefinition
        {
            Id = tutorialId,
            Title = "Existing Tutorial",
            ContentHtml = "<h2>Existing Step</h2><p>Saved content.</p>"
        });
        seedContext.SaveChanges();

        Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(CreateFactory("existing-html"));

        var cut = RenderComponent<Edit>(parameters => parameters.Add(component => component.Id, tutorialId));

        Assert.Contains("<h2>Existing Step</h2>", cut.Markup);
        Assert.Contains("<p>Saved content.</p>", cut.Markup);
    }

    [Fact]
    public async Task Save_PreservesStoredHtmlWithoutMarkdownConversion()
    {
        Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(CreateFactory("save-html"));

        var cut = RenderComponent<Edit>();
        cut.Find("#tutorial-title").Change("Created Tutorial");

        var updateTask = (Task)cut.Instance.GetType()
            .GetMethod("OnContentHtmlChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(cut.Instance, ["<h2>Updated</h2><p><strong>Saved</strong> content.</p>"])!;
        await updateTask;

        var saveTask = (Task)cut.Instance.GetType()
            .GetMethod("Save", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(cut.Instance, [])!;
        await saveTask;

        using var verifyContext = CreateContext("save-html");
        var savedTutorial = verifyContext.Tutorials.Single();
        Assert.Equal("Created Tutorial", savedTutorial.Title);
        Assert.Equal("<h2>Updated</h2><p><strong>Saved</strong> content.</p>", savedTutorial.ContentHtml);
    }

    private static IDbContextFactory<ApplicationDbContext> CreateFactory(string databaseName = "edit-tests")
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestDbContextFactory(options);
    }

    private static ApplicationDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed class TestDbContextFactory(DbContextOptions<ApplicationDbContext> options) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext()
        {
            return new ApplicationDbContext(options);
        }

        public ValueTask<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(CreateDbContext());
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string WebRootPath { get; set; } = Path.Combine("C:\\repos\\InsTK", "InsTK.Server", "wwwroot");

        public string EnvironmentName { get; set; } = "Development";

        public string ContentRootPath { get; set; } = Path.Combine("C:\\repos\\InsTK", "InsTK.Server");

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
