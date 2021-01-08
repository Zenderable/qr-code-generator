using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace QRCodeGenerator
{
    /// <summary>
    /// About page
    /// </summary>
    public sealed partial class aboutPage : Page
    {
        public aboutPage()
        {
            this.InitializeComponent();
        }
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            /// navigate to main page
            Frame.Navigate(typeof(MainPage));
        }
    }
}
