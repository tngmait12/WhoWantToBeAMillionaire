using System.ComponentModel.DataAnnotations;

namespace WhoWantToBeAMillionaire.Models
{
    public class TopicModel
    {
        [Key]
        public int Id { get; set; }              // Khóa chính
        public string Name { get; set; }         // Tên chủ đề
        public int Status { get; set; }
    }
}
