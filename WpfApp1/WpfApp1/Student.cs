using System.Collections.Generic;

namespace CourseSelectionApp.Model
{
    public class Student
    {
        public string Id { get; set; }                      // 學號
        public string Name { get; set; }                    // 姓名
        public List<Course> SelectedCourses { get; set; }   // 已選課程

            = new List<Course>();

        public override string ToString()
        {
            // 顯示在 ComboBox 的文字
            return $"{Id} {Name}";
        }
    }
}
