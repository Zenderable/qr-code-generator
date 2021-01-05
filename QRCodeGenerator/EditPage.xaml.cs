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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QRCodeGenerator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditPage : Page
    {
        public EditPage()
        {
            this.InitializeComponent();
            var codes = new List<Code>(); 
            codes = GetData();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
        public class Code
        {
            public string Title { get; set; }
            public string Url { get; set; }

        }

        public static List<Code> GetData()
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

                    entries.Add(new Code {
                        Title = query.GetString(0),
                        Url = query.GetString(1)
                    });
                }

                db.Close();
            }


            return entries;
        }
       
    }
}
