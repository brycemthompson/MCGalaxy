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
using System.Runtime.Serialization;
using System.Threading;
using MCGalaxy.Games;

namespace MCGalaxy.Modules.Games.FootballGame
{    
    public partial class FootballGame : RoundsGame 
    {
        string lastScorer = "";
        int goalCombo = 0;
        
        protected override void DoRound() {
            if (!Running) return;

            List<Player> players = DoRoundCountdown(Config.FootballCountdown);
            if (players == null) return;

            if (!Running) return;
            RoundInProgress = true;
            StartRound(players);
            
            if (!Running) return;
            DoCoreGame();
        }

        void StartRound(List<Player> players) {
            TimeSpan duration = Map.Config.RoundTime;
            Map.Message("This round will last for &a" + duration.Shorten(true, true));
            RoundEnd = DateTime.UtcNow.Add(duration);
            RandomlyAssignTeams(players);
        }

        void RandomlyAssignTeams(List<Player> players) {
            Random rnd = new Random();
            bool teamFlipper = true;
            int totalPlayers = players.Count;

            for (int i = 0; i < totalPlayers; i++) {
                int playerIndex = rnd.Next(0, players.Count);
                Player p = players[playerIndex];

                if (teamFlipper) {
                    PandaTeam.AddPlayer(p);
                    Map.Message("&b" + p.name + " &Sjoined the &bPanda Team&S!");
                    teamFlipper = false;
                }
                else {
                    HomerTeam.AddPlayer(p);
                    Map.Message("&e" + p.name + " &Sjoined the &eHomer Team&S!");
                    teamFlipper = true;
                }
                Entities.GlobalRespawn(p, false);
                TabList.Add(p, p, Entities.SelfID);
                players.Remove(p);
            }
        }
    
        
        void DoCoreGame() {
            string lastTimeLeft = null;
            int lastCountdown = -1;
            Random random = new Random();
            
            while (Running && RoundInProgress) {
                
                int seconds = (int)(RoundEnd - DateTime.UtcNow).TotalSeconds;
                if (seconds <= 0) {
                    MessageMap(CpeMessageType.Announcement, ""); return;
                }
                
                if (seconds <= 5 && seconds != lastCountdown) {
                    string suffix = seconds == 1 ? " &4second" : " &4seconds";
                    MessageMap(CpeMessageType.Announcement, 
                               "&4Ending in &f" + seconds + suffix);
                    lastCountdown = seconds;
                }
                
                // Update the round time left shown in the top right
                string timeLeft = GetTimeLeft(seconds);
                if (lastTimeLeft != timeLeft) {
                    UpdateAllStatus1();
                    lastTimeLeft = timeLeft;
                }
                
                //DoCollisions(alive, infected, random);
                CheckInvisibilityTime();
                //Thread.Sleep(Config.CollisionsCheckInterval);
            }
        }
        
        /*void DoCollisions(Player[] aliveList, Player[] deadList, Random random) {
            int dist = (int)(Config.HitboxDist * 32);
            foreach (Player killer in deadList)
            {
                FootballData killerData = Get(killer);
                killer.infected = true;
                aliveList = Alive.Items;

                foreach (Player alive in aliveList) 
                {
                    if (alive == killer) continue;
                    if (!InRange(alive, killer, dist)) continue;
                    
                    if (killer.infected && !IsInfected(alive)
                        && !alive.Game.Referee && !killer.Game.Referee
                        && killer.level == Map && alive.level == Map)
                    {
                        InfectPlayer(alive, killer);
                        
                        if (lastKiller == killer.name) {
                            infectCombo++;
                            if (infectCombo >= 2) {
                                killer.Message("You gained " + (2 + infectCombo) + " " + Server.Config.Currency);
                                killer.SetMoney(killer.money + (2 + infectCombo));
                                Map.Message("&c" + killer.DisplayName + " &Sis on a rampage! " + (infectCombo + 1) + " infections in a row!");
                            }
                        } else {
                            infectCombo = 0;
                        }
                        
                        lastKiller = killer.name;
                        killerData.CurrentInfected++;
                        killerData.TotalInfected++;
                        killerData.MaxInfected = Math.Max(killerData.CurrentInfected, killerData.MaxInfected);
                        
                        ShowInfectMessage(random, alive, killer);
                        Thread.Sleep(50);
                    }
                }
            }
        }*/
        
        void CheckInvisibilityTime() {
            DateTime now = DateTime.UtcNow;
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players) 
            {
                if (p.level != Map) continue;
                FootballData data = Get(p);
                if (!data.Invisible) continue;
                
                DateTime end = data.InvisibilityEnd;
                if (now >= end) {
                    p.Message("&cYou are &bvisible &cagain");
                    ResetInvisibility(p, data);
                    continue;
                }
                
                int left = (int)Math.Ceiling((end - now).TotalSeconds);
                if (left == data.InvisibilityTime) continue;
                data.InvisibilityTime = left;
                
                string msg = "&bInvisibility for &a" + left;
                if (p.Supports(CpeExt.MessageTypes)) {
                    p.SendCpeMessage(CpeMessageType.BottomRight2, msg);
                } else {
                    p.Message(msg);
                }
            }
        }
        
        void ShowGoalMessage(Random random, Player scorer) {
            string text = null;
            List<string> goalMsgs = Get(scorer).GoalMessages;
            
            if (goalMsgs != null && goalMsgs.Count > 0 && random.Next(0, 10) < 5) {
                text = goalMsgs[random.Next(goalMsgs.Count)];
            } else {
                text = goalMessages[random.Next(goalMessages.Count)];
            }

            Map.Message(FootballConfig.FormatGoalMessage(text, scorer));
        }

        internal static void RespawnPlayer(Player p) {
            Entities.GlobalRespawn(p, false);
            TabList.Add(p, p, Entities.SelfID);
        }

        public override void EndRound() {
            if (!RoundInProgress) return;
            RoundInProgress = false;
            RoundStart = DateTime.MinValue;
            RoundEnd = DateTime.MinValue;
            UpdateAllStatus1();
            
            if (!Running) return;
            Map.Message("&aThe game has ended!");

            // Determine which team won
            if (PandaTeam.Score > HomerTeam.Score) {
                Map.Message("&SThe &bPanda Team &Shas won this round!");
                Map.Config.RoundsPandaTeamWon++;
                AnnounceWinners(PandaTeam);
                IncreasePlayerStats(PandaTeam, true);
                IncreasePlayerStats(HomerTeam, false);
            }
            else if (HomerTeam.Score > PandaTeam.Score) {
                Map.Message("&SThe &eHomer Team &Shas won this round!");
                Map.Config.RoundsHomerTeamWon++;
                AnnounceWinners(HomerTeam);
                IncreasePlayerStats(HomerTeam, true);
                IncreasePlayerStats(PandaTeam, false);
            }
            else {
                Map.Message("&SBoth teams have tied this round!");
                IncreasePlayerStats(PandaTeam, false);
                IncreasePlayerStats(HomerTeam, false);
            }

            AnnounceScore();
            
            Map.Config.RoundsPlayed++;
            
            Map.SaveSettings();
        }
        void AnnounceScore() {
            if (PandaTeam.Score > HomerTeam.Score) {
                Map.Message("&bPanda Team &S" + PandaTeam.Score + " - " + HomerTeam.Score + " &eHomer Team");
            }
            else if (HomerTeam.Score > PandaTeam.Score) {
                Map.Message("&eHomer Team &S" + HomerTeam.Score + " - " + PandaTeam.Score + " &bPanda Team");
            }
            else {
                Map.Message("&bPanda Team &S" + PandaTeam.Score + " - " + HomerTeam.Score + " &eHomer Team (Tied)");
            }
        }

        void AnnounceWinners(FootballTeam winningTeam) {
            if (winningTeam.Players.Count > 0) {
                Map.Message(winningTeam.Players.Join(p => p.ColoredName)); return;
            }

            int maxGoals = 0;
            List<Player> bestScorers = new List<Player>();
            for (int i = 0; i < winningTeam.Players.Count; i++) {
                if (maxGoals <= Get(winningTeam.Players[i]).CurrentRoundGoals) 
                {
                    maxGoals = Get(winningTeam.Players[i]).CurrentRoundGoals;
                    bestScorers.Add(winningTeam.Players[i]);
                }
            }

            if (bestScorers.Count > 1) {
                string formattedNames = string.Join(", ", bestScorers.Select(p => p.ColoredName));
                Map.Message("&8Best scorers this round are " + formattedNames + " &8with &a" + maxGoals + " &8goals.");
            }
            else {
                string bestScorer = bestScorers[0].ColoredName;
                Map.Message("&8Best scorer this round is " + bestScorer + " &8with &a" + maxGoals + " &8goals.");
            }
        }

        void IncreasePlayerStats(FootballTeam team, bool winningTeam) {

            foreach (Player p in team.Players) {
                FootballData data = Get(p);
                if (winningTeam) {
                    data.TotalWon++;
                    data.MaxRoundGoals = Math.Max(data.MaxRoundGoals, data.CurrentRoundGoals);
                }
                else {
                    data.TotalLost++;
                }
                GiveMoney(p, winningTeam);
            }
        }

        void GiveMoney(Player p, bool winningTeam) {
            Player[] online = PlayerInfo.Online.Items;
            Random rand = new Random();

            FootballData data = Get(p);
            data.ResetInvisibility();
            RewardMoney(p, data, winningTeam);
                
            ResetRoundState(p, data);
 
            if (p.Game.Referee) {
                p.Message("You gained one " + Server.Config.Currency + " because you're a ref. Would you like a medal as well?");
                p.SetMoney(p.money + 1);
            }
                
            RespawnPlayer(p);
            UpdateStatus3(p);
            
        }

        void RewardMoney(Player p, FootballData data, bool winningTeam) {
            if (p.IsLikelyInsideBlock()) {
                p.Message("You may not hide inside a block! No " + Server.Config.Currency + " for you.");
                return;
            }
            
            if (winningTeam) {
                AwardMoney(p, Config.PlayerRewardMin, Config.PlayerRewardMax,
                           new Random(), data.CurrentRoundGoals * Config.GoalScoredMultiplier);
            } else { 
                AwardMoney(p, Config.PlayerRewardMin, Config.PlayerRewardMin,
                           new Random(), data.CurrentRoundGoals * Config.GoalScoredMultiplier);
            }
        }
        
        public override void OutputTimeInfo(Player p) {
            TimeSpan delta = RoundEnd - DateTime.UtcNow;
            if (delta.TotalSeconds > 0) {
                p.Message("&a{0} &Suntil the round ends.", delta.Shorten(true));
            } else {
                delta = RoundStart - DateTime.UtcNow;
                if (delta.TotalSeconds > 0)
                    p.Message("&a{0} &Suntil the round starts.", delta.Shorten(true));
            }
        }
    }
}
