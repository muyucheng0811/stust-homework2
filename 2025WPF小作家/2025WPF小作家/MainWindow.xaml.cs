using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfNotepad
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. 初始化字型列表
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                cmbFontFamily.Items.Add(font);
            }
            cmbFontFamily.SelectedIndex = 0;

            // 2. 初始化字體大小
            List<double> sizes = new List<double>() { 8, 10, 12, 14, 16, 18, 24, 36, 48, 72 };
            foreach (double size in sizes)
            {
                cmbFontSize.Items.Add(size);
            }
            cmbFontSize.SelectedIndex = 2; // 預設 12

            // 3. 初始化顏色列表 (修正 Null 警告)
            var colors = typeof(Colors).GetProperties();
            foreach (var colorProp in colors)
            {
                // 安全地取得顏色值
                var colorValue = colorProp.GetValue(null, null);

                // 只有當取到的值確實是 Color 型別時才加入
                if (colorValue is Color color)
                {
                    cmbFontColor.Items.Add(new { Name = colorProp.Name, Color = color });
                    cmbBackColor.Items.Add(new { Name = colorProp.Name, Color = color });
                }
            }

            // 預設選中黑色 (Black) 與 透明/白色
            // 這裡簡單設定為索引 7 (通常是 Black，視系統排序而定)，也可透過程式碼搜尋 Black
            cmbFontColor.SelectedIndex = 7;

            UpdateStatus("應用程式已就緒");
        }

        // --- 輔助方法：更新 StatusBar ---
        private void UpdateStatus(string message)
        {
            StatusText.Text = $"{DateTime.Now:HH:mm:ss} | {message}";
        }

        // --- 功能：開新檔案 ---
        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            MainEditor.Document.Blocks.Clear();
            UpdateStatus("已建立新檔案");
        }

        // --- 功能：清除全部 ---
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            MainEditor.Document.Blocks.Clear();
            UpdateStatus("已清除所有內容");
        }

        // --- 功能：開啟舊檔 ---
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|Text File (*.txt)|*.txt|All files (*.*)|*.*";

            if (dlg.ShowDialog() == true)
            {
                FileStream? fileStream = null;
                try
                {
                    fileStream = new FileStream(dlg.FileName, FileMode.Open);
                    TextRange range = new TextRange(MainEditor.Document.ContentStart, MainEditor.Document.ContentEnd);

                    if (System.IO.Path.GetExtension(dlg.FileName).ToLower() == ".rtf")
                    {
                        range.Load(fileStream, DataFormats.Rtf);
                    }
                    else
                    {
                        range.Load(fileStream, DataFormats.Text);
                    }
                    UpdateStatus($"已開啟檔案: {dlg.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("開啟失敗: " + ex.Message);
                }
                finally
                {
                    fileStream?.Close();
                }
            }
        }

        // --- 功能：儲存檔案 (RTF 與 HTML) ---
        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|HTML File (*.html)|*.html";

            if (dlg.ShowDialog() == true)
            {
                string ext = System.IO.Path.GetExtension(dlg.FileName).ToLower();

                if (ext == ".rtf")
                {
                    FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
                    TextRange range = new TextRange(MainEditor.Document.ContentStart, MainEditor.Document.ContentEnd);
                    range.Save(fileStream, DataFormats.Rtf);
                    fileStream.Close();
                    UpdateStatus($"已儲存為 RTF: {dlg.FileName}");
                }
                else if (ext == ".html")
                {
                    string htmlContent = ConvertToSimpleHtml(MainEditor.Document);
                    File.WriteAllText(dlg.FileName, htmlContent);
                    UpdateStatus($"已儲存為 HTML: {dlg.FileName}");
                }
            }
        }

        // --- 功能：簡易 HTML 轉換器 ---
        private string ConvertToSimpleHtml(FlowDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html><body>");

            foreach (Block block in doc.Blocks)
            {
                if (block is Paragraph p)
                {
                    sb.Append("<p>");
                    TextRange range = new TextRange(p.ContentStart, p.ContentEnd);
                    sb.Append(range.Text);
                    sb.AppendLine("</p>");
                }
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        // --- 功能：字型樣式 (粗體/斜體/底線) ---
        private void Style_Click(object sender, RoutedEventArgs e)
        {
            // 即使沒有選取文字，通常也允許切換樣式以便輸入新文字時套用，
            // 但這裡為了簡單，若無選取或焦點不在編輯器則不做動作
            if (sender is not System.Windows.Controls.Primitives.ToggleButton btn) return;

            // 為了讓樣式生效，確保焦點在編輯器
            MainEditor.Focus();

            if (btn == btnBold)
            {
                var currentWeight = MainEditor.Selection.GetPropertyValue(TextElement.FontWeightProperty);
                var newWeight = (currentWeight != DependencyProperty.UnsetValue && (FontWeight)currentWeight == FontWeights.Bold)
                    ? FontWeights.Normal : FontWeights.Bold;
                MainEditor.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, newWeight);
                UpdateStatus("切換粗體");
            }
            else if (btn == btnItalic)
            {
                var currentStyle = MainEditor.Selection.GetPropertyValue(TextElement.FontStyleProperty);
                var newStyle = (currentStyle != DependencyProperty.UnsetValue && (FontStyle)currentStyle == FontStyles.Italic)
                    ? FontStyles.Normal : FontStyles.Italic;
                MainEditor.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, newStyle);
                UpdateStatus("切換斜體");
            }
            else if (btn == btnUnderline)
            {
                var currentDecor = MainEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
                if (currentDecor != DependencyProperty.UnsetValue && currentDecor == TextDecorations.Underline)
                    MainEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                else
                    MainEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                UpdateStatus("切換底線");
            }
        }

        // --- 功能：改變字型 ---
        private void CmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontFamily.SelectedItem != null)
            {
                MainEditor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, cmbFontFamily.SelectedItem);
                UpdateStatus($"變更字型: {cmbFontFamily.SelectedItem}");
                MainEditor.Focus();
            }
        }

        // --- 功能：改變字體大小 ---
        private void CmbFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontSize.SelectedItem != null)
            {
                MainEditor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, cmbFontSize.SelectedItem);
                UpdateStatus($"變更大小: {cmbFontSize.SelectedItem}");
                MainEditor.Focus();
            }
        }

        // --- 功能：改變字體顏色 ---
        private void CmbFontColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontColor.SelectedItem != null)
            {
                dynamic selectedItem = cmbFontColor.SelectedItem;
                // 因為我們在初始化時存入的是 Color 物件，這裡直接用 SolidColorBrush
                Color c = selectedItem.Color;
                MainEditor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(c));
                UpdateStatus($"變更字色: {selectedItem.Name}");
                MainEditor.Focus();
            }
        }

        // --- 功能：改變背景顏色 ---
        private void CmbBackColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBackColor.SelectedItem != null)
            {
                dynamic selectedItem = cmbBackColor.SelectedItem;
                Color c = selectedItem.Color;
                MainEditor.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(c));
                UpdateStatus($"變更背景: {selectedItem.Name}");
                MainEditor.Focus();
            }
        }

        // --- 介面更新：游標移動時更新 Toolbar 狀態 ---
        private void MainEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object temp = MainEditor.Selection.GetPropertyValue(TextElement.FontWeightProperty);
            btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));

            temp = MainEditor.Selection.GetPropertyValue(TextElement.FontStyleProperty);
            btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));

            // 這裡可以繼續實作底線按鈕的狀態更新...
        }
    }
}