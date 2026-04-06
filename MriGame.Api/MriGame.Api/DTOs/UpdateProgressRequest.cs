namespace MriGame.Api.DTOs;

public class UpdateProgressRequest
{
    public int CurrentLevel { get; set; }
    public bool TutorialCompleted { get; set; }
    public bool MriPreparationCompleted { get; set; }
    public bool ScanCompleted { get; set; }
    public int Stars { get; set; }
}