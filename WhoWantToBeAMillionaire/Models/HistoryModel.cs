namespace WhoWantToBeAMillionaire.Models
{
    public class HistoryModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } // ID của người chơi
        public int RoomId { get; set; } // ID của phòng
        public DateTime PlayedAt { get; set; } // Thời điểm chơi
        public int Score { get; set; } // Điểm số đạt được

        public decimal Reward { get; set; } // Tiền thưởng đạt được

        public bool Completed { get; set; } // Trạng thái hoàn thành game
        public TimeSpan Duration { get; set; } // Thời gian hoàn thành

        public AppUserModel User { get; set; } // Điều hướng đến người dùng
        public RoomModel Room { get; set; } // Điều hướng đến phòng
    }
}
