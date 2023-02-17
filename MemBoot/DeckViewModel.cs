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
        private readonly Random rnd = new();
        private readonly Deck deck;
        private readonly CardType cardType;
        private Fact? currentFact = null;

        private readonly string HTMLTemplate = @"<!DOCTYPE html>
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
                if (currentFact!= null)
                {
                    output = DeckProcessor.DoTemplateReplacement(deck, currentFact, cardType.QuestionTemplate);
                }
                return output;
            }
        }

        public string CurrentAnswer
        {
            get
            {
                string output = string.Empty;
                if (currentFact != null)
                {
                    output = DeckProcessor.DoTemplateReplacement(deck, currentFact, cardType.AnswerTemplate);
                }
                return output;
            }
        }

        public string HTMLQuestion
        {
            get
            {
                string html = HTMLTemplate.Replace(ClassPlaceholder, "question").Replace(ContentsPlaceholder, CurrentQuestion);
                return html;
            }
        }

        public string HTMLAnswer
        {
            get
            {
                string html = HTMLTemplate.Replace(ClassPlaceholder, "answer").Replace(ContentsPlaceholder, CurrentAnswer);
                return html;
            }
        }

        public DeckViewModel(Deck deck, CardType cardType)
        {
            this.deck = deck;
            this.cardType = cardType;
        }

        public void Good()
        {
            if (currentFact != null)
            {
                DeckProcessor.UpdateFactMastery(deck, cardType, currentFact, true);
            }
        }

        public void Bad()
        {
            if (currentFact != null)
            {
                DeckProcessor.UpdateFactMastery(deck, cardType, currentFact, false);
            }
        }

        public void Next()
        {
            currentFact = DeckProcessor.GetRandomFact(rnd, deck, cardType, currentFact);
        }
    }
}
