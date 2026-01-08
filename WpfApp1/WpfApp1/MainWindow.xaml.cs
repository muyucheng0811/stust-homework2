using CourseSelectionApp.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace CourseSelectionApp
{
    public partial class MainWindow : Window
    {
        private List<Student> _students = new List<Student>();
        private List<Teacher> _teachers = new List<Teacher>();
        private List<Course> _allCourses = new List<Course>();

        public MainWindow()
        {
            InitializeComponent();
            InitData();   // 建立假資料
            InitUI();     // 綁定到介面
        }

        // ---------------------------------------
        // 1. 建立三位老師 + 課程 & 三位學生
        // ---------------------------------------
        private void InitData()
        {
            // 老師 1
            var t1 = new Teacher { Id = "T001", Name = "陳定宏" };
            t1.Courses.Add(new Course
            {
                Id = "C001",
                Name = "視窗程式設計",
                Credit = 3,
                ClassName = "四技資工2A",
                TeacherName = t1.Name
            });
            t1.Courses.Add(new Course
            {
                Id = "C002",
                Name = "程式設計概論",
                Credit = 3,
                ClassName = "四技資工1甲",
                TeacherName = t1.Name
            });
            t1.Courses.Add(new Course
            {
                Id = "C003",
                Name = "資料結構",
                Credit = 3,
                ClassName = "四技資工2丙",
                TeacherName = t1.Name
            });

            // 老師 2
            var t2 = new Teacher { Id = "T002", Name = "林怡君" };
            t2.Courses.Add(new Course
            {
                Id = "C004",
                Name = "資料庫系統",
                Credit = 3,
                ClassName = "四技資管3乙",
                TeacherName = t2.Name
            });
            t2.Courses.Add(new Course
            {
                Id = "C005",
                Name = "系統分析與設計",
                Credit = 3,
                ClassName = "四技資管3乙",
                TeacherName = t2.Name
            });
            t2.Courses.Add(new Course
            {
                Id = "C006",
                Name = "行動程式設計",
                Credit = 2,
                ClassName = "四技資管4甲",
                TeacherName = t2.Name
            });

            // 老師 3
            var t3 = new Teacher { Id = "T003", Name = "王美玲" };
            t3.Courses.Add(new Course
            {
                Id = "C007",
                Name = "網頁設計",
                Credit = 2,
                ClassName = "四技資工1乙",
                TeacherName = t3.Name
            });
            t3.Courses.Add(new Course
            {
                Id = "C008",
                Name = "多媒體設計",
                Credit = 3,
                ClassName = "四技資工2甲",
                TeacherName = t3.Name
            });
            t3.Courses.Add(new Course
            {
                Id = "C009",
                Name = "UI/UX 設計",
                Credit = 2,
                ClassName = "四技資工3甲",
                TeacherName = t3.Name
            });

            _teachers.Add(t1);
            _teachers.Add(t2);
            _teachers.Add(t3);

            _allCourses = _teachers.SelectMany(t => t.Courses).ToList();

            // 三位學生
            _students.Add(new Student { Id = "S001", Name = "陳小明" });
            _students.Add(new Student { Id = "S002", Name = "林怡君" });
            _students.Add(new Student { Id = "S003", Name = "王美玲" });
        }

        // ---------------------------------------
        // 2. 把資料綁定到畫面
        // ---------------------------------------
        private void InitUI()
        {
            // 學生 ComboBox
            cboStudents.ItemsSource = _students;
            cboStudents.SelectedIndex = 0;

            // TreeView：以教師分類
            tvTeachers.Items.Clear();
            foreach (var teacher in _teachers)
            {
                var teacherNode = new TreeViewItem
                {
                    Header = $"{teacher.Name} ({teacher.Courses.Count}門課)",
                    Tag = teacher,
                    IsExpanded = true
                };

                foreach (var course in teacher.Courses)
                {
                    var courseNode = new TreeViewItem
                    {
                        Header = course.ToString(),
                        Tag = course
                    };
                    teacherNode.Items.Add(courseNode);
                }

                tvTeachers.Items.Add(teacherNode);
            }

            // 所有課程 ListBox
            lstAllCourses.ItemsSource = _allCourses;

            // 顯示第一位學生的已選課程
            cboStudents_SelectionChanged(null, null);
        }

        // ---------------------------------------
        // 3. 學生切換
        // ---------------------------------------
        private void cboStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var student = cboStudents.SelectedItem as Student;
            if (student == null) return;

            lstSelectedCourses.ItemsSource = student.SelectedCourses;
            lstSelectedCourses.Items.Refresh();
            txtStatus.Text = $"目前學生：{student}";
        }

        // 取得目前 Tab 中選到的課程
        private Course GetSelectedCourseFromCurrentTab()
        {
            // Tab0：TreeView
            if (tabCourses.SelectedIndex == 0)
            {
                if (tvTeachers.SelectedItem is TreeViewItem item)
                {
                    return item.Tag as Course;
                }
            }
            // Tab1：所有課程 ListBox
            else
            {
                return lstAllCourses.SelectedItem as Course;
            }

            return null;
        }

        // ---------------------------------------
        // 4. 選課
        // ---------------------------------------
        private void btnAddCourse_Click(object sender, RoutedEventArgs e)
        {
            var student = cboStudents.SelectedItem as Student;
            if (student == null)
            {
                MessageBox.Show("請先選擇學生！", "提示");
                return;
            }

            var course = GetSelectedCourseFromCurrentTab();
            if (course == null)
            {
                MessageBox.Show("請在左邊選一門課程！", "提示");
                return;
            }

            // 檢查是否已選過
            if (student.SelectedCourses.Any(c => c.Id == course.Id))
            {
                MessageBox.Show("已選過這門課囉！", "提示");
                return;
            }

            student.SelectedCourses.Add(course);
            lstSelectedCourses.Items.Refresh();
            txtStatus.Text = $"[{student.Name}] 已選：{course.Name}";
        }

        // ---------------------------------------
        // 5. 退選（從右邊 ListBox 選）
        // ---------------------------------------
        private void btnDropCourse_Click(object sender, RoutedEventArgs e)
        {
            var student = cboStudents.SelectedItem as Student;
            if (student == null)
            {
                MessageBox.Show("請先選擇學生！", "提示");
                return;
            }

            var course = lstSelectedCourses.SelectedItem as Course;
            if (course == null)
            {
                MessageBox.Show("請在右邊選要退選的課程！", "提示");
                return;
            }

            student.SelectedCourses.Remove(course);
            lstSelectedCourses.Items.Refresh();
            txtStatus.Text = $"[{student.Name}] 已退選：{course.Name}";
        }

        // ---------------------------------------
        // 6. 存檔成 JSON
        // ---------------------------------------
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 直接把所有學生的選課狀況存成一個 json
                string json = JsonConvert.SerializeObject(
                 _students,
                   Newtonsoft.Json.Formatting.Indented);

                string filePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "選課紀錄.json");

                File.WriteAllText(filePath, json, Encoding.UTF8);

                txtStatus.Text = $"選課資料已存到：{filePath}";
                MessageBox.Show("存檔完成！", "訊息");
            }
            catch (Exception ex)
            {
                MessageBox.Show("存檔失敗：" + ex.Message, "錯誤");
            }
        }
    }
}
