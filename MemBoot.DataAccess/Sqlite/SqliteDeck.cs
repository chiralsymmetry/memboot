using Dapper;
using MemBoot.Core;
using MemBoot.Core.Models;
using System.Data.SQLite;

namespace MemBoot.DataAccess.Sqlite;

public class SqliteDeck : IFlashcard
{
    private readonly Random rnd = new();
    private readonly Deck deck;
    private readonly string connectionString;
    private readonly CardType cardType;
    private Fact? currentFact;

    public SqliteDeck(Deck deck, CardType cardType, string connectionString)
    {
        this.deck = deck;
        this.cardType = cardType;
        this.connectionString = connectionString;
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
            UpdateSqlMastery(currentFact);
        }
    }

    public void AnswerIncorrectly()
    {
        if (currentFact != null)
        {
            deck.UpdateFactMastery(cardType, currentFact, false);
            UpdateSqlMastery(currentFact);
        }
    }

    private void UpdateSqlMastery(Fact fact)
    {
        var mastery = deck.MasteryRecords[cardType][fact];
        using var connection = new SQLiteConnection(connectionString);
        if (connection != null)
        {
            var parameters = new { cardTypeId = cardType.Id, factId = fact.Id, mastery };

            string sql = "SELECT sort_id FROM mastery_records WHERE cardtype_id = @cardTypeId AND fact_id = @factId;";
            var hit = connection.Query(sql, parameters).Any();

            if (hit)
            {
                sql = "UPDATE mastery_records SET mastery = @mastery WHERE cardtype_id = @cardTypeId AND fact_id = @factId;";
                connection.Execute(sql, parameters);
            }
            else
            {
                sql = "INSERT INTO mastery_records (cardtype_id, fact_id, mastery) VALUES (@cardTypeId, @factId, @mastery);";
                connection.Execute(sql, parameters);
            }
        }
    }

    public IFlashcard Next()
    {
        var recordsBefore = new HashSet<Fact>();
        if (deck.MasteryRecords.ContainsKey(cardType))
        {
            recordsBefore = new HashSet<Fact>(deck.MasteryRecords[cardType].Keys);
        }

        currentFact = deck.GetRandomFact(rnd, cardType, currentFact);

        if (deck.MasteryRecords.ContainsKey(cardType))
        {
            var recordsAfter = new HashSet<Fact>(deck.MasteryRecords[cardType].Keys);
            if (recordsAfter.Count > recordsBefore.Count)
            {
                foreach (var fact in recordsAfter.Except(recordsBefore))
                {
                    UpdateSqlMastery(fact);
                }
            }
        }

        return this;
    }

    public string? GetRealResourcePath(string resourcePath)
    {
        return deck.Resources.Values.FirstOrDefault(r => r.OriginalPath == resourcePath)?.Path;
    }
}
