using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using TweetSharp;

namespace TWPoster
{
    public class MainProcess
    {
        private TimeSpan tiempoEspera = TimeSpan.FromMinutes(60);

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
                // Fetch decks.
                string deckJsons = DeckManager.ObtainTopTenLadderWinRateDecks();
                List<Deck> decks = JsonConvert.DeserializeObject<List<Deck>>(deckJsons);
                OnDeckObtained(decks);
            }
            catch (Exception ex)
            {
                File.WriteAllText("log_process.err", ex.Message);
                MessageBox.Show("Error a la hora de obtener la nueva frase.");
            }
            finally
            {
                timer.Change(tiempoEspera, TimeSpan.Zero);
            }
        }
    }
}
