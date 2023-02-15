using MemBoot.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MemBoot
{
    public class DeckViewModel
    {
        private readonly Deck deck;
        private Flashcard? currentCard = null;
        private string HTMLTemplate = @"<!DOCTYPE html>
	<head>
		<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
		<style>
			body {
				margin: 0;
			}
			.container {
				display: flex;
				align-items: center;
				justify-content: center;
				height: 100vh;
			}
			.question, .answer {
				font-size: 32px;
				font-weight: bold;
				text-align: center;
				padding: 30px;
			}
		</style>
	</head>
	<body>
		<div class=""container"">
			<div class=""CLASS_GOES_HERE"">CONTENTS_GO_HERE</div>
		</div>
	</body>
</html>";
        private readonly string ClassPlaceholder = "CLASS_GOES_HERE";
        private readonly string ContentsPlaceholder = "CONTENTS_GO_HERE";

        public string CurrentQuestion
        {
            get
            {
                string output = string.Empty;
                if (currentCard != null)
                {
                    output = currentCard.QuestionSide;
                }
                return output;
            }
        }

        public string CurrentAnswer
        {
            get
            {
                string output = string.Empty;
                if (currentCard != null)
                {
                    output = currentCard.AnswerSide;
                }
                return output;
            }
        }

        public string HTMLQuestion
        {
            get
            {
                string q = WebUtility.HtmlEncode(CurrentQuestion);
                string html = HTMLTemplate.Replace(ClassPlaceholder, "question").Replace(ContentsPlaceholder, q);
                return html;
            }
        }

        public string HTMLAnswer
        {
            get
            {
                string a = WebUtility.HtmlEncode(CurrentAnswer);
                string html = HTMLTemplate.Replace(ClassPlaceholder, "answer").Replace(ContentsPlaceholder, a);
                return html;
            }
        }

        public DeckViewModel(Deck deck)
        {
            this.deck = deck;
        }

        public void Good()
        {
            if (currentCard != null)
            {
                deck.AnswerCorrectly(currentCard);
            }
        }

        public void Bad()
        {
            if (currentCard != null)
            {
                deck.AnswerIncorrectly(currentCard);
            }
        }

        public void Next()
        {
            currentCard = deck.RandomIntroducedCard();
        }
    }
}
