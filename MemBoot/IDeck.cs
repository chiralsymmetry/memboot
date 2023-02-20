using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot
{
    public interface IDeck
    {
        string CurrentAnswer { get; }
        string CurrentQuestion { get; }
        void AnswerCorrectly();
        void AnswerIncorrectly();
        void Next();
    }
}
