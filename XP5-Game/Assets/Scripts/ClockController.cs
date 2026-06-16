using UnityEngine;
using TMPro;

public class ClockController : MonoBehaviour
{
    [Header("Clock UI")]
    [SerializeField] private TMP_Text clockText;
    private float elaspedTime;

    [Header("Time In a Day")]
    [SerializeField] private float timeInADay = 86400f;

    [Header("Speed Time")]
    [SerializeField] private float timeScale = 2.0f;

    private void Start()
    {
        //elaspedTime = 12 * 3600f;
    }

    private void Update()
    {
        elaspedTime += Time.deltaTime * timeScale;
        elaspedTime %= timeInADay;
        UpdateClockUI();
    }

    void UpdateClockUI()
    {
        int hours = Mathf.FloorToInt(elaspedTime / 3600f);
        int minutes = Mathf.FloorToInt((elaspedTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt((elaspedTime % 60f));

        string ampm = hours < 12 ? "AM" : "PM";
        hours = hours % 12;
        if (hours == 0)
        {
            hours = 12;
        }

        string clockString = string.Format("{0:00}:{1:00} {2}", hours, minutes, ampm);
        clockText.text = clockString;
    }

    //Define a visibilidade do relógio
    public void SetVisible(bool visible)
    {
        clockText.gameObject.SetActive(visible);
    }
}

