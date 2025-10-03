using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ScoreManager
{
    [System.Serializable]
    public class ScoreEntry
    {
        public string playerName;
        public int score;
        public float survivalTime;
        public int level;
        public string date;

        public ScoreEntry(string name, int scoreValue, float time, int playerLevel)
        {
            playerName = name;
            score = scoreValue;
            survivalTime = time;
            level = playerLevel;
            date = System.DateTime.Now.ToString("dd/MM/yyyy");
        }
    }

    private const string SCORES_KEY = "HighScores";
    private const int MAX_SCORES = 10;

    public static void SaveScore(string playerName, int score, float survivalTime, int level)
    {
        List<ScoreEntry> scores = GetHighScores();
        
        // Add new score
        ScoreEntry newScore = new ScoreEntry(playerName, score, survivalTime, level);
        scores.Add(newScore);

        // Sort by score (descending)
        scores = scores.OrderByDescending(s => s.score).ToList();

        // Keep only top scores
        if (scores.Count > MAX_SCORES)
        {
            scores = scores.Take(MAX_SCORES).ToList();
        }

        // Save to PlayerPrefs
        SaveScoresToPlayerPrefs(scores);

        Debug.Log($"Score saved: {playerName} - {score} points");
    }

    public static List<ScoreEntry> GetHighScores()
    {
        List<ScoreEntry> scores = new List<ScoreEntry>();

        string scoresJson = PlayerPrefs.GetString(SCORES_KEY, "");
        if (!string.IsNullOrEmpty(scoresJson))
        {
            try
            {
                ScoreList scoreList = JsonUtility.FromJson<ScoreList>(scoresJson);
                if (scoreList != null && scoreList.scores != null)
                {
                    scores = scoreList.scores.ToList();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to load scores: {e.Message}");
            }
        }

        return scores.OrderByDescending(s => s.score).ToList();
    }

    public static bool IsHighScore(int score)
    {
        List<ScoreEntry> scores = GetHighScores();
        
        if (scores.Count < MAX_SCORES)
        {
            return true;
        }

        return score > scores.Last().score;
    }

    public static int GetRank(int score)
    {
        List<ScoreEntry> scores = GetHighScores();
        
        for (int i = 0; i < scores.Count; i++)
        {
            if (score >= scores[i].score)
            {
                return i + 1;
            }
        }

        return scores.Count + 1;
    }

    public static void ClearAllScores()
    {
        PlayerPrefs.DeleteKey(SCORES_KEY);
        Debug.Log("All scores cleared!");
    }

    private static void SaveScoresToPlayerPrefs(List<ScoreEntry> scores)
    {
        ScoreList scoreList = new ScoreList { scores = scores.ToArray() };
        string json = JsonUtility.ToJson(scoreList);
        PlayerPrefs.SetString(SCORES_KEY, json);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    private class ScoreList
    {
        public ScoreEntry[] scores;
    }

    // Utility methods
    public static string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }

    public static string GetScoreText(ScoreEntry score)
    {
        return $"{score.playerName} - {score.score} pts ({FormatTime(score.survivalTime)})";
    }
}