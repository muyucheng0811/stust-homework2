using System.Collections.Generic;

namespace CourseSelectionApp.Model
{
    public class Teacher
    {
        public string Id { get; set; }       // 教師代號
        public string Name { get; set; }     // 教師姓名

        public List<Course> Courses { get; set; } = new List<Course>();

        public override string ToString()
        {
            return Name;
        }
    }
}
