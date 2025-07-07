/*
    Copyright 2011 MCForge
    
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
using System.Collections.Generic;
using MCGalaxy.DB;
using MCGalaxy.Games;

namespace MCGalaxy.Modules.Games.FootballGame
{
    sealed class CmdViewTeam : Command2 
    {
        public override string name { get { return "ViewTeam"; } }
        public override string shortcut { get { return "vt"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override void Use(Player p, string message, CommandData data) {
            string teamName = message.Trim();

            if (teamName.Length == 0) {
                FootballTeam team = GetTeam(p);
                if (team == null) {
                    DisplayTeam(p, FootballGame.Instance.PandaTeam);
                    DisplayTeam(p, FootballGame.Instance.HomerTeam);
                }
                else {
                    DisplayTeam(p, team);
                }
            }
        }

        public void DisplayTeam(Player p, FootballTeam team) {
            string teamColor = string.Empty;
            if (team.Name == FootballGame.Instance.PandaTeam.Name) {
                teamColor = "&b";
            }
            else {
                teamColor = "&e";
            }

            Player[] onlinePlayers = PlayerInfo.Online.Items;
            List<Player> teamMembers = new List<Player>();

            foreach (Player player in onlinePlayers) {
                if (team.HasPlayer(player)) {
                    teamMembers.Add(player);
                }
            }

            string formattedMembers = string.Join(", ", teamMembers.ConvertAll(player => player.ColoredName).ToArray());
            p.Message("&S==== {0}{1} Team &S====");
            p.Message("{0}Team Members: {1}", teamColor, formattedMembers);
        }

        public FootballTeam GetTeam(Player p) {
            if (FootballGame.Instance.PandaTeam.HasPlayer(p)) {
                return FootballGame.Instance.PandaTeam;
            }
            else if (FootballGame.Instance.HomerTeam.HasPlayer(p)) {
                return FootballGame.Instance.HomerTeam;
            }
            else {
                return null;
            }
        }
        
        public override void Help(Player p) {
            p.Message("&T/Infected");
            p.Message("&HShows who is infected/a zombie");
        }
    }
}
