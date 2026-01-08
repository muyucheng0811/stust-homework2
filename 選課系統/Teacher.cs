using System.Collections.ObjectModel;

namespace 選課系統
{
    public class Teacher
    {
        public string TeacherName { get; set; } = string.Empty;

        public ObservableCollection<Course> TeachingCourses { get; set; }

        public Teacher()
        {
            TeachingCourses = new ObservableCollection<Course>();
        }
    }
}