using Hawki.EventObserver;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupEndGame : MonoBehaviour
{
    [SerializeField] private Text txtTitle;
    [SerializeField] private Text txtScore;
    [SerializeField] private Button btnRestart;
    // Start is called before the first frame update
    void Start()
    {
        btnRestart.onClick.AddListener(onClickRestartBtn);
    }

    public void ShowPopup(PopupEndGameData data)
    {
        txtTitle.text = data.title;
        txtScore.text = "Score: " + data.score.ToString();
        if(!data.isWin) btnRestart.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }    
    public void HidePopup()
    {
        gameObject.SetActive(false);
    }    

    private void onClickRestartBtn()
    {
        gameObject.SetActive(false);
        EventObs.Instance.ExcuteEvent(EventName.REPLAY, new ReplayEvent{});
    }
}
public class PopupEndGameData
{
    public string title;
    public int score;
    public bool isWin;

    public PopupEndGameData(string title, int score, bool isWin)
    {
        this.title = title;
        this.score = score;
        this.isWin = isWin;
    }
}

