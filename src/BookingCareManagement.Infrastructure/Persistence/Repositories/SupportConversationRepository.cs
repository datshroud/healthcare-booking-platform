using System;
using System.Linq;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.SupportChat;
using Microsoft.EntityFrameworkCore;
using BookingCareManagement.Infrastructure.Persistence;

namespace BookingCareManagement.Infrastructure.Persistence.Repositories;

public class SupportConversationRepository : ISupportConversationRepository
{
    private readonly ApplicationDBContext _context;

    public SupportConversationRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public void Add(SupportConversation conversation)
    {
        _context.SupportConversations.Add(conversation);
    }

    public async Task AddMessageAsync(SupportMessage message, CancellationToken cancellationToken = default)
    {
        await _context.SupportMessages.AddAsync(message, cancellationToken);
    }

    public async Task<SupportConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SupportConversations
            .Include(c => c.Messages)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<SupportConversation?> GetByParticipantsAsync(string customerId, string staffId, bool includeMessages = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(staffId))
        {
            return null;
        }

        IQueryable<SupportConversation> query = _context.SupportConversations;

        if (includeMessages)
        {
            query = query.Include(c => c.Messages);
        }

        return await query
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.StaffId == staffId, cancellationToken);
    }

    public async Task<IReadOnlyList<SupportConversation>> GetForCustomerAsync(string customerId, bool includeMessages = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return Array.Empty<SupportConversation>();
        }

        IQueryable<SupportConversation> query = _context.SupportConversations
            .Where(c => c.CustomerId == customerId);

        if (includeMessages)
        {
            query = query.Include(c => c.Messages);
        }

        return await query
            .OrderByDescending(c => c.UpdatedAtUtc)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportConversation>> GetForStaffAsync(string staffId, bool includeMessages = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(staffId))
        {
            return Array.Empty<SupportConversation>();
        }

        IQueryable<SupportConversation> query = _context.SupportConversations
            .Where(c => c.StaffId == staffId);

        if (includeMessages)
        {
            query = query.Include(c => c.Messages);
        }

        return await query
            .OrderByDescending(c => c.UpdatedAtUtc)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportMessage>> GetMessagesAsync(Guid conversationId, DateTime? newerThanUtc = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SupportMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId);

        if (newerThanUtc.HasValue)
        {
            query = query.Where(m => m.CreatedAtUtc > newerThanUtc.Value);
        }

        var items = await query
            .OrderBy(m => m.CreatedAtUtc)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);

        return items;
    }

}
