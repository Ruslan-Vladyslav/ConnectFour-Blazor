namespace ConnectFour
{
    public class Player
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool IsAI { get; set; } = false;
    }

    public class GamePlayer
    {
        public Player Player1 { get; set; } = new Player();
        public Player Player2 { get; set; } = new Player();
    }
}
