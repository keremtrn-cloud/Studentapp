using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentApp.Models
{
    [Table("Interventions")]
    public class Intervention
    {
        [Key]
        public int Id { get; set; }
        public string Notes { get; set; } = null!;
        public string Type { get; set; } = null!;
        public DateTime Date { get; set; } = DateTime.Now;

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
    }
}