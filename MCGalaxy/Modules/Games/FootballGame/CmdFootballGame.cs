/*
    Copyright 2010 MCLawl Team -
    Created by Snowl (David D.) and Cazzar (Cayde D.)

    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using System.ComponentModel;
using MCGalaxy.Commands;
using MCGalaxy.Commands.Fun;
using MCGalaxy.Games;

namespace MCGalaxy.Modules.Games.FootballGame
{
    sealed class CmdFootballGame : RoundsGameCmd 
    {
        public override string name { get { return "FootballGame"; } }
        public override string shortcut { get { return "fb"; } }
        protected override RoundsGame Game { get { return FootballGame.Instance; } }
        public override CommandPerm[] ExtraPerms {
            get { return new[] { new CommandPerm(LevelPermission.Operator, "can manage football game") }; }
        }
        
        protected override void HandleSet(Player p, RoundsGame game_, string[] args) {
            FootballGame game  = (FootballGame)game_;
            FootballConfig cfg = game.Config;
            string prop  = args[1];
            LevelConfig lCfg = p.level.Config;
            
            if (prop.CaselessEq("map")) {
                p.Message("Pillaring allowed: &b" + lCfg.Pillaring);
                p.Message("Build type: &b" + lCfg.BuildType);
                p.Message("Round time: &b{0}" + lCfg.RoundTime.Shorten(true, true));
                return;
            }
            if (args.Length < 3) { Help(p, "set"); return; }  
            
            if (prop.CaselessEq("hitbox")) {
                if (!CommandParser.GetReal(p, args[2], "Hitbox detection", ref cfg.HitboxDist, 0, 4)) return;
                p.Message("Set hitbox detection to &a" + cfg.HitboxDist + " &Sblocks apart");
                
                cfg.Save(); return;
            } else if (prop.CaselessEq("maxmove")) {
                if (!CommandParser.GetReal(p, args[2], "Max move distance", ref cfg.MaxMoveDist, 0, 4)) return;
                p.Message("Set max move distance to &a" + cfg.MaxMoveDist + " &Sblocks apart");
                
                cfg.Save(); return;
            } else if (prop.CaselessEq("pillaring")) {
                if (!CommandParser.GetBool(p, args[2], ref lCfg.Pillaring)) return;
                
                p.Message("Set pillaring allowed to &b" + lCfg.Pillaring);
                game.UpdateAllStatus2();
            } else if (prop.CaselessEq("build")) {
                if (!CommandParser.GetEnum(p, args[2], "Build type", ref lCfg.BuildType)) return;
                p.level.UpdateBlockPermissions();
                
                p.Message("Set build type to &b" + lCfg.BuildType);
                game.UpdateAllStatus2();
            } else if (prop.CaselessEq("roundtime")) {
                if (!ParseTimespan(p, "round time", args, ref lCfg.RoundTime)) return;
            } else {
                Help(p, "set"); return;
            }
            p.level.SaveSettings();
        }

        protected override void HandleGo(Player p, RoundsGame game) {
            if (!game.Running) {
                p.Message("{0} is not running", game.GameName);
            }
            else {
                FootballTeam footballTeam = FootballGame.Instance.GetFootballTeam(p);
                if (footballTeam == null) {
                    p.Message("&cYou must join a team before playing! Use &a/JoinTeam <teamname> &cto join a team!");
                    return;
                }
                PlayerActions.ChangeMap(p, game.Map);
            }
        }

        static bool ParseTimespan(Player p, string arg, string[] args, ref TimeSpan span) {
            if (!CommandParser.GetTimespan(p, args[2], ref span, "set " + arg + " to", "m")) return false;
            p.Message("Set {0} to &b{1}", arg, span.Shorten(true));
            return true;
        }
        
        public override void Help(Player p, string message) {
            if (message.CaselessEq("set")) {
                p.Message("&T/Help fb map &H- Views help for per-map settings");
            } 
            else if (message.CaselessEq("map")) {
                p.Message("&T/fb set map &H-Views map settings");
                p.Message("&T/fb set pillaring [yes/no]");
                p.Message("&HSets whether players are allowed to pillar");
                p.Message("&T/fb set build [normal/modifyonly/nomodify]");
                p.Message("&HSets build type of the map");
                p.Message("&T/fb set roundtime [timespan]");
                p.Message("&HSets how long a round is");
            } else {
                base.Help(p, message);
            }
        }
        
        public override void Help(Player p) {
            p.Message("&T/fb start <map> &H- Starts Football Game");
            p.Message("&T/fb stop &H- Stops Football Game");
            p.Message("&T/fb end &H- Ends current round of Football Game");
            p.Message("&T/fb add/remove &H- Adds/removes current map from map list");
            p.Message("&T/fb set [property] &H- Sets a property. See &T/Help fb set");
            p.Message("&T/fb status &H- Outputs current status of Football Game");
            p.Message("&T/fb go &H- Moves you to the current Football Game map");
        }
    }
}
