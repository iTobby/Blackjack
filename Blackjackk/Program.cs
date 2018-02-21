using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blackjack
{
    class Program
    {
        const int BlackjackThreshold = 21;

        static void Main(string[] args)
        {
            ComputeHands();
        }

        private static void ComputeHands()
        {
            var players = GetPlayers();
            var dealer = GetDealer();

            if (dealer == null || !players.Any())
                return;

            bool houseWins;
            var outcomes = GetAllPlayerOutcomes(players, dealer, out houseWins);
            outcomes.ForEach(Console.WriteLine);
            if (houseWins)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("HOUSE WINS!!!");
            }
            Console.ReadLine();

        }

        private static List<string> GetAllPlayerOutcomes(List<IPlayer> players, IPlayer dealr, out bool houseWins)
        {
            List<string> results = new List<string>();
            bool oneOrMorePlayerWon = false;

            var dealer = ((Dealer)dealr);

            foreach (var p in players)
            {
                var player = (Player)p;
                if (player.CardCount == 5)
                {
                    //Check if player with 5 cards won
                    var playerWins = GetFiveCardsResult(player);
                    if (playerWins)
                    {
                        oneOrMorePlayerWon = true;
                        results.Add(FormatResult(player.Name, PlayerOutcome.Win));
                        continue;
                    }
                }

                var playerOutcome = GetPlayerOutcome(player, dealer);

                if (!oneOrMorePlayerWon && playerOutcome == PlayerOutcome.Win)
                    oneOrMorePlayerWon = true;

                results.Add(FormatResult(player.Name, playerOutcome));
            }

            houseWins = !oneOrMorePlayerWon;

            return results;
        }
        private static string FormatResult(string playerName, PlayerOutcome outcome)
        {
            return $"{playerName} {(outcome == PlayerOutcome.Win ? "beats the dealer" : "loses")}";

        }
        private static bool GetFiveCardsResult(Player player)
        {
            if (player.Sum <= BlackjackThreshold)
            {
                //Wins
                return true;
            }

            if (!player.HasUnusedAceCard) return false;

            //Recalculate player sum with an ace
            var acedSum = player.AcedSum;
            return acedSum <= BlackjackThreshold;
        }
        private static PlayerOutcome GetPlayerOutcome(Player player, Dealer dealer)
        {
            var dealerSum = dealer.Sum;
            if (dealer.HasUnusedAceCard)
            {
                dealerSum = dealer.AcedSum;
            }

            var playerSum = player.Sum;
            if (player.HasUnusedAceCard)
            {
                playerSum = player.AcedSum;
            }

            if (playerSum <= BlackjackThreshold && playerSum > dealerSum)
            {
                return PlayerOutcome.Win;
            }

            return PlayerOutcome.Lost;
        }
        private static List<IPlayer> GetPlayers()
        {
            List<IPlayer> players = new List<IPlayer>();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Enter number of players");
            int numberOfPlayers;
            if (int.TryParse(Console.ReadLine(), out numberOfPlayers))
            {
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    var index = i + 1;
                    Console.WriteLine($"Player {index}: Enter number of cards");
                    int cardCount;
                    if (int.TryParse(Console.ReadLine(), out cardCount))
                    {
                        var playerId = index.ToString();
                        var player = new Player(playerId, cardCount);
                        players.Add(player);
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

            return players;
        }
        private static IPlayer GetDealer()
        {
            //Enter Dealer card details
            Console.WriteLine($"Dealer: Enter number of cards");
            int dealerCount;
            if (int.TryParse(Console.ReadLine(), out dealerCount))
            {
                return new Dealer(dealerCount);
            }

            Console.WriteLine($"Only accept numbers");
            return GetDealer();
        }
    }



    public interface IPlayer
    {

    }

    public class Player : IPlayer
    {
        const string Jack = "j";
        const string Queen = "q";
        const string King = "k";

        const string Heart = "h";
        const string Diamond = "d";
        const string Spade = "s";
        const string Club = "c";
        private string _playerId;


        protected Player(int cardCount) : this("Dealer", cardCount)
        { }

        public Player(string playerId, int cardCount)
        {
            //if (cardCount == 0)
            //    throw new ArgumentException();
            _playerId = playerId.ToString();
            CardCount = cardCount;
            Cards = new List<Card>(cardCount);
            GetCards();
        }

        public string Name => $"Player {_playerId}";
        public int CardCount { get; }
        public int Sum => GetSum();
        public int AcedSum => GetSum(acedSum: true);
        public bool HasUnusedAceCard => Cards != null && Cards.Any(c => c.IsAceOfSpade && !c.AceUsed);

        public List<Card> Cards { get; set; }

        protected void GetCards()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Card Types - Spade (S), Heart (H), Diamond (D), Club (C)");

            for (int i = 0; i < CardCount; i++)
            {
                var index = i + 1;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"What is the type of Card {index}?");
                var cardType = Console.ReadLine();
                if (IsValidCardType(cardType))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Enter value on card {index}. Between 1 and 10, (J)ack, (Q)ueen, (K)ing");
                    var card = ValidateCard(Console.ReadLine(), cardType);

                    //Invalid card - retry!
                    if (card == null)
                    {
                        i--;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong entry, try again!");
                        continue;
                    }

                    if (card.IsAceOfSpade && Cards.Any(x => x.IsAceOfSpade))
                    {
                        i--;
                        Console.WriteLine("Deck cannot have more than one ace of spade, try again!");
                    }
                    else
                    {
                        Cards.Add(card);
                    }
                }
                else
                {
                    //Invalid card type - Retry!
                    i--;
                }

            }
        }
        private void UseAceCard()
        {
            if (!HasUnusedAceCard) return;

            //Only update card stack if ace hasnt been used
            var acedCards = new List<Card>();
            Cards.ForEach(x =>
            {
                if (x.IsAceOfSpade)
                {
                    x.Value = 1;
                    x.AceUsed = true;
                }
                acedCards.Add(x);
            });
            Cards = acedCards;

        }
        protected int GetSum(bool acedSum = false)
        {
            if (acedSum)
            {
                //Update stack with ace value
                UseAceCard();
            }
            return Cards.Sum(x => x.Value);
        }

        private static bool IsValidCardType(string val)
        {
            List<string> types = new List<string> { Club, Heart, Diamond, Spade };

            if (val.Length > 1) return false;
            return types.Contains(val.ToLower());
        }
        private static Card ValidateCard(string val, string cardType)
        {
            Card card = null;
            const int defaultAceValue = 11;
            List<string> specialCards = new List<string> { Jack, Queen, King };

            //Special cards - jack/queen/king
            if (specialCards.Contains(val.ToLower().FirstOrDefault().ToString()))
            {
                card = new Card() { Value = 10 };
            }
            else
            {
                int value;
                int.TryParse(val, out value);
                //Spade with ace
                if (value == 1 && cardType.ToLower().StartsWith(Spade))
                {
                    card = new Card() { Value = defaultAceValue, IsAceOfSpade = true };
                }
                //Others
                else if (value >= 1 && value <= 10)
                {
                    card = new Card() { Value = value };
                }
            }

            return card;
        }
    }
    public sealed class Dealer : Player
    {
        public Dealer(int numberOfCards) : base(numberOfCards)
        {

        }
    }

    [DebuggerDisplay("Is Ace Of Spade - {IsAceOfSpade}, Value - {Value}")]
    public class Card
    {
        //public Card(string type, string specialValue)
        //{

        //}

        public int Value { get; set; }
        public bool IsAceOfSpade { get; set; }
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