using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YourNamespace.Models;

namespace YourNamespace.ViewModels
{
    public class IndicatorOption : INotifyPropertyChanged
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public Func<AqiRecord, double> ValueSelector { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        // 指定題目 API
        private const string ApiUrl =
            "https://data.moenv.gov.tw/api/v2/aqx_p_432?api_key=4c89a32a-a214-461b-bf29-30ff32a61a8a&limit=1000&sort=ImportDate%20desc&format=JSON";

        public ObservableCollection<AqiRecord> Records { get; } = new();
        public ObservableCollection<IndicatorOption> Indicators { get; } = new();

        private SeriesCollection _seriesCollection = new();
        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set { _seriesCollection = value; OnPropertyChanged(); }
        }

        private string[] _labels = Array.Empty<string>();
        public string[] Labels
        {
            get => _labels;
            set { _labels = value; OnPropertyChanged(); }
        }

        public Func<double, string> YFormatter { get; } = v => v.ToString("0.##");

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); ((RelayCommand)ReloadCommand).RaiseCanExecuteChanged(); }
        }

        public ICommand ReloadCommand { get; }

        public MainViewModel()
        {
            ReloadCommand = new RelayCommand(async () => await LoadAsync(), () => !IsLoading);

            // ✅ 動態產生「空氣品質指標」CheckBox（你也可依課堂需求增減）
            Indicators.Add(new IndicatorOption { Key = "AQI", DisplayName = "AQI", ValueSelector = r => ToDouble(r.AQI) });
            Indicators.Add(new IndicatorOption { Key = "PM25", DisplayName = "PM2.5", ValueSelector = r => ToDouble(r.PM25) });
            Indicators.Add(new IndicatorOption { Key = "PM10", DisplayName = "PM10", ValueSelector = r => ToDouble(r.PM10) });
            Indicators.Add(new IndicatorOption { Key = "O3", DisplayName = "O3", ValueSelector = r => ToDouble(r.O3) });
            Indicators.Add(new IndicatorOption { Key = "CO", DisplayName = "CO", ValueSelector = r => ToDouble(r.CO) });
            Indicators.Add(new IndicatorOption { Key = "SO2", DisplayName = "SO2", ValueSelector = r => ToDouble(r.SO2) });
            Indicators.Add(new IndicatorOption { Key = "NO2", DisplayName = "NO2", ValueSelector = r => ToDouble(r.NO2) });

            // ✅ 勾選任一指標就更新圖表
            foreach (var opt in Indicators)
            {
                opt.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(IndicatorOption.IsSelected))
                        UpdateChart();
                };
            }
        }

        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;

                using var http = new HttpClient();
                var json = await http.GetStringAsync(ApiUrl);

                var data = JsonConvert.DeserializeObject<AqiApiResponse>(json);
                var list = data?.Records ?? new();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Records.Clear();
                    foreach (var r in list) Records.Add(r);
                });

                // 初次載入：預設勾選 AQI（你也可改成全不勾）
                if (Indicators.All(i => !i.IsSelected) && Indicators.FirstOrDefault() != null)
                    Indicators.First().IsSelected = true;

                UpdateChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"載入 AQI 失敗：{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateChart()
        {
            // 取「每個測站最新一筆」避免同測站重複，並限制前 20 個（讓圖表不爆炸）
            var latestBySite = Records
                .GroupBy(r => r.SiteName)
                .Select(g => g.First())
                .Take(20)
                .ToList();

            Labels = latestBySite.Select(r => r.SiteName).ToArray();

            var selected = Indicators.Where(i => i.IsSelected).ToList();

            var sc = new SeriesCollection();

            foreach (var ind in selected)
            {
                var values = new ChartValues<double>(
                    latestBySite.Select(r => ind.ValueSelector(r))
                );

                sc.Add(new ColumnSeries
                {
                    Title = ind.DisplayName,
                    Values = values
                });
            }

            SeriesCollection = sc;
        }

        private static double ToDouble(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            // API 常回傳字串；用 InvariantCulture 解析更穩
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
