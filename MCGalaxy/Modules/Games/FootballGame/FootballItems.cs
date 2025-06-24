/*
    Copyright 2015-2024 MCGalaxy
    
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
using MCGalaxy.Commands;
using MCGalaxy.Eco;

namespace MCGalaxy.Modules.Games.FootballGame 
{    
    sealed class BlocksItem : SimpleItem 
    {    
        public BlocksItem() {
            Aliases = new string[] { "blocks", "bl", "b" };
            Enabled = true;
            Price   = 1;
        }
        
        public override string Name { get { return "10Blocks"; } }

        public override void OnPurchase(Player p, string args) {
            int count = 1;
            const string group = "Number of groups of 10 blocks";
            if (args.Length > 0 && !CommandParser.GetInt(p, args, group, ref count, 0, 10)) return;
            
            if (!CheckPrice(p, count * Price, (count * 10) + " blocks")) return;
            
            FootballData data = FootballGame.Get(p);
            data.BlocksLeft += 10 * count;
            Economy.MakePurchase(p, Price * count, "%310Blocks: " + (10 * count));
        }
        
        protected internal override void OnStoreCommand(Player p) {
            p.Message("&T/Buy 10blocks [num]");
            p.Message("&HCosts &a{0} * [num] &H{1}", Price, Server.Config.Currency);
            p.Message("Increases the blocks you are able to place by 10 * [num].");
        }
    }
    
    sealed class QueueLevelItem : SimpleItem 
    {    
        public QueueLevelItem() {
            Aliases = new string[] { "queuelevel", "queuelvl", "queue" };
            Enabled = true;
            Price   = 150;
        }
        
        public override string Name { get { return "QueueLevel"; } }
        
        public override void OnPurchase(Player p, string args) {
            if (FootballGame.Instance.Picker.QueuedMap != null) {
                p.Message("Someone else has already queued a level."); return;
            }
            
            if (args.Length == 0) { OnStoreCommand(p); return; }
            if (!CheckPrice(p)) return;
            
            if (!FootballGame.Instance.SetQueuedLevel(p, args)) return;
            Economy.MakePurchase(p, Price, "%3QueueLevel: " + args);
        }
        
        protected internal override void OnStoreCommand(Player p) {
            p.Message("&T/Buy {0} [level]", Name);
            OutputItemInfo(p);
            p.Message("The map used for the next round of " +
                           "zombie survival will be the given map.");
        }
    }
    
    sealed class GoalMessageItem : SimpleItem 
    {    
        public GoalMessageItem() {
            Aliases = new string[] { "goalmessage", "goalmsg" };
            Enabled = true;
            Price   = 150;
        }
        
        public override string Name { get { return "GoalMessage"; } }
        
        public override void OnPurchase(Player p, string msg) {
            if (msg.Length == 0) { OnStoreCommand(p); return; }
            
            if (!msg.Contains(FootballConfig.ScorerPlaceholder)) {
                p.Message("You need to include a \"{0}\" (placeholder for scoring player name) " +
                               "in the goal message.",
                               FootballConfig.ScorerPlaceholder);
                return;
            }
            
            if (!CheckPrice(p)) return;
            FootballData data = FootballGame.Get(p);
            if (data.GoalMessages == null) data.GoalMessages = new List<string>();
            data.GoalMessages.Add(msg);
            
            FootballConfig.AppendPlayerGoalMessage(p.name, msg);
            p.Message("&aAdded goal message: &f" + msg);
            Economy.MakePurchase(p, Price, "%3GoalMessage: " + msg);
        }

        protected internal override void OnStoreCommand(Player p) {
            base.OnStoreCommand(p);
            p.Message("&HGoal messages must include \"{0}\" (placeholder for scoring player name) in them",
                FootballConfig.ScorerPlaceholder);
        }
    }
    
    sealed class InvisibilityItem : SimpleItem 
    {    
        public InvisibilityItem() {
            // old aliases for when invisibility and zombie invisibility were seperate
            Aliases = new string[] { "invisibility", "invisible", "invis" };
            Enabled = true;
            Price   = 3;
        }
        
        public override string Name { get { return "Invisibility"; } }

        public override void OnPurchase(Player p, string args) {
            if (!CheckPrice(p, Price, "an invisibility potion")) return;
            if (!FootballGame.Instance.RoundInProgress) {
                p.Message("You can only buy an invisiblity potion " +
                          "when a round of football game is in progress."); return;
            }
            
            FootballData data  = FootballGame.Get(p);
            if (data.Invisible) { p.Message("You are already invisible."); return; }
            FootballConfig cfg = FootballGame.Instance.Config;
            
            int maxPotions = cfg.InvisibilityPotions;
            if (data.InvisibilityPotions >= maxPotions) {
                p.Message("You cannot buy any more invisibility potions this round."); return;
            }
            
            DateTime end = FootballGame.Instance.RoundEnd;
            if (DateTime.UtcNow.AddSeconds(60) > end) {
                p.Message("You cannot buy an invisibility potion during the last minute of a round."); return;
            }
            
            int duration = cfg.InvisibilityDuration;
            data.InvisibilityPotions++;
            int left = maxPotions - data.InvisibilityPotions;
            
            p.Message("Lasts for &a{0} &Sseconds. You can buy &a{1} &Smore this round.", duration, left);
            FootballGame.Instance.GoInvisible(p, duration);
            Economy.MakePurchase(p, Price, "%3Invisibility: " + duration);
        }
        
        protected internal override void OnStoreCommand(Player p) {
            FootballConfig cfg = FootballGame.Instance.Config;
            p.Message("&T/Buy " + Name);
            OutputItemInfo(p);
            
            p.Message("Makes you invisible to the other team for {0} seconds", cfg.InvisibilityDuration);
            p.Message("  &WYou can still play the game while invisible");
        }
    }
}
