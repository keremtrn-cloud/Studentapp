namespace StudentApp.Models
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int CriticalCount { get; set; }
        public int WarningCount { get; set; }
        public double TodayAttendancePercent { get; set; }

        public int LowRiskCount { get; set; }
        public int MediumRiskCount { get; set; }
        public int HighRiskCount { get; set; }

        public List<DepartmentRiskItem> DepartmentRisks { get; set; } = new List<DepartmentRiskItem>();
        public List<RecentIncidentItem> RecentIncidents { get; set; } = new List<RecentIncidentItem>();
    }

    public class DepartmentRiskItem
    {
        public string Department { get; set; } = "";
        public int TotalRiskPoints { get; set; }
    }

    public class RecentIncidentItem
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public DateTime TriggerDate { get; set; }
        public int RiskScore { get; set; }
        public int AbsenceCount { get; set; }
    }
}
