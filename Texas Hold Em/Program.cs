using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texas_Hold_Em
{
    public enum Suit
    {
        Clubs = 0,
        Diamonds = 1,
        Hearts = 2,
        Spades = 3
    }

    public enum Number
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    public enum Hand
    {
        HighCard = 0,
        Pair = 1,
        TwoPair = 2,
        ThreeKind = 3,
        Straight = 4,
        Flush = 5,
        FullHouse = 6,
        FourKind = 7,
        StraightFlush = 8,
        RoyalFlush = 9
    }

    /// <summary>
    /// Card object which holds suit and number.
    /// </summary>
	/// test 
    public class Card
    {
        public Suit suit { get; set; }
        public Number number { get; set; }
    }

    /// <summary>
    /// Uses Card class defined above to simulate 52-card playing deck without jokers.
    /// </summary>
    public class Deck
    {

        public Deck()
        {
            // Creates the deck and shuffles, otherwise deck will be created in perfect order. Also opens SQL connection.
            InitializeDeck();
            this.Shuffle();
        }

        public List<Card> deck { get; set; }
        
        public void InitializeDeck()
        {
            // Uses LinQ to create a deck by using creating initializing 4 suits and then 13 cards per suit. Combines all into single deck with LinQ SelectMany.
            deck = Enumerable.Range(0, 4)
                .SelectMany(x => Enumerable.Range(2, 13)
                    .Select(y => new Card() 
                    { 
                        suit = (Suit)x,
                        number = (Number)y 
                    })).ToList();
        }

        public void Shuffle()
        {
            // Uses Guid to randomly sort deck. 
            deck = deck.OrderBy(x => Guid.NewGuid()).ToList();
        }

        public Card DealCard()
        {
            // Deals one card and removes the card from the deck so it can't be reused in the same hand.
            var card = deck.FirstOrDefault();
            deck.Remove(card);

            return card;
        }

        public IEnumerable<Card> DealCards(int num_cards)
        {
            // Allows dealing multiple cards. Used to discard cards from deck to simulate additional players.
            var cards = deck.Take(num_cards);
            var deal_cards = cards as Card[] ?? cards.ToArray();
            deck.RemoveAll(deal_cards.Contains);

            return deal_cards;
        }
    }

    public class SqlEntry
    {
        public int player_count;
        public string pocket;
        public string flop;
        public string turn;
        public string river;
        public Int64 royal_flush;
        public Int64 straight_flush;
        public Int64 four_of_a_kind;
        public Int64 full_house;
        public Int64 flush;
        public Int64 straight;
        public Int64 three_of_a_kind;
        public Int64 two_pair;
        public Int64 pair;
        public Int64 high_card;
    }

    class Program
    {
        static SqlConnection m_connection = null;

        static void Main(string[] args)
        {
            Deck deck = new Deck();
            m_connection = new SqlConnection("Server=localhost;Database=texas;Integrated Security=SSPI;MultipleActiveResultSets=true");
            m_connection.Open();

            int iterations = 10000;
            
            //// Test code to ensure Deck class performs as expected.
            //Hashtable ht = new Hashtable();
            //int counter = 0;

            //foreach (Card card in deck.deck)
            //{
            //    try
            //    {
            //        ht.Add(card, card);
            //        Console.WriteLine(counter + ". " + card.number + " of " + card.suit);
            //        counter++;
            //    }
            //    catch
            //    {
            //        Console.WriteLine("Duplicate card in deck found: " + card.number + ", " + card.suit);
            //    }
            //}

            //TestCardCombinations();

            for (int i = 0; i < iterations; i++)
            {
                DealHoldEmHand(deck, 8, 5);
                deck = new Deck();
            }

            m_connection.Close();

            Console.WriteLine("Finished.");
            Console.ReadKey();
        }

        public static void TestCardCombinations()
        {
            // Deal 7 cards and test combinations.
            List<Card> hand = new List<Card>();
            Card card1 = new Card();
            card1.suit = (Suit)0;
            card1.number = (Number)14;
            Card card2 = new Card();
            card2.suit = (Suit)1;
            card2.number = (Number)6;
            Card card3 = new Card();
            card3.suit = (Suit)2;
            card3.number = (Number)14;
            Card card4 = new Card();
            card4.suit = (Suit)3;
            card4.number = (Number)14;
            Card card5 = new Card();
            card5.suit = (Suit)0;
            card5.number = (Number)9;
            Card card6 = new Card();
            card6.suit = (Suit)1;
            card6.number = (Number)14;
            Card card7 = new Card();
            card7.suit = (Suit)2;
            card7.number = (Number)4;

            hand.Add(card1);
            hand.Add(card2);
            hand.Add(card3);
            hand.Add(card4);
            hand.Add(card5);
            hand.Add(card6);
            hand.Add(card7);

            bool test = hand.ContainsFourOfAKind();
        }

        public static void DealHoldEmHand(Deck deck, int players, int position)
        {
            // Creates lists for player's cards, other players' cards, and shared community cards.
            List<Card> players_cards = new List<Card>();
            List<List<Card>> others_cards = new List<List<Card>>();
            List<Card> community_cards = new List<Card>();

            // Simulates dealing cards for all players before the player.
            int players_before = position - 1;
            deck.DealCards(players_before * 2);

            // Deals cards to the player.
            players_cards.AddRange(deck.DealCards(2));
            
            // Finishes dealing cards to simulated players.
            int players_after = players - position;
            deck.DealCards(players_after * 2);

            // Burn a card.
            deck.DealCard();

            // Deal flop.
            community_cards.AddRange(deck.DealCards(3));

            // Burn a card.
            deck.DealCard();

            // Deal turn.
            community_cards.Add(deck.DealCard());

            // Burn a card.
            deck.DealCard();

            // Deal river.
            community_cards.Add(deck.DealCard());

            // Check hands.
            string result = CheckHands(players_cards, community_cards);

            // Update DB
            UpdateDB(players, players_cards, community_cards, result);
        }

        public static void UpdateDB(int player_count, List<Card> pocket, List<Card> community, string result)
        {
            bool IsNew = false;
            SqlCommand IsInDB = new SqlCommand("SELECT COUNT(*) FROM results WHERE pocket = @pocket AND flop = @flop AND turn = @turn AND river = @river", m_connection);
            
            pocket = pocket.OrderBy(x => x.number).OrderBy(x => x.suit).ToList();
            string pocket_string = "";
            foreach (Card card in pocket)
            {
                pocket_string += ConvertFromCardToDB(card) + ",";
            }
            pocket_string = pocket_string.Substring(0, pocket_string.Length - 1);

            string flop_string = "";

            List<Card> flop = new List<Card>();
            for (int i = 0; i < 3; i++)
            {
                flop.Add(community[i]);
            }
            flop = flop.Where(x => x != null).OrderBy(x => x.number).ThenBy(y => y.suit).ToList();

            for (int i = 0; i < 3; i++)
            {
                flop_string += ConvertFromCardToDB(flop[i]) + ",";   
            }
            flop_string = flop_string.Substring(0, flop_string.Length - 1);

            string turn_string = ConvertFromCardToDB(community[3]);
            turn_string = turn_string.Substring(0, turn_string.Length);

            string river_string = ConvertFromCardToDB(community[4]);
            river_string = river_string.Substring(0, river_string.Length);

            IsInDB.Parameters.AddWithValue("@pocket", pocket_string);
            IsInDB.Parameters.AddWithValue("@flop", flop_string);
            IsInDB.Parameters.AddWithValue("@turn", turn_string);
            IsInDB.Parameters.AddWithValue("@river", river_string);

            int count = (int)IsInDB.ExecuteScalar();

            SqlCommand update_command;

            if (count > 0)
            {
                update_command = new SqlCommand("UPDATE results SET " + result + " = " + result + " + 1 WHERE pocket = @pocket AND flop = @flop AND turn = @turn AND river = @river", m_connection);
            }
            else
            {
                IsNew = true;
                update_command = new SqlCommand("INSERT INTO results (player_count, pocket, flop, turn, river, royal_flush, straight_flush, four_of_a_kind, full_house, flush, straight, three_of_a_kind, two_pair, pair, high_card) VALUES (@player_count, @pocket, @flop, @turn, @river, @royal_flush, @straight_flush, @four_of_a_kind, @full_house, @flush, @straight, @three_of_a_kind, @two_pair, @pair, @high_card)", m_connection);
            }

            update_command.Parameters.AddWithValue("@player_count", player_count);
            update_command.Parameters.AddWithValue("@pocket", pocket_string);
            update_command.Parameters.AddWithValue("@flop", flop_string);
            update_command.Parameters.AddWithValue("@turn", turn_string);
            update_command.Parameters.AddWithValue("@river", river_string);
            update_command.Parameters.AddWithValue("@royal_flush", 0);
            update_command.Parameters.AddWithValue("@straight_flush", 0);
            update_command.Parameters.AddWithValue("@four_of_a_kind", 0);
            update_command.Parameters.AddWithValue("@full_house", 0);
            update_command.Parameters.AddWithValue("@flush", 0);
            update_command.Parameters.AddWithValue("@straight", 0);
            update_command.Parameters.AddWithValue("@three_of_a_kind", 0);
            update_command.Parameters.AddWithValue("@two_pair", 0);
            update_command.Parameters.AddWithValue("@pair", 0);
            update_command.Parameters.AddWithValue("@high_card", 0);
            

            update_command.ExecuteNonQuery();

            if (IsNew == true)
            {
                UpdateDB(player_count, pocket, community, result);
            }
        }

        public static string ConvertFromCardToDB (Card card)
        {
            string result = "";
            result += ((int)card.number).ToString() + ":" + ((int)card.suit).ToString();

            return result;
        }

        public static string CheckHands(List<Card> player, List<Card> community)
        {
            List<Card> complete = new List<Card>();
            complete.AddRange(player);
            complete.AddRange(community);

            if (complete.ContainsRoyalFlush())
            {
                return "royal_flush";
            }

            if (complete.ContainsStraightFlush())
            {
                return "straight_flush";
            }

            if (complete.ContainsFourOfAKind())
            {
                return "four_of_a_kind";
            }

            if (complete.ContainsFullHouse())
            {
                return "full_house";
            }

            if (complete.ContainsFlush())
            {
                return "flush";
            }

            if (complete.ContainsStraight())
            {
                return "straight";
            }

            if (complete.ContainsThreeOfAKind())
            {
                return "three_of_a_kind";
            }

            if (complete.ContainsTwoPair())
            {
                return "two_pair";
            }

            if (complete.ContainsPair())
            {
                return "pair";
            }

            return "high_card";
        }

        public static void PrintHand(List<Card> hand)
        {
            int counter = 0;
            foreach (Card card in hand)
            {
                Console.WriteLine(counter + ". " + card.number + " of " + card.suit);
                counter++;
            }
            Console.WriteLine();
        }
    }
}
