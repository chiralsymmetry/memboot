using MemBoot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot.Core
{
    public interface IFlashcard
    {
        string CurrentAnswer { get; }
        string CurrentQuestion { get; }
        void AnswerCorrectly();
        void AnswerIncorrectly();
        IFlashcard Next();
    }
}
