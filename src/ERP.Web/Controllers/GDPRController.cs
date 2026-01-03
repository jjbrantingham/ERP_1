using ERP.Application.AUDIT.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for GDPR compliance features.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class GDPRController : ControllerBase
{
    private readonly IMediator _mediator;

    public GDPRController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Export all user data (GDPR Right to Data Portability - Article 20).
    /// Users can export their own data. Administrators can export any user's data.
    /// </summary>
    [HttpGet("export/{userId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportUserData(long userId)
    {
        // TODO: Add authorization check - users can only export their own data unless they're admins
        // if (!User.IsInRole("Administrator") && GetCurrentUserId() != userId)
        //     return Forbid();

        var command = new ExportUserDataCommand { UserId = userId };
        var json = await _mediator.Send(command);

        var filename = $"user-data-export-{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";

        return File(
            System.Text.Encoding.UTF8.GetBytes(json),
            "application/json",
            filename
        );
    }

    /// <summary>
    /// Anonymize user data (GDPR Right to be Forgotten - Article 17).
    /// Requires Administrator role. Anonymizes rather than deletes to maintain referential integrity.
    /// </summary>
    [HttpPost("anonymize/{userId}")]
    [Authorize(Roles = "Administrator,DataProtectionOfficer")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AnonymizeUserData(
        long userId,
        [FromBody] AnonymizeUserDataRequest request)
    {
        var command = new AnonymizeUserDataCommand
        {
            UserId = userId,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command);

        return Ok(new
        {
            success = result,
            message = $"User data for UserId {userId} has been anonymized",
            timestamp = DateTime.UtcNow,
            gdprArticle = "Article 17 - Right to erasure ('right to be forgotten')"
        });
    }

    /// <summary>
    /// Get consent status for a user (placeholder for future consent management).
    /// </summary>
    [HttpGet("consent/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConsentStatus(long userId)
    {
        // TODO: Implement consent tracking
        await Task.CompletedTask;

        return Ok(new
        {
            userId,
            consents = new[]
            {
                new { purpose = "DataProcessing", granted = true, date = DateTime.UtcNow },
                new { purpose = "Marketing", granted = false, date = (DateTime?)null }
            },
            message = "Consent management not yet fully implemented"
        });
    }

    /// <summary>
    /// Record user consent (placeholder for future consent management).
    /// </summary>
    [HttpPost("consent/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordConsent(
        long userId,
        [FromBody] ConsentRequest request)
    {
        // TODO: Implement consent tracking
        await Task.CompletedTask;

        return Ok(new
        {
            userId,
            purpose = request.Purpose,
            granted = request.Granted,
            recordedAt = DateTime.UtcNow,
            message = "Consent management not yet fully implemented"
        });
    }
}

/// <summary>
/// Request model for anonymizing user data.
/// </summary>
public class AnonymizeUserDataRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request model for recording consent.
/// </summary>
public class ConsentRequest
{
    public string Purpose { get; set; } = string.Empty;
    public bool Granted { get; set; }
}
