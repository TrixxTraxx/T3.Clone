using System.Security.Claims;
using T3.Clone.Dtos.Threads;
using T3.Clone.Server.Data;
using T3.Clone.Server.Mappers;

namespace T3.Clone.Server.Service;

public class ThreadService(
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor
)
{
    public async Task<ThreadUpdateDto> GetThreads(int? clientVersion)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        var threads = context.MessageThreads
            .Where(x => x.UserId == userId && x.Version > clientVersion)
            .Select(x => new
            {
                //select the thread and its message Ids
                Thread = x,
                MessageIds = x.Messages.Select(m => m.Id).ToList()
            })
            .ToList();

        var threadDtos = threads.Select(thread =>
        {
            var dto = ThreadMapper.Map(thread.Thread);
            dto.MessageIds = thread.MessageIds;
            return dto;
        });
        
        return new ThreadUpdateDto()
        {
            UpdatedThreads = threadDtos.ToList(),
            UpdatedVersion = threads.Any() ? threads.Max(x => x.Thread.Version) : clientVersion ?? 0
        };
    }
}