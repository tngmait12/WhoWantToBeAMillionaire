using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhoWantToBeAMillionaire.Models
{
    public class QuestionModel
    {
        [Key]
        public int Id { get; set; }
        // Khóa chính
        public int TopicId { get; set; }          // Id chủ đề
        public string Content { get; set; }       // Nội dung câu hỏi
        public string CorrectAnswer { get; set; } // Câu trả lời đúng
        public string Wrong_1 { get; set; }       // Đáp án A
        public string Wrong_2 { get; set; }       // Đáp án B
        public string Wrong_3 { get; set; }       // Đáp án C 

        public byte Difficulty { get; set; }
        //public int CreatedBy { get; set; }        // Id người tạo câu hỏi

        // Navigation Properties
        public TopicModel Topic { get; set; }
        public int? RoomId { get; set; } // Liên kết với phòng (có thể null nếu không thuộc phòng nào)
        [JsonIgnore]
        public virtual RoomModel Room { get; set; }

        // Danh sách chứa câu trả lời trộn lẫn
        [NotMapped] // Không lưu trong database
        public List<string> ShuffledAnswers { get; set; }

        public void ShuffleAnswers()
        {
            ShuffledAnswers = new List<string> { CorrectAnswer, Wrong_1, Wrong_2, Wrong_3 };
            Random random = new Random();
            ShuffledAnswers = ShuffledAnswers.OrderBy(x => random.Next()).ToList();
        }
    }
}
