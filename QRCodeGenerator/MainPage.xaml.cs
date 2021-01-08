using System;
using System.IO;
using System.Linq;
using System.Net;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.System;
using Microsoft.Data.Sqlite;

namespace QRCodeGenerator
{
    /// <summary>
    /// Main Page
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int size = 500;
        string selected = "png", color = "000000", url = "http://api.qrserver.com/v1/create-qr-code/?data=Example&size=500x500&color=000000&format=png";
        public MainPage()
        {
            this.InitializeComponent();
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            // navigate to gallery page
            Frame.Navigate(typeof(EditPage));
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            // navigate to about page
            Frame.Navigate(typeof(aboutPage));
        }
        private void ListBoxFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get selected format (png, jpg etc.)
            selected = ((ComboBoxItem)lbFormat.SelectedItem).Content.ToString();
        }
        private void btnSetSize_Click(object sender, RoutedEventArgs e)
        {
            // set size and handle too low or too high value
            if (tbSetSize.Text != "") 
                size = int.Parse(tbSetSize.Text);
            if (size < 100)
            {
                size = 100;
                tbSetSize.Text = size.ToString();
            } 
            if (size > 1000)
            {
                size = 1000;
                tbSetSize.Text = size.ToString();
            }
        }
        private void cpColorPicked_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            // get selected color of qr code
            color = cpColorPicked.Color.ToString().Remove(0, 3);
        }
        private void tbSetSize_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            // allow user to type only digits
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            // generate url of qr code
            var content = WebUtility.UrlEncode(tbQRData.Text);
            if (content == "")
                content = "Example";
            url = "http://api.qrserver.com/v1/create-qr-code/?data=" + content + "&size=" + size + "x" + size + "&color=" + color;
            imgGenerated.Source = new BitmapImage(new Uri(url));
            url += $"&format={selected}";
        }
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // save qr code to local machine and sqlite database
            var title = tbQRTitle.Text;
            if(title == "")
                title = "Title";
            var myFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            myFilter.AllowUI = false;
            // get image from http
            Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient(myFilter);
            // use generated url
            Windows.Web.Http.HttpResponseMessage result = await client.GetAsync(new Uri(url));

            // save file with title and selected format
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{title}.{selected}", CreationCollisionOption.GenerateUniqueName);
            using (var filestream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await result.Content.WriteToStreamAsync(filestream);
                await filestream.FlushAsync();
                // open local folder where it's saved
                await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
            }
            // add to database
            AddData(title, url);
        }
        public static void AddData(string Title, string Url)
        {
            // get path of sqliteData.db
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteData.db");
            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                // open database
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;
                
                // add values to database
                insertCommand.CommandText = "INSERT INTO QRCodes VALUES (NULL, @Entry1, @Entry2);";
                insertCommand.Parameters.AddWithValue("@Entry1",Title);
                insertCommand.Parameters.AddWithValue("@Entry2", Url);

                insertCommand.ExecuteReader();

                // close database
                db.Close();
            }
        }
    }
}
