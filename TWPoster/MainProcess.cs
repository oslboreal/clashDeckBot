using Newtonsoft.Json;
using Pekka.ClashRoyaleApi.Client.Contracts;
using Pekka.ClashRoyaleApi.Client.Standalone;
using Pekka.Core;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace TWPoster
{
    /// <summary>
    /// Main process
    /// </summary>
    public class MainProcess
    {
        private const int TIEMPO_ESPERA = 60;
        private const string ARCHIVO_CARTAS = "cartas.json";
        private int ultimoDiaPublicoCartaDelDia = 0;

        private TimeSpan tiempoEspera = TimeSpan.FromMinutes(TIEMPO_ESPERA);

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

        public void UpdateCardsCollection()
        {
            var client = new RestClient("https://api.clashroyale.com/v1/cards");
            var request = new RestRequest(Method.GET);
            request.AddParameter("Authorization", string.Format("Bearer " + "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiIsImtpZCI6IjI4YTMxOGY3LTAwMDAtYTFlYi03ZmExLTJjNzQzM2M2Y2NhNSJ9.eyJpc3MiOiJzdXBlcmNlbGwiLCJhdWQiOiJzdXBlcmNlbGw6Z2FtZWFwaSIsImp0aSI6ImI2YTk2MDU5LWU3N2UtNDQwYy1hN2FmLTE4NDUwYTVhOTk0MyIsImlhdCI6MTU1ODY1NTI3Niwic3ViIjoiZGV2ZWxvcGVyL2EzYWQ1NDNiLTcxZDYtNTg1ZC04ZDJjLTlhNjA0MTJlY2FmZiIsInNjb3BlcyI6WyJyb3lhbGUiXSwibGltaXRzIjpbeyJ0aWVyIjoiZGV2ZWxvcGVyL3NpbHZlciIsInR5cGUiOiJ0aHJvdHRsaW5nIn0seyJjaWRycyI6WyIxODEuNDUuNDMuMTIxIl0sInR5cGUiOiJjbGllbnQifV19.ZCgUa9X81blhEAWFh7WfFixhI-ZynJ4sqFRzfjHdulVUpId99jSKzwq43Si1jfongiKuk933m-ewyB9JKqEhSA"),
            ParameterType.HttpHeader);
            string json = client.Execute(request).Content;
            
            File.WriteAllText(ARCHIVO_CARTAS, json);
        }

        /// <summary>
        /// Publica la carta del día.
        /// </summary>
        public void PublicarCartaDelDia()
        {
            if (ultimoDiaPublicoCartaDelDia == 0 || DateTime.Now.Day != ultimoDiaPublicoCartaDelDia)
            {
                int indexCartaActual = 0;

                // Obtengo el index de la carta actual
                if (!File.Exists("proximaCartaDelDia.bin"))
                    File.WriteAllText("proximaCartaDelDia.bin", "0");
                else
                    indexCartaActual = int.Parse(File.ReadAllText("proximaCartaDelDia.bin"));

                // Si no existe el archivo de las cartas, actualizo.
                if (!File.Exists(ARCHIVO_CARTAS))
                    UpdateCardsCollection();

                string json = File.ReadAllText(ARCHIVO_CARTAS);
                var cards = JsonConvert.DeserializeObject<Pekka.ClashRoyaleApi.Client.Models.CardList>(json);

                Pekka.ClashRoyaleApi.Client.Models.Card publishingCard = null;

                // Agarro la carta en cuestión.
                if (indexCartaActual <= cards.Items.Count - 1)
                    publishingCard = cards.Items.GetRange(indexCartaActual, 1)[0];

                if (publishingCard != null)
                {
                    var imagePath = SaveCardImage(publishingCard.Name, publishingCard.IconUrls.Medium).Replace("\\\\", "\\");

                    // Sends tweet.
                    Jarvis.sendTweet($"A ver que tan buen jugador te consideras. -> ¿Qué carta es fuerte y que carta es debil contra {publishingCard.Name}?", imagePath);

                    // Seteamos el index de la próxima carta.
                    int indexProximaCarta = indexCartaActual + 1;
                    File.WriteAllText("proximaCartaDelDia.bin", indexProximaCarta.ToString());

                    // Actualizamos el último día que se publicó una carta.
                    ultimoDiaPublicoCartaDelDia = DateTime.Now.Day;
                }
            }
        }

        private string SaveCardImage(string cardName, string imgUrl)
        {
            try
            {
                string cardFilePath = Environment.CurrentDirectory + "\\Data\\" + cardName.Replace(".", String.Empty) + ".png";

                // If the card doesn't exist, i'll save it on disk.
                if (!File.Exists(cardFilePath))
                    using (WebClient wc = new WebClient())
                    using (Stream s = wc.OpenRead(imgUrl))
                    using (Bitmap bmp = new Bitmap(s))
                        bmp.Save(cardFilePath);

                return cardFilePath;
            }
            catch (Exception)
            {
                return null;
            }

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

                    // Repeated filter.
                    int init = decks.Count;

                    for (int i = 0; i < decks.Count; i++)
                    {
                        for (int j = 1; j < decks.Count; j++)
                        {
                            var firstDeck = decks[i];
                            var secondDeck = decks[j];
                            if (firstDeck == secondDeck)
                                decks.Remove(decks[i]);
                        }
                    }

                    int finish = decks.Count;

                    messages.Add($"{DateTime.Now.ToString()} - Deck actualizados.");

                    PublicarCartaDelDia();
                    messages.Add($"{DateTime.Now.ToString()} - Carta del día publicada..");
                }

                messages.Add($"{DateTime.Now.ToString()} - Cantidad restante: {decks.Count.ToString()}");

                foreach (var item in decks)
                {
                    // Image saving.
                    List<string> images = new List<string>();
                    foreach (var card in item.Cards)
                    {
                        string imgUrl = card.Icon;
                        string name = card.Name;

                        string cardFilePath = SaveCardImage(name, imgUrl);

                        images.Add(cardFilePath);
                    }

                    // Merging cards.
                    CardMerge cardMerge = new CardMerge(images);
                    var imagePath = cardMerge.GetMerge();

                    // Elixir cost calculation.
                    double elixirCost = 0;

                    foreach (var card in item.Cards)
                        elixirCost += card.Elixir;

                    elixirCost = elixirCost / 8;

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
