using System.Collections.Generic;
using System.Text;

namespace TWPoster
{
    public class Deck
    {
        public Deck(List<Card> cards, string decklink, int popularity)
        {
            Cards = cards;
            Decklink = decklink;
            Popularity = popularity;
            Published = false;
        }

        public List<Card> Cards { get; set; }
        public string Decklink { get; set; }
        public int Popularity { get; set; }
        public bool Published { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Cards)
            {
                sb.AppendLine(item.Name);
            }
            return sb.ToString();
        }
    }
}
