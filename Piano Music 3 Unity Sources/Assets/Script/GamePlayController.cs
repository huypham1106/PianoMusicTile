using Hawki.EventObserver;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hawki;
using System.Linq;
using DG.Tweening;

public class NodeModel
{
    public int id;
    public int touchIndex;
    public bool isClicked;
    public float time;

    public NodeModel()
    {
    }    
}    


public class GamePlayController : MonoSingleton<GamePlayController>, IRegister
{
    [SerializeField] private Button btnPlay;
    [SerializeField] private Text txtScore;
    [SerializeField] private List<SpawnNode> spawnNodes;
    [SerializeField] private PopupEndGame popupEndGame;
    [SerializeField] private StarBar starBar;

    private MapSO mapData = null;
    private int countNode;
    private float time = 0f;
    private int currentTouchIndex;
    private Coroutine crGenerateNode;
    private Coroutine crCoundownTimeMusic;

    private int lastNodeId;
    private float lastTimeClickNode;
    private Dictionary<int, NodeModel> dictNode = new Dictionary<int, NodeModel>();

    private void Awake()
    {
        btnPlay.onClick.AddListener(onClickPlayButton);
    }
    private void Start()
    {
        resetMap();       
    }

    private void InitData()
    {
        dictNode.Clear();
        for(int i = 0; i< mapData.nodeDataList.Count; i++)
        {
            NodeModel node = new NodeModel();
            node.id = i;
            node.touchIndex = mapData.nodeDataList[i].touchIndex;
            node.isClicked = false;
            node.time = mapData.nodeDataList[i].time;
            dictNode.Add(i, node);
        }    
    }    

    private void OnEnable()
    {
        EventObs.Instance.AddRegister(EventName.REPLAY, this);
        EventObs.Instance.AddRegister(EventName.ADD_POINT, this);
        EventObs.Instance.AddRegister(EventName.LOSE_GAME, this);
    }
    private void OnDisable()
    {
        EventObs.Instance.RemoveRegister(EventName.REPLAY, this);
        EventObs.Instance.RemoveRegister(EventName.ADD_POINT, this);
        EventObs.Instance.RemoveRegister(EventName.LOSE_GAME, this);
    }

    private void onClickPlayButton()
    {
        btnPlay.gameObject.SetActive(false);
        loadMap();     
        resetMap();
        InitData();
        StartCoroutine(playMusicBackground());
        if (crGenerateNode != null) StopCoroutine(crGenerateNode);
        crGenerateNode =  StartCoroutine(generateNode());
    }    
    private IEnumerator playMusicBackground()
    {
        SoundManager.I.StopMusic();
        yield return new WaitForSeconds(0.8f);      
        SoundManager.I.PlayMusic(Global.SoundName.abc, false);
        if(crCoundownTimeMusic != null)
        {
            StopCoroutine(crCoundownTimeMusic);
        }
        crCoundownTimeMusic = StartCoroutine(countdownTimeMusic());
    }    

    private void loadMap()
    {

        int currentLevel = 1;
        MapSO map = Utilities.LoadMapSO(currentLevel);
        if (map != null) mapData = map;
    }
    private IEnumerator countdownTimeMusic()
    {
        float countdownTime = mapData.totalTime;
        while (countdownTime > 0)
        {
            yield return new WaitForSeconds(0.1f);
            countdownTime -= 0.1f;
        }

        PopupEndGameData data = new PopupEndGameData("WIN", ScoreSystem.Instance.TotalScore(), false);
        popupEndGame.ShowPopup(data);

    }

    private IEnumerator generateNode()
    {
        while(countNode <= mapData.nodeDataList.Count-1)
        {
            time += Time.deltaTime;
            if(mapData.nodeDataList[countNode].time <= time)
            {
                for(int i=0; i< spawnNodes.Count; i++)
                {
                    if(mapData.nodeDataList[countNode].line.Contains( spawnNodes[i].ID))
                    {
                        spawnNodes[i].SpawnRealNode(mapData.nodeDataList[countNode], mapData.speedFall, mapData.speedScroll);
                    }   
                    else
                    {
                        spawnNodes[i].SpawnFakeNode(mapData.nodeDataList[countNode], mapData.speedFall, mapData.speedScroll);
                    }    
                }
                countNode++;
            }   
            yield return null;

        }    
    }

    public void OnEvent(string eventId, EventBase data)
    {
        switch (eventId)
        {
            case EventName.REPLAY:
                replayGame();
                break;
            case EventName.LOSE_GAME:
                StartCoroutine(loseGame());
                break;
        }
    }
    private IEnumerator loseGame()
    {
        StopCoroutine(crGenerateNode);
        pauseNodeFall();
        yield return new WaitForSeconds(1f);
        PopupEndGameData data = new PopupEndGameData("LOSE", ScoreSystem.Instance.TotalScore(), false);
        
        popupEndGame.ShowPopup(data);
        
    }    

    private void replayGame()
    {
        btnPlay.gameObject.SetActive(true);
        for(int i=0; i < spawnNodes.Count; i++)
        {
            spawnNodes[i].ResetNode();
        }
        resetMap();        
    }   
    private void resetMap()
    {
        countNode = 0;
        currentTouchIndex = 0;
        lastTimeClickNode = Time.time;
        ScoreSystem.Instance.ResetScore();
        txtScore.text = ScoreSystem.Instance.TotalScore().ToString();
        time = 0f;
        Time.timeScale = 1; 
        popupEndGame.HidePopup();
        starBar.ResetStarAndBar();
        StopAllCoroutines();
        SoundManager.I.StopMusic();

    }
    private void pauseNodeFall()
    {
        for(int i=0; i< spawnNodes.Count; i++)
        {
            spawnNodes[i].PauseNode();
        }    
    }    

    
    public bool TryClickNode(int index)
    {
        if (CheckAllowClickNode(index))
        {
            dictNode[index].isClicked = true;
            currentTouchIndex = dictNode[index].touchIndex;


            float offSetTime = Time.time - lastTimeClickNode;
            float offSetTimeBetweenNode = dictNode[index].time - index == 0 ? lastTimeClickNode : dictNode[lastNodeId].time;
            var pointData = ScoreSystem.Instance.CaculatePoint(index == 0 ? 0: Mathf.Abs(offSetTime - offSetTimeBetweenNode));
            txtScore.text = pointData.totalPoint.ToString();
            txtScore.transform.DOScale(1.4f, 0.3f).OnComplete(() => { txtScore.transform.DOScale(1f, 0f); });

            starBar.SetBarAndStar((float)pointData.totalPoint / (float)mapData.start3);

            lastNodeId = index;
            lastTimeClickNode = Time.time;

            //Debug.Log("=====  " + currentTouchIndex);
            return true;
        }
        else
        {

            //Debug.Log("=====  " + currentTouchIndex);
            return false;
        }
    }    


    public bool IsClicked(int id)
    {
        return dictNode[id].isClicked;
    }    

    public bool CheckAllowClickNode(int index)
    {
        NodeData nodeData = mapData.nodeDataList[index];
        int touchIndex = nodeData.touchIndex;

        //Debug.Log("=====  " + touchIndex +"index:  " + index);
        var node = dictNode.Values.ToList().Find(x => x.touchIndex == currentTouchIndex && x.isClicked == false);

        if(node != null)
        {
            return touchIndex == node.touchIndex;
        }    
        else
        {
            return currentTouchIndex == touchIndex - 1;
        }    
        
        
    }    
}
