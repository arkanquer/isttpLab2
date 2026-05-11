public class Player {
    public int Id { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Game> Library { get; set; } = new List<Game>(); // Додати гру до своєї бібліотеки
}