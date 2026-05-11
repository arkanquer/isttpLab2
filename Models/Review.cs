public class Review {
    public int Id { get; set; }
    public int Rating { get; set; } // Оцінити гру
    public string Text { get; set; } // Написати текст відгуку
    
    public int GameId { get; set; }
    public virtual Game Game { get; set; }
    
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; }
}