using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace File_copy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Window center position
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        // Browse button
        private void btnBrowseSource_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".*";
            dlg.Filter = "All Files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            string filePath = null;
            if (result == true)
            {
                filePath = dlg.FileName;
                // Use filePath variable to work with the selected file
            }

            Source.Text = filePath;
        }

        // Where to copy button
        private void btnBrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            // Create FolderBrowserDialog
            var dlg = new System.Windows.Forms.FolderBrowserDialog();

            // Show FolderBrowserDialog by calling ShowDialog method
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            // Get the selected folder path and display in a TextBox
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Get folder path from SelectedPath property
                string folderPath = dlg.SelectedPath;

                // Show folder path in TextBox
                Destination.Text = folderPath;
            }
        }

        // Копирует файл блоками по 4096 байт в указанное место
        //private readonly SynchronizationContext UiContext;
        private void Task1(string source, string destination, Action<int> progressCallback)
        {
            try
            {
                using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read))
                using (var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    long totalBytesRead = 0;
                    long fileSize = sourceStream.Length;

                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destinationStream.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        int percentage = (int)(totalBytesRead * 100 / fileSize);
                        progressCallback?.Invoke(percentage);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        // Copy button
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            // Task - асинхронная операция - элементарная единица исполнения
            //Task tsk1 = new Task(Task1);
            try
            {

                string source = Source.Text;
                string destination = Destination.Text;

                // Create a Task to execute the copy operation in the background
                Task task = Task.Run(() =>
                {
                    Task1(source, destination, percentage =>
                    {
                        // Update the progress bar on the UI thread using the Dispatcher property
                        Dispatcher.Invoke(() =>
                        {
                            ProgressBar.Value = percentage;
                        });
                    });
                });
                task.Wait();
                MessageBox.Show("Копирование выполнено!");
                // Wait for the Task to complete

                //// Start запускает Task.
                //tsk1.Start();
                //// Wait ожидает завершения выполнения объекта Task.
                //tsk1.Wait();
                //MessageBox.Show("Копирование выполнено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Dispose - освобождение ресурсов, используемых задачами
                //tsk1.Dispose();
            }
        }
    }
}
