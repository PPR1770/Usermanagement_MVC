namespace ChatApp.Models.ChatModels
{
    public class GroupChat
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string GroupImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedByUserId { get; set; }

        public ICollection<GroupMember> Members { get; set; }
            = new List<GroupMember>();

        public ICollection<ChatMessage> Messages { get; set; }
            = new List<ChatMessage>();
    }
    public class GroupMember
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        public GroupChat Group { get; set; }

        public string UserId { get; set; }

        public bool IsAdmin { get; set; }
    }
}
