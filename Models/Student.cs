using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace StudentApp.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Fullname")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Department")]
        public string Department { get; set; } = "";

        [Display(Name = "GradeLevel")]
        public int GradeLevel { get; set; }

        [Display(Name = "RiskScore")]
        public int RiskScore { get; set; }

        [Display(Name = "Mail")]
        public string ParentEmail { get; set; } = null!;

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
    }
}
