namespace LernDotnet.Models;
 
public sealed class Note : IHasTimestamps
{
    public int Id { get; set; }
 
    public string Title { get; set; } = string.Empty;
 
    public string Content { get; set; } = string.Empty;
 
    public DateTimeOffset CreatedAt { get; set; }
 
    public DateTimeOffset UpdatedAt { get; set; }
}
