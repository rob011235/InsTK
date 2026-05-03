using AngleSharp.Dom;
using Bunit;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using InsTK.Server.Components.Pages.Tutorials;
using InsTK.Server.Data;
using InsTK.Shared.Models.Tutorials;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests;

public sealed class TutorialEditComponentTests : TestContext
{
    public TutorialEditComponentTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        Services
            .AddBlazorise(options => options.Immediate = true)
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();
    }

    [Fact]
    public void EditPage_UsesCurrentTutorialTitleForDefaultBrightspaceAssignmentTitle()
    {
        Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(CreateFactory());

        var cut = RenderComponent<Edit>();

        FindInputByLabel(cut, "Title").Change("Routing Basics");
        cut.FindAll("button").Single(button => button.TextContent.Contains("Brightspace Assignment", StringComparison.Ordinal)).Click();

        var assignmentTitle = FindInputByLabel(cut, "Assignment Title");
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

        FindInputByLabel(cut, "Title").Change("Updated Tutorial Title");
        cut.FindAll("button").Single(button => button.TextContent.Contains("Brightspace Assignment", StringComparison.Ordinal)).Click();

        var assignmentTitle = FindInputByLabel(cut, "Assignment Title");
        Assert.Equal("Custom Assignment Name", assignmentTitle.GetAttribute("value"));
    }

    [Fact]
    public void TutorialExportPopup_UsesUnsavedDraftValues()
    {
        Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(CreateFactory());

        var cut = RenderComponent<Edit>();

        FindInputByLabel(cut, "Title").Change("Unsaved Tutorial Title");
        cut.FindAll("button").First(button => button.TextContent.Trim() == "Export").Click();

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

        FindTextAreaByLabel(cut, "Assignment Instructions").Change("Review the draft instructions.");
        cut.FindAll("button").First(button => button.TextContent.Trim() == "Export").Click();

        Assert.Contains("Brightspace Assignment Export: Tutorial", cut.Markup);
        Assert.Contains("Brightspace Assignment HTML", cut.Markup);
        Assert.DoesNotContain("WordPress Markdown", cut.Markup);
        Assert.Contains("Review the draft instructions.", cut.Markup);
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

    private static IElement FindInputByLabel(IRenderedFragment cut, string labelText)
    {
        var label = cut.FindAll("label").Single(label => label.TextContent.Trim() == labelText);
        return label.NextElementSibling!;
    }

    private static IElement FindTextAreaByLabel(IRenderedFragment cut, string labelText)
    {
        var label = cut.FindAll("label").Single(label => label.TextContent.Trim() == labelText);
        return label.NextElementSibling!;
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
}
