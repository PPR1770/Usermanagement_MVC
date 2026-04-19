using ChatApp.Data;
using ChatApp.Models.ChatModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendMessage(string receiverId, string message, string fileUrl = null, string fileType = null, string fileName = null)
    {
        var senderId = Context.UserIdentifier;

        var msg = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Message = message,
            FileUrl = fileUrl,
            FileType = fileType,
            FileName = fileName,
            IsDelivered = true
        };

        _context.ChatMessages.Add(msg);
        await _context.SaveChangesAsync();

        await Clients.User(receiverId).SendAsync("ReceiveMessage", msg);
    }

    public async Task Typing(string receiverId)
    {
        var senderId = Context.UserIdentifier;

        await Clients.User(receiverId)
            .SendAsync("UserTyping", senderId);
    }

    public async Task MarkAsRead(int messageId)
    {
        var msg = await _context.ChatMessages.FindAsync(messageId);

        if (msg != null)
        {
            msg.IsRead = true;
            await _context.SaveChangesAsync();

            await Clients.User(msg.SenderId)
                .SendAsync("MessageSeen", messageId);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;

        var user = _context.Users.FirstOrDefault(x => x.Id == userId);
        if (user != null)
        {
            user.IsOnline = true;
            await _context.SaveChangesAsync();
        }

        await Clients.All.SendAsync("UserStatusChanged", userId, true, null);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.UserIdentifier;

        var user = _context.Users.FirstOrDefault(x => x.Id == userId);
        if (user != null)
        {
            user.IsOnline = false;
            user.LastSeenAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        await Clients.All.SendAsync("UserStatusChanged", userId, false, DateTime.UtcNow);
        await base.OnDisconnectedAsync(exception);
    }

    // Group chat
    public async Task JoinGroup(string groupId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            groupId);
    }

    public async Task LeaveGroup(string groupId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            groupId);
    }

    public async Task SendGroupMessage(string groupId, string message, string fileUrl = null, string fileType = null, string fileName = null)
    {
        var senderId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(senderId))
            throw new HubException("Unauthenticated user");

        if (!int.TryParse(groupId, out var gid))
            throw new HubException("Invalid group id");

        var group = await _context.GroupChats.FindAsync(gid);
        if (group == null)
            throw new HubException("Group not found");

        var msg = new ChatMessage
        {
            SenderId = senderId,
            GroupId = gid,
            Message = message,
            FileUrl = fileUrl,
            FileType = fileType,
            FileName = fileName,
            IsDelivered = true
        };

        _context.ChatMessages.Add(msg);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // log ex and rethrow a clear hub exception
            throw new HubException("Failed to save group message");
        }

        await Clients.Group(groupId).SendAsync("ReceiveGroupMessage", msg);
    }
    public async Task GroupTyping(string groupId)
    {
        await Clients.OthersInGroup(groupId)
        .SendAsync("GroupTyping", Context.UserIdentifier);
    }
}