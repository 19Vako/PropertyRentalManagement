using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;
using PropertyRental.Domain.Identity;
using PropertyRental.Infrastructure.Data;
using PropertyRental.Web.Controllers;
using PropertyRental.Web.Models;
using PropertyRental.Web.Services;
using Xunit;

namespace PropertyRental.Web.Tests;

public sealed class ApplicationAuthorizationTests
{
    [Fact]
    public async Task ApplicantDashboardContainsOnlyApplicationsOwnedByCurrentUser()
    {
        await using var dbContext = CreateDbContext();
        await SeedApplicationsAsync(dbContext);
        var service = new ApplicationVisibilityService(dbContext);
        var applicant = CreatePrincipal("applicant-1", RoleNames.Applicant);

        var visibleApplications = await service.GetVisibleApplicationsAsync(applicant);

        Assert.Equal(new[] { 101, 102 }, visibleApplications.Select(application => application.Id).Order());
    }

    [Fact]
    public async Task ApplicantCannotReadAnotherApplicantsApplicationDetails()
    {
        await using var dbContext = CreateDbContext();
        await SeedApplicationsAsync(dbContext);
        var service = new RentalApplicationWorkflowService(
            dbContext,
            propertyManagementService: null!,
            leaseManagementService: null!);
        var applicant = CreatePrincipal("applicant-1", RoleNames.Applicant);

        var application = await service.GetMyApplicationAsync(applicant, 201);

        Assert.Null(application);
    }

    [Fact]
    public async Task ApplicantCannotUseManagerReviewQueueService()
    {
        await using var dbContext = CreateDbContext();
        var service = new RentalApplicationWorkflowService(
            dbContext,
            propertyManagementService: null!,
            leaseManagementService: null!);
        var applicant = CreatePrincipal("applicant-1", RoleNames.Applicant);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.GetReviewQueueAsync(applicant, null, null));
    }

    [Fact]
    public async Task ManagerCannotCreateApplicationOnBehalfOfApplicant()
    {
        await using var dbContext = CreateDbContext();
        var service = new RentalApplicationWorkflowService(
            dbContext,
            propertyManagementService: null!,
            leaseManagementService: null!);
        var manager = CreatePrincipal("manager-1", RoleNames.PropertyManager);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.CreateDraftAsync(manager, new RentalApplicationFormViewModel()));
    }

    [Fact]
    public async Task ApplicantCannotApproveApplicationThroughWorkflowService()
    {
        await using var dbContext = CreateDbContext();
        var service = new RentalApplicationWorkflowService(
            dbContext,
            propertyManagementService: null!,
            leaseManagementService: null!);
        var applicant = CreatePrincipal("applicant-1", RoleNames.Applicant);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ApproveAsync(applicant, 201, "Approval note"));
    }

    [Theory]
    [InlineData(typeof(PropertiesController), "Create")]
    [InlineData(typeof(PropertiesController), "Edit")]
    [InlineData(typeof(PropertiesController), "DeleteConfirmed")]
    [InlineData(typeof(UnitsController), "Create")]
    [InlineData(typeof(UnitsController), "Edit")]
    [InlineData(typeof(UnitsController), "DeleteConfirmed")]
    [InlineData(typeof(LeasesController), "Terminate")]
    [InlineData(typeof(LeasesController), "MarkExpired")]
    [InlineData(typeof(ApplicationsController), "Approve")]
    [InlineData(typeof(ApplicationsController), "Reject")]
    [InlineData(typeof(ApplicationsController), "ReturnForCorrection")]
    public void MutatingEndpointsRequireAntiforgery(Type controllerType, string actionName)
    {
        var method = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(candidate => candidate.Name == actionName
                && candidate.GetCustomAttribute<HttpPostAttribute>() is not null);

        Assert.NotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
    }

    [Theory]
    [InlineData(typeof(PropertiesController))]
    [InlineData(typeof(LeasesController))]
    public void ManagerControllersRequireManagerPolicy(Type controllerType)
    {
        var authorization = controllerType.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorization);
        Assert.Equal("PropertyManagerOnly", authorization!.Policy);
    }

    [Fact]
    public void ApplicantCannotCallManagerOnlyApproveEndpoint()
    {
        var method = typeof(ApplicationsController).GetMethod(nameof(ApplicationsController.Approve))!;
        var authorization = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorization);
        Assert.Equal("PropertyManagerOnly", authorization!.Policy);
    }

    [Theory]
    [InlineData(typeof(DashboardController), "Applicant", "ApplicantOnly")]
    [InlineData(typeof(DashboardController), "Manager", "PropertyManagerOnly")]
    [InlineData(typeof(UnitsController), "Available", "ApplicantOnly")]
    [InlineData(typeof(UnitsController), "Index", "PropertyManagerOnly")]
    [InlineData(typeof(UnitsController), "Create", "PropertyManagerOnly")]
    [InlineData(typeof(UnitsController), "Details", "PropertyManagerOnly")]
    [InlineData(typeof(UnitsController), "Edit", "PropertyManagerOnly")]
    [InlineData(typeof(UnitsController), "Delete", "PropertyManagerOnly")]
    [InlineData(typeof(ApplicationsController), "Index", "ApplicantOnly")]
    [InlineData(typeof(ApplicationsController), "Create", "ApplicantOnly")]
    [InlineData(typeof(ApplicationsController), "Edit", "ApplicantOnly")]
    [InlineData(typeof(ApplicationsController), "Details", "ApplicantOnly")]
    [InlineData(typeof(ApplicationsController), "Submit", "ApplicantOnly")]
    [InlineData(typeof(ApplicationsController), "Withdraw", "ApplicantOnly")]
    [InlineData(typeof(ApplicationsController), "ReviewQueue", "PropertyManagerOnly")]
    [InlineData(typeof(ApplicationsController), "Review", "PropertyManagerOnly")]
    [InlineData(typeof(ApplicationsController), "StartReview", "PropertyManagerOnly")]
    [InlineData(typeof(ApplicationsController), "Approve", "PropertyManagerOnly")]
    [InlineData(typeof(ApplicationsController), "Reject", "PropertyManagerOnly")]
    [InlineData(typeof(ApplicationsController), "ReturnForCorrection", "PropertyManagerOnly")]
    public void RoleSensitiveEndpointsDeclareExpectedPolicy(
        Type controllerType,
        string actionName,
        string expectedPolicy)
    {
        var actionMethods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(candidate => candidate.Name == actionName)
            .ToArray();

        Assert.NotEmpty(actionMethods);
        Assert.All(actionMethods, method =>
        {
            var authorization = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(authorization);
            Assert.Equal(expectedPolicy, authorization!.Policy);
        });
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task SeedApplicationsAsync(ApplicationDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        var status = await dbContext.ApplicationStatuses
            .SingleAsync(item => item.Id == (byte)ApplicationStatus.Draft);
        var property = new Property
        {
            Id = 1,
            ManagerUserId = "manager-1",
            Name = "North House",
            StreetAddress = "1 Main Street",
            City = "Example City",
            Region = "Region",
            PostalCode = "10000",
            CountryCode = "US",
            IsActive = true,
            CreatedUtc = DateTime.UtcNow
        };
        var unit = new Unit
        {
            Id = 1,
            PropertyId = property.Id,
            Property = property,
            UnitNumber = "1A",
            MonthlyRent = 1200,
            Status = UnitStatus.Available,
            CreatedUtc = DateTime.UtcNow
        };

        dbContext.Properties.Add(property);
        dbContext.Units.Add(unit);
        dbContext.RentalApplications.AddRange(
            CreateApplication(101, "applicant-1", unit, status),
            CreateApplication(102, "applicant-1", unit, status),
            CreateApplication(201, "applicant-2", unit, status));
        await dbContext.SaveChangesAsync();
    }

    private static RentalApplication CreateApplication(
        int id,
        string applicantId,
        Unit unit,
        ApplicationStatusRecord status) => new()
    {
        Id = id,
        ApplicantUserId = applicantId,
        UnitId = unit.Id,
        Unit = unit,
        StatusId = status.Id,
        Status = status,
        HouseholdSize = 1,
        CreatedUtc = DateTime.UtcNow,
        UpdatedUtc = DateTime.UtcNow
    };

    private static ClaimsPrincipal CreatePrincipal(string userId, string role) => new(
        new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        ],
        authenticationType: "test"));
}