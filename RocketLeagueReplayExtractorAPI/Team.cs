using DNARocketLeagueReplayParser.ReplayStructure;
using DNARocketLeagueReplayParser.ReplayStructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Represents a Team in a Rocket League Match
    /// </summary>
    public class Team
    {

        /// <summary>
        /// The ID of the Team being Represented
        /// </summary>
        public int TeamID => TeamProperties.TryGetProperty(GameProperties.Team, 0);


        /// <summary>
        /// Array containing the Players Info on the Team
        /// </summary>
        public PlayerInfo[] Players { get; private set; }

        /// <summary>
        /// The Properties of the Team
        /// </summary>
        public RocketLeaguePropertyDictionary TeamProperties { get; private set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="replay"></param>
        /// <param name="teamID"></param>
        public Team(PsyonixReplay replay, int teamID)
        {
            TeamProperties = new RocketLeaguePropertyDictionary();
            

            TeamProperties.Add(GameProperties.Team, new RocketLeagueProperty(GameProperties.Team, "int", teamID));
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
                statValue += player.PlayerProperties.TryGetProperty<float>(stat);

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

        public string[] GetTeamScoreboard()
        {
            return [GetTeamStat(GameProperties.Score).ToString(), GetTeamStat(GameProperties.Goals).ToString(), GetTeamStat(GameProperties.Assists).ToString(), GetTeamStat(GameProperties.Saves).ToString(), GetTeamStat(GameProperties.Shots).ToString(), GetTeamStat(GameProperties.BallTouches).ToString(), GetTeamStat(GameProperties.BallTouchPercentage).ToString(), GetTeamStat(GameProperties.BallPossessionTime).ToString(), GetTeamStat(GameProperties.BallPossessionTimePercentage).ToString()];
        }
    }
}
