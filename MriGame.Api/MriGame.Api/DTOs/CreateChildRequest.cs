namespace MriGame.Api.DTOs;

public class CreateChildRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int Age { get; set; }
    public string Gender { get; set; } = "";
}