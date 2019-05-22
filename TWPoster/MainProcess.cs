using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Linq;

namespace TWPoster
{
    /// <summary>
    /// Main process
    /// </summary>
    public class MainProcess
    {
        private TimeSpan tiempoEspera = TimeSpan.FromMinutes(30);

        public delegate void DeckObtainedEventRaiser(List<string> decks);
        public event DeckObtainedEventRaiser OnDeckObtained;

        private List<Deck> publishedDecks = new List<Deck>();

        private static readonly MainProcess _instance = new MainProcess();

        public static MainProcess Instance { get => _instance; }

        private Timer timer;

        /// <summary>
        /// Starts a new clock instance.
        /// </summary>
        public void Start()
        {
            timer = new Timer(timer_callBack);
            timer.Change(0, 0);
        }

        /// <summary>
        /// Clock event.
        /// </summary>
        /// <param name="state"></param>
        private void timer_callBack(object state)
        {
            try
            {
                List<Deck> decks = null;
                List<string> messages = new List<string>();

                // If needs more decks, fetch.
                if (decks == null || decks.Count == 0)
                {
                    string deckJsons = DeckManager.ObtainTopTenLadderWinRateDecks();
                    decks = JsonConvert.DeserializeObject<List<Deck>>(deckJsons);
                    publishedDecks.Clear();
                    messages.Add($"{DateTime.Now.ToString()} - Deck actualizados.");
                }

                messages.Add($"{DateTime.Now.ToString()} - Cantidad restante: {decks.Count.ToString()}");

                foreach (var item in decks)
                {
                    // If the item exists in published decks collection..
                    var selected = publishedDecks.Where(deck => deck.Decklink == item.Decklink).ToList();

                    // Avoid repeat a published deck.
                    if (selected.Count > 0)
                    {
                        decks.Remove(item);

                        // Fill repeated items in de list.
                        foreach (var publishedItem in selected)
                        {
                            var repeated = decks.Where(deck => deck.Decklink == publishedItem.Decklink).ToList();

                            // Clean decks collection.
                            for (int i = 0; i < repeated.Count; i++)
                                decks.Remove(repeated[i]);
                        }

                        continue;
                    }

                    // Image saving.
                    List<string> images = new List<string>();
                    foreach (var card in item.Cards)
                    {
                        string imgUrl = card.Icon;
                        string name = card.Name;
                        name = name.Replace(".", String.Empty) + ".png";
                        string cardFilePath = Environment.CurrentDirectory + "\\Data\\" + name;


                        // If the card doesn't exist, i'll save it on disk.
                        if (!File.Exists(cardFilePath))
                            using (WebClient wc = new WebClient())
                            using (Stream s = wc.OpenRead(imgUrl))
                            using (Bitmap bmp = new Bitmap(s))
                                bmp.Save(cardFilePath);

                        images.Add(cardFilePath);
                    }

                    // Merging cards.
                    CardMerge cardMerge = new CardMerge(images);
                    var imagePath = cardMerge.GetMerge();

                    // Elixir cost calculation.
                    double elixirCost = 0;

                    foreach (var card in item.Cards)
                    {
                        elixirCost += card.Elixir;
                        elixirCost = elixirCost / 8;
                    }

                    // Elixir cost formatting.
                    var elixirAverage = String.Format("{0:0.0}", elixirCost);

                    // Decklink formatting
                    var deckLink = item.Decklink.Replace("en?", "es?");

                    // Send tw.
                    Jarvis.sendTweet($"Deck TOP mundial -> Costo promedio de elixir:{elixirAverage} - Enlace del deck: {deckLink}", imagePath);

                    // RM Current deck.
                    decks.Remove(item);

                    // Add this deck to published decks.
                    publishedDecks.Add(item);

                    // Thro a new event.
                    OnDeckObtained(messages);

                    // Wait next.
                    return;
                }

            }
            catch (Exception ex)
            {
                File.AppendAllText("log_process.err", "\n" + ex.Message);
                MessageBox.Show("Error a la hora de obtener las cartas." + ex.Message);
            }
            finally
            {
                timer.Change(tiempoEspera, TimeSpan.Zero);
            }
        }
    }
}
