using DNARocketLeagueReplayParser.ReplayStructure;
using DNARocketLeagueReplayParser.ReplayStructure.Mapping;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Represents a Team in a Rocket League Match
    /// </summary>
    public class Team : RLAnalysisObject<Team>
    {
        /// <summary>
        /// The ID of the Team being Represented
        /// </summary>
        public int TeamID => Properties.TryGetProperty(GameProperties.Team, 0);

        /// <summary>
        /// The Name of the Team based on the Team ID (0 = Blue, 1 = Orange)
        /// </summary>
        public string TeamName => TeamID == 0 ? GameProperties.BlueTeam : GameProperties.OrangeTeam;

        /// <summary>
        /// Array containing the Players Info on the Team
        /// </summary>
        public PlayerInfo[] Players { get; private set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="replay"></param>
        /// <param name="teamID"></param>
        public Team(PsyonixReplay replay, int teamID)
        {
            Properties = new RocketLeaguePropertyDictionary();

            Properties.Add(GameProperties.Team, new RocketLeagueProperty(GameProperties.Team, "int", teamID));
            Players = ExtractPlayers(replay);
        }


        /// <summary>
        /// Gets the Teams Stat Value 
        /// </summary>
        /// <param name="blueTeam"> Flag determining if the Total stats should be from the Blue Team or not </param>
        /// <param name="stat"> The Game Stat of Interest </param>
        /// <returns> The Teams Stat Value </returns>
        public float GetTeamStat(string stat)
        {
            float statValue = 0;

            foreach (PlayerInfo player in Players)
            {
                try
                {
                    statValue += player.Properties.TryGetProperty<float>(stat);
                }
                catch (Exception e)
                {
                    statValue += (float)player.Properties.TryGetProperty<int>(stat);
                }
            }

            return statValue;
        }

        /// <summary>
        /// Extracts the Players from the Replay Info Object and Formats them into PlayerInfo Objects
        /// </summary>
        private PlayerInfo[] ExtractPlayers(PsyonixReplay replayInfo)
        {
            ArrayProperty playerStatsArray = (ArrayProperty)replayInfo.Properties[GameProperties.PlayerStats];
            List<PropertyDictionary> playerStats = (List<PropertyDictionary>)playerStatsArray.Value;
            List<PlayerInfo> players = new List<PlayerInfo>();

            foreach (PropertyDictionary playerStat in playerStats)
            {
                PlayerInfo player = new PlayerInfo(playerStat);

                if (player.Team == TeamID)
                    players.Add(new PlayerInfo(playerStat));
            }

            return players.ToArray();
        }

        /// <summary>
        /// Gets the Teams Scoreboard
        /// </summary>
        /// <returns> The Striong Array that will be Displayed on the Score Board for the Entire Teams Info </returns>
        public string[] GetTeamScoreboard()
        {
            List<string> scoreboard = [TeamName];

            foreach (string stat in PlayerInfo.DisplayStats)
                scoreboard.Add(GetTeamStat(stat).ToString());

            return scoreboard.ToArray();
        }
    }
}
