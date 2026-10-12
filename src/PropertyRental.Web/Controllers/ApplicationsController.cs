using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;
using PropertyRental.Web.Models;
using PropertyRental.Web.Services;

namespace PropertyRental.Web.Controllers;

[Authorize]
public sealed class ApplicationsController(
    IRentalApplicationWorkflowService workflowService,
    IPropertyManagementService propertyManagementService) : Controller
{
    [Authorize(Policy = "ApplicantOnly")]
    public async Task<IActionResult> Index(
        ApplicationStatus? status,
        string? search,
        CancellationToken cancellationToken)
    {
        var applications = await workflowService.GetMyApplicationsAsync(User, status, search, cancellationToken);
        ViewData["Status"] = status;
        ViewData["Search"] = search;
        return View(applications);
    }

    [Authorize(Policy = "ApplicantOnly")]
    public async Task<IActionResult> Create(
        int unitId,
        DateOnly? desiredMoveInDate,
        CancellationToken cancellationToken)
    {
        var moveInDate = desiredMoveInDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var unit = await FindAvailableUnitAsync(unitId, moveInDate, cancellationToken);
        if (unit is null)
        {
            TempData["ErrorMessage"] = "This unit is no longer available for that date.";
            return RedirectToAction("Available", "Units", new { moveInDate });
        }

        return View("Form", new RentalApplicationFormViewModel
        {
            UnitId = unit.Id,
            UnitLabel = $"{unit.Property.Name}, unit {unit.UnitNumber}",
            DesiredMoveInDate = moveInDate
        });
    }

    [Authorize(Policy = "ApplicantOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        RentalApplicationFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateUnitLabelAsync(model, cancellationToken);
            return View("Form", model);
        }

        try
        {
            var application = await workflowService.CreateDraftAsync(User, model, cancellationToken);
            TempData["StatusMessage"] = "Draft saved.";
            return RedirectToAction(nameof(Details), new { id = application.Id });
        }
        catch (ApplicationWorkflowException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PopulateUnitLabelAsync(model, cancellationToken);
            return View("Form", model);
        }
    }

    [Authorize(Policy = "ApplicantOnly")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var application = await workflowService.GetMyApplicationAsync(User, id, cancellationToken);
        if (application is null)
        {
            return NotFound();
        }

        if (application.StatusId != (byte)ApplicationStatus.Draft
            && application.StatusId != (byte)ApplicationStatus.ReturnedForCorrection)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        return View("Form", Map(application));
    }

    [Authorize(Policy = "ApplicantOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        RentalApplicationFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateUnitLabelAsync(model, cancellationToken);
            return View("Form", model);
        }

        try
        {
            if (!await workflowService.UpdateDraftAsync(User, id, model, cancellationToken))
            {
                return NotFound();
            }
        }
        catch (ApplicationWorkflowException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PopulateUnitLabelAsync(model, cancellationToken);
            return View("Form", model);
        }

        TempData["StatusMessage"] = "Application saved.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Policy = "ApplicantOnly")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var application = await workflowService.GetMyApplicationAsync(User, id, cancellationToken);
        return application is null
            ? NotFound()
            : View(BuildDetails(application, isManager: false));
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> ReviewQueue(
        ApplicationStatus? status,
        string? search,
        CancellationToken cancellationToken)
    {
        var applications = await workflowService.GetReviewQueueAsync(User, status, search, cancellationToken);
        ViewData["Status"] = status;
        ViewData["Search"] = search;
        return View(applications);
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> Review(int id, CancellationToken cancellationToken)
    {
        var application = await workflowService.GetApplicationForReviewAsync(User, id, cancellationToken);
        return application is null
            ? NotFound()
            : View("Details", BuildDetails(application, isManager: true));
    }

    [Authorize(Policy = "ApplicantOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, CancellationToken cancellationToken)
    {
        try
        {
            if (!await workflowService.SubmitAsync(User, id, cancellationToken))
            {
                return NotFound();
            }
            TempData["StatusMessage"] = "Application submitted.";
        }
        catch (ApplicationWorkflowException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Policy = "ApplicantOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id, CancellationToken cancellationToken)
    {
        try
        {
            if (!await workflowService.WithdrawAsync(User, id, cancellationToken))
            {
                return NotFound();
            }
            TempData["StatusMessage"] = "Application withdrawn.";
        }
        catch (ApplicationWorkflowException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartReview(int id, CancellationToken cancellationToken)
    {
        try
        {
            if (!await workflowService.StartReviewAsync(User, id, cancellationToken))
            {
                return NotFound();
            }
            TempData["StatusMessage"] = "Review started.";
        }
        catch (ApplicationWorkflowException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Review), new { id });
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(
        ReviewActionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Approval notes are required and may contain at most 2000 characters.";
            return RedirectToAction(nameof(Review), new { id = model.ApplicationId });
        }

        try
        {
            if (!await workflowService.ApproveAsync(User, model.ApplicationId, model.Notes, cancellationToken))
            {
                return NotFound();
            }
            TempData["StatusMessage"] = "Application approved and 12-month lease created.";
        }
        catch (ApplicationWorkflowException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Review), new { id = model.ApplicationId });
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(
        ReviewActionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Rejection notes are required and may contain at most 2000 characters.";
            return RedirectToAction(nameof(Review), new { id = model.ApplicationId });
        }

        try
        {
            if (!await workflowService.RejectAsync(User, model.ApplicationId, model.Notes, cancellationToken))
            {
                return NotFound();
            }
            TempData["StatusMessage"] = "Application rejected.";
        }
        catch (ApplicationWorkflowException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Review), new { id = model.ApplicationId });
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnForCorrection(
        ReviewActionViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Review notes are required and may contain at most 2000 characters.";
            return RedirectToAction(nameof(Review), new { id = model.ApplicationId });
        }

        try
        {
            if (!await workflowService.ReturnForCorrectionAsync(
                    User, model.ApplicationId, model.Notes, cancellationToken))
            {
                return NotFound();
            }
            TempData["StatusMessage"] = "Application returned to the applicant for correction.";
        }
        catch (ApplicationWorkflowException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Review), new { id = model.ApplicationId });
    }

    private async Task<Unit?> FindAvailableUnitAsync(int unitId, DateOnly date, CancellationToken cancellationToken)
    {
        var units = await propertyManagementService.GetAvailableUnitsAsync(date, cancellationToken);
        return units.FirstOrDefault(unit => unit.Id == unitId);
    }

    private async Task PopulateUnitLabelAsync(
        RentalApplicationFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.DesiredMoveInDate.HasValue)
        {
            var unit = await FindAvailableUnitAsync(model.UnitId, model.DesiredMoveInDate.Value, cancellationToken);
            model.UnitLabel = unit is null
                ? "Unit is no longer available"
                : $"{unit.Property.Name}, unit {unit.UnitNumber}";
        }
    }

    private static RentalApplicationDetailsViewModel BuildDetails(RentalApplication application, bool isManager)
    {
        var statusId = application.StatusId;
        return new RentalApplicationDetailsViewModel
        {
            Application = application,
            IsManager = isManager,
            CanEdit = !isManager && (statusId == (byte)ApplicationStatus.Draft
                || statusId == (byte)ApplicationStatus.ReturnedForCorrection),
            CanSubmit = !isManager && (statusId == (byte)ApplicationStatus.Draft
                || statusId == (byte)ApplicationStatus.ReturnedForCorrection),
            CanWithdraw = !isManager && (statusId == (byte)ApplicationStatus.Draft
                || statusId == (byte)ApplicationStatus.Submitted
                || statusId == (byte)ApplicationStatus.UnderReview
                || statusId == (byte)ApplicationStatus.ReturnedForCorrection),
            CanStartReview = isManager && statusId == (byte)ApplicationStatus.Submitted,
            CanApprove = isManager && statusId == (byte)ApplicationStatus.UnderReview,
            CanReject = isManager && statusId == (byte)ApplicationStatus.UnderReview,
            CanReturnForCorrection = isManager && (statusId == (byte)ApplicationStatus.Submitted
                || statusId == (byte)ApplicationStatus.UnderReview)
        };
    }

    private static RentalApplicationFormViewModel Map(RentalApplication application) => new()
    {
        Id = application.Id,
        UnitId = application.UnitId,
        UnitLabel = $"{application.Unit.Property.Name}, unit {application.Unit.UnitNumber}",
        DesiredMoveInDate = application.DesiredMoveInDate,
        HouseholdSize = application.HouseholdSize,
        MonthlyIncome = application.MonthlyIncome,
        ApplicantMessage = application.ApplicantMessage
    };
}