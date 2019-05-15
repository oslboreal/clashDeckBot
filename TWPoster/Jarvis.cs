using System;
using System.Collections.Generic;
using System.Text;
using TweetSharp;
using System.Threading;

namespace TWPoster
{
    static class Jarvis
    {
        // Jarvis tools
        private static JarvisResponse jResponse;
        private static TwitterService mainService;
        private static List<TwitterUser> auxUserList;
        private static List<TwitterStatus> auxTwitList;
        private static UserConfiguration config = UserConfiguration.GetConfiguration();

        public static long likeCount = 0;
        public static long followCount = 0;
        public static long unfollowCount = 0;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Jarvis()
        {
            Jarvis.mainService = new TwitterService(config.CostumerKey, config.CostumerSecret, config.AccessToken, config.AccessTokenSecret);
        }

        #region Jarvis Methods

        /// <summary>
        /// This method send a Tweet.
        /// </summary>
        /// <returns>Boolean</returns>
        public static bool sendTweet(string _status)
        {
            bool bret = false;
            mainService.SendTweet(new SendTweetOptions { Status = _status }, (tweet, response) =>
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("- LOG: Tweet enviado Correctamente - {0}", DateTime.Now);
                    Console.ResetColor();
                }
            });
            return bret;
        }

        /// <summary>
        /// This method update the List of followers
        /// </summary>
        /// <returns>A new User List.</returns>
        public static List<TwitterUser> getFollowers()
        {
            // Inicializamos un nuevo espacio en memoria.
            auxUserList = new List<TwitterUser>();
            foreach (var follower in mainService.ListFollowers(new ListFollowersOptions { }))
            {
                auxUserList.Add(follower);
            }
            // Retornamos la colección.
            return auxUserList;
        }

        /// <summary>
        /// This method return a String with de list of followers
        /// </summary>
        /// <returns></returns>
        public static string getFollowersString()
        {
            StringBuilder aux = new StringBuilder();
            Jarvis.getFollowers();
            aux.Append("FOLLOWERS\n");
            foreach (var follower in Jarvis.auxUserList)
            {
                aux.Append("NAME: " + follower.Name + " ID: " + follower.Id + " FOLLOWERS: " + follower.FollowersCount);
                aux.AppendLine("\n");
            }
            // Retornamos la colección.
            return aux.ToString();
        }

        /// <summary>
        /// Follow a new user
        /// </summary>
        /// <returns>Boolean</returns>
        public static void followUser(TwitterUser user)
        {
            try
            {
                mainService.FollowUser(new FollowUserOptions { UserId = user.Id });
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// Obtain a List of the following users that are non followers.
        /// </summary>
        /// <returns>List</returns>
        public static List<TwitterUser> getFollowingUsers()
        {
            Jarvis.auxUserList = new List<TwitterUser>();
            var colection = mainService.ListFriends(new ListFriendsOptions { });
            foreach (var user in colection)
            {
                auxUserList.Add(user);
            }
            return Jarvis.auxUserList;
        }

        /// <summary>
        /// This method return 15 pages of users (The maximum allowed by twitter) 15requests/15minutes
        /// </summary>
        /// <returns>List of Twitter Users.</returns>
        public static List<TwitterUser> getAllFollowingUsers()
        {
            long nextCursor = -1;
            int count = 0;
            int countOfPages = 0;
            Jarvis.auxUserList = new List<TwitterUser>();
            // New coletion.
            var collection = mainService.ListFriends(new ListFriendsOptions { Cursor = nextCursor });
            while (nextCursor != 0 && countOfPages < 14)
            {
                Thread.Sleep(300);
                foreach (var user in collection)
                {
                    auxUserList.Add(user);
                    count++;
                }
                // Actualizamos la colección.
                collection = mainService.ListFriends(new ListFriendsOptions { Cursor = nextCursor });
                countOfPages++;
            }
            return auxUserList;
        }

        /// <summary>
        /// Like all mentioning tweets.
        /// </summary>
        /// <returns>int Count of likes</returns>
        public static int autoLikeMentions()
        {
            int count = 0;

            // Recorremos la colecion de Menciones
            foreach (var tweet in Jarvis.getMentions())
            {
                // En caso de que el Tweet en cuestión no esté Likeado, lo likeamos.
                if (tweet.IsFavorited == false)
                {
                    mainService.FavoriteTweet(new FavoriteTweetOptions { Id = tweet.Id });
                    count++;
                }
            }

            Jarvis.likeCount += count;
            return count;
        }

        /// <summary>
        /// This method GETS a collection of "Following" users and unfollow them if the user isnt in a Friendship
        /// </summary>
        /// <returns>JarvisResponse - This method returns a JarvisResponse Object with a Message and Counting info.</returns>
        public static JarvisResponse autoUnfollowNoFriendshipsUsers()
        {
            StringBuilder aux = new StringBuilder();
            Jarvis.jResponse = new JarvisResponse();
            jResponse.count = 0;
            aux.AppendLine("Unfollowed Users:");
            // Collection with all followers.
            var colectionOfFollowers = Jarvis.getAllFollowingUsers();
            // Travel all collection

            // Append message footer.
            aux.AppendLine("Users: " + jResponse.count + "/" + colectionOfFollowers.Count);
            jResponse.message = aux.ToString();
            return jResponse;
        }

        /// <summary>
        /// Like all timeline tweets.
        /// </summary>
        /// <returns>int Count of likes</returns>
        public static int autoLikeTimeline()
        {
            int count = 0;

            var collection = Jarvis.getTimeLine(20);

            foreach (var tweet in collection)
            {
                // En caso de que el Tweet en cuestión no esté Likeado, lo likeamos.
                if (tweet.IsFavorited == false)
                {
                    mainService.FavoriteTweet(new FavoriteTweetOptions { Id = tweet.Id });
                    // Aumentamos la cantidad de Likes dados en la instancia de procesamiento actual.
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Obtain a colection of all HOmeTimeline Tweets
        /// </summary>
        /// <returns>List<TwitterStatus> </returns>
        public static List<TwitterStatus> getTimeLine()
        {
            Jarvis.auxTwitList = new List<TwitterStatus>();
            foreach (var item in mainService.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions { Count = 10 }))
            {
                if (auxTwitList.Count >= 20)
                {
                    // En caso de que la colección tenga 20 o más elementos cortamos y retornamos.
                    break;
                }
                else
                {
                    // Elementos traidos de la consulta de twitter.
                    auxTwitList.Add(item);
                }
            }
            return auxTwitList;
        }


        /// <summary>
        /// Obtain a colection of all HOmeTimeline Tweets
        /// </summary>
        /// <returns>List<TwitterStatus> </returns>
        public static List<TwitterStatus> getTimeLine(int cant)
        {
            Jarvis.auxTwitList = new List<TwitterStatus>();
            foreach (var item in mainService.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions { Count = cant }))
            {
                auxTwitList.Add(item);
            }
            return auxTwitList;
        }

        /// <summary>
        /// List all mentioning tweets in a colection.
        /// </summary>
        /// <returns>List<TwitterStatus</returns>
        public static List<TwitterStatus> getMentions()
        {
            Jarvis.auxTwitList = new List<TwitterStatus>();
            foreach (var tweet in mainService.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions { Count = 20 }))
            {
                auxTwitList.Add(tweet);
            }
            return auxTwitList;
        }

        /// <summary>
        /// List all mentioning tweets in a colection.
        /// </summary>
        /// <returns>String</returns>
        public static string getMentionsString()
        {
            StringBuilder aux = new StringBuilder();
            foreach (var item in Jarvis.getMentions())
            {
                aux.Append("\nAUTOR:" + item.Author.ScreenName + "\nTEXT: " + item.Text);
            }
            return aux.ToString();
        }

        /// <summary>
        /// This method return a String with de list of followers
        /// </summary>
        /// <returns></returns>
        public static string getFollowingString()
        {
            StringBuilder aux = new StringBuilder();
            Jarvis.getFollowingUsers();
            aux.Append("FOLLOWING\n");
            foreach (var follower in Jarvis.auxUserList)
            {
                aux.Append("NAME: " + follower.Name + " ID: " + follower.Id + " FOLLOWERS: " + follower.FollowersCount);
                aux.AppendLine("\n");
            }
            // Retornamos la colección.
            return aux.ToString();
        }

        /// <summary>
        /// This method get the list of Following Users IDs
        /// </summary>
        /// <returns></returns>
        public static List<long> getFollowingIds()
        {
            List<long> auxIds = new List<long>();
            foreach (var user in Jarvis.getFollowingUsers())
            {
                auxIds.Add(user.Id);
            }
            return auxIds;
        }

        /// <summary>
        /// Check if the user is fozlowing or not.
        /// </summary>
        /// <returns>Boolean</returns>
        public static bool checkFriendship(long id)
        {
            TwitterFriendship test = mainService.GetFriendshipInfo(new GetFriendshipInfoOptions { TargetId = id.ToString() });
            return test.Relationship.Target.Following;
        }

        /// <summary>
        /// Básicamente retorna la lista de usuarios que publicaron cosas en la Timeline (Generalmente la mayoría son Retweets por lo tanto son usuarios que no sigo.
        /// </summary>
        /// <returns>List of twitter users</returns>
        public static List<TwitterUser> getUsersToFollow()
        {
            Jarvis.auxUserList = new List<TwitterUser>();
            var collection = Jarvis.getTimeLine();
            foreach (var item in collection)
            {
                auxUserList.Add(item.User);
            }
            return Jarvis.auxUserList;
        }

        /// <summary>
        /// Obtains the list of users to follow
        /// </summary>
        /// <returns>List of twitter users</returns>
        public static List<long> getTimelineIds()
        {
            List<long> aux = new List<long>();
            var collection = Jarvis.getTimeLine(30);
            foreach (var post in collection)
            {
                if (post.RetweetedStatus != null)
                {
                    aux.Add(post.RetweetedStatus.User.Id);
                }
            }
            return aux;
        }

        /// <summary>
        /// Recibe una lista de usuarios y si estos no me siguen deja de seguirlos.
        /// </summary>
        /// <param name="listUsers"></param>
        /// <returns>JarvisResponse</returns>
        public static JarvisResponse unfollowBadUsers(List<TwitterUser> listUsers)
        {
            int counter = 0;
            JarvisResponse jResponse = new JarvisResponse();
            StringBuilder aux = new StringBuilder();
            foreach (var item in listUsers)
            {
                counter++;
                // Para manejar el Rate limit de Twitter usamos contador.
                // 15 Requests cada 15 Minutos.
                if (counter < 50)
                {
                    TwitterFriendship test = mainService.GetFriendshipInfo(new GetFriendshipInfoOptions { TargetId = item.Id.ToString() });

                    if (test.Relationship.Target.FollowedBy == true && test.Relationship.Target.Following == false)
                    {
                        jResponse.count++;
                        mainService.UnfollowUser(new UnfollowUserOptions { UserId = item.Id });
                        aux.AppendLine("@" + item.ScreenName + " - ID: " + item.Id);
                        aux.AppendLine("Relationship state = " + test.Relationship.Target.Following);
                    }
                }
            }
            aux.AppendLine("Cantidad de usuarios seguidos: " + jResponse.count);
            Jarvis.unfollowCount += jResponse.count;
            jResponse.message = aux.ToString();
            return jResponse;
        }

        /// <summary>
        /// This method AutoFollow new users that not follows our Account
        /// </summary>
        /// <param name="listUsers"></param>
        /// <returns>JarvisResponse</returns>
        public static JarvisResponse followNewUsers(List<long> idList)
        {
            int counter = 0;
            JarvisResponse jResponse = new JarvisResponse();
            StringBuilder aux = new StringBuilder();
            foreach (var item in idList)
            {
                counter++;
                // Para manejar el Rate limit de Twitter usamos contador.
                // 15 Requests cada 15 Minutos.
                if (counter < 30)
                {
                    jResponse.count++;
                    mainService.FollowUser(new FollowUserOptions { UserId = item });
                    aux.AppendLine("ID: " + item);
                }
            }
            aux.AppendLine("Cantidad de usuarios seguidos: " + jResponse.count);
            Jarvis.followCount += jResponse.count;
            jResponse.message = aux.ToString();
            return jResponse;
        }

        public static JarvisResponse getRelationInformation(long userId)
        {
            JarvisResponse jResponse = new JarvisResponse();
            StringBuilder sBuilder = new StringBuilder();
            TwitterFriendship rs = mainService.GetFriendshipInfo(new GetFriendshipInfoOptions { TargetId = userId.ToString() });
            sBuilder.AppendLine("FollowedBy: " + rs.Relationship.Target.FollowedBy.ToString());
            sBuilder.AppendLine("Following: " + rs.Relationship.Target.Following.ToString());
            jResponse.message = sBuilder.ToString();
            return jResponse;
        }


        #endregion
    }
}