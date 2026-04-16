using ChatApp.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult GetUsers()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var users = _context.Users
            .Where(x => x.Id != currentUserId)
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.ProfilePicture,
                IsOnline = x.IsOnline
            })
            .ToList();

        return Json(users);
    }

    public IActionResult GetMessages(string userId)
    {
        var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var messages = _context.ChatMessages
            .Where(x => (x.SenderId == currentUser && x.ReceiverId == userId)
                     || (x.SenderId == userId && x.ReceiverId == currentUser))
            .OrderBy(x => x.SentAt)
            .ToList();

        return Json(messages);
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null) return BadRequest();

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var path = Path.Combine("wwwroot/uploads", fileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Json(new
        {
            fileUrl = "/uploads/" + fileName,
            fileType = file.ContentType,
            fileName = file.FileName
        });
    }
}