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
            request.AddHeader("auth", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MjYxMCwiaWRlbiI6IjU3NjEyMDczNjMxODc1MDc0MCIsIm1kIjp7InVzZXJuYW1lIjoiam12MTk5NCIsImtleVZlcnNpb24iOjMsImRpc2NyaW1pbmF0b3IiOiIyNzgwIn0sInRzIjoxNTU4MzAyNTM1MTk3fQ.V1w4pNNPFlbBLtrt3bP1OljFYvVu-js4IqLIyxOrFDY");
            IRestResponse response = client.Execute(request);
            return response.Content;
        }

    }

}
