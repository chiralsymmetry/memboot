using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot
{
    public class Flashcard
    {
        public string QuestionSide { get; set; } = string.Empty;
        public string AnswerSide { get; set; } = string.Empty;
        public bool IsComposable { get; set; } = false;
        public RingList<bool> Answers { get; set; }
        private readonly int AnswerHistorySize;
        public float Accuracy => (float)Answers.Where(a => a).Count() / (float)AnswerHistorySize;

        public Flashcard(int answerHistorySize = 100)
        {
            Answers = new(answerHistorySize);
            AnswerHistorySize = answerHistorySize;
        }

        public void Answer(bool answer)
        {
            Answers.Add(answer);
        }

        public float LastNAccuracy(int n)
        {
            return (float)Answers.Skip(AnswerHistorySize - n).Where(a => a).Count() / (float)AnswerHistorySize;
        }
    }
}
