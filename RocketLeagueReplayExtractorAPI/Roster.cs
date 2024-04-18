using DNARocketLeagueReplayParser.ReplayStructure;
using DNARocketLeagueReplayParser.ReplayStructure.Actors;
using DNARocketLeagueReplayParser.ReplayStructure.Frames;
using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;

namespace RocketLeagueReplayParserAPI
{
    public class Roster
    {
        /// <summary>
        /// The Player Name Property
        /// </summary>
        private const string PLAYER_NAME = "Engine.PlayerReplicationInfo:PlayerName";

        /// <summary>
        /// The Player Replication Info Property
        /// </summary>
        private const string PLAYER_REPLICATION_INFO = "Engine.Pawn:PlayerReplicationInfo";


        public Team BlueTeam => RosterProperties.TryGetProperty<Team>(GameProperties.BlueTeam);

        public Team OrangeTeam => RosterProperties.TryGetProperty<Team>(GameProperties.OrangeTeam);

        /// <summary>
        /// Mapping of Actor ID to Player Name
        /// </summary>
        public Dictionary<uint, string> ActorIDToName => RosterProperties.TryGetProperty<Dictionary<uint, string>>(GameProperties.ActorIDToNameMap);

        /// <summary>
        /// The Teams in the Match 
        /// </summary>
        public Team[] Teams => [BlueTeam, OrangeTeam];

        /// <summary>
        /// Array of all the Players in the Match
        /// </summary>
        public PlayerInfo[] AllPlayers { get; private set; }


        public RocketLeaguePropertyDictionary RosterProperties { get; private set; }

        public Roster(Replay replay)
        {
            RosterProperties = new RocketLeaguePropertyDictionary();

            Dictionary<uint, string> map = GetActorToPlayerMap(replay._replayInfo);

            RosterProperties.Add(GameProperties.ActorIDToNameMap, new RocketLeagueProperty(GameProperties.ActorIDToNameMap, "ActorIDToNameMap", map));

            RosterProperties.Add(GameProperties.BlueTeam, new RocketLeagueProperty(GameProperties.BlueTeam, "Team", new Team(replay._replayInfo, GameProperties.BlueTeamID)));
            RosterProperties.Add(GameProperties.OrangeTeam, new RocketLeagueProperty(GameProperties.OrangeTeam, "Team", new Team(replay._replayInfo, GameProperties.OrangeTeamID)));
        }

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

        /// <summary>
        /// Calculates the Ball Touches that Occur during the Replay and Assigns them to the Players 
        /// </summary>
        public void CalculateBallTouches(Replay replay)
        {
            TouchCalculator touchCalculator = new TouchCalculator(replay);
            List<BallTouch> ballTouches = touchCalculator.GetBallTouches();
            Dictionary<string, List<BallTouch>> playerTouchDictionary = new Dictionary<string, List<BallTouch>>();

            foreach (BallTouch ballTouch in ballTouches)
            {
                PlayerInfo? player = GetAllPlayers().FirstOrDefault(player => player.PlayerName == ActorIDToName[ballTouch.ActorID]);

                if (player == null)
                    continue;

                if (!playerTouchDictionary.ContainsKey(player.PlayerName))
                    playerTouchDictionary.Add(player.PlayerName, new List<BallTouch>());

                playerTouchDictionary[player.PlayerName].Add(ballTouch);
            }

            int blueTeamTouches = 0;
            int orangeTeamTouches = 0;
            float blueTeamPossessionTime = 0;
            float orangeTeamPossessionTime = 0;

            foreach (string playerName in playerTouchDictionary.Keys)
            {
                PlayerInfo? player = GetAllPlayers().FirstOrDefault(player => player.PlayerName == playerName);

                if (player == null)
                    continue;

                if (player.Team == GameProperties.BlueTeamID)
                {
                    blueTeamTouches += playerTouchDictionary[playerName].Count;
                    blueTeamPossessionTime += playerTouchDictionary[playerName].Sum(touch => touch.TimeUntilNextTouch);
                }
                else
                {
                    orangeTeamTouches += playerTouchDictionary[playerName].Count;
                    orangeTeamPossessionTime += playerTouchDictionary[playerName].Sum(touch => touch.TimeUntilNextTouch);
                }

                //player.PlayerProperties.Add(GameProperties.BallTouchCount, new RocketLeagueProperty(GameProperties.BallTouchCount, "List<BallTouch>", playerTouchDictionary[playerName]));

                player.SetBallTouches(playerTouchDictionary[playerName]);
            }

            int touchTotal = blueTeamTouches + orangeTeamTouches;
            float possessionTimeTotal = blueTeamPossessionTime + orangeTeamPossessionTime;

            foreach (string playerName in playerTouchDictionary.Keys)
            {
                PlayerInfo? player = GetAllPlayers().FirstOrDefault(player => player.PlayerName == playerName);

                if (player == null)
                    continue;

                player.PlayerProperties.Add(GameProperties.BallPossessionTime, new RocketLeagueProperty(GameProperties.BallPossessionTime, "float", playerTouchDictionary[playerName].Sum(touch => touch.TimeUntilNextTouch)));
                //player.BallPossessionTime = playerTouchDictionary[playerName].Sum(touch => touch.TimeUntilNextTouch);

                if (player.Team == GameProperties.BlueTeamID)
                {
                    player.PlayerProperties.Add(GameProperties.BallTouchPercentage, new RocketLeagueProperty(GameProperties.BallTouchPercentage, "float", 100 * (float)playerTouchDictionary[playerName].Count / blueTeamTouches));
                    player.PlayerProperties.Add(GameProperties.BallPossessionTimePercentage, new RocketLeagueProperty(GameProperties.BallPossessionTimePercentage, "float", 100 * player.BallPossessionTime / blueTeamPossessionTime));

                   /* player.BallTouchPossessionPercentage = 100 * (float)playerTouchDictionary[playerName].Count / blueTeamTouches;
                    player.BallPossessionPercentage = 100 * player.BallPossessionTime / blueTeamPossessionTime;*/
                }
                else
                {
                    player.PlayerProperties.Add(GameProperties.BallTouchPercentage, new RocketLeagueProperty(GameProperties.BallTouchPercentage, "float", 100 * (float)playerTouchDictionary[playerName].Count / orangeTeamTouches));
                    player.PlayerProperties.Add(GameProperties.BallPossessionTimePercentage, new RocketLeagueProperty(GameProperties.BallPossessionTimePercentage, "float", 100 * player.BallPossessionTime / orangeTeamPossessionTime));

                    /*player.BallTouchPossessionPercentage = 100 * (float)playerTouchDictionary[playerName].Count / orangeTeamTouches;
                    player.BallPossessionPercentage = 100 * player.BallPossessionTime / orangeTeamPossessionTime;*/
                }
            }
        }
    }
}
