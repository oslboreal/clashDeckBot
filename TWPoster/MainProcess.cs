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
        private TimeSpan tiempoEspera = TimeSpan.FromMinutes(120);

        public delegate void DeckObtainedEventRaiser(List<Deck> decks);
        public event DeckObtainedEventRaiser OnDeckObtained;

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

                // If needs more decks, fetch.
                if (decks == null || decks.Count == 0)
                {
                    string deckJsons = DeckManager.ObtainTopTenLadderWinRateDecks();
                    decks = JsonConvert.DeserializeObject<List<Deck>>(deckJsons);
                    decks = decks.GetRange(0, 24);
                    OnDeckObtained(decks);
                }

                foreach (var item in decks)
                {

                    if (item.Published == false)
                    {
                        int count = item.Cards.Count;
                        List<string> images = new List<string>();
                        foreach (var card in item.Cards)
                        {
                            string imgUrl = card.Icon;
                            string name = card.Name;
                            name = name.Replace(".", String.Empty) + ".png";
                            string cardFilePath = Environment.CurrentDirectory + "\\Data\\" + name;


                            if (!File.Exists(cardFilePath))
                            {
                                using (WebClient wc = new WebClient())
                                {
                                    using (Stream s = wc.OpenRead(imgUrl))
                                    {
                                        using (Bitmap bmp = new Bitmap(s))
                                        {
                                            bmp.Save(cardFilePath);

                                            // Add bmp to be merged. 
                                            count--;
                                        }
                                    }
                                }
                            }

                            images.Add(cardFilePath);


                        }
                        CardMerge cardMerge = new CardMerge(images);
                        var merge = cardMerge.GetMerge();

                        // Image jam.

                        // Send tw.
                        Jarvis.sendTweet(item.Popularity.ToString());

                        // Rm current decks.
                        decks.Remove(item);

                        // Wait next.
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                File.WriteAllText("log_process.err", ex.Message);
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
