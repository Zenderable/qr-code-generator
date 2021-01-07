using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using DataAccessLibrary;
using Windows.Storage;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QRCodeGenerator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class imageClaz
    {
        public BitmapImage ImgQR { get; set; }
    }

    public class ExampleItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public int Index { get; set; }
    }

    public class ExampleViewModel
    {
        private ObservableCollection<ExampleItem> exampleItems = new ObservableCollection<ExampleItem>();
        public ObservableCollection<ExampleItem> ExampleItems {get { return this.exampleItems; } }


        public class Code
        {
            public string Title { get; set; }
            public string Url { get; set; }
           
            
        }
        public ExampleViewModel()
        {
            this.Create_QR_Gallery();
        }
        List<Code> getData()
        {
            var entries = new List<Code>();
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteData.db");
            using (SqliteConnection db =
                new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT Title,Url from QRCodes", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {

                    entries.Add(new Code
                    {
                        Title = query.GetString(0),
                        Url = query.GetString(1)
                    });
                }

                db.Close();
            }

            return entries;
        }

        public void Create_QR_Gallery()
        {
            List<Code> entries = getData();
            //Debug.WriteLine(entries[4].Title);
            for (int i = 0; i < entries.Count; i++)
            {
                this.exampleItems.Add(new ExampleItem()
                {
                    Title = entries[i].Title,
                    Url = entries[i].Url,
                    Index = i

                });
            }
        }
    }
    public sealed partial class EditPage : Page
    {
   
        public EditPage()
        {
            this.InitializeComponent();
            this.ViewModel = new ExampleViewModel();
            
        }

        public ExampleViewModel ViewModel { get; set; }

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

            // It's phase 1, so show this item's subtitle.
            var templateRoot = args.ItemContainer.ContentTemplateRoot as StackPanel;
            var image = templateRoot.Children[1] as Image;
            string strImgPath = (args.Item as ExampleItem).Url;
            
            imageClaz obj = new imageClaz
            {
                ImgQR = new BitmapImage(new Uri(strImgPath, UriKind.Absolute))
            };
            
            image.Source = obj.ImgQR;

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var i = this.Name;
            Debug.WriteLine(i); 
        }

        private void StackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var i = this.Name;
            Debug.WriteLine(i);
        }
    }
}
