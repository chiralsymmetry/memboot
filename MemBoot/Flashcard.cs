using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot
{
    public class Flashcard
    {
        public string QuestionSide { get; set; }
        public string AnswerSide { get; set; }

        public float Mastery { get; set; }

        public Flashcard(string question, string answer, float mastery = 0f)
        {
            QuestionSide = question;
            AnswerSide = answer;
            Mastery = mastery;
        }
    }
}
