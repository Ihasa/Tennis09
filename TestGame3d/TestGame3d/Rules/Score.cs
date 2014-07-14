using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tennis01.Objects;
using Tennis01.Scenes;
namespace Tennis01.Rules
{
    enum Points
    {
        Love,
        Fifteen,
        Thirty,
        Fourty,
        Advantage,
    }
    public enum Scores
    {
        Points,
        Games,
        Sets
    }
    /// <summary>
    /// テニスのスコアを表す(通常時)
    /// </summary>
    class Score : ICloneable
    {
        public Points Point;
        MatchRule matchRule;
        public bool isTieBreak { get; private set; }
        public TiebreakScore TieScore { get; private set; }
        public int Sets{get;private set;}
        public int Games{get;private set;}
        public int TotalGames { get; private set; }
        public Score LastScore { get; private set; }
        public Score(MatchRule rule)
        {
            matchRule = rule;
            isTieBreak = rule.Games <= 0 && matchRule.TieBreak;
            TieScore = new TiebreakScore();
        }
        public Score(MatchRule rule,int setCount, int gameCount, Points pointCount)
        {
            matchRule = rule;
            Sets = setCount;
            Games = gameCount;
            Point = pointCount;
            TotalGames = Games;
        }
        public object Clone()
        {
            Score score = new Score(matchRule);
            score.Point = Point;
            score.Games = Games;
            score.Sets = Sets;
            score.TieScore = (TiebreakScore)TieScore.Clone();
            return score;
        }
        public void Increment(ref Score rivalScore)
        {
            GameMain.debugStr["isTieBreak"] = "" + isTieBreak;
            if (!isTieBreak)
            {
                //ポイントの加算と、それに伴うゲームの加算
                if (this.Point == Points.Fourty)
                {
                    if (rivalScore.Point == Points.Fourty)//相手も40
                    {
                        if (matchRule.Advantage)//デュースあり
                            this.Point = Points.Advantage;
                        else//ノーアド→ゲーム
                        {
                            this.Games++;
                            TotalGames++;
                            this.Point = rivalScore.Point = Points.Love;
                        }
                    }
                    else if (rivalScore.Point == Points.Advantage)//相手がAdvantageだった
                    {
                        rivalScore.Point = Points.Fourty;
                    }
                    else//その他の場合
                    {
                        this.Games++;
                        TotalGames++;
                        this.Point = rivalScore.Point = Points.Love;
                    }
                }
                else if (this.Point == Points.Advantage)//アドバンテージだったらゲーム
                {
                    this.Games++;
                    TotalGames++;
                    this.Point = rivalScore.Point = Points.Love;
                }
                else
                {
                    this.Point++;
                }
                LastScore = (Score)this.Clone();
                rivalScore.LastScore = (Score)rivalScore.Clone();
                //ゲームの加算に伴うセットの加算
                if (Games >= matchRule.Games)
                {
                    if (Games - rivalScore.Games >= 2)
                    {
                        set(rivalScore);
                    }
                    else if (matchRule.TieBreak && Games == rivalScore.Games)//タイブレ突入
                    {
                        isTieBreak = true;
                        rivalScore.isTieBreak = true;
                        TieScore = new TiebreakScore();
                        rivalScore.TieScore = new TiebreakScore();
                    }
                }
            }
            else
            {
                TieScore.Increase();
                LastScore = (Score)this.Clone();
                rivalScore.LastScore = (Score)rivalScore.Clone();
                if (TieScore.Points >= 7 && TieScore.Points - rivalScore.TieScore.Points >= 2)
                {
                    Games++;
                    LastScore = (Score)this.Clone();
                    rivalScore.LastScore = (Score)rivalScore.Clone();
                    set(rivalScore);
                }
            }
        }
        void set(Score rivalScore)
        {
            Sets++;
            Games = rivalScore.Games = 0;
            isTieBreak = rivalScore.isTieBreak = false;
        }
        //public void Increase()
        //{
        //    if (point == Points.Fourty)
        //    {
        //        if (rivalScore.point == Points.Fourty)
        //        {
        //            point = Points.Advantage;
        //        }
        //        else if (rivalScore.point == Points.Advantage)
        //        {
        //            rivalScore.point = Points.Fourty;
        //        }
        //        else
        //        {
        //            games++;
        //            point = Points.Love;
        //        }
        //    }
        //    else if (point == Points.Advantage)
        //    {
        //        games++;
        //    }
        //    else
        //    {
        //        point++;
        //    }
        //}
        //void game(bool get)
        //{
        //    point = Points.Love;
        //    rivalScore.point = Points.Love;
        //    if (get)
        //    {
        //        games++;
        //    }
            
        //}
        //public void DeuseAgain()
        //{

        //}
        //public static Score operator ++(Score score)
        //{
        //    Score res = score;
        //    res.Increase();
        //    return res;
        //}
        public string ToString(Scores score)
        {
            string res = "";
            switch (score)
            {
                case Scores.Points:
                    if (!isTieBreak)
                    {
                        switch (Point)
                        {
                            case Points.Love:
                                res = "0";
                                break;
                            case Points.Fifteen:
                                res = "15";
                                break;
                            case Points.Thirty:
                                res = "30";
                                break;
                            case Points.Fourty:
                                res = "40";
                                break;
                            case Points.Advantage:
                                res = "Adv.";
                                break;
                        }
                    }
                    else
                    {
                        res = "" + TieScore.Points;
                    }
                    break;
                case Scores.Games:
                    res = "" + (int)Games;
                    break;
                case Scores.Sets:
                    res = "" + (int)Sets;
                    break;
            }
            return res;
        }
    }
    /// <summary>
    /// タイブレーク時のスコアを表す
    /// </summary>
    class TiebreakScore : ICloneable
    {
        public int Points { get; private set; }
        public void Increase()
        {
            Points++;
        }
        public object Clone()
        {
            TiebreakScore res = new TiebreakScore();
            res.Points = Points;
            return res;
        }
    }
    //試合の得点を数える
    class ScoreManager
    {
        TennisEvents events;
        MatchRule rule;
        Score score0, score1;
        List<Tuple<Score, Score>> pastScores = new List<Tuple<Score, Score>>();
        public string[] PastScores
        {
            get
            {
                string[] res = new string[pastScores.Count];
                for (int i = 0; i < res.Length; i++)
                {
                    Score s0 = pastScores[i].Item1;
                    Score s1 = pastScores[i].Item2;
                    if (s0.Games < s1.Games)
                    {
                        Score buf = s0;
                        s0 = s1;
                        s1 = buf;
                    }
                    res[i] = s0.ToString(Scores.Games) + " - " + s1.ToString(Scores.Games);
                    if (s0.isTieBreak)
                    {
                        res[i] += string.Format("({0} - {1})", s0.TieScore.Points, s1.TieScore.Points); ;
                    }
                }
                return res;
            }
        }
        public bool GameSetMatch { get { return score0.Sets >= rule.Sets || score1.Sets >= rule.Sets; } }
        public Player[] Winners 
        { 
            get
            {
                if (GameSetMatch)
                {
                    int id = score0.Sets > score1.Sets ? 0 : 1;
                    List<Player> list = new List<Player>();
                    foreach (KeyValuePair<Player,int> pair in playerIDs)
                    {
                        if (pair.Value == id)
                            list.Add(pair.Key);
                    }
                    return list.ToArray();
                }
                return null;
            } 
        }
        public Player[] Losers
        {
            get
            {
                if (GameSetMatch)
                {
                    int id = score0.Sets < score1.Sets ? 0 : 1;
                    List<Player> list = new List<Player>();
                    foreach (KeyValuePair<Player, int> pair in playerIDs)
                    {
                        if (pair.Value == id)
                            list.Add(pair.Key);
                    }
                    return list.ToArray();
                }                 
                return null;
            }
        }
        //public int Games { get { return score0.Games + score1.Games; } }
        //public int TotalGames { get { return score0.TotalGames + score1.TotalGames; } }
        //public int Sets { get { return score0.Sets + score1.Sets; } }
        public Score GetScore(Player player)
        {
            int id = playerIDs[player];
            if (id == 0)
                return score0;
            else
                return score1;
        }
        Dictionary<Objects.Player, int> playerIDs;
        public event Action Point;
        public event Action Game;
        public event Action Set;
        public event Action GameSet;
        public ScoreManager(TennisEvents events,MatchRule rule,params Player[] players)
        {
            this.events = events;
            this.rule = rule;
            score0 = new Score(rule);
            score1 = new Score(rule);
            playerIDs = new Dictionary<Player, int>();
            for (int i = 0; i < players.Length; i++)
            {
                if (i < players.Length / 2)
                {
                    playerIDs.Add(players[i], 0);
                }
                else
                {
                    playerIDs.Add(players[i], 1);
                }
            }
        }

        /// <summary>
        /// 得点
        /// </summary>
        /// <param name="pointGetter"></param>
        public void GotPoint(Player scorer)
        {
            int id = playerIDs[scorer];
            
            if (id == 0)
            {
                score0.Increment(ref score1);// incrementScore(ref score0, ref score1);
            }
            else if (id == 1)
            {
                score1.Increment(ref score0);// incrementScore(ref score1, ref score0);
            }
            onEvents();
        }
        /// <summary>
        /// 失点
        /// </summary>
        /// <param name="loster"></param>
        public void LostPoint(Player loster)
        {
            int id = playerIDs[loster];

            if (id == 1)
            {
                score0.Increment(ref score1);
            }
            else if (id == 0)
            {
                score1.Increment(ref score0);
            }
            onEvents();
        }
        public bool IsTieBreak()
        {
            return score0.Games == rule.Games && score1.Games == rule.Games && rule.TieBreak && score0.TieScore.Points + score1.TieScore.Points != 0;
        }
        private void onEvents()
        {
            onPoint();
            if (!IsTieBreak() && score0.Point == Points.Love && score1.Point == Points.Love)
            {
                onGame();
                if (score0.Games == 0 && score1.Games == 0)
                {
                    pastScores.Add(new Tuple<Score,Score>((Score)score0.LastScore.Clone(), (Score)score1.LastScore.Clone()));
                    onSet();
                }
            }
            if (GameSetMatch)
                onGameSet();
        }
        void onPoint()
        {
            if (Point != null)
                Point();
        }
        void onGame()
        {
            if (Game != null)
                Game();
        }
        void onSet()
        {
            if (Set != null)
                Set();
        }
        void onGameSet()
        {
            if (GameSet != null)
                GameSet();
        }

        public string ToString(Scores score,Player server,Player receiver)
        {
            string pre = "";
            string s0 = score0.ToString(score);
            string s1 = score1.ToString(score);
            switch (score)
            {
                case Scores.Points:
                    s0 = GetScore(server).ToString(score);
                    s1 = GetScore(receiver).ToString(score);
                    if (rule.Advantage && s0 == "40" && s1 == "40")
                    {
                        return "Deuse";
                    }
                    break;
                case Scores.Games:
                    if (s0 == "" + rule.Games && s1 == "" + rule.Games)
                    {
                        return "Tie Break";
                    }
                    pre = "Game: ";
                    break;
                case Scores.Sets:
                    pre = "Set: ";
                    break;
            }
            
            return pre + s0 + " - " + s1;
        }
    }
}
