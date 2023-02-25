using Microsoft.Web.WebView2.Wpf;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MemBoot.Pages
{
    /// <summary>
    /// Interaction logic for Flashcard.xaml
    /// </summary>
    public partial class FlashcardPage : Page
    {
        private enum Mode
        {
            Question,
            Answer
        }

        private Mode CurrentMode;
        private readonly FlashcardViewModel DeckViewModel;

        public FlashcardPage(FlashcardViewModel deckViewModel)
        {
            InitializeComponent();

            DataContext = deckViewModel;
            DeckViewModel = deckViewModel;

            SetQuestionMode();
        }

        private static async Task NavigateToString(WebView2 browser, string html)
        {
            await browser.EnsureCoreWebView2Async();
            browser.NavigateToString(html);
        }

        private string GetModeHTML()
        {
            if (CurrentMode == Mode.Question) return DeckViewModel.HTMLQuestion;
            else return DeckViewModel.HTMLQuestionAndAnswer;
        }

        private void SetQuestionMode()
        {
            CurrentMode = Mode.Question;
            DeckViewModel.Next();
            NavigateToString(Browser, GetModeHTML()).GetAwaiter().OnCompleted(() => { });
            AnswerButtons.Visibility = Visibility.Collapsed;
            AnswerButtons.Opacity = 0;
            CorrectButton.IsEnabled = false;
            IncorrectButton.IsEnabled = false;
            QuestionButtons.Opacity = 1;
            QuestionButtons.Visibility = Visibility.Visible;
            ShowAnswerButton.IsEnabled = true;
            ShowAnswerButton.Focus();
        }

        private void SetAnswerMode()
        {
            CurrentMode = Mode.Answer;
            NavigateToString(Browser, GetModeHTML()).GetAwaiter().OnCompleted(() => { });
            QuestionButtons.Visibility = Visibility.Collapsed;
            QuestionButtons.Opacity = 0;
            ShowAnswerButton.IsEnabled = false;
            AnswerButtons.Opacity = 1;
            AnswerButtons.Visibility = Visibility.Visible;
            CorrectButton.IsEnabled = true;
            IncorrectButton.IsEnabled = true;
            CorrectButton.Focus();
        }

        private void ShowAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            SetAnswerMode();
        }

        private void CorrectButton_Click(object sender, RoutedEventArgs e)
        {
            DeckViewModel.Good();
            SetQuestionMode();
        }

        private void IncorrectButton_Click(object sender, RoutedEventArgs e)
        {
            DeckViewModel.Bad();
            SetQuestionMode();
        }

        private void AcceptCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentMode == Mode.Question) SetAnswerMode();
            else
            {
                DeckViewModel.Good();
                SetQuestionMode();
            }
        }

        private void DeclineCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CurrentMode == Mode.Question) SetAnswerMode();
            else
            {
                DeckViewModel.Bad();
                SetQuestionMode();
            }
        }
    }
}
