using System;

namespace CourseSelectionApp.Model
{
    public class Course
    {
        public string Id { get; set; }          // 課號
        public string Name { get; set; }        // 課程名稱
        public int Credit { get; set; }         // 學分數
        public string ClassName { get; set; }   // 開課班級
        public string TeacherName { get; set; } // 授課老師

        public override string ToString()
        {
            // 顯示在 TreeView / ListBox 的文字
            return $"{Name}（{Credit}學分） - {ClassName} - {TeacherName}";
        }
    }
}
