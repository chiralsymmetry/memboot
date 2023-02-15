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
    /// Interaction logic for Answer.xaml
    /// </summary>
    public partial class Answer : Page
    {
        public Answer(DeckViewModel deckViewModel)
        {
            InitializeComponent();
            DataContext = deckViewModel;
            NavigateToString(QuestionBrowser, deckViewModel.HTMLQuestion).GetAwaiter().OnCompleted(() => { });
            NavigateToString(AnswerBrowser, deckViewModel.HTMLAnswer).GetAwaiter().OnCompleted(() => { });
            Loaded += Answer_Loaded;
        }

        private void Answer_Loaded(object sender, RoutedEventArgs e)
        {
            CorrectButton.Focus();
        }

        private static async Task NavigateToString(WebView2 browser, string html)
        {
            await browser.EnsureCoreWebView2Async();
            browser.NavigateToString(html);
        }

        private void CorrectButton_Click(object sender, RoutedEventArgs e)
        {
            var deckViewModel = (DeckViewModel)DataContext;
            deckViewModel.Good();
            GoToNextQuestion();
        }

        private void IncorrectButton_Click(object sender, RoutedEventArgs e)
        {
            var deckViewModel = (DeckViewModel)DataContext;
            deckViewModel.Bad();
            GoToNextQuestion();
        }

        private void GoToNextQuestion()
        {
            var deckViewModel = (DeckViewModel)DataContext;
            Question questionPage = new(deckViewModel);
            NavigationService.Navigate(questionPage);
            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }
        }
    }
}
