using ChatApp.Data;
using ChatApp.Models.ChatModels;
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
                IsOnline = x.IsOnline,
                LastSeenAt = x.LastSeenAt
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

    [HttpPost]
    public async Task<IActionResult> CreateGroup(string groupName, List<string> memberIds)
    {
        try
        {
            memberIds ??= new List<string>();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var group = new GroupChat
            {
                Name = groupName,
                CreatedByUserId = userId,
                GroupImageUrl = "/images/default-user.png",
                CreatedAt = DateTime.UtcNow.Date
            };

            _context.GroupChats.Add(group);

            var rows1 = await _context.SaveChangesAsync();

            Console.WriteLine("Rows inserted first save: " + rows1);

            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                IsAdmin = true
            });

            foreach (var memberId in memberIds)
            {
                _context.GroupMembers.Add(new GroupMember
                {
                    GroupId = group.Id,
                    UserId = memberId,
                });
            }

            var rows2 = await _context.SaveChangesAsync();

            Console.WriteLine("Rows inserted second save: " + rows2);

            return Ok(new
            {
                success = true,
                groupId = group.Id
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    public IActionResult GetGroups()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        var groups = _context.GroupMembers
            .Where(x => x.UserId == userId)
            .Select(x => new
            {
                x.Group.Id,
                x.Group.Name
            })
            .ToList();

        return Json(groups);
    }

    public IActionResult GetGroupMessages(int groupId)
    {
        var messages = _context.ChatMessages
            .Where(x => x.GroupId == groupId)
            .OrderBy(x => x.SentAt)
            .ToList();

        return Json(messages);
    }

    [HttpPost]
    public async Task<IActionResult> AddMembers(int groupId, List<string> memberIds)
    {
        foreach (var id in memberIds)
        {
            bool exists =
            _context.GroupMembers.Any(x =>

            x.GroupId == groupId

            &&

            x.UserId == id);

            if (!exists)
            {
                _context.GroupMembers.Add(

                 new GroupMember
                 {

                     GroupId = groupId,

                     UserId = id

                 });

            }
        }

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult>
 RemoveMember(int groupId, string userId)
    {
        var member =

        _context.GroupMembers

        .FirstOrDefault(x =>

        x.GroupId == groupId

        &&

        x.UserId == userId);

        if (member == null)
            return NotFound();


        _context.GroupMembers
        .Remove(member);

        await _context
        .SaveChangesAsync();

        return Ok();

    }
    [HttpPost]
    public async Task<IActionResult> LeaveGroup(int groupId)
    {
        var userId =
        User.FindFirstValue(
        ClaimTypes.NameIdentifier);

        var member =

        _context.GroupMembers

        .FirstOrDefault(x =>

        x.GroupId == groupId

        &&

        x.UserId == userId);

        if (member == null)
            return NotFound();

        _context.GroupMembers
        .Remove(member);

        await _context
        .SaveChangesAsync();

        return Ok();
    }
    [HttpPost]
    public async Task<IActionResult> RenameGroup(int groupId, string name)
    {
        var group = await _context.GroupChats.FindAsync(groupId);
        if (group == null) return NotFound();
        group.Name = name;
        await _context.SaveChangesAsync();
        return Ok();
    }

    public IActionResult GetGroupDetails(
    int groupId)
    {
        var group =
        _context.GroupChats
        .Where(g => g.Id == groupId)

        .Select(g => new
        {

            g.Id,

            g.Name,

            MemberCount =
             g.Members.Count(),

            Members =
             g.Members
              .Select(m => new
              {

                  m.UserId,

                  UserName =
               _context.Users
                .Where(u =>
                  u.Id == m.UserId)

                .Select(u => u.UserName)

                .FirstOrDefault()

              }).ToList()

        })

        .FirstOrDefault();

        return Json(group);
    }


}
