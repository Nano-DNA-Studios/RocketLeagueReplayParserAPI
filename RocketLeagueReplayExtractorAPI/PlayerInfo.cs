using DNARocketLeagueReplayParser.ReplayStructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Describes the Information of a Player in the Replay
    /// </summary>
    public class PlayerInfo
    {
        private const string SCORE = "Score";
        private const string SAVES = "Saves";
        private const string GOALS = "Goals";
        private const string ASSISTS = "Assists";
        private const string NAME = "Name";
        private const string SHOTS = "Shots";
        private const string TEAM = "Team";


        /// <summary>
        /// The ID assigned to the Player at the Start of the Match
        /// </summary>
        public int PlayerID { get; private set; }

        /// <summary>
        /// The Name of the Player
        /// </summary>
        public string PlayerName { get; private set; }

        /// <summary>
        /// The Number of Points the Player got
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// The Number of Goals the Player Scored 
        /// </summary>
        public int Goals { get; private set; }

        /// <summary>
        /// The Number of Assists the Player got
        /// </summary>
        public int Assists { get; private set; }

        /// <summary>
        /// The Number of Saves the Player got
        /// </summary>
        public int Saves { get; private set; }

        /// <summary>
        /// The Number of Shots the Player took
        /// </summary>
        public int Shots { get; private set; }

        //0 = Blue, 1 = Orange
        public int Team { get; private set; }


        //Eventually support
        //Clears
        //Demos
        //Epic vs Normal Saves
        //Touches
        //Boost Usage
        //ect

        /// <summary>
        /// Initializes a new Player Info Object
        /// </summary>
        /// <param name="playerID"> The ID Assigned to the Player </param>
        /// <param name="playerName"> The Name of the Player </param>
        public PlayerInfo(int playerID, string playerName)
        {
            PlayerID = playerID;
            PlayerName = playerName;
        }

        public PlayerInfo(PropertyDictionary playerStats)
        {

            PlayerName = (string)playerStats[NAME].Value;
            Score = (int)playerStats[SCORE].Value;
            Goals = (int)playerStats[GOALS].Value;
            Assists = (int)playerStats[ASSISTS].Value;
            Saves = (int)playerStats[SAVES].Value;
            Shots = (int)playerStats[SHOTS].Value;
            Team = (int)playerStats[TEAM].Value;

        }




        /// <summary>
        /// Sets the Number of Poinrts the Player got
        /// </summary>
        /// <param name="score"> The Number of Points/Score the Player got </param>
        public void SetScore(int score)
        {
            Score = score;
        }

        /// <summary>
        /// Sets the Number of Goals the Player Scored
        /// </summary>
        /// <param name="goals"> The Number of Goals the Player Scored </param>
        public void SetGoals(int goals)
        {
            Goals = goals;
        }

        /// <summary>
        /// Sets the Number of Assists the Player got
        /// </summary>
        /// <param name="assists"> The Number of Assists </param>
        public void SetAssists(int assists)
        {
            Assists = assists;
        }

        /// <summary>
        /// Sets the Number of Saves the Player got
        /// </summary>
        /// <param name="saves"> The Number of Saves</param>
        public void SetSaves(int saves)
        {
            Saves = saves;
        }

        /// <summary>
        /// Sets the Number of Shots the Player took
        /// </summary>
        /// <param name="shots"> The Number of Shots </param>
        public void SetShots(int shots)
        {
            Shots = shots;
        }

        /// <summary>
        /// Gets the Information that would displayed on the Scoreboard in the same Format as the Scoreboard
        /// </summary>
        /// <returns> Formatted info that would be Displayed on the Scoreboard</returns>
        public string[] GetScoreboardInfo ()
        {
            return [PlayerName, Score.ToString(), Goals.ToString(), Assists.ToString(), Saves.ToString(), Shots.ToString()];
        }
    }
}
