namespace WhoWantToBeAMillionaire.Models
{
    public class PlayerModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public int Score { get; set; } = 0;
    }

}
