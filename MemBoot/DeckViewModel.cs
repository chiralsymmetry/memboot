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
        private readonly IDeck deck;

        private readonly string HTMLTemplatePart1 = @"<!DOCTYPE html>
	<head>
		<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
		<style>
			body {
				margin: 0;
			}
			.container {
				display: flex;
				height: 100vh;
			}
			.column {
				flex: 1;
				display: flex;
				align-items: center;
				justify-content: center;
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
		<div class=""container"">";
        private readonly string HTMLTemplatePart2 = @"		</div>
	</body>
</html>";


        private string GetHTMLTemplate(IList<string> columnClasses, IList<string> columnContents)
        {
            string columns = string.Empty;
            for (int i = 0; i < columnClasses.Count; i++)
            {
                string column = @"			<div class=""column"">
				<div class=""columnClass"">columnContent</div>
			</div>";
                column = column.Replace("columnClass", columnClasses[i]);
                column = column.Replace("columnContent", columnContents[i]);
                columns += column;
            }
            return $"{HTMLTemplatePart1}{Environment.NewLine}{columns}{Environment.NewLine}{HTMLTemplatePart2}";
        }

        public string HTMLQuestion
        {
            get
            {
                string html = GetHTMLTemplate(new string[] { "question" }, new string[] { deck.CurrentQuestion });
                return html;
            }
        }

        public string HTMLQuestionAndAnswer
        {
            get
            {
                string html = GetHTMLTemplate(new string[] { "question", "answer" }, new string[] { deck.CurrentQuestion, deck.CurrentAnswer });
                return html;
            }
        }

        public DeckViewModel(IDeck deck)
        {
            this.deck = deck;
        }

        public void Good()
        {
            deck.AnswerCorrectly();
        }

        public void Bad()
        {
            deck.AnswerIncorrectly();
        }

        public void Next()
        {
            deck.Next();
        }
    }
}
