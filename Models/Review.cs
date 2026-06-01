public class Review {
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    
    public int GameId { get; set; }
    public virtual Game? Game { get; set; }
    
    public int PlayerId { get; set; }
    public virtual Player? Player { get; set; }

    public bool IsDeleted { get; set; } = false;
}