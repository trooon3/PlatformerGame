using UnityEngine;
using TMPro;

public class LeaderboardEntryUI : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI deathsText;
    public TextMeshProUGUI scoreText;

    public void SetData(int rank, string playerName, int rawData)
    {
        int deaths = rawData % 100;                     
        int timeInSeconds = (rawData / 100) % 10000;     
        int score = rawData / 1000000;                   

        if (rawData < 100000)
        {
            score = rawData;
            timeInSeconds = 0;
            deaths = 0;
        }

        rankText.text = "";
        nameText.text = string.IsNullOrEmpty(playerName) ? "Аноним" : playerName;

        timeText.text = $"{timeInSeconds / 60}:{(timeInSeconds % 60):00}";

        deathsText.text = deaths.ToString();
        scoreText.text = score.ToString();
    }
}