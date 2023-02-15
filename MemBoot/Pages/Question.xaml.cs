using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace MemBoot.Pages
{
    /// <summary>
    /// Interaction logic for Question.xaml
    /// </summary>
    public partial class Question : Page
    {
        public Question(DeckViewModel deckViewModel)
        {
            InitializeComponent();
            DataContext = deckViewModel;
            deckViewModel.Next();
            NavigateToString(QuestionBrowser, deckViewModel.HTMLQuestion).GetAwaiter().OnCompleted(() => { });
            Loaded += Question_Loaded;
        }

        private void Question_Loaded(object sender, RoutedEventArgs e)
        {
            ShowAnswerButton.Focus();
        }

        private static async Task NavigateToString(WebView2 browser, string html)
        {
            await browser.EnsureCoreWebView2Async();
            browser.NavigateToString(html);
        }

        private void ShowAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            var deckViewModel = (DeckViewModel)DataContext;
            Answer answerPage = new(deckViewModel);
            NavigationService.Navigate(answerPage);
            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }
        }
    }
}
