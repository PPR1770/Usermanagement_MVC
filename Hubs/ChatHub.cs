using ChatApp.Data;
using ChatApp.Models.ChatModels;
using Microsoft.AspNetCore.SignalR;

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
            FileName = fileName
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

        await Clients.All.SendAsync("UserStatusChanged", userId, true);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.UserIdentifier;

        var user = _context.Users.FirstOrDefault(x => x.Id == userId);
        if (user != null)
        {
            user.IsOnline = false;
            await _context.SaveChangesAsync();
        }

        await Clients.All.SendAsync("UserStatusChanged", userId, false);

        await base.OnDisconnectedAsync(exception);
    }
}