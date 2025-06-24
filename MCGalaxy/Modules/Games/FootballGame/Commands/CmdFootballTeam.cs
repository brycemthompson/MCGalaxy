namespace MCGalaxy.Modules.Games.FootballGame.Commands {
    sealed class CmdFootballTeam : Command2 {
        public override string name { get { return "FootballTeam"; } }
        public override string shortcut { get { return "fbteam"; } }
        public override string type { get { return CommandTypes.Games; } }

        public override void Use(Player p, string message, CommandData data) {
            
            // Display team if no args provided (if user has one)
            if (string.IsNullOrEmpty(message)) {
                if (HasTeam(p)) DisplayTeam(p);
                else Help(p);
                return;
            }

            string[] args = message.Split(' ');
            if (args.Length < 2) {
                Help(p); return;
            }

            // Handle command actions
            string action = args[0].ToLower();
            switch (action) {
                case "join":
                    JoinTeam(p, args[1]);
                    break;
                case "leave":
                    LeaveTeam(p, args[1]);
                    break;
                default:
                    Help(p);
                    break;
            }
        }

        /// <summary>
        /// DisplayTeam - Displays the user's current football team if they have one.
        /// </summary>
        /// <param name="p"></param>
        public void DisplayTeam(Player p) {
            if (FootballGame.Instance.PandaTeam.HasMember(p)) {
                p.Message("&aYou are on the &bPanda Team&a.");
            }
            else {
                p.Message("&aYou are on the &eHomer Team&a.");
            }
        }

        /// <summary>
        /// HasTeam - Checks if the player is on a team in the football game.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool HasTeam(Player p) {
            return FootballGame.Instance.PandaTeam.HasMember(p) ||
                   FootballGame.Instance.HomerTeam.HasMember(p);
        }

        /// <summary>
        /// IsValidTeamName - Checks if the provided team name is valid.
        /// </summary>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public bool IsValidTeamName(string teamName) {
            if (teamName.CaselessEq(FootballGame.Instance.PandaTeam.Name) ||
                teamName.CaselessEq(FootballGame.Instance.HomerTeam.Name)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// JoinTeam - Adds the player to the specified team if they are not already on a team.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="teamName"></param>
        public void JoinTeam(Player p, string teamName) {
            string team = teamName.Trim();
            if (IsValidTeamName(teamName)) {
                if (HasTeam(p)) {
                    p.Message("&cYou are already on a team. Leave your current team first.");
                    return;
                }
                else {
                    if (team.CaselessEq(FootballGame.Instance.PandaTeam.Name)) {
                        FootballGame.Instance.PandaTeam.AddMember(p.truename);
                        p.Message("&aYou have joined the &bPanda Team&a.");
                    }
                    else if (team.CaselessEq(FootballGame.Instance.HomerTeam.Name)) {
                        FootballGame.Instance.HomerTeam.AddMember(p.truename);
                        p.Message("&aYou have joined the &eHomer Team&a.");
                    }
                }
            }
        }

        /// <summary>
        /// LeaveTeam - Removes the player from the specified team if they are currently on it.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="teamName"></param>
        public void LeaveTeam(Player p, string teamName) {
            string team = teamName.Trim();
            if (IsValidTeamName(teamName)) {
                if (!HasTeam(p)) {
                    p.Message("&cYou are not currently on a team.");
                    return;
                }
                else {
                    if (team.CaselessEq(FootballGame.Instance.PandaTeam.Name)) {
                        FootballGame.Instance.PandaTeam.RemoveMember(p.truename);
                        p.Message("&aYou have &cleft &athe &bPanda Team&a.");
                    }
                    else if (team.CaselessEq(FootballGame.Instance.HomerTeam.Name)) {
                        FootballGame.Instance.HomerTeam.RemoveMember(p.truename);
                        p.Message("&aYou have &cleft &athe &eHomer Team&a.");
                    }
                }
            }
        }

        public override void Help(Player p) {
            p.Message("&T/FootballTeam");
            p.Message("&HDisplays your current football team if you have one.");
            p.Message("&T/FootballTeam <join/leave> <teamName>");
            p.Message("&HJoin a football team so you can start playing and earning rewards!");
            p.Message("&TUsage: /FootballTeam <join/leave> <Pandas/Homers>");
        }
    }
}
