using RestSharp;

namespace TWPoster
{
    static class DeckManager
    {


        static DeckManager()
        {

        }

        public static string ObtainTopTenLadderWinRateDecks()
        {
            var client = new RestClient("https://api.royaleapi.com/popular/decks");
            var request = new RestRequest(Method.GET);
            request.AddHeader("auth", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MjYxMCwiaWRlbiI6IjU3NjEyMDczNjMxODc1MDc0MCIsIm1kIjp7fSwidHMiOjE1NTc5MzQ2NzM1MTJ9.tfrcwt_wEOe6KjJGuFIcdAxDdS3S9qU4letM4IBzpWg");
            IRestResponse response = client.Execute(request);
            return response.Content;
        }

    }

}
