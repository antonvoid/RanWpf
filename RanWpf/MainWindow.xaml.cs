using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Win32;

namespace RanWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private long pointertime { get; set; }
        private long pointer {  get; set; }
        private long len {  get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //открываем файл для анализа
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "(*.dat)|*.dat";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                textbox1.Text = selectedFileName;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textbox1.Text == "")
                    throw new Exception("Выберете файл");

                string path = textbox1.Text;
                double max = bar.Maximum;

                FileInfo fileInfo = new FileInfo(path);
                len = fileInfo.Length;
                const long part = 2048000;

                //создаем строчки таблицы
                Line[] lines = new Line[10];
                for (int i = 0; i < 10; i++)
                {
                    lines[i] = new Line($"Кадр {i + 1}", 0, 0, 0);
                }

                Byte[] c = new Byte[part];
                Examination exam = new Examination();

                //создаем событие для вычисления скорости
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(0.1);
                timer.Tick += Timer_Tick;
                timer.Start();

                using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    int i = 0;
                    pointer = 0;
                  
                    await Task.Run(() =>
                    {
                        while (pointer <= len - part)
                        {
                            i++;
                            // начинаем считывать файл
                            c = reader.ReadBytes((int)part);
                            pointer += exam.SearchFrame(c, lines);
                            if (pointer <= len - part)
                            {
                                reader.BaseStream.Seek(pointer, 0);
                            }
                            else { break; }

                            //обновляем progressbar
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                bar.Value = 1000 * (1 - (double)(len - pointer) / len);
                            });

                        }
                    });
                    timer.Stop();

                    if (len - pointer >= 2048)
                    {
                        c = reader.ReadBytes((int)(len - pointer));
                        exam.SearchFrame(c, lines);
                    }
                    texttime.Text = "Процесс завершен";
                }
                bar.Value = max;

                // вычисляем строку "Итого:"
                int AllFrame = 0;
                int AllErrorNumbering = 0;
                int AllErrorCrc = 0;
                foreach (Line l in lines)
                {
                    data.Items.Add(l);
                    AllFrame += l.Quantity;
                    AllErrorNumbering += l.NumberingError;
                    AllErrorCrc += l.CrcError;
                }
                Line line = new Line("Итого:", AllFrame, AllErrorNumbering, AllErrorCrc);
                data.Items.Add(line);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }

        }
        // метод для вычисления скорости и времени до конца сканирования
        private void Timer_Tick(object sender, EventArgs e)
        {
            texttime.Text = $"Скорость сканирования {10*(pointer-pointertime)/1024} Мб/с.\n" +
                $" До конца сканирования осталось {(len-pointer)/(10*(pointer - pointertime+1))} с";
            pointertime = pointer;
        }
    }
}