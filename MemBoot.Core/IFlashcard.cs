namespace MemBoot.Core;

public interface IFlashcard
{
    string CurrentAnswer { get; }
    string CurrentQuestion { get; }
    void AnswerCorrectly();
    void AnswerIncorrectly();
    IFlashcard Next();
    string? GetRealResourcePath(string resourcePath);
}
