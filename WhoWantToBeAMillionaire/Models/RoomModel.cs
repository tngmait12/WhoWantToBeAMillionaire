using Newtonsoft.Json;
using System.Collections.Generic;

namespace WhoWantToBeAMillionaire.Models
{
    public class RoomModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RoomCode { get; set; }
        public string HostPlayerId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual AppUserModel HostPlayer { get; set; }
        public virtual ICollection<PlayerModel> Players { get; set; } = new List<PlayerModel>();

        // Quan hệ với Question
        [JsonIgnore]
        public virtual ICollection<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
    }

}
