using System.Text.Json.Serialization;
public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}