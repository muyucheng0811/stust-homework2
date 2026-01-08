namespace 選課系統
{
    public class Course
    {
        // 給預設值 ""，避免 Null 警告
        public string CourseName { get; set; } = string.Empty;
        public string Type { get; set; } = "選修";
        public int Credits { get; set; }
        public string ClassID { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{CourseName} - {ClassID}";
        }
    }
}