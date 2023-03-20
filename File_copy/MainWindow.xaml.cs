using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace File_copy;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Window center position
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    // Browse button
    private void btnBrowseSource_Click(object sender, RoutedEventArgs e)
    {
        // Create OpenFileDialog
        var dlg = new OpenFileDialog();

        // Set filter for file extension and default file extension
        dlg.DefaultExt = ".*";
        dlg.Filter = "All Files (*.*)|*.*";

        // Display OpenFileDialog by calling ShowDialog method
        var result = dlg.ShowDialog();

        // Get the selected file name and display in a TextBox
        string filePath = null;
        if (result == true) filePath = dlg.FileName;
        // Use filePath variable to work with the selected file
        Source.Text = filePath;
    }

    // Where to copy button
    private void btnBrowseDestination_Click(object sender, RoutedEventArgs e)
    {
        // Create FolderBrowserDialog
        var dlg = new FolderBrowserDialog();

        // Show FolderBrowserDialog by calling ShowDialog method
        var result = dlg.ShowDialog();

        // Get the selected folder path and display in a TextBox
        if (result == System.Windows.Forms.DialogResult.OK)
        {
            // Get folder path from SelectedPath property
            var folderPath = dlg.SelectedPath;

            // Show folder path in TextBox
            Destination.Text = folderPath;
        }
    }

    // Copies the file in blocks of 4096 bytes to the specified location
    private void Task1(string source, string destination, Action<int> progressCallback)
    {
        // Get filename from source string
        var fileName = Path.GetFileName(source);

        // Add filename to destination string
        destination = Path.Combine(destination, fileName);

        try
        {
            using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            using (var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
            {
                var buffer = new byte[4096];
                int bytesRead;
                long totalBytesRead = 0;
                var fileSize = sourceStream.Length;

                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destinationStream.Write(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    var percentage = (int)(totalBytesRead * 100 / fileSize);
                    progressCallback?.Invoke(percentage);
                }

                sourceStream.Close();
                destinationStream.Close();
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
        // Task - an asynchronous operation - an elementary unit of execution
        Task task = null;
        try
        {
            var source = Source.Text;
            var destination = Destination.Text;

            // Create a Task to execute the copy operation in the background
            task = Task.Run(() =>
            {
                Task1(source, destination, percentage =>
                {
                    // Update the progress bar on the UI thread using the Dispatcher property
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar.Value = percentage;
                        if (percentage == 100) MessageBox.Show("Copy completed!");
                    });
                });
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}