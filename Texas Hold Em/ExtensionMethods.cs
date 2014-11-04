using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texas_Hold_Em
{
    public static class ExtensionMethods
    {
        public static bool ContainsRoyalFlush(this List<Card> hand)
        {
            try
            {
                // Orders 7 cards in descending order and removes last two (royal flush must be the 5 highest cards in the game therefore we don't care about the extra 2 cards).
                hand = hand.OrderByDescending(x => x.number).ToList();
                hand.RemoveAt(hand.Count - 1);
                hand.RemoveAt(hand.Count - 1);

                // Checks to see if remaining 5 cards is a straight.
                try
                {
                    List<Card> straight = hand.OrderBy(x => x.number).Select((i, j) => new { i, j }).GroupBy(x => x.i.number - x.j).Where(y => y.Count() >= 5).Select(x => x.Select(xx => xx.i)).LastOrDefault().ToList();

                    // If it's a straight, check to see if the 5 cards is a flush.
                    if (straight.Count == 5 && straight[0].number == Number.Ten)
                    {
                        bool flush = straight.ContainsFlush();
                        if (flush)
                        {
                            // If both straight and a flush, return royal flush (initial conditions proves it must be a straight from 10 through Ace).
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
            catch 
            {
                return false;
            }
        }

        public static bool ContainsStraightFlush(this List<Card> hand)
        {
            try
            {
                // Checks for at least a 5 card flush.
                List<Card> flush = hand.GroupBy(x => x.suit).Where(y => y.Count() >= 5).SelectMany(x => x).ToList();
                if (flush.Count >= 5)
                {
                    // If at least a 5 card flush exists, try to find at least a 5 card straight.
                    List<Card> straight = flush.OrderBy(x => x.number).Select((i, j) => new { i, j }).GroupBy(x => x.i.number - x.j).Where(y => y.Count() >= 5).Select(x => x.Select(xx => xx.i)).LastOrDefault().ToList();
                    if (straight.Count >= 5)
                    {
                        // If there is at least a 5 card straight flush trim off until we have the highest 5 card straight flush.
                        straight = TrimExcess(straight);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ContainsFourOfAKind(this List<Card> hand)
        {
            try
            {
                // Looks to see if there exists a group of 4 cards that are the same.
                List<Card> four = hand.GroupBy(x => x.number).Where(y => y.Count() == 4).SelectMany(x => x).ToList();
                if (four.Count == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ContainsFullHouse(this List<Card> hand)
        {
            try
            {
                // Looks to see if there are groups of 3 cards the same.
                List<Card> three = hand.GroupBy(x => x.number).Where(y => y.Count() == 3).SelectMany(x => x).ToList();

                // Checks if we have 2 groups of 3 (which is also a full house)
                if (three.Count == 6)
                {
                    return true;
                }

                // Looks to see if there are groups of 2 cards the same.
                List<Card> two = hand.GroupBy(x => x.number).Where(y => y.Count() == 2).SelectMany(x => x).ToList();

                if (three.Count == 3 && two.Count >= 2)
                {
                    // If there is a group of 3 and at least one group of 2, we have a full house.
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ContainsFlush(this List<Card> hand)
        {
            try
            {
                // Looks for at least a group of 5 cards of the same suit.
                List<Card> flush = hand.GroupBy(x => x.suit).Where(y => y.Count() >= 5).SelectMany(x => x).ToList();
                if (flush.Count >= 5)
                {
                    // If there is at least a 5 card flush, trim any excess utnikl there is exactly a 5 card flush.
                    flush = TrimExcess(flush);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ContainsStraight(this List<Card> hand)
        {
            // Looks for at least a 5 card straight.
            try
            {
                List<Card> straight = hand.OrderBy(x => x.number).Select((i, j) => new { i, j }).GroupBy(x => x.i.number - x.j).Where(y => y.Count() >= 5).Select(x => x.Select(xx => xx.i)).LastOrDefault().ToList();
                if (straight.Count >= 5)
                {
                    // If there is at least a 5 card straight, trim any excess until there is exactly a 5 card straight.
                    straight = TrimExcess(straight);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ContainsThreeOfAKind(this List<Card> hand)
        {

            try
            {
                // Looks to see if there are groups of 3 cards the same.
                List<Card> three = hand.GroupBy(x => x.number).Where(y => y.Count() == 3).SelectMany(x => x).ToList();
                if (three.Count >= 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ContainsTwoPair(this List<Card> hand)
        {
            try
            {
                // Looks to see if there are groups of 2 cards the same.
                List<Card> twopair = hand.GroupBy(x => x.number).Where(y => y.Count() == 2).SelectMany(x => x).ToList();
                if (twopair.Count >= 4)
                {
                    // If there are at least 2 groups, then we have a two pair.
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ContainsPair(this List<Card> hand)
        {
            try
            {
                // Looks to see if there are groups of 2 cards the same.
                List<Card> pair = hand.GroupBy(x => x.number).Where(y => y.Count() == 2).SelectMany(x => x).ToList();
                if (pair.Count == 2)
                {
                    // Checks to make sure we only have one group of 2 cards the same (another check to make sure we don't report a two pair as a pair).
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static List<Card> TrimExcess(List<Card> hand)
        {
            if (hand.Count > 5)
            {
                hand = hand.OrderByDescending(x => x.number).ToList();
                hand.RemoveAt(hand.Count - 1);

                return hand = TrimExcess(hand);
            }
            else
            {
                return hand;
            }
        }
    }
}
