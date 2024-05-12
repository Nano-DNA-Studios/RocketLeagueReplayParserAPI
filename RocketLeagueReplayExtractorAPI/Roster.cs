using DNARocketLeagueReplayParser.ReplayStructure;
using DNARocketLeagueReplayParser.ReplayStructure.Actors;
using DNARocketLeagueReplayParser.ReplayStructure.Frames;

namespace RocketLeagueReplayParserAPI
{
    public class Roster : RLAnalysisObject<Roster>
    {
        /// <summary>
        /// The Player Name Property
        /// </summary>
        private const string PLAYER_NAME = "Engine.PlayerReplicationInfo:PlayerName";

        /// <summary>
        /// The Player Replication Info Property
        /// </summary>
        private const string PLAYER_REPLICATION_INFO = "Engine.Pawn:PlayerReplicationInfo";

        /// <summary>
        /// Gets the Blue Team from the Game Roster
        /// </summary>
        public Team BlueTeam => Properties.TryGetProperty<Team>(GameProperties.BlueTeam);

        /// <summary>
        /// Gets the Orcange Team from the Game Roster
        /// </summary>
        public Team OrangeTeam => Properties.TryGetProperty<Team>(GameProperties.OrangeTeam);

        /// <summary>
        /// Mapping of Actor ID to Player Name
        /// </summary>
        public Dictionary<uint, string> ActorIDToName => Properties.TryGetProperty<Dictionary<uint, string>>(GameProperties.ActorIDToNameMap);

        /// <summary>
        /// The Teams in the Match 
        /// </summary>
        public Team[] Teams => [BlueTeam, OrangeTeam];

        /// <summary>
        /// Array of all the Players in the Match
        /// </summary>
        public PlayerInfo[] AllPlayers { get; private set; }

        /// <summary>
        /// Default Constructor for the Roster
        /// </summary>
        /// <param name="replay"> The Replay File </param>
        public Roster(Replay replay)
        {
            Dictionary<uint, string> map = GetActorToPlayerMap(replay._replayInfo);

            Properties.Add(GameProperties.ActorIDToNameMap, new RocketLeagueProperty(GameProperties.ActorIDToNameMap, "ActorIDToNameMap", map));

            Properties.Add(GameProperties.BlueTeam, new RocketLeagueProperty(GameProperties.BlueTeam, "Team", new Team(replay._replayInfo, GameProperties.BlueTeamID)));
            Properties.Add(GameProperties.OrangeTeam, new RocketLeagueProperty(GameProperties.OrangeTeam, "Team", new Team(replay._replayInfo, GameProperties.OrangeTeamID)));
        }

        /// <summary>
        /// Returns all the Players in the Match
        /// </summary>
        /// <returns> An IEnumrable of all the Players in the Match </returns>
        public IEnumerable<PlayerInfo> GetAllPlayers()
        {
            List<PlayerInfo> players = new List<PlayerInfo>();

            foreach (Team team in Teams)
                players.AddRange(team.Players);

            return players.ToArray();
        }

        /// <summary>
        /// Extracts the Info necessary to map an Actor / GameObject to a Player Name
        /// </summary>
        /// <returns> A Dictionary Map of ActorID to Player Name </returns>
        private Dictionary<uint, string> GetActorToPlayerMap(PsyonixReplay replay)
        {
            Dictionary<uint, string> ActorToPlayerNameMap = new Dictionary<uint, string>();

            Dictionary<uint, string> IDtoName = new Dictionary<uint, string>();

            Dictionary<uint, int> ActorIDtoNameID = new Dictionary<uint, int>();

            foreach (PsyonixFrame frame in replay.Frames)
            {
                foreach (ActorState actorState in frame.ActorStates)
                {
                    foreach (ActorStateProperty property in actorState.Properties.Values)
                    {
                        if (property.PropertyName == PLAYER_REPLICATION_INFO)
                        {
                            if (!ActorIDtoNameID.ContainsKey(actorState.Id))
                                ActorIDtoNameID.Add(actorState.Id, ((ActiveActor)property.Data).ActorId);
                        }

                        if (property.PropertyName == PLAYER_NAME)
                        {
                            if (!IDtoName.ContainsKey(actorState.Id))
                                IDtoName.Add(actorState.Id, (string)property.Data);
                        }
                    }
                }
            }

            foreach (uint actorID in ActorIDtoNameID.Keys)
                ActorToPlayerNameMap.Add(actorID, IDtoName[(uint)ActorIDtoNameID[actorID]]);

            return ActorToPlayerNameMap;
        }
    }
}
