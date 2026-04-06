namespace MriGame.Api.Models;

public class GameProgress
{
    public Guid Id { get; set; }

    public Guid ChildProfileId { get; set; }
    public ChildProfile ChildProfile { get; set; } = null!;

    public int CurrentLevel { get; set; }
    public bool TutorialCompleted { get; set; }
    public bool MriPreparationCompleted { get; set; }
    public bool ScanCompleted { get; set; }
    public int Stars { get; set; }

    public DateTime? LastPlayedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}