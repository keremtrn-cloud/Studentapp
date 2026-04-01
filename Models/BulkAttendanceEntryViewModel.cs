namespace StudentApp.Models
{
    public class BulkAttendanceEntryViewModel
    {
        public string? SelectedDepartment { get; set; }
        public List<string> Departments { get; set; } = new List<string>();
        public List<BulkAttendanceRowViewModel> Rows { get; set; } = new List<BulkAttendanceRowViewModel>();
    }

    public class BulkAttendanceRowViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string Department { get; set; } = "";
        public string Status { get; set; } = "Present";
    }
}
