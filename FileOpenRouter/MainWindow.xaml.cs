using FileOpenRouter.Models;
using FileOpenRouter.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace FileOpenRouter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<RouteRule> _rules;

        public MainWindow()
        {
            InitializeComponent();
            WindowBackdropHelper.Apply(this);
            Title = "文件打开路由器 " + ConfigService.GetConfigPath();
            LoadConfigToView();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (InfoTextBlock.Visibility == Visibility.Visible)
            {
                InfoTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                InfoTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void AddRule_Click(object sender, RoutedEventArgs e)
        {
            var rule = new RouteRule
            {
                Name = "新规则",
                Enabled = true
            };

            _rules.Add(rule);
            RulesGrid.SelectedItem = rule;
            RulesGrid.ScrollIntoView(rule);
        }

        private void DeleteSelectedRule_Click(object sender, RoutedEventArgs e)
        {
            var selectedRule = RulesGrid.SelectedItem as RouteRule;
            if (selectedRule == null)
            {
                ShowInfo("请先选择一条规则。");
                return;
            }

            _rules.Remove(selectedRule);
        }

        private void ChooseRuleFolder_Click(object sender, RoutedEventArgs e)
        {
            var selectedRule = RulesGrid.SelectedItem as RouteRule;
            if (selectedRule == null)
            {
                ShowInfo("请先选择一条规则。");
                return;
            }

            var selectedPath = ChooseFolder(selectedRule.Folder);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                selectedRule.Folder = selectedPath;
                RulesGrid.Items.Refresh();
            }
        }

        private void ChooseRuleProgram_Click(object sender, RoutedEventArgs e)
        {
            var selectedRule = RulesGrid.SelectedItem as RouteRule;
            if (selectedRule == null)
            {
                ShowInfo("请先选择一条规则。");
                return;
            }

            var selectedPath = ChooseProgram(selectedRule.Program);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                selectedRule.Program = selectedPath;
                RulesGrid.Items.Refresh();
            }
        }

        private void ChooseFallbackProgram_Click(object sender, RoutedEventArgs e)
        {
            var selectedPath = ChooseProgram(FallbackProgramTextBox.Text);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                FallbackProgramTextBox.Text = selectedPath;
            }
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RulesGrid.CommitEdit();
                RulesGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

                var config = new RouterConfig
                {
                    Rules = _rules.ToList(),
                    FallbackProgram = FallbackProgramTextBox.Text.Trim()
                };

                var validationMessage = ConfigValidationService.ValidateForSave(config);
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    ShowError(validationMessage);
                    return;
                }

                ConfigService.Save(config);
                ShowInfo("配置已保存。");
            }
            catch (Exception ex)
            {
                ShowError("保存配置失败：" + Environment.NewLine + ex.Message);
            }
        }

        private void LoadConfigToView()
        {
            try
            {
                var config = ConfigService.Load();
                _rules = new ObservableCollection<RouteRule>(config.Rules ?? Enumerable.Empty<RouteRule>());
                RulesGrid.ItemsSource = _rules;
                FallbackProgramTextBox.Text = config.FallbackProgram ?? string.Empty;
            }
            catch (Exception ex)
            {
                _rules = new ObservableCollection<RouteRule>();
                RulesGrid.ItemsSource = _rules;
                FallbackProgramTextBox.Text = string.Empty;
                ShowError(ex.Message);
            }
        }

        private static string ChooseFolder(string currentPath)
        {
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                dialog.Description = "选择要匹配的文件夹";
                dialog.ShowNewFolderButton = true;

                if (!string.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath))
                {
                    dialog.SelectedPath = currentPath;
                }

                return dialog.ShowDialog() == WinForms.DialogResult.OK
                    ? dialog.SelectedPath
                    : null;
            }
        }

        private static string ChooseProgram(string currentPath)
        {
            var dialog = new OpenFileDialog
            {
                Title = "选择打开程序",
                Filter = "可执行程序 (*.exe)|*.exe",
                CheckFileExists = true,
                Multiselect = false
            };

            if (!string.IsNullOrWhiteSpace(currentPath) && File.Exists(currentPath))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(currentPath);
                dialog.FileName = currentPath;
            }

            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }

        private static void ShowInfo(string message)
        {
            MessageBox.Show(message, "FileOpenRouter", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(message, "FileOpenRouter", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public sealed class RoundedClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 ||
                !(values[0] is double width) ||
                !(values[1] is double height) ||
                width <= 0 ||
                height <= 0)
            {
                return Geometry.Empty;
            }

            var radius = 0d;
            if (parameter != null)
            {
                double.TryParse(parameter.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out radius);
            }

            return new RectangleGeometry(new Rect(0, 0, width, height), radius, radius);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
