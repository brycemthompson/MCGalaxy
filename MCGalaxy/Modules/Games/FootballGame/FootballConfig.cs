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
using System.IO;
using MCGalaxy.Config;
using MCGalaxy.Games;

namespace MCGalaxy.Modules.Games.FootballGame
{    
    public sealed class FootballConfig : RoundsGameConfig 
    {
        [ConfigStringList("football-levels-list", "Round")]
        public List<string> Levels = new List<string>();
        [ConfigInt("football-start-countdown", "Round", 30, 0)]
        public int FootballCountdown = 30;
        [ConfigBool("no-pillaring-during-game", "Round", true)]
        public bool NoPillaring = true;
        [ConfigInt("player-invisibility-duration", "Round", 7, 1)]
        public int InvisibilityDuration = 7;
        [ConfigInt("player-invisibility-potions", "Round", 7, 1)]
        public int InvisibilityPotions = 7;

        [ConfigFloat("football-hitbox-distance", "Collisions", 1f)]
        public float HitboxDist = 1f;
        [ConfigFloat("football-max-move-distance", "Collisions", 1.5625f)]
        public float MaxMoveDist = 1.5625f;
        [ConfigInt("collisions-check-interval", "Collisions", 150, 20, 2000)]
        public int CollisionsCheckInterval = 150;
        
        [ConfigString("pandas-tablist-group", "Football", "&bPandas")]
        public string PandasTabListGroup = "&bPandas";
        [ConfigString("homers-tablist-group", "Football", "&eHomers")]
        public string HomersTabListGroup = "&eHomers";
        [ConfigBool("spectate-upon-death", "Football", true)]
        public bool SpectateUponDeath = true;
        [ConfigString("player-model-during-game", "Football", "humanoid")]
        public string PlayerModel = "humanoid";
        [ConfigString("ball-model", "Football", "head")]
        public string BallModel = "head";
        
        [ConfigInt("player-win-reward-min", "Football rewards", 1, 0)]
        public int PlayerRewardMin = 1;
        [ConfigInt("player-win-reward-max", "Football rewards", 5, 0)]
        public int PlayerRewardMax = 5;
        [ConfigInt("goal-scored-multiplier", "Football rewards", 1, 0)]
        public int GoalScoredMultiplier = 1;
        
        static ConfigElement[] cfg;
        public override bool AllowAutoload { get { return true; } }
        protected override string GameName { get { return "Football Game"; } }
        
        public override void Save() {
            if (cfg == null) cfg = ConfigElement.GetAll(typeof(FootballConfig));
            
            using (StreamWriter w = FileIO.CreateGuarded(Path)) 
            {
                w.WriteLine("#   no-pillaring-during-game      = Disables pillaring while Football Game is activated.");
                w.WriteLine();
                ConfigElement.Serialise(cfg, w, this);
            }
        }
        
        public override void Load() {
            if (cfg == null) cfg = ConfigElement.GetAll(typeof(FootballConfig));
            PropertiesFile.Read(Path, ProcessConfigLine);
        }
        
        void ProcessConfigLine(string key, string value) {
            // backwards compatibility
            if (key.CaselessEq("football-levels-list")) {
                Maps = new List<string>(value.SplitComma());
            } else {
                ConfigElement.Parse(cfg, this, key, value);
            }
        }

        public const string ScorerPlaceholder = "<name>";
        const string ScorerObjectPlaceholder = "<object>";

        static string[] defaultMessages = new string[] { "<name> scored!", "<name> slammed the ball in the goal!",
                                                     "GOALLLL by <name>"};

        public static string FormatGoalMessage(string goalMsg, Player scorer) {
            return goalMsg
                .Replace(ScorerPlaceholder, scorer.ColoredName + "&S")
                .Replace(ScorerObjectPlaceholder, scorer.pronouns.Object);
        }

        public static List<string> LoadGoalMessages() {
            List<string> msgs = new List<string>();
            try {
                if (!File.Exists("text/goalmessages.txt")) {
                    File.WriteAllLines("text/goalmessages.txt", defaultMessages);
                }
                msgs = Utils.ReadAllLinesList("text/goalmessages.txt");
            } catch (Exception ex) {
                Logger.LogError("Error loading goal messages list", ex);
            }
            
            if (msgs.Count == 0) msgs = new List<string>(defaultMessages);
            return ConvertGoalMessages(msgs);
        }
        
        static string GoalPath(string name) { return "text/goal/" + name.ToLower() + ".txt"; }
        public static List<string> LoadPlayerGoalMessages(string name) {
            string path = GoalPath(name);
            if (!File.Exists(path)) return null;

            List<string> msgs = Utils.ReadAllLinesList(path);
            return ConvertGoalMessages(msgs);
        }
        
        public static void AppendPlayerGoalMessage(string name, string msg) {
            if (!Directory.Exists("text/goal"))
                Directory.CreateDirectory("text/goal");
            
            string path = GoalPath(name);
            File.AppendAllText(path, msg + Environment.NewLine);
        }

        static List<string> ConvertGoalMessages(List<string> messages) {
            for (int i = 0; i < messages.Count; i++)
            {
                messages[i] = messages[i]
                                .Replace("{0}", ScorerPlaceholder);
            }
            return messages;
        }
    }
}
