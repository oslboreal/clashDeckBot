using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace TWPoster
{
    public class MainProcess
    {
        private TimeSpan tiempoEspera = TimeSpan.FromMinutes(30);

        public delegate void DeckObtainedEventRaiser(List<string> decks);
        public event DeckObtainedEventRaiser OnDeckObtained;

        private Deck lastDeck = null;

        private static readonly MainProcess _instance = new MainProcess();

        public static MainProcess Instance
        {
            get
            {
                return _instance;
            }
        }

        private Timer timer;

        public MainProcess()
        {

        }

        public void Start()
        {
            timer = new Timer(timer_callBack);
            timer.Change(0, 0);
        }

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
                    decks = decks.GetRange(0, 24);
                    messages.Add($"{DateTime.Now.ToString()} - Deck actualizados.");
                }

                messages.Add($"{DateTime.Now.ToString()} - Cantidad restante: {decks.Count.ToString()}");

                foreach (var item in decks)
                {
                    if (lastDeck != null)
                    {
                        int count = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            // If they are the same cards.
                            if (lastDeck.Cards[i].Description == item.Cards[i].Description)
                                count++;
                        }

                        if (count == 8)
                        {
                            messages.Add($"{DateTime.Now.ToString()} - Deck igual al anterior.");
                            continue;
                        }
                    }


                    if (item.Published == false)
                    {
                        List<string> images = new List<string>();

                        foreach (var card in item.Cards)
                        {
                            string imgUrl = card.Icon;
                            string name = card.Name;
                            name = name.Replace(".", String.Empty) + ".png";
                            string cardFilePath = Environment.CurrentDirectory + "\\Data\\" + name;


                            // Si la carta no existe la descargo.
                            if (!File.Exists(cardFilePath))
                                using (WebClient wc = new WebClient())
                                using (Stream s = wc.OpenRead(imgUrl))
                                using (Bitmap bmp = new Bitmap(s))
                                    bmp.Save(cardFilePath);

                            images.Add(cardFilePath);
                        }

                        CardMerge cardMerge = new CardMerge(images);

                        // Image jam.
                        var imagePath = cardMerge.GetMerge();

                        long elixirCost = 0;

                        foreach (var card in item.Cards)
                            elixirCost += card.Elixir;

                        elixirCost = elixirCost / 8;

                        // Send tw.
                        Jarvis.sendTweet($"Deck tendencia mundial -> Costo de elixir:{elixirCost} - Enlace del deck: {item.Decklink}", imagePath);

                        // Rm current decks.
                        decks.Remove(item);

                        // Set the last deck.
                        lastDeck = item;

                        OnDeckObtained(messages);
                        // Wait next.
                        return;
                    }
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

        public void UnificarImagenes(string image1)
        {

        }
    }
}
