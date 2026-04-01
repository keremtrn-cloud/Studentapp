using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }
        
        // Devamsızlık Tarihi
        public DateTime Date { get; set; } = DateTime.Now;
        
        // Durum: 'Present' (Var) veya 'Absent' (Yok)
        public string Status { get; set; } = null!; 

        // Hangi Öğrenci? (İlişki)
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
    }
}