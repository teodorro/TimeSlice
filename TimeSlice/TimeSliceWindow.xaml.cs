using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace TimeSlice
{
    /// <summary>
    /// Interaction logic for TimeSliceWindow.xaml
    /// </summary>
    public partial class TimeSliceWindow : Window
    {
        private ObservableCollection<FileItem> _files = new ObservableCollection<FileItem>();
        private ImageCreator imageCreator = new ImageCreator();

        public TimeSliceWindow()
        {
            InitializeComponent();
        }

        private void ButtonOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Text files|*.txt"
            };

            if (ofd.ShowDialog(this) == true)
            {
                _files = new ObservableCollection<FileItem>();
                try
                {
                    foreach (var file in ofd.FileNames)
                        _files.Add(new FileItem(file));
                }
                catch (Exception)
                {
                    MessageBox.Show("Что-то пошло не так", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            DataGridFiles.ItemsSource = _files;
            ButtonConvert.IsEnabled = _files.Any();
            //            TextBoxTime.IsEnabled = _files.Any();
            //            TextBoxTime.Text = "0";
            TextBlockTime.Text = "0";

            SliderTime.Maximum = _files.Any() ? _files.Select(x => x.MaxTime).Max() - 1 : 0;
            SliderTime.IsEnabled = _files.Any();
            
            SliderTime.SmallChange = _files.Any() ? FileItem.Discrete : 1;

            
            if (_files.Any())
            {
                Render();
            }
            else
            {
                var height = (int)((Grid) ImageCtrl1.Parent).RowDefinitions[2].ActualHeight;
                var width = (int)((Grid) ImageCtrl1.Parent).ActualWidth;
                ImageCtrl1.Source = imageCreator.CreateBitmapSource(Colors.Gray, width, height);
            }
        }
        
        private void ButtonConvert_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Excel files|*.xlsx"
            };

            if (sfd.ShowDialog(this) == true)
            {
                try
                {
                    var time = SliderTime.Value;
                    var extractor = new SliceExtractor();
                    extractor.WriteFile(sfd.FileName, _files.ToList(), time);
                    Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Что-то пошло не так", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

//        private void TextBoxTime_TextChanged(object sender, TextChangedEventArgs e)
//        {
//            try
//            {
//                double val = 0;
//                if (!Double.TryParse(TextBoxTime.Text.Replace('.', ','), out val))
//                    throw new FormatException("Неверный формат строки");
//
//                TextBoxTime.BorderBrush = new SolidColorBrush(Colors.Black);
//                if (_files.Any())
//                {
//                    ButtonConvert.IsEnabled = true;
//
//                    Render();
//                }
//            }
//            catch (FormatException ex)
//            {
//                MessageBox.Show("Ошибка перевода строки в число");
//                TextBoxTime.BorderBrush = new SolidColorBrush(Colors.Red);
//                ButtonConvert.IsEnabled = false;
//            }
//        }

        private void Render()
        {
            var height = (int) ((Grid) ImageCtrl1.Parent).RowDefinitions[2].ActualHeight;
            var width = (int) ((Grid) ImageCtrl1.Parent).ActualWidth;
            var image = imageCreator.GetImage(_files.ToList(), width, height, (int)SliderTime.Value);
            ImageCtrl1.Source = image;
        }
        
        private void DataGridFiles_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (((PropertyDescriptor)e.PropertyDescriptor).IsBrowsable == false)
                e.Cancel = true;
        }

        private void ImageCtrl1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Render();
        }

        private void SliderTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Render();
        }
    }


}
