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
        [TestCase("Resources\\GoldenGoose.replay", 6)]
        [TestCase("Resources\\Replay1.replay", 3)]
        [TestCase("Resources\\Replay2.replay", 6)]
        [TestCase("Resources\\Replay3.replay", 6)]
        [TestCase("Resources\\Replay4.replay", 4)]
        [TestCase("Resources\\Replay5.replay", 6)]
        public void PlayerNumber(string replayFilePath, int expectedPlayerNum)
        {
            Replay replay = LoadReplay(replayFilePath);

            Assert.That(replay.MatchRoster.GetAllPlayers().Count(), Is.EqualTo(expectedPlayerNum));
        }

        /// <summary>
        /// Tests that all the Stats are not empty and have all been extracted
        /// </summary>
        /// <param name="replayFilePath"> The Replay File Path </param>
        [Test]
        [TestCase("Resources\\GoldenGoose.replay")]
        [TestCase("Resources\\Replay1.replay")]
        [TestCase("Resources\\Replay2.replay")]
        [TestCase("Resources\\Replay3.replay")]
        [TestCase("Resources\\Replay4.replay")]
        [TestCase("Resources\\Replay5.replay")]
        public void StatsArentEmpty(string replayFilePath)
        {
            Replay replay = LoadReplay(replayFilePath);

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
        [TestCase("Resources\\GoldenGoose.replay", "alffaz", new int[] { 757, 2, 0, 3, 5 })]
        [TestCase("Resources\\GoldenGoose.replay", "Stats16", new int[] { 218, 0, 0, 1, 3 })]
        [TestCase("Resources\\GoldenGoose.replay", "Mar2D2", new int[] { 260, 0, 0, 1, 2 })]
        [TestCase("Resources\\GoldenGoose.replay", "MyTyranosaur", new int[] { 880, 3, 0, 4, 4 })]
        [TestCase("Resources\\GoldenGoose.replay", "Evan", new int[] { 262, 0, 1, 1, 3 })]
        [TestCase("Resources\\GoldenGoose.replay", "Vanilla Rain", new int[] { 585, 0, 2, 2, 4 })]
        public void CorrectStats(string replayFilePath, string playerName, int[] stats)
        {
            Replay replay = LoadReplay(replayFilePath);

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
        [TestCase("Resources\\GoldenGoose.replay", new string[] { "alffaz", "757", "2", "0", "3", "5" })]
        [TestCase("Resources\\GoldenGoose.replay", new string[] { "Stats16", "218", "0", "0", "1", "3" })]
        [TestCase("Resources\\GoldenGoose.replay", new string[] { "Mar2D2", "260", "0", "0", "1", "2" })]
        [TestCase("Resources\\GoldenGoose.replay", new string[] { "MyTyranosaur", "880", "3", "0", "4", "4" })]
        [TestCase("Resources\\GoldenGoose.replay", new string[] { "Evan", "262", "0", "1", "1", "3" })]
        [TestCase("Resources\\GoldenGoose.replay", new string[] { "Vanilla Rain", "585", "0", "2", "2", "4" })]
        public void ScoreBoardInfo(string replayFilePath, string[] scoreBoardInfo)
        {
            Replay replay = LoadReplay(replayFilePath);

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
        [TestCase("Resources\\GoldenGoose.replay", "alffaz", GameProperties.Score, 757)]
        [TestCase("Resources\\GoldenGoose.replay", "alffaz", GameProperties.Goals, 2)]
        [TestCase("Resources\\GoldenGoose.replay", "alffaz", GameProperties.Assists, 0)]
        [TestCase("Resources\\GoldenGoose.replay", "alffaz", GameProperties.Saves, 3)]
        [TestCase("Resources\\GoldenGoose.replay", "alffaz", GameProperties.Shots, 5)]
        [TestCase("Resources\\GoldenGoose.replay", "alffaz", GameProperties.None, 0)]
        [TestCase("Resources\\GoldenGoose.replay", "MyTyranosaur", GameProperties.Score, 880)]
        [TestCase("Resources\\GoldenGoose.replay", "MyTyranosaur", GameProperties.Goals, 3)]
        [TestCase("Resources\\GoldenGoose.replay", "MyTyranosaur", GameProperties.Assists, 0)]
        [TestCase("Resources\\GoldenGoose.replay", "MyTyranosaur", GameProperties.Saves, 4)]
        [TestCase("Resources\\GoldenGoose.replay", "MyTyranosaur", GameProperties.Shots, 4)]
        [TestCase("Resources\\GoldenGoose.replay", "MyTyranosaur", GameProperties.None, 0)]
        public void GetStat(string replayFilePath, string playerName, string gameStat, int expectedValue)
        {
            Replay replay = LoadReplay(replayFilePath);

            foreach (PlayerInfo player in replay.MatchRoster.GetAllPlayers())
            {
                if (player.PlayerName != playerName)
                    continue;

                if (!player.StatExists(gameStat))
                    Assert.Throws<KeyNotFoundException>(() => player.GetStat<int>(gameStat));

                Assert.That(player.GetStat<int>(gameStat), Is.EqualTo(expectedValue));
            }
        }
    }
}
