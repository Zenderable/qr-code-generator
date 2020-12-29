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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QRCodeGenerator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int size = 500;
        string selected = "png", color, url;
        public MainPage()
        {
            this.InitializeComponent();
           
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ListBoxFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             selected = lbFormat.SelectedValue.ToString();
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
            url = "http://api.qrserver.com/v1/create-qr-code/?data=" + content + "&size=" + size + "x" + size + "&color=" + color + "&format=" + selected;
            imgGenerated.Source = new BitmapImage(new Uri(url));
            
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
                       
            //string localfolder = ApplicationData.Current.LocalFolder.Path;
            //var array = localfolder.Split('\\');
            //var username = array[2];
            //var path = tbQRTitle.Text + "." + selected;
            WebClient webClient = new WebClient();
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\asd.png"; 


            //tu wyrzuca zawsze błąd odnośnie braku uprawnień do zapisywania, próbowałem odpalac jako admin, sprawdzałem różne miejsca zapisu i dupa, chyba znaleźć inny sposób na zapis
            webClient.DownloadFile(url, path);
            
        }
    }
}
