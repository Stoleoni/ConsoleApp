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
        public int Id { get; private set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Club { get; set; }
        public string Nationality { get; set; }
        public string Position { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? LastModified { get; set; }


        public Player(int id, string firstName, string lastName, string club, string nationality, string position, DateTime? dateOfBirth, DateTime? lastModified)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Club = club;
            Nationality = nationality;
            Position = position;
            DateOfBirth = dateOfBirth;
            LastModified = lastModified;

        }


        public override string ToString()
        {
            return $"Id: {Id}\nFirst Name: {FirstName}\nLast Name: {LastName}\nClub: {Club}\nNationality: {Nationality}\nPosition: {Position}\nDate of birth: {DateOfBirth}\nLast Modified: {LastModified}\n";  
        }

        public static explicit operator Player(HttpContent v)
        {
            throw new NotImplementedException();
        }
    }


    class Program
    {
        static HttpClient client = new HttpClient();
        static DateTime timeLastModified = new DateTime();

        static void Main(string[] args)
        {
            string directoryAll = @"C:\Api\AllPlayers";
            string directoryOne = @"C:\Api\OnePlayer";

            client.BaseAddress = new Uri("https://localhost:7048");
            var val = "application/json";
            var media = new MediaTypeWithQualityHeaderValue(val);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(media);

            var rez = GetPlayers();
            var lastPulled = DateTime.Now;
            int allPlayersFlag = 0;
            
            var Players = JsonConvert.DeserializeObject<List<Player>>(rez);

            try
            {
                DirectoryInfo diAll = Directory.CreateDirectory(directoryAll);
                DirectoryInfo diOne = Directory.CreateDirectory(directoryOne);
            } catch(Exception e)
            {
                Console.WriteLine("Greška pri izradi direktorija!");
            }

            //SPREMANJE ZASEBNIH NOGOMETASA U FILE
            foreach (Player player in Players)
            {
                int numVersionOnePlayer = 2;
                string fileName = @"C:/Api/OnePlayer/" + player.FirstName + player.Id + player.LastName + numVersionOnePlayer + ".txt";

                while (File.Exists(fileName))
                {
                    numVersionOnePlayer++;
                    fileName = @"C:/Api/OnePlayer/" + player.FirstName + player.Id + player.LastName + numVersionOnePlayer + ".txt";
                    
                }

                numVersionOnePlayer--;
                fileName = @"C:/Api/OnePlayer/" + player.FirstName + player.Id + player.LastName + numVersionOnePlayer + ".txt";

                if (File.Exists(fileName))
                {
                    string line;
                    DateTime parsedTime;
                    using (StreamReader sr = new StreamReader(fileName))
                    {
                        line = sr.ReadLine();
                        parsedTime = DateTime.Parse(line);
                        if(parsedTime.CompareTo(player.LastModified) < 0)
                        {
                            numVersionOnePlayer = numVersionOnePlayer + 1;
                            fileName = @"C:/Api/OnePlayer/" + player.FirstName + player.Id + player.LastName + numVersionOnePlayer + ".txt";
                            allPlayersFlag = 1;

                            using (StreamWriter sw2 = new StreamWriter(fileName))
                            {
                                sw2.WriteLine(lastPulled);
                                sw2.WriteLine();
                                sw2.WriteLine();
                                sw2.WriteLine(player);
                            }
                        } else
                        {
                            Console.WriteLine("Ništa nije promjenjeno");
                        }
                    }
                } 
                else
                {
                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        sw.WriteLine(lastPulled);
                        sw.WriteLine();
                        sw.WriteLine();
                        sw.WriteLine(player);
                    }
                }
            }

            printAllPlayers(allPlayersFlag, lastPulled);

            bool flag = true;
            Console.WriteLine("Unesite slova c,r,u ili d ovisno o operaciji koju želite\nc - create\nr - read\nu - update\nd - delete\n");
            while (flag){
                var c = Console.ReadLine();
                //READ
                if (c.Equals("r"))
                {
                    rez = GetPlayers();
                    Players = JsonConvert.DeserializeObject<List<Player>>(rez);
                    foreach (Player player in Players)
                    {
                        Console.WriteLine(player);
                    }
                }
                //CREATE
                else if (c.Equals("c"))
                {
                    Console.WriteLine("\nName:");
                    string newName = Console.ReadLine();
                    Console.WriteLine("\nSurname:");
                    string newSurname = Console.ReadLine();
                    Console.WriteLine("\nClub:");
                    string newClub = Console.ReadLine();
                    Console.WriteLine("\nNationality:");
                    string newNationality = Console.ReadLine();
                    Console.WriteLine("\nPosition:");
                    string newPosition = Console.ReadLine();
                    Console.WriteLine("\nDate of Birth:");
                    Console.WriteLine("\n\tYear:");
                    int year = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("\n\tMonth:");
                    int month = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("\n\tDay:");
                    int day = Convert.ToInt32(Console.ReadLine());
                    DateTime newDateOfBirth = new DateTime(year, month, day);
                    DateTime newLastaModified = DateTime.Now;

                    Player newPlayer = new Player(0,
                        newName,
                        newSurname,
                        newClub,
                        newNationality,
                        newPosition,
                        newDateOfBirth,
                        DateTime.Now);

                    string response = AddPlayer(newPlayer);
                    timeLastModified = DateTime.Now;
                }

                //UPDATE
                else if (c.Equals("u"))
                {
                    Console.WriteLine("Unesite Id:");
                    int updateId = Convert.ToInt32(Console.ReadLine());
                    List<string> playerToUpdate = GetPlayer(updateId);
                    string updateP = null;

                    foreach(string playerToUpdateItem in playerToUpdate)
                    {
                        updateP = playerToUpdateItem;
                    }

                    var updateObj = JsonConvert.DeserializeObject<Player>(updateP);

                    Console.WriteLine("Nogometaš kojeg želite promjeniti:");
                    Console.WriteLine(updateP);
                    Console.WriteLine("\nOdaberite atribut koji želite izmjeniti:");
                    Console.WriteLine("1 - Ime\n2 - Prezime\n3 - Klub\n4 - Nacionalnost\n5 - Pozicija\n6 - Datum rođenja\n");
                    int num = Convert.ToInt32(Console.ReadLine());

                    if (num.Equals(1))
                    {
                        Console.WriteLine("Unesite novo ime:\n");
                        updateObj.FirstName = Console.ReadLine();
                    }
                    else if (num.Equals(2))
                    {
                        Console.WriteLine("Unesite novo prezime:\n");
                        updateObj.LastName = Console.ReadLine();
                    }
                    else if (num.Equals(3))
                    {
                        Console.WriteLine("Unesite novi klub:\n");
                        updateObj.Club = Console.ReadLine();
                    }
                    else if (num.Equals(4))
                    {
                        Console.WriteLine("Unesite novu nacionalnost:\n");
                        updateObj.Nationality = Console.ReadLine();
                    }
                    else if (num.Equals(5))
                    {
                        Console.WriteLine("Unesite novu poziciju:\n");
                        updateObj.Position = Console.ReadLine();
                    }
                    else if (num.Equals(6))
                    {
                        Console.WriteLine("Unesite novi datum rođenja:\n");
                        Console.WriteLine("Unesite godinu: ");
                        int year = Convert.ToInt32(Console.ReadLine());

                        Console.WriteLine("Unesite mjesec: ");
                        int month = Convert.ToInt32(Console.ReadLine());

                        Console.WriteLine("Unesite dan: ");
                        int day = Convert.ToInt32(Console.ReadLine());

                        updateObj.DateOfBirth = new DateTime(year,month,day);
                    }

                    Console.WriteLine("Izmjenjen nogometaš:");
                    Console.WriteLine(updateObj);
                    Console.WriteLine();
  
                    var message = UpdatePlayers(updateId, updateObj);
                    timeLastModified = DateTime.Now;
                   
                    List<string> lista = GetPlayer(updateId);
                    Console.WriteLine($"Read after update: {lista.FirstOrDefault()}");

                }
                //DELETE
                else if (c.Equals("d"))
                {
                    Console.WriteLine("Unesite Id nogometaša kojeg želite izbrisati:");
                    int deleteId = Convert.ToInt32(Console.ReadLine());
                    string resultDelete = DeletePlayer(deleteId);
                    timeLastModified = DateTime.Now;
                    Console.WriteLine("Nogometaš uspješno izbrisan.");
                }
                else if (c.Equals("x"))
                {
                    Console.WriteLine("Napuštate aplikaciju.");
                    break;
                }
            }   
        }

        private static void printAllPlayers(int allPlayersFlag, DateTime lastPulled)
        {
            string pathAllPlayers = $"C:/Api/AllPlayers/AllPlayers.txt";
            //SPREMANJE SVIH NOGOMETASA U FILE
            if (allPlayersFlag != 0)
            {
                if (File.Exists(pathAllPlayers))
                {
                    int numVersionAllPlayers = 1;
                    pathAllPlayers = $"C:/Api/AllPlayers/AllPlayers{numVersionAllPlayers}.txt";

                    while (File.Exists(pathAllPlayers))
                    {
                        numVersionAllPlayers++;
                        pathAllPlayers = $"C:/Api/AllPlayers/AllPlayers{numVersionAllPlayers}.txt";

                    }
                    pathAllPlayers = $"C:/Api/AllPlayers/AllPlayers{numVersionAllPlayers}.txt";
                }
                using (StreamWriter sw = File.CreateText(pathAllPlayers))
                {
                    var filePlayers = GetPlayers();
                    var pomPlayers = JsonConvert.DeserializeObject<List<Player>>(filePlayers);

                    //Console.WriteLine(filePlayers.Count);
                    sw.Write(lastPulled);
                    sw.WriteLine();
                    sw.WriteLine();
                    foreach (var player in pomPlayers)
                    {
                        sw.WriteLine(player);
                    }
                    sw.WriteLine();
                }
            }
            else
            {
                if (!File.Exists(pathAllPlayers))
                {
                    using (StreamWriter sw = File.CreateText(pathAllPlayers))
                    {
                        var filePlayers = GetPlayers();
                        var pomPlayers = JsonConvert.DeserializeObject<List<Player>>(filePlayers);

                        //Console.WriteLine(filePlayers.Count);
                        sw.Write(lastPulled);
                        sw.WriteLine();
                        sw.WriteLine();
                        foreach (var player in pomPlayers)
                        {
                            sw.WriteLine(player);
                        }
                        sw.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("\nU file-u All Players nije došlo do promjena.");
                }
            }
        }

        private static string AddPlayer(Player player)
        {
            var action = "api/Player/Add";
            var request = client.PostAsJsonAsync(action, player);

            var response = request.Result.Content.ReadAsStringAsync();
            timeLastModified = DateTime.Now;
            printAllPlayers(1, timeLastModified);
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


        private static string UpdatePlayers(int id, Player player)
        {
            var action = $"api/Player/Update/{id}";
            player.LastModified = DateTime.Now;
            var request = client.PutAsJsonAsync(action, player);

            var response = request.Result.Content.ReadAsStringAsync();

            timeLastModified = DateTime.Now;
            printAllPlayers(1, timeLastModified);
            return response.Result;
        }

        private static string DeletePlayer(int id)
        {
            var action = $"api/Player/Delete/{id}";
            var request = client.DeleteAsync(action);

            var response = request.Result.Content.ReadAsStringAsync();
            timeLastModified = DateTime.Now;
            printAllPlayers(1, timeLastModified);
            return response.Result;
        }
    }
}