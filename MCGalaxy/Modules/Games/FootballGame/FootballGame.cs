/*
    Copyright 2010 MCLawl Team -
    Created by Snowl (David D.) and Cazzar (Cayde D.)

    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using MCGalaxy.Games;
using MCGalaxy.SQL;

namespace MCGalaxy.Modules.Games.FootballGame 
{      
    internal sealed class FootballData 
    {
        public int BlocksLeft = 50, BlocksStacked;
        internal int LastX, LastY, LastZ;
        
        public bool AkaMode, Invisible;
        public DateTime InvisibilityEnd;
        public List<string> GoalMessages;

        public int TotalWon, TotalLost, MaxRoundGoals, MaxConsecutiveGoals, MaxConsecutiveWins;
        public int InvisibilityTime = -1, InvisibilityPotions;

        public DateTime LastPillarWarn;
        public bool PillarFined;
        /// <summary> Whether the player has pledged that they will win this round. </summary>
        public bool PledgeWin;
        
        public void ResetInvisibility() {
            Invisible = false;
            InvisibilityEnd = DateTime.MinValue;
            InvisibilityTime = -1;
        }
    }
    
    public partial class FootballGame : RoundsGame 
    {
        public FootballConfig Config = new FootballConfig();
        public override string GameName { get { return "Football Game"; } }
        public override RoundsGameConfig GetConfig() { return Config; }
        
        public static FootballGame Instance = new FootballGame();
        public FootballGame() { Picker = new SimpleLevelPicker(); }
        
        protected override string WelcomeMessage {
            get { return "&2Football &Sis running! Type &T/football go &Sto join"; }
        }
        
        public DateTime RoundEnd;
        public FootballTeam PandaTeam = new FootballTeam("Pandas");
        public FootballTeam HomerTeam = new FootballTeam("Homers");
        public Player BallBot;
        internal List<string> goalMessages = new List<string>();
        
        static bool hooked;
        
        const string footballExtrasKey = "MCG_FOOTBALL_DATA";
        internal static FootballData Get(Player p) {
            FootballData data = TryGet(p);
            if (data != null) return data;
            data = new FootballData();

            // TODO: Is this even thread-safe
            // TODO don't load here, add a LoadGoalMessages method
            FootballStats s = LoadStats(p.name);
            data.GoalMessages = FootballConfig.LoadPlayerGoalMessages(p.name);
            data.TotalWon = s.TotalWon;     
            data.TotalLost = s.TotalLost;
            data.MaxRoundGoals = s.MaxRoundGoals; 
            data.MaxConsecutiveGoals = s.MaxConsecutiveGoals;
            data.MaxConsecutiveWins = s.MaxConsecutiveWins;
            
            p.Extras[footballExtrasKey] = data;
            return data;
        }

        internal static FootballData TryGet(Player p) {
            object data; 
            p.Extras.TryGet(footballExtrasKey, out data); 
            return (FootballData)data;
        }
        
        // TODO: Move football map config to per-game properties
        public override void UpdateMapConfig() { }
        
        protected override List<Player> GetPlayers() {
            Player[] players = PlayerInfo.Online.Items;
            List<Player> playing = new List<Player>();
            
            foreach (Player pl in players) 
            {
                if (pl.level != Map || pl.Game.Referee) continue;
                playing.Add(pl);
            }
            return playing;
        }
        
        public override void OutputStatus(Player p) {
            p.Message("{0}: {1}, {2}: {3}", PandaTeam.Name, PandaTeam.Score, HomerTeam.Name, HomerTeam.Score);
        }
        
        public override void Start(Player p, string map, int rounds) {
            // Football starts on current map by default
            if (!p.IsSuper && map.Length == 0) map = p.level.name;
            base.Start(p, map, rounds);
        }
        
        protected override void StartGame() {
            Database.CreateTable("FootballStats", footballTable); 
            if (hooked) return;
            hooked = true;
            ResetGoals();
            HookStats();
            HookCommands();
            HookItems();
        }

        protected override void EndGame() {
            RoundEnd = DateTime.MinValue;
            hooked   = false;
            UnhookStats();
            UnhookCommands();
            UnhookItems();
            ResetGoals();
            
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players) 
            {
                if (pl.level != Map) continue;
                FootballData data = Get(pl);
                
                ResetRoundState(pl, data);
                ResetInvisibility(pl, data);
            }
        }

        public static bool IsInfected(Player p) { return p.infected; }

        public FootballTeam GetFootballTeam(Player p) {
            if (PandaTeam.HasMember(p)) return PandaTeam;
            else if (HomerTeam.HasMember(p)) return HomerTeam;
            else return null; // Player is not on any team
        }

        public void ResetGoals() {
            PandaTeam.Score = 0;
            HomerTeam.Score = 0;
        }

        static void ResetRoundState(Player p, FootballData data) {
            data.BlocksLeft          = 50;
            data.InvisibilityPotions = 0;
        }
        
        void UpdatePlayer(Player p, FootballData data, bool infected) {
            p.infected      = infected;
            data.BlocksLeft = infected ? 25 : 50;
            
            ResetInvisibility(p, data);
            UpdateAllStatus1();
            UpdateStatus3(p);
        }
        
        static void ResetInvisibility(Player p, FootballData data) {
            if (!data.Invisible) return;
            p.SendCpeMessage(CpeMessageType.BottomRight2, "");
            
            data.ResetInvisibility();
            Entities.GlobalSpawn(p, false);
        }
        
        public override void PlayerJoinedGame(Player p) {
            bool announce = false;
            HandleJoinedLevel(p, Map, Map, ref announce);
        }

        /*public override string GetPrefix(Player p) {
            if (!Running) return "";
            int winStreak = Get(p).CurrentRoundsSurvived;
            
            if      (winStreak == 1) return "&4*" + p.color;
            else if (winStreak == 2) return "&7*" + p.color;
            else if (winStreak == 3) return "&6*" + p.color;
            else if (winStreak > 0)  return "&6"  + winStreak + p.color;
            return "";
        }*/
        
        public void GoInvisible(Player p, int duration) {
            FootballData data    = Get(p);
            data.Invisible = true;
            data.InvisibilityEnd = DateTime.UtcNow.AddSeconds(duration);

            Map.Message(p.ColoredName + " &Svanished. &a*POOF*");
            Entities.GlobalDespawn(p, false, false);
        }
        
        public override void OutputMapInfo(Player p, string map, LevelConfig cfg) {
            int winChance = cfg.RoundsPlayed == 0 ? 100 : (cfg.RoundsHumanWon * 100) / cfg.RoundsPlayed;
            p.Message("&a{0} &Srounds played total, &a{1}% &Swin chance for humans.",
                      cfg.RoundsPlayed, winChance);
        }
        
        static string GetTimeLeft(int seconds) {
            if (seconds < 0) return "";
            if (seconds <= 10) return "10s left";
            if (seconds <= 30) return "30s left";
            if (seconds <= 60) return "1m left";
            return ((seconds + 59) / 60) + "m left";
        }
        
        protected override string FormatStatus1(Player p) {
            int left = (int)(RoundEnd - DateTime.UtcNow).TotalSeconds;
            string timespan = GetTimeLeft(left);
            int playerCount = PlayerInfo.Online.Items.Count();
            
            string format = timespan.Length == 0 ? "&a{0} &Splayers &S(map: {1})" :
                "&a{0} &Splayers &S({2}, map: {1})";
            return string.Format(format, playerCount, Map.MapName, timespan);
        }
        
        protected override string FormatStatus2(Player p) {
            string pillar = "&SPillaring " + (Map.Config.Pillaring ? "&aYes" : "&cNo");
            string type = "&S, Type is &a" + Map.Config.BuildType;
            return pillar + type;
        }

        protected override string FormatStatus3(Player p) {
            FootballTeam footballTeam = GetFootballTeam(p);
            string money = "&a" + p.money + " &S" + Server.Config.Currency;
            string team = ", you are on team " + footballTeam.Name;
            return money + team;
        }
        
        public bool SetQueuedLevel(Player p, string name) {
            string map = Matcher.FindMaps(p, name);
            if (map == null) return false;
                
            p.Message(map + " was queued.");
            Picker.QueuedMap = map.ToLower();
            
            if (Map != null) Map.Message(map + " was queued as the next map.");
            return true;
        }
        
        public override void ReloadConfig() {
            base.ReloadConfig();
            goalMessages = FootballConfig.LoadGoalMessages();
        }
    }
}
