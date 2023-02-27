using Dapper;
using MemBoot.Core;
using MemBoot.Core.Models;
using System.Data;
using System.Data.SQLite;

namespace MemBoot.DataAccess.Sqlite
{
    public class SqliteDeckStorage : IDeckStorage
    {
        private readonly string connectionString;

        public SqliteDeckStorage(string connectionString)
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            this.connectionString = connectionString;
            using var connection = new SQLiteConnection(connectionString);
            connection?.Execute(SqlStatements.CreateAllTables);
        }

        public bool AddDeck(Deck deck)
        {
            var output = true;
            string sql = string.Empty;

            using var connection = new SQLiteConnection(connectionString);
            if (connection != null)
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try
                {
                    sql = "INSERT INTO decks (id, name, description) VALUES (@Id, @Name, @Description);";
                    output &= connection.Execute(sql, deck) == 1;

                    sql = "INSERT INTO fields (deck_id, name, allow_html) VALUES (@DeckId, @Name, @AllowHTML);";
                    output &= connection.Execute(sql, deck.Fields.Select(f => new { DeckId = deck.Id, f.Name, f.AllowHTML }).ToList<dynamic>()) == deck.Fields.Count;

                    sql = "INSERT INTO facts (deck_id, id) VALUES (@DeckId, @Id);";
                    output &= connection.Execute(sql, deck.Facts.Select(f => new { DeckId = deck.Id, f.Id }).ToList<dynamic>()) == deck.Facts.Count;

                    sql = "INSERT INTO facts_contents (fact_id, deck_id, field_name, content) VALUES (@FactId, @DeckId, @FieldName, @Content);";
                    foreach (var fact in deck.Facts)
                    {
                        output &= connection.Execute(sql, fact.FieldsContents.Select(fc => new { FactId = fact.Id, DeckId = deck.Id, FieldName = fc.Key.Name, Content = fc.Value }).ToList<dynamic>()) == fact.FieldsContents.Count;
                    }

                    sql = "INSERT INTO cardtypes (deck_id, id, name, question_template, answer_template, styling, initial_probability, transition_probability, slipping_probability, lucky_guess_probability, mastery_threshold, competency_threshold, cards_are_composable) VALUES (@DeckId, @Id, @Name, @QuestionTemplate, @AnswerTemplate, @Styling, @InitialProbability, @TransitionProbability, @SlippingProbability, @LuckyGuessProbability, @MasteryThreshold, @CompetencyThreshold, @CardsAreComposable);";
                    output &= connection.Execute(sql, deck.CardTypes.Select(ct => new
                    {
                        DeckId = deck.Id,
                        ct.Id,
                        ct.Name,
                        ct.QuestionTemplate,
                        ct.AnswerTemplate,
                        ct.Styling,
                        ct.InitialProbability,
                        ct.TransitionProbability,
                        ct.SlippingProbability,
                        ct.LuckyGuessProbability,
                        ct.MasteryThreshold,
                        ct.CompetencyThreshold,
                        ct.CardsAreComposable
                    }).ToList<dynamic>()) == deck.CardTypes.Count;

                    {
                        var dataToMD5 = new Dictionary<byte[], byte[]>();
                        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                        {
                            foreach (var resource in deck.Resources.Values)
                            {
                                var md5OfData = md5.ComputeHash(resource);
                                dataToMD5[resource] = md5OfData;
                            }
                            sql = "INSERT INTO resources (md5, data) VALUES (@Md5, @Data);";
                            output &= connection.Execute(sql, dataToMD5.Select(kvp => new { Md5 = kvp.Value, Data = kvp.Key }).ToList<dynamic>()) == dataToMD5.Count;
                        }

                        sql = "INSERT INTO decks_to_resources (deck_id, resource_md5, name) VALUES (@DeckId, @ResourceMd5, @Name);";
                        output &= connection.Execute(sql, deck.Resources.Select(kvp => new { DeckId = deck.Id, ResourceMd5 = dataToMD5[kvp.Value], Name = kvp.Key }).ToList<dynamic>()) == deck.Resources.Keys.Count;
                    }

                    sql = "INSERT INTO mastery_records (cardtype_id, fact_id, mastery) VALUES (@CardTypeId, @FactId, @Mastery);";
                    foreach (var kvp in deck.MasteryRecords)
                    {
                        var cardType = kvp.Key;
                        var mrs = kvp.Value;
                        output &= connection.Execute(sql, mrs.Select(kvp => new { CardTypeId = cardType.Id, FactId = kvp.Key.Id, Mastery = kvp.Value }).ToList<dynamic>()) == mrs.Count;
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }

            return output;
        }

        private Deck? GetDeckFromId(Guid? deckId = null, Guid? cardTypeId = null, Guid? factId = null)
        {
            Deck? output = null;
            string sql = string.Empty;

            using var connection = new SQLiteConnection(connectionString);
            if (connection != null && (deckId == null && (cardTypeId != null || factId != null)))
            {
                if (cardTypeId != null)
                {
                    sql = "SELECT deck_id FROM cardtypes WHERE id = @cardTypeId;";
                    deckId = connection.QueryFirstOrDefault<Guid>(sql, new { cardTypeId });
                }
                else if (factId != null)
                {
                    sql = "SELECT deck_id FROM facts WHERE id = @factId;";
                    deckId = connection.QueryFirstOrDefault<Guid>(sql, new { factId });
                }
            }
            if (connection != null && deckId != null)
            {
                sql = "SELECT id, name, description FROM decks WHERE id = @deckId;";
                output = connection.QuerySingleOrDefault<Deck>(sql, new { deckId }, null);

                sql = "SELECT name, allow_html FROM fields WHERE deck_id = @deckId;";
                output.Fields = connection.Query<dynamic>(sql, new { deckId }).Select(d => new Field(d.name, d.allow_html != 0)).ToList();

                sql = "SELECT id FROM facts WHERE deck_id = @deckId ORDER BY sort_id ASC;";
                output.Facts = connection.Query<Fact>(sql, new { deckId }).ToList();

                {
                    sql = "SELECT f.id, fc.field_name, fc.content FROM facts f INNER JOIN facts_contents fc ON f.id = fc.fact_id WHERE fc.deck_id = @deckId;";
                    var fact_contents = connection.Query<dynamic>(sql, new { deckId });
                    foreach (var d in fact_contents)
                    {
                        Fact fact = output.Facts.First(f => f!.Id.Equals(new Guid(d.id)));
                        Field field = output.Fields.First(f => f!.Name == d.field_name);
                        if (fact != null && field != null)
                        {
                            fact.FieldsContents[field] = d.content;
                        }
                    }
                }

                sql = "SELECT id, name, question_template, answer_template, styling, initial_probability, transition_probability, slipping_probability, lucky_guess_probability, mastery_threshold, competency_threshold, cards_are_composable FROM cardtypes WHERE deck_id = @deckId;";
                output.CardTypes = connection.Query<dynamic>(sql, new { deckId }).Select(d => new CardType(
                    new Guid(d.id), d.name, d.question_template, d.answer_template, d.styling,
                    d.initial_probability, d.transition_probability, d.slipping_probability, d.lucky_guess_probability,
                    d.mastery_threshold, d.competency_threshold, d.cards_are_composable != 0)
                ).ToList();

                sql = "SELECT dr.name, r.data FROM resources r INNER JOIN decks_to_resources dr ON r.md5 = dr.resource_md5 WHERE dr.deck_id = @deckId;";
                output.Resources = connection.Query<dynamic>(sql, new { deckId }).ToDictionary(d => (string)d.name, d => (byte[])d.data);

                {
                    sql = "SELECT mrec.cardtype_id, mrec.fact_id, mrec.mastery FROM mastery_records mrec INNER JOIN cardtypes ct ON ct.id = mrec.cardtype_id WHERE ct.deck_id = @deckId;";
                    var results = connection.Query<dynamic>(sql, new { deckId });
                    foreach (var d in results)
                    {
                        CardType cardType = output.CardTypes.First(ct => ct.Id.Equals(new Guid(d.cardtype_id)));
                        Fact fact = output.Facts.First(f => f.Id.Equals(new Guid(d.fact_id)));
                        double mastery = d.mastery;
                        if (!output.MasteryRecords.ContainsKey(cardType))
                        {
                            output.MasteryRecords[cardType] = new();
                        }
                        output.MasteryRecords[cardType][fact] = mastery;
                    }
                }
            }

            return output;
        }

        public Deck? GetDeck(Guid deckId)
        {
            return GetDeckFromId(deckId);
        }

        public ICollection<Tuple<string, Guid>> GetCardTypeIds()
        {
            var output = new List<Tuple<string, Guid>>();

            IEnumerable<Guid>? deckIds = null;
            using var connection = new SQLiteConnection(connectionString);
            if (connection != null)
            {
                const string sql = "SELECT id FROM decks;";
                deckIds = connection.Query<Guid>(sql);
            }
            if (deckIds != null)
            {
                foreach (var deckId in deckIds)
                {
                    var deck = GetDeck(deckId);
                    foreach (var cardType in deck!.CardTypes)
                    {
                        output.Add(new(cardType.Name, cardType.Id));
                    }
                }
            }

            return output;
        }

        public IFlashcard? GetFlashcard(Guid cardTypeId)
        {
            IFlashcard? output = null;
            Deck? deck = GetDeckFromId(cardTypeId: cardTypeId);
            if (deck != null)
            {
                var cardType = deck.CardTypes.First(ct => ct.Id.Equals(cardTypeId));
                output = new SqliteDeck(deck, cardType, connectionString);
            }
            return output;
        }
    }
}
