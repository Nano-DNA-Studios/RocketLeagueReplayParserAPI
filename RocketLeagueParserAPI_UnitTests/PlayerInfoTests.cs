using RocketLeagueReplayParserAPI;

namespace RocketLeagueParserAPI_UnitTests
{
    /// <summary>
    /// Tests the PlayerInfo Class and it's functions
    /// </summary>
    [TestFixture]
    public class PlayerInfoTests
    {
        /// <summary>
        /// Bank of Loaded Replays to use for Unit Testing
        /// </summary>
        private Replay[] _replayBank { get; set; }

        /// <summary>
        /// Enum Object with an int Index associated with each Replay
        /// </summary>
        public enum Replays
        {
            GoldenGoose = 0,
            Replay1 = 1,
            Replay2 = 2,
            Replay3 = 3,
            Replay4 = 4,
            Replay5 = 5,
        }

        /// <summary>
        /// Loads 
        /// </summary>
        [OneTimeSetUp]
        public void LoadReplays()
        {
            string[] paths = ["Resources\\GoldenGoose.replay", "Resources\\Replay1.replay", "Resources\\Replay2.replay", "Resources\\Replay3.replay", "Resources\\Replay4.replay", "Resources\\Replay5.replay"];

            _replayBank = new Replay[paths.Length];

            for (int i = 0; i < paths.Length; i++)
                _replayBank[i] = new Replay(paths[i]);
        }
        
        /// <summary>
        /// Gets the Replay from the Loaded Bank of Replays
        /// </summary>
        /// <param name="replay"> Index of the Replay </param>
        /// <returns> The Replay Instance that was Loaded </returns>
        private Replay GetReplay(Replays replay)
        {
            return _replayBank[(int)replay];
        }

        /// <summary>
        /// Loads a new Instance of the Replay Object based off the Path 
        /// </summary>
        /// <param name="path"> The Path to the Replay File </param>
        /// <returns> A loaded instance of the Replay </returns>
        private Replay LoadReplay(string path)
        {
            string fullPath = Path.GetFullPath(path);
            return new Replay(fullPath);
        }

        /// <summary>
        /// Tests if the right Number of playersd have been extracted from the Replay
        /// </summary>
        /// <param name="replayFilePath"> The replay file path </param>
        /// <param name="expectedPlayerNum"> The expected number of players </param>
        [Test]
        [TestCase(Replays.GoldenGoose, 6)]
        [TestCase(Replays.Replay1, 3)]
        [TestCase(Replays.Replay2, 6)]
        [TestCase(Replays.Replay3, 6)]
        [TestCase(Replays.Replay4, 4)]
        [TestCase(Replays.Replay5, 6)]
        public void PlayerNumber(Replays replayFile, int expectedPlayerNum)
        {
            Replay replay = GetReplay(replayFile);

            Assert.That(replay.MatchRoster.GetAllPlayers().Count(), Is.EqualTo(expectedPlayerNum));
        }

        /// <summary>
        /// Tests that all the Stats are not empty and have all been extracted
        /// </summary>
        /// <param name="replayFilePath"> The Replay File Path </param>
        [Test]
        [TestCase(Replays.GoldenGoose)]
        [TestCase(Replays.Replay1)]
        [TestCase(Replays.Replay2)]
        [TestCase(Replays.Replay3)]
        [TestCase(Replays.Replay4)]
        [TestCase(Replays.Replay5)]
        public void StatsArentEmpty(Replays replayFile)
        {
            Replay replay = GetReplay(replayFile);

            foreach (PlayerInfo player in replay.MatchRoster.GetAllPlayers())
            {
                Assert.That(player.PlayerName, Is.Not.Null);
                Assert.That(player.Team, Is.AtLeast(0));
                Assert.That(player.Score, Is.AtLeast(0));
                Assert.That(player.Goals, Is.AtLeast(0));
                Assert.That(player.Assists, Is.AtLeast(0));
                Assert.That(player.Saves, Is.AtLeast(0));
                Assert.That(player.Shots, Is.AtLeast(0));
            }
        }

        /// <summary>
        /// Tests that the Stats have been extracted correctly and all values match the expected values
        /// </summary>
        /// <param name="replayFilePath"> The Replay File Path </param>
        /// <param name="playerName"> The Name of the Player </param>
        /// <param name="stats"> The Stats of the Player in array format </param>
        [Test]
        [TestCase(Replays.GoldenGoose, "alffaz", new int[] { 757, 2, 0, 3, 5 })]
        [TestCase(Replays.GoldenGoose, "Stats16", new int[] { 218, 0, 0, 1, 3 })]
        [TestCase(Replays.GoldenGoose, "Mar2D2", new int[] { 260, 0, 0, 1, 2 })]
        [TestCase(Replays.GoldenGoose, "MyTyranosaur", new int[] { 880, 3, 0, 4, 4 })]
        [TestCase(Replays.GoldenGoose, "Evan", new int[] { 262, 0, 1, 1, 3 })]
        [TestCase(Replays.GoldenGoose, "Vanilla Rain", new int[] { 585, 0, 2, 2, 4 })]
        public void CorrectStats(Replays replayFile, string playerName, int[] stats)
        {
            Replay replay = GetReplay(replayFile);

            foreach (PlayerInfo player in replay.MatchRoster.GetAllPlayers())
            {
                if (player.PlayerName != playerName)
                    continue;

                Assert.That(player.Score, Is.EqualTo(stats[0]));
                Assert.That(player.Goals, Is.EqualTo(stats[1]));
                Assert.That(player.Assists, Is.EqualTo(stats[2]));
                Assert.That(player.Saves, Is.EqualTo(stats[3]));
                Assert.That(player.Shots, Is.EqualTo(stats[4]));
            }
        }

        /// <summary>
        /// Tests that the <see cref="PlayerInfo.GetScoreboardInfo"/> function returns the correct values
        /// </summary>
        /// <param name="replayFilePath"> The Replay File Path </param>
        /// <param name="scoreBoardInfo"> The Expected Output of the function </param>
        [Test]
        [TestCase(Replays.GoldenGoose, new string[] { "alffaz", "757", "2", "0", "3", "5" })]
        [TestCase(Replays.GoldenGoose, new string[] { "Stats16", "218", "0", "0", "1", "3" })]
        [TestCase(Replays.GoldenGoose, new string[] { "Mar2D2", "260", "0", "0", "1", "2" })]
        [TestCase(Replays.GoldenGoose, new string[] { "MyTyranosaur", "880", "3", "0", "4", "4" })]
        [TestCase(Replays.GoldenGoose, new string[] { "Evan", "262", "0", "1", "1", "3" })]
        [TestCase(Replays.GoldenGoose, new string[] { "Vanilla Rain", "585", "0", "2", "2", "4" })]
        public void ScoreBoardInfo(Replays replayFile, string[] scoreBoardInfo)
        {
            Replay replay = GetReplay(replayFile);

            foreach (PlayerInfo player in replay.MatchRoster.GetAllPlayers())
            {
                if (player.PlayerName != scoreBoardInfo[0])
                    continue;

                Assert.That(player.PlayerName.ToString(), Is.EqualTo(scoreBoardInfo[0]));
                Assert.That(player.Score.ToString(), Is.EqualTo(scoreBoardInfo[1]));
                Assert.That(player.Goals.ToString(), Is.EqualTo(scoreBoardInfo[2]));
                Assert.That(player.Assists.ToString(), Is.EqualTo(scoreBoardInfo[3]));
                Assert.That(player.Saves.ToString(), Is.EqualTo(scoreBoardInfo[4]));
                Assert.That(player.Shots.ToString(), Is.EqualTo(scoreBoardInfo[5]));
            }
        }

        /// <summary>
        /// Tests that the <see cref="PlayerInfo.GetStat"/> function returns the correct values
        /// </summary>
        /// <param name="replayFilePath"> The Replay File Path </param>
        /// <param name="playerName"> The Name of the Player </param>
        /// <param name="gameStat"> The Game Stat to extract </param>
        /// <param name="expectedValue"> The Expected Output of the Function </param>
        [Test]
        [TestCase(Replays.GoldenGoose, "alffaz", GameProperties.Score, 757)]
        [TestCase(Replays.GoldenGoose, "alffaz", GameProperties.Goals, 2)]
        [TestCase(Replays.GoldenGoose, "alffaz", GameProperties.Assists, 0)]
        [TestCase(Replays.GoldenGoose, "alffaz", GameProperties.Saves, 3)]
        [TestCase(Replays.GoldenGoose, "alffaz", GameProperties.Shots, 5)]
        [TestCase(Replays.GoldenGoose, "alffaz", GameProperties.None, 0)]
        [TestCase(Replays.GoldenGoose, "MyTyranosaur", GameProperties.Score, 880)]
        [TestCase(Replays.GoldenGoose, "MyTyranosaur", GameProperties.Goals, 3)]
        [TestCase(Replays.GoldenGoose, "MyTyranosaur", GameProperties.Assists, 0)]
        [TestCase(Replays.GoldenGoose, "MyTyranosaur", GameProperties.Saves, 4)]
        [TestCase(Replays.GoldenGoose, "MyTyranosaur", GameProperties.Shots, 4)]
        [TestCase(Replays.GoldenGoose, "MyTyranosaur", GameProperties.None, 0)]
        public void GetStat(Replays replayFile, string playerName, string gameStat, int expectedValue)
        {
            Replay replay = GetReplay(replayFile);

            foreach (PlayerInfo player in replay.MatchRoster.GetAllPlayers())
            {
                if (player.PlayerName != playerName)
                    continue;

                if (!player.StatExists(gameStat))
                    Assert.Throws<KeyNotFoundException>(() => player.GetStat<int>(gameStat));
                else
                    Assert.That(player.GetStat<int>(gameStat), Is.EqualTo(expectedValue));
            }
        }
    }
}
