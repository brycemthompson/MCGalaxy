using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCGalaxy.Modules.Games.FootballGame {
    public class FootballTeam {
        public string Name { get; set; }
        public int Score { get; set; }
        public List<Player> Players { get; private set; }
        //public const string FOOTBALL_TEAM_PATH = "extra/footballteams/";

        public FootballTeam(string name) {
            Name = name;
            Score = 0;
            Players = new List<Player>();
        }

        public void AddPlayer(Player p) {
            if (!Players.Contains(p)) {
                Players.Add(p);
            }
        }

        public bool HasMember(Player p) {
            return this.Players.Contains(p);
        }

        public void RemoveMember(Player p) {
            Players.Remove(p);
        }

        /*public List<string> GetMembers() {
            if (!File.Exists(FOOTBALL_TEAM_PATH + Name + ".txt")) {
                File.Create(FOOTBALL_TEAM_PATH + Name + ".txt").Close();
                return new List<string>();
            }
            return File.ReadAllLines(FOOTBALL_TEAM_PATH + Name + ".txt").ToList();
        }

        public void SaveMembers() {
            File.WriteAllLines(FOOTBALL_TEAM_PATH + Name + ".txt", Members.ToArray());
        }*/

        public override string ToString() {
            List<string> playerNames = Players.Select(p => p.ColoredName).ToList();
            return $"{Name} (Score: {Score}, Players: {string.Join(", ", playerNames)})";
        }
    }
}
