using System.Collections.Generic;
using System.Security.Cryptography;
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

        public new string GetHashCode()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in Cards)
                stringBuilder.Append(item.Id);

            return stringBuilder.ToString();
        }

        private string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
