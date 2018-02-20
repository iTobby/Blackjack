﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blackjack
{
    class Program
    {
        const int BlackJackThreshold = 21;
        static void Main(string[] args)
        {
            ComputeDeal();
        }

        private static void ComputeDeal()
        {
            var playersWithCards = GetPlayerCards();
            var dealerCards = GetDealerCards();

            if (!playersWithCards.Any() || !dealerCards.Any())
                return;

            var outcomes = GetAllPlayerOutcomes(playersWithCards, dealerCards.ToList());
            outcomes.ForEach(x => Console.WriteLine(x));
            Console.ReadKey();

        }

        private static Dictionary<int, List<Card>> GetPlayerCards()
        {
            Dictionary<int, List<Card>> playerWithCards = new Dictionary<int, List<Card>>();

            Console.WriteLine("Enter number of players");
            if (int.TryParse(Console.ReadLine(), out int numberOfPlayers))
            {
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    var index = i + 1;
                    Console.WriteLine($"Player {index}: Enter number of cards");
                    if (int.TryParse(Console.ReadLine(), out int cardCount))
                    {
                        var cards = GetCards(cardCount);
                        playerWithCards.Add(index, cards);
                    }
                    else
                    {
                        i--;
                        Console.WriteLine("Only accepts numbers");
                    }
                }
            }
            else
            {
                Console.WriteLine("Only accepts numbers");
            }

            return playerWithCards;
        }

        private static IEnumerable<Card> GetDealerCards()
        {
            //Enter Dealer card details
            Console.WriteLine($"Dealer: Enter number of cards");
            if (int.TryParse(Console.ReadLine(), out int dealerCount))
            {
                return GetCards(dealerCount);
            }
            else
            {
                Console.WriteLine($"Only accept numbers");
                GetDealerCards();
            }
            return Enumerable.Empty<Card>();

        }

        private static List<string> GetAllPlayerOutcomes(Dictionary<int, List<Card>> playerCards, List<Card> dealerCards)
        {
            List<string> results = new List<string>();

            foreach (var player in playerCards)
            {
                var cards = player.Value;
                if (cards.Count == 5)
                {
                    //
                    var playerWins = GetFiveCardsResult(cards);
                    if (playerWins)
                    {
                        results.Add(FormatResult(player.Key, PlayerOutcome.Win));
                        continue;
                    }
                }

                var playerOutcome = GetPlayerOutcome(cards, dealerCards);
                results.Add(FormatResult(player.Key, playerOutcome));
            }
            return results;
        }

        private static PlayerOutcome GetPlayerOutcome(List<Card> playerCards, List<Card> dealerCards)
        {
            var dealerSum = dealerCards.Sum(x => x.Value);
            var dealerHasUnusedAce = dealerCards.Any(d => d.HasAceOfSpade && !d.AceUsed);

            var acedDealerCards = new List<Card>();
            if (dealerHasUnusedAce)
            {
                dealerCards.ForEach(d =>
                {
                    if (d.HasAceOfSpade)
                    {
                        d.Value = 1;
                        d.AceUsed = true;
                    }
                    acedDealerCards.Add(d);
                });

                //Use ace for dealer
                dealerSum = acedDealerCards.Sum(x => x.Value);
            }


            var playerSum = playerCards.Sum(x => x.Value);
            if (playerSum <= BlackJackThreshold && playerSum > dealerSum)
            {
                return PlayerOutcome.Win;
            }

            if (playerCards.Any(a => a.HasAceOfSpade && !a.AceUsed))
            {
                var acedPlayerCards = new List<Card>();
                var aceCard = playerCards.SingleOrDefault(x => x.HasAceOfSpade);
                var aceIndex = playerCards.IndexOf(aceCard);
                playerCards.ForEach(x =>
                {
                    if (x.HasAceOfSpade)
                    {
                        x.Value = 1;
                        x.AceUsed = true;
                    }
                    acedPlayerCards.Add(x);
                });

                GetPlayerOutcome(acedPlayerCards, dealerHasUnusedAce ? acedDealerCards : dealerCards);
            }

            return PlayerOutcome.Lost;
        }

        private static string FormatResult(int playerId, PlayerOutcome outcome)
        {
            return $"Player {playerId} {(outcome == PlayerOutcome.Win ? "beats the dealer" : "loses")}";

        }

        private static bool GetFiveCardsResult(List<Card> cards)
        {
            var sum = cards.Sum(x => x.Value);

            if (sum <= BlackJackThreshold)
            {
                //Wins
                return true;
            }
            else if (sum > BlackJackThreshold && cards.Any(x => x.HasAceOfSpade && !x.AceUsed))
            {
                var aceCard = cards.SingleOrDefault(x => x.HasAceOfSpade);
                var aceIndex = cards.IndexOf(aceCard);
                //Change ace value to 1
                aceCard.Value = 1;
                aceCard.AceUsed = true;

                if (aceIndex != -1)
                    cards[aceIndex] = aceCard;

                GetFiveCardsResult(cards);
            }

            return false;
        }

        private static List<Card> GetCards(int countOfCards)
        {

            List<Card> cards = new List<Card>();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Card Types - Spade (S), Heart (H), Diamond (D), Club (C)");

            for (int i = 0; i < countOfCards; i++)
            {
                var index = i + 1;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"What is the type of Card {index}?");

                if (IsValidCardType(Console.ReadLine()))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Enter value on card {index}. Between 1 and 10, (J)ack, (Q)ueen, (K)ing");
                    var card = ValidateCard(Console.ReadLine());

                    Console.ForegroundColor = ConsoleColor.Red;
                    if (card.HasAceOfSpade && cards.Any(x => x.HasAceOfSpade))
                    {
                        i--;
                        Console.WriteLine("Deck cannot have more than one ace of spade, try again!");
                        continue;
                    }
                    if (card == null) //Retry
                    {
                        i--;
                        Console.WriteLine("Wrong entry, try again!");
                    }

                    else
                        cards.Add(card);
                }
                else
                {
                    //Retry
                    i--;
                }

            }

            return cards;
        }

        private static bool IsValidCardType(string val)
        {
            List<string> types = new List<string> { "s", "h", "d", "c" };

            if (val.Length > 1) return false;
            return types.Contains(val.ToLower());
        }

        private static Card ValidateCard(string val)
        {
            Card card = null;
            const int defaultAceValue = 11;
            List<string> specialCards = new List<string> { "j", "q", "k" };

            if (specialCards.Contains(val.ToLower().FirstOrDefault().ToString()))
            {
                card = new Card() { Value = 10 };
            }
            else
            {
                int.TryParse(val, out var value);
                if (value == 1)
                {
                    card = new Card() { Value = defaultAceValue, HasAceOfSpade = true };
                }
                else if (value >= 2 && value <= 10)
                {
                    card = new Card() { Value = value };
                }
            }

            return card;
        }
    }

    [DebuggerDisplay("Has Ace Of Spade - {HasAceOfSpade}, Value - {Value}")]
    public class Card
    {
        //public Card(string type, string specialValue)
        //{

        //}
        public int Value { get; set; }
        public bool HasAceOfSpade { get; set; }
        public bool AceUsed { get; set; }
        //public override string ToString()
        //{
        //    return base.ToString();
        //}
    }

    public enum PlayerOutcome
    {
        Win,
        Lost
    }
}