using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        private Deck deck;
        public Main()
        {
            InitializeComponent();
            deck = new Deck();
            deck.CreateFlashcard("α", "alpha / a");
            deck.CreateFlashcard("β", "beta / b");
            deck.CreateFlashcard("γ", "gamma / g");
            deck.CreateFlashcard("δ", "delta / d");
            deck.CreateFlashcard("ε", "epsilon / e");
            deck.CreateFlashcard("ζ", "zêta / z");
            deck.CreateFlashcard("η", "êta / ê");
            deck.CreateFlashcard("θ", "thêta / th");
            deck.CreateFlashcard("ι", "iota / i");
            deck.CreateFlashcard("κ", "kappa / k");
            deck.CreateFlashcard("λ", "lambda / l");
            deck.CreateFlashcard("μ", "mu / m");
            deck.CreateFlashcard("ν", "nu / n");
            deck.CreateFlashcard("ξ", "xi / ks");
            deck.CreateFlashcard("ο", "omikron / o");
            deck.CreateFlashcard("π", "pi / p");
            deck.CreateFlashcard("ρ", "rho / r");
            deck.CreateFlashcard("σ", "sigma / s");
            deck.CreateFlashcard("τ", "tau / t");
            deck.CreateFlashcard("υ", "upsilon / u");
            deck.CreateFlashcard("φ", "phi / f");
            deck.CreateFlashcard("χ", "chi / ch");
            deck.CreateFlashcard("ψ", "psi / ps");
            deck.CreateFlashcard("ω", "omega / ô");
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var deckViewModel = new DeckViewModel(deck);
            Question questionPage = new(deckViewModel);
            NavigationService.Navigate(questionPage);
            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }
        }
    }
}
