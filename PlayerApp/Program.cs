using MediatR;
using Newtonsoft.Json;
using PlayerApp.Constants;
using RestSharp;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PlayerApp
{
    public class Player
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Club { get; set; }
        public string Nationality { get; set; }
        public string Position { get; set; }

        public Player(int id, string firstName, string lastName, string club, string nationality, string position)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Club = club;
            Nationality = nationality;
            Position = position;

        }

        public override string ToString()
        {
            return $"Id({Id}) - First Name({FirstName}) - Last Name({LastName})";  
        }

        public static explicit operator Player(HttpContent v)
        {
            throw new NotImplementedException();
        }
    }


    class Program
    {
        static HttpClient client = new HttpClient();
        static void Main(string[] args)
        {
            string fileName = @"C:\Api\AllPlayers.txt";
            string fileOnePlayer = @"C:\Api\OnePlayer.txt";

            client.BaseAddress = new Uri("https://localhost:7048");
            var val = "application/json";
            var media = new MediaTypeWithQualityHeaderValue(val);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(media);

            var rez = GetPlayers();
            var time = DateTime.Now;
            var Players = JsonConvert.DeserializeObject<List<Player>>(rez);


            Console.WriteLine("Odaberite zelite li jednoga ili sve igrace\tUkoliko želite samo jednoga unesite njegov id (1-10), a za sve unesite 0");
            int number = Console.Read();
            if (Enumerable.Range(1, 10).Contains(number))
            {
                writing(fileOnePlayer, Players);
            } else
            {
                writing(fileName, Players);
            }

            try
            {
                var message = string.Empty;
                Player player = new Player(10, "MILJENKO", "Milic", "Osijek", "HRV", "CM");

                //create
                message = AddPlayer(player);

                //read ALL
                List<string> players = GetPlayer();

                //read 1
                List<string> players2 = GetPlayer(5);               

                //update
                message = UpdatePlayers(4, player);
                players2 = GetPlayer(5);

                //delete
                message = DeletePlayer(7);
                players2 = GetPlayer(5);

            } catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ReadLine();
        }

        private static string AddPlayer(Player player)
        {
            var action = "api/Player/Add";
            var request = client.PostAsJsonAsync(action, player);

            var response = request.Result.Content.ReadAsStringAsync();

            return response.Result;
        }

        private static List<string> GetPlayer(int? id = null)
        {
            List<string> playersList = new List<string>();
            var action = $"api/Player/Get/{id}";
            var request = client.GetAsync(action);

            var response = request.Result.Content.ReadAsStringAsync();
            var result = response.Result;

            if (id == null)
            {
                playersList = result.Split('\u002C').ToList();
            } else
            {
                playersList.Add(result);
            }
            return playersList;
        }

        private static string GetPlayers()
        {
            var action = $"api/Player/Get";
            var request = client.GetAsync(action);

            var response = request.Result.Content.ReadAsStringAsync();
            var result = response.Result;

            return result;
        }

        private static void writing(string filename, List<Player> Players)
        {
            if (!File.Exists(filename))
            {
                using (StreamWriter writer = File.CreateText(filename))
                {
                    writer.WriteLine(DateTime.Now);
                    for (int i = 0; i < Players.Count; i++)
                    {
                        writer.WriteLine(Players[i].ToString());
                    }
                }
            }
        }

        private static string UpdatePlayers(int id, Player player)
        {
            var action = $"api/Player/Update/{id}";
            var request = client.PutAsJsonAsync(action, player);

            var response = request.Result.Content.ReadAsStringAsync();

            return response.Result;
        }

        private static string DeletePlayer(int id)
        {
            var action = $"api/Player/Delete/{id}";
            var request = client.DeleteAsync(action);

            var response = request.Result.Content.ReadAsStringAsync();
            return response.Result;
        }
    }
}