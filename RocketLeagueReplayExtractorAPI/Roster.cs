using DNARocketLeagueReplayParser.ReplayStructure;

namespace RocketLeagueReplayParserAPI
{
    public class Roster
    {
        public Team BlueTeam => Teams[GameProperties.BlueTeamID];

        public Team OrangeTeam => Teams[GameProperties.OrangeTeamID];

        /// <summary>
        /// The Teams in the Match
        /// </summary>
        public Team[] Teams { get; private set; }

        /// <summary>
        /// Array of all the Players in the Match
        /// </summary>
        public PlayerInfo[] AllPlayers { get; private set; }

        public Roster(PsyonixReplay replay)
        {
            Teams = new Team[2];

            Teams[GameProperties.BlueTeamID] = new Team(replay, GameProperties.BlueTeamID);
            Teams[GameProperties.OrangeTeamID] = new Team(replay, GameProperties.OrangeTeamID);
        }

        public IEnumerable<PlayerInfo> GetAllPlayers ()
        {
            List<PlayerInfo> players = new List<PlayerInfo>();

            foreach (Team team in Teams)
                players.AddRange(team.Players);

            return players.ToArray();
        }
    }
}
