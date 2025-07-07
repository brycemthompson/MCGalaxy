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
using MCGalaxy.DB;
using MCGalaxy.Eco;
using MCGalaxy.Games;
using MCGalaxy.SQL;
using System.Runtime.InteropServices;

namespace MCGalaxy.Modules.Games.FootballGame
{    
    public partial class FootballGame : RoundsGame 
    {
        //public int TotalWins, TotalLosses, MaxRoundGoals, MaxConsecutiveGoals, MaxConsecutiveWins;
        struct FootballStats { public int TotalWon, TotalLost, CurrentRoundGoals, MaxRoundGoals, MaxConsecutiveGoals, MaxConsecutiveWins; }
        
        static TopStat statTotalWon, statTotalLost, statCurrentRoundGoals, statMaxRoundGoals, statMaxConsecutiveGoals, statMaxConsecutiveWins;
        static OfflineStatPrinter offlineFootballStats;
        static OnlineStatPrinter onlineFootballStats;
        static ChatToken wonToken, lostToken;
        
        static void HookStats() {
            statTotalWon            = new DBTopStat("Wins", "Total rounds won",
                                             "FootballStats", "TotalWon", TopStat.FormatInteger);
            statTotalLost           = new DBTopStat("Losses", "Total rounds lost",
                                             "FootballStats", "TotalLost", TopStat.FormatInteger);
            statCurrentRoundGoals   = new DBTopStat("CurrentRoundGoals", "Goals scored in the current round",
                                             "FootballStats", "CurrentRoundGoals", TopStat.FormatInteger);
            statMaxRoundGoals       = new DBTopStat("MaxRoundGoals", "Most goals within a round",
                                             "FootballStats", "MaxRoundGoals", TopStat.FormatInteger);
            statMaxConsecutiveGoals = new DBTopStat("MaxConsecutiveGoals", "Most consecutive goals within a round", 
                                             "FootballStats", "MaxConsecutiveGoals", TopStat.FormatInteger);
            statMaxConsecutiveWins = new DBTopStat("MaxConsecutiveWins", "Most consecutive wins",
                                             "FootballStats", "MaxConsecutiveWins", TopStat.FormatInteger);

            wonToken = new ChatToken("$won", "A player's total number of wins",
                                          p => Get(p).TotalWon.ToString());
            lostToken = new ChatToken("$lost", "A player's total number of losses",
                                          p => Get(p).TotalLost.ToString());
            
            offlineFootballStats = PrintOfflineFootballStats;
            onlineFootballStats = PrintOnlineFootballStats;
            OfflineStat.Stats.Add(offlineFootballStats);
            OnlineStat.Stats.Add(onlineFootballStats);
            ChatTokens.Standard.Add(wonToken);
            ChatTokens.Standard.Add(lostToken);
            
            TopStat.Register(statTotalWon);
            TopStat.Register(statTotalLost);
            TopStat.Register(statCurrentRoundGoals);
            TopStat.Register(statMaxRoundGoals);
            TopStat.Register(statMaxConsecutiveGoals);
            TopStat.Register(statMaxConsecutiveWins);
        }
        
        static void UnhookStats() {
            OfflineStat.Stats.Remove(offlineFootballStats);
            OnlineStat.Stats.Remove(onlineFootballStats);
            ChatTokens.Standard.Remove(wonToken);
            ChatTokens.Standard.Remove(lostToken);
            
            TopStat.Unregister(statTotalWon);
            TopStat.Unregister(statTotalLost);
            TopStat.Unregister(statCurrentRoundGoals);
            TopStat.Unregister(statMaxRoundGoals);
            TopStat.Unregister(statMaxConsecutiveGoals);
            TopStat.Unregister(statMaxConsecutiveWins);
        }
        
        static void PrintOnlineFootballStats(Player p, Player who) {
            FootballData data = Get(who);
            PrintFootballStats(p, data.TotalWon, data.TotalLost, 
                        data.CurrentRoundGoals, data.MaxRoundGoals, 
                        data.MaxConsecutiveGoals, data.MaxConsecutiveWins);
        }
        
        static void PrintOfflineFootballStats(Player p, PlayerData who) {
            FootballStats stats = LoadStats(who.Name);
            PrintFootballStats(p, stats.TotalWon, stats.TotalLost,
                         stats.CurrentRoundGoals, stats.MaxRoundGoals, 
                         stats.MaxConsecutiveGoals, stats.MaxConsecutiveWins);
        }
        
        static void PrintFootballStats(Player p, int won, int lost, int currentRoundGoals, int goalsMax, int consecutiveGoalsMax, int consecutiveWinsMax) {
            p.Message("  Won &a{0} &Srounds (max consecutive &e{1}&S)", won, consecutiveWinsMax);
            p.Message("  Lost &a{0} &Srounds)", lost);
            p.Message("  Scored &a{0} &Sgoals (max consecutive &e{1}&S)", goalsMax, consecutiveGoalsMax);
            p.Message("  Scored &a{0} &Sgoals in the current round", currentRoundGoals);
        }        
        
                
        static ColumnDesc[] footballTable = new ColumnDesc[] {
            new ColumnDesc("ID", ColumnType.Integer, priKey: true, autoInc: true, notNull: true),
            new ColumnDesc("Name", ColumnType.Char, 20),
            new ColumnDesc("TotalWon", ColumnType.Int32),
            new ColumnDesc("TotalLost", ColumnType.Int32),
            new ColumnDesc("CurrentRoundGoals", ColumnType.Int32),
            new ColumnDesc("MaxRoundGoals", ColumnType.Int32),
            new ColumnDesc("MaxConsecutiveGoals", ColumnType.Int32),
            new ColumnDesc("MaxConsecutiveWins", ColumnType.Int32),
            // reserve space for possible future additions
            new ColumnDesc("Additional1", ColumnType.Int32),
        };
        
        static FootballStats ParseStats(ISqlRecord record) {
            FootballStats stats;
            stats.TotalWon              = record.GetInt("TotalWon");
            stats.TotalLost             = record.GetInt("TotalLost");
            stats.CurrentRoundGoals     = record.GetInt("CurrentRoundGoals");
            stats.MaxRoundGoals         = record.GetInt("MaxRoundGoals");
            stats.MaxConsecutiveGoals   = record.GetInt("MaxConsecutiveGoals");
            stats.MaxConsecutiveWins    = record.GetInt("MaxConsecutiveWins");
            return stats;
        }
        
        static FootballStats LoadStats(string name) {
            FootballStats stats = default(FootballStats);
            Database.ReadRows("FootballStats", "*", 
                                record => stats = ParseStats(record), 
                                "WHERE Name=@0", name);
            return stats;
        }
        
        protected override void SaveStats(Player p) {
            FootballData data = TryGet(p);
            if (data == null || (data.TotalWon == 0 && data.TotalLost == 0)) return;
            
            object[] args = new object[] {
                data.TotalWon, data.TotalLost, 
                data.MaxRoundGoals, data.MaxConsecutiveGoals,
                data.MaxConsecutiveWins, p.name
            };
            
            int changed = Database.UpdateRows("FootballStats", "TotalWon=@0,TotalLost=@1,CurrentRoundGoals=@2,MaxRoundGoals=@3,MaxConsecutiveGoals=@4,MaxConsecutiveWins=@5",
                                              "WHERE Name=@6", args);
            if (changed == 0) {
                Database.AddRow("FootballStats", "TotalWon,TotalLost,CurrentRoundGoals,MaxRoundGoals,MaxConsecutiveGoals,MaxConsecutiveWins,Name", args);
            }
        }
        
        // TODO: Look into changing these commands as needed
        static void HookCommands() {
            Command.TryRegister(true, cmdLastLevels, cmdQueue, cmdShowQueue);
        }
        
        static void UnhookCommands() {
            Command.Unregister(cmdLastLevels, cmdQueue, cmdShowQueue);
        }
        
        static Command cmdLastLevels = new CmdLastLevels();
        static Command cmdQueue      = new CmdQueue();
        static Command cmdShowQueue  = new CmdShowQueue();
        
        static void HookItems() {
            Economy.RegisterItem(itemQueue);
            Economy.RegisterItem(itemBlocks);
            Economy.RegisterItem(itemGoalMsg);
            Economy.RegisterItem(itemInv);
        }
        
        static void UnhookItems() {
            Economy.Items.Remove(itemQueue);
            Economy.Items.Remove(itemBlocks);
            Economy.Items.Remove(itemGoalMsg);
            Economy.Items.Remove(itemInv);
        }       
        
        static Item itemQueue     = new QueueLevelItem();
        static Item itemBlocks    = new BlocksItem();
        static Item itemGoalMsg = new GoalMessageItem();
        static Item itemInv       = new InvisibilityItem();
    }
}
