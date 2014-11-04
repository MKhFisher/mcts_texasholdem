using System;
using System.Collections;
using System.Collections.Generic;
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
            // Creates the deck and shuffles, otherwise deck will be created in perfect order.
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

    class Program
    {
        static void Main(string[] args)
        {
            Deck deck = new Deck();

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

            DealHoldEmHand(deck, 8, 5);

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
            Console.WriteLine(result);
        }

        public static string CheckHands(List<Card> player, List<Card> community)
        {
            List<Card> complete = new List<Card>();
            complete.AddRange(player);
            complete.AddRange(community);

            if (complete.ContainsRoyalFlush())
            {
                return "Royal Flush";
            }

            if (complete.ContainsStraightFlush())
            {
                return "Straight Flush";
            }

            if (complete.ContainsFourOfAKind())
            {
                return "Four of a Kind";
            }

            if (complete.ContainsFullHouse())
            {
                return "Full House";
            }

            if (complete.ContainsFlush())
            {
                return "Flush";
            }

            if (complete.ContainsStraight())
            {
                return "Straight";
            }

            if (complete.ContainsThreeOfAKind())
            {
                return "Three of a Kind";
            }

            if (complete.ContainsTwoPair())
            {
                return "Two Pair";
            }

            if (complete.ContainsPair())
            {
                return "Pair";
            }

            return "High Card";
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
