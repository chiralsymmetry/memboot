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
        public float Accuracy
        {
            get
            {
                float accuracy = 0;
                if (Answers.Count > 0)
                {
                    accuracy = (float)Answers.Where(a => a).Count() / (float)Answers.Count;
                }
                return accuracy;
            }
        }

        public Flashcard(int answerHistorySize = 100)
        {
            Answers = new(answerHistorySize);
        }

        public void Answer(bool answer)
        {
            Answers.Add(answer);
        }

        public float LastNAccuracy(int n)
        {
            float accuracy = 0;
            if (Answers.Count > 0)
            {
                accuracy = (float)Answers.Skip(Answers.Count - n).Where(a => a).Count() / (float)Answers.Count;
            }
            return accuracy;
        }
    }
}
