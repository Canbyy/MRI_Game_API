namespace MriGame.Api.Models;

public class ChildProfile
{
    public Guid Id { get; set; }

    public Guid ParentUserId { get; set; }
    public ParentUser ParentUser { get; set; } = null!;

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int Age { get; set; }
    public Gender Gender { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public GameProgress? Progress { get; set; }
}