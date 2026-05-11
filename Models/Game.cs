public class Game {
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsArchived { get; set; } = false;
    
    public int GenreId { get; set; }
    public virtual Genre Genre { get; set; }
    
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}