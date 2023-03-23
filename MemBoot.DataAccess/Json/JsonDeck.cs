using MemBoot.Core;
using MemBoot.Core.Models;
using System.Text.Json;

namespace MemBoot.DataAccess.Json;

public class JsonDeck : IFlashcard
{
    private readonly Random rnd = new();
    private readonly Deck deck;
    private readonly string savePath;
    private readonly CardType cardType;
    private Fact? currentFact = null;

    public JsonDeck(Deck deck, CardType cardType, string savePath)
    {
        this.deck = deck;
        this.cardType = cardType;
        this.savePath = savePath;
    }

    public string CurrentQuestion
    {
        get
        {
            string output = string.Empty;
            if (currentFact != null)
            {
                output = deck.DoTemplateReplacement(currentFact, cardType.QuestionTemplate);
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
                output = deck.DoTemplateReplacement(currentFact, cardType.AnswerTemplate);
            }
            return output;
        }
    }

    public void AnswerCorrectly()
    {
        if (currentFact != null)
        {
            deck.UpdateFactMastery(cardType, currentFact, true);
            SaveDeck();
        }
    }

    public void AnswerIncorrectly()
    {
        if (currentFact != null)
        {
            deck.UpdateFactMastery(cardType, currentFact, false);
            SaveDeck();
        }
    }

    public IFlashcard Next()
    {
        currentFact = deck.GetRandomFact(rnd, cardType, currentFact);
        return this;
    }

    private void SaveDeck()
    {
        ExportFile(deck, savePath);
    }

    public static Deck? FromJson(string json)
    {
        var options = new JsonSerializerOptions()
        {
            Converters = { new DeckConverter() }
        };
        return JsonSerializer.Deserialize<Deck>(json, options);
    }

    public static Deck? FromJsonFile(string path)
    {
        var json = File.ReadAllText(path);
        var deck = FromJson(json);
        if (deck?.Resources.Any() == true)
        {
            var basePath = Path.GetDirectoryName(path) ?? $".{Path.PathSeparator}";
            Files.ResourceDirectory.CopyResources(deck, basePath);
        }
        return deck;
    }

    public static string ToJson(Deck deck)
    {
        var options = new JsonSerializerOptions()
        {
            Converters = { new DeckConverter() }
        };
        return JsonSerializer.Serialize(deck, options);
    }

    public static Deck? ImportFile(string path)
    {
        Deck? output = null;

        if (File.Exists(path))
        {
            output = FromJsonFile(path);
        }

        return output;
    }

    public static bool ExportFile(Deck deck, string path)
    {
        var output = false;

        var json = ToJson(deck);
        if (json != null)
        {
            using StreamWriter writer = new(path);
            writer.Write(json);
            output = true;
        }

        return output;
    }

    public IFlashcard WithCardType(CardType cardType)
    {
        return new JsonDeck(deck, cardType, savePath);
    }

    public string? GetRealResourcePath(string resourcePath)
    {
        return deck.Resources.Values.FirstOrDefault(r => r.OriginalPath == resourcePath)?.Path;
    }
}
