using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCGalaxy.Modules.Games.FootballGame {
    public class FootballTeam {
        public string Name { get; set; }
        public int Score { get; set; }
        public List<string> Members { get; private set; }
        public const string FOOTBALL_TEAM_PATH = "extra/footballteams/";

        public FootballTeam(string name) {
            Name = name;
            Score = 0;
            Members = GetMembers();
        }

        public void AddMember(string playerName) {
            if (!Members.Contains(playerName)) {
                Members.Add(playerName);
            }
            SaveMembers();
        }

        public bool HasMember(Player p) {
            return this.Members.Contains(p.truename);
        }

        public void RemoveMember(string playerName) {
            Members.Remove(playerName);
            SaveMembers();
        }

        public List<string> GetMembers() {
            if (!File.Exists(FOOTBALL_TEAM_PATH + Name + ".txt")) {
                File.Create(FOOTBALL_TEAM_PATH + Name + ".txt").Close();
                return new List<string>();
            }
            return File.ReadAllLines(FOOTBALL_TEAM_PATH + Name + ".txt").ToList();
        }

        public void SaveMembers() {
            File.WriteAllLines(FOOTBALL_TEAM_PATH + Name + ".txt", Members.ToArray());
        }

        public override string ToString() {
            return $"{Name} (Score: {Score}, Players: {string.Join(", ", Members)})";
        }
    }
}
