using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Storage;
using Microsoft.Data.Sqlite;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using Windows.System;
using System.Text.RegularExpressions;

namespace QRCodeGenerator
{
    /// <summary>
    /// Gallery page
    /// </summary>
    /// 
    public class ImageClass
    {
        // get and set bitmap image
        public BitmapImage ImgQR { get; set; }
    }

    public class QRDataContent
    {
        // get title and url, bind in xaml view
        public string Title { get; set; }
        public string Url { get; set; }
    }

    public class Code
    {
        // get and set title (for database usage)
        public string Title { get; set; }
        public string Url { get; set; }
    }

    public class QRViewModel
    {
        // this is for xaml usage
        private ObservableCollection<QRDataContent> qrDataContents = new ObservableCollection<QRDataContent>();
        public ObservableCollection<QRDataContent> QRDataContents {get { return this.qrDataContents; }}

        // create list of Code entries
        List<Code> entries = new List<Code>();

        public QRViewModel()
        {
            this.Create_QR_Gallery();
        }
        List<Code> GetData()
        {
            // get data from sqlite database
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteData.db");
            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbpath}"))
            {
                //open database
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Title,Url from QRCodes", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add(new Code
                    {
                        // add entries from database to list
                        Title = query.GetString(0),
                        Url = query.GetString(1)
                    });
                }
                // close database
                db.Close();
            }

            return entries;
        }
        public void Create_QR_Gallery()
        {
            // create QR gallery from Code list
            List<Code> entries = GetData();
            for (int i = 0; i < entries.Count; i++)
            {
                // add content to xaml view
                this.qrDataContents.Add(new QRDataContent()
                {
                    Title = entries[i].Title,
                    Url = entries[i].Url,
                });
            }
        }
    }
    public sealed partial class EditPage : Page
    {
        public EditPage()
        {
            this.InitializeComponent();
            this.ViewModel = new QRViewModel();
        }
        public QRViewModel ViewModel { get; set; }
        private void GridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Phase != 0)
            {
                throw new System.Exception("We should be in phase 0, but we are not.");
            }
            // It's phase 0, so this item's title will already be bound and displayed.

            args.RegisterUpdateCallback(this.ShowUrl);

            args.Handled = true;
        }

        private void ShowUrl(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Phase != 1)
            {
                throw new System.Exception("We should be in phase 1, but we are not.");
            }

            // It's phase 1, so show qr codes.
            var templateRoot = args.ItemContainer.ContentTemplateRoot as StackPanel;
            var image = templateRoot.Children[1] as Image;
            string strImgPath = (args.Item as QRDataContent).Url;
            
            ImageClass obj = new ImageClass
            {
                ImgQR = new BitmapImage(new Uri(strImgPath, UriKind.Absolute))
            };
            
            // show bitmap image
            image.Source = obj.ImgQR;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // navigate to main page
            Frame.Navigate(typeof(MainPage));
        }

        private async void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // handle click on qr code
            string url = (String)((StackPanel)sender).Tag;
            string title = (String)((StackPanel)sender).Name;

            // get format like .png
            string regex = @"format=(.*)";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            Match m = r.Match(url);
            Group g = m.Groups[1];
            string selected = g.ToString();

            // use http to download image and transfer it to file on local machine
            var myFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            myFilter.AllowUI = false;

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
        }
    }
}
