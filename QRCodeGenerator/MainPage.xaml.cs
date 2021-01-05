using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.System;
using Windows.Networking.BackgroundTransfer;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using System.Xml;
using System.Xml.Linq;
using System.Data.Common;
using DataAccessLibrary;
using Microsoft.Data.Sqlite;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QRCodeGenerator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int size = 500;
        string selected = "png", color = "000000", url = "https://api.qrserver.com/v1/create-qr-code/?size=150x150&amp;data=Example";
        
        public MainPage()
        {
            this.InitializeComponent();
        }
      
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(EditPage));
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
             Frame.Navigate(typeof(aboutPage));
        }

        private void ListBoxFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             selected = ((ComboBoxItem)lbFormat.SelectedItem).Content.ToString();
        }

        private void btnSetSize_Click(object sender, RoutedEventArgs e)
        {
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
            color = cpColorPicked.Color.ToString().Remove(0, 3);
        }

        private void tbSetSize_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            var content = WebUtility.UrlEncode(tbQRData.Text);
            if (content == "")
                content = "Example";
            url = "http://api.qrserver.com/v1/create-qr-code/?data=" + content + "&size=" + size + "x" + size + "&color=" + color;
            imgGenerated.Source = new BitmapImage(new Uri(url));
            url += $"&format={selected}";
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var title = tbQRTitle.Text;
            if(title == "")
                title = "Title";
            var myFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            myFilter.AllowUI = false;
            Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient(myFilter);
            Windows.Web.Http.HttpResponseMessage result = await client.GetAsync(new Uri(url));

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{title}.{selected}", CreationCollisionOption.GenerateUniqueName);
            Console.WriteLine(file);
            using (var filestream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await result.Content.WriteToStreamAsync(filestream);
                await filestream.FlushAsync();
                await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
            }

            AddData(title, url);
      

        }
        public static void AddData(string Title, string Url)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteData.db");
            using (SqliteConnection db =
            new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                
                insertCommand.CommandText = "INSERT INTO QRCodes VALUES (NULL, @Entry1, @Entry2);";
                insertCommand.Parameters.AddWithValue("@Entry1",Title);
                insertCommand.Parameters.AddWithValue("@Entry2", Url);

                insertCommand.ExecuteReader();

                db.Close();
            }

        }
    }
}
