using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot
{
    public class CardType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string QuestionTemplate { get; set; }
        public string AnswerTemplate { get; set; }
        public string Styling { get; set; }
        public double MasteryThreshold { get; set; }
        public double CompetencyThreshold { get; set; }
        public double InitialProbability { get; set; }
        public double TransitionProbability { get; set; }
        public double SlippingProbability { get; set; }
        public double LuckyGuessProbability { get; set; }
        public bool CardsAreComposable { get; set; }
        public CardType(Guid id, string name, string questionTemplate, string answerTemplate, string styling, double initialProbability, double transitionProbability, double slippingProbability, double luckyGuessProbability, double masteryThreshold, double competencyThreshold, bool cardsAreComposable)
        {
            Id = id;
            Name = name;
            QuestionTemplate = questionTemplate;
            AnswerTemplate = answerTemplate;
            Styling = styling;
            InitialProbability = initialProbability;
            TransitionProbability = transitionProbability;
            SlippingProbability = slippingProbability;
            LuckyGuessProbability = luckyGuessProbability;
            MasteryThreshold = masteryThreshold;
            CompetencyThreshold = competencyThreshold;
            CardsAreComposable = cardsAreComposable;
        }
        public CardType(Guid id, string name, string questionTemplate, string answerTemplate) : this(id, name, questionTemplate, answerTemplate, string.Empty, 0.0, 0.1, 0.1, 1.0/3.0, 0.95, 0.85, false)
        {
        }
    }
}
