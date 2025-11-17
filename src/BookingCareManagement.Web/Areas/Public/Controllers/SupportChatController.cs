using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.SupportChat.Commands;
using BookingCareManagement.Application.Features.SupportChat.Dtos;
using BookingCareManagement.Application.Features.SupportChat.Queries;
using BookingCareManagement.Domain.Aggregates.SupportChat;
using BookingCareManagement.Domain.Aggregates.User;
using BookingCareManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Web.Areas.Public.Controllers;

[Area("Public")]
[Route("api/support-chat")]
[ApiController]
[Authorize]
public class SupportChatController : ControllerBase
{
    private readonly ApplicationDBContext _dbContext;
    private readonly UserManager<AppUser> _userManager;

    public SupportChatController(ApplicationDBContext dbContext, UserManager<AppUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpGet("targets")]
    public async Task<ActionResult<IEnumerable<SupportRecipientDto>>> GetRecipients(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var recipients = await BuildRecipientsAsync(userId, cancellationToken);
        return Ok(recipients);
    }

    [HttpPost("conversations")]
    public async Task<ActionResult<SupportConversationDto>> EnsureConversation(
        [FromServices] EnsureSupportConversationCommandHandler handler,
        [FromBody] EnsureConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var command = new EnsureSupportConversationCommand
        {
            CustomerId = userId,
            StaffId = request.StaffId ?? string.Empty,
            StaffRole = request.StaffRole,
            DoctorId = request.DoctorId,
            IncludeMessages = true
        };

        try
        {
            var dto = await handler.Handle(command, cancellationToken);
            return Ok(dto);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Không thể mở cuộc trò chuyện",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpGet("conversation")]
    [Obsolete("Use POST /api/support-chat/conversations to select một người nhận cụ thể")] 
    public Task<ActionResult<SupportConversationDto>> GetConversation(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ActionResult<SupportConversationDto>>(BadRequest(new ProblemDetails
        {
            Title = "API đã thay đổi",
            Detail = "Vui lòng chọn người nhận và gọi POST /api/support-chat/conversations.",
            Status = StatusCodes.Status400BadRequest
        }));
    }

    [HttpGet("messages")]
    public async Task<ActionResult<IEnumerable<SupportMessageDto>>> GetMessages(
        [FromServices] GetSupportMessagesQueryHandler handler,
        [FromQuery] Guid conversationId,
        [FromQuery] DateTime? afterUtc,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var dto = await handler.Handle(new GetSupportMessagesQuery
        {
            ConversationId = conversationId,
            RequesterId = userId,
            AfterUtc = afterUtc
        }, cancellationToken);

        return Ok(dto);
    }

    [HttpPost("messages")]
    public async Task<ActionResult<SupportMessageDto>> SendMessage(
        [FromServices] SendSupportMessageCommandHandler handler,
        [FromBody] SendSupportMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        command.SenderId = userId;

        try
        {
            var dto = await handler.Handle(command, cancellationToken);
            return Ok(dto);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Không thể gửi tin nhắn",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task<IReadOnlyList<SupportRecipientDto>> BuildRecipientsAsync(string customerId, CancellationToken cancellationToken)
    {
        var recipients = new List<SupportRecipientDto>();

        var adminRecipient = await GetAdminRecipientAsync(cancellationToken);
        if (adminRecipient is not null)
        {
            recipients.Add(adminRecipient);
        }

        var doctorItems = await (from appointment in _dbContext.Appointments.AsNoTracking()
                                 join doctor in _dbContext.Doctors.AsNoTracking() on appointment.DoctorId equals doctor.Id
                                 join staffUser in _dbContext.Users.AsNoTracking() on doctor.AppUserId equals staffUser.Id
                                 where appointment.PatientId == customerId
                                 select new
                                 {
                                     doctor.Id,
                                     doctor.AppUserId,
                                     staffUser.FullName,
                                     staffUser.AvatarUrl
                                 }).ToListAsync(cancellationToken);

        foreach (var group in doctorItems.GroupBy(x => x.Id))
        {
            var sample = group.First();
            recipients.Add(new SupportRecipientDto(
                sample.AppUserId,
                sample.FullName ?? "Bác sĩ",
                SupportConversationStaffRole.Doctor,
                group.Key,
                sample.AvatarUrl));
        }

        return recipients
            .OrderBy(r => r.StaffRole == SupportConversationStaffRole.Admin ? 0 : 1)
            .ThenBy(r => r.DisplayName)
            .ToList();
    }

    private async Task<SupportRecipientDto?> GetAdminRecipientAsync(CancellationToken cancellationToken)
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var admin = admins
            .OrderBy(u => u.CreatedAt)
            .FirstOrDefault();

        if (admin is null)
        {
            return null;
        }

        return new SupportRecipientDto(
            admin.Id,
            admin.FullName ?? "Hỗ trợ BookingCare",
            SupportConversationStaffRole.Admin,
            null,
            admin.AvatarUrl);
    }

    public class EnsureConversationRequest
    {
        public string? StaffId { get; set; }
        public SupportConversationStaffRole StaffRole { get; set; }
        public Guid? DoctorId { get; set; }
    }

    public record SupportRecipientDto(
        string StaffId,
        string DisplayName,
        SupportConversationStaffRole StaffRole,
        Guid? DoctorId,
        string? AvatarUrl);
}
