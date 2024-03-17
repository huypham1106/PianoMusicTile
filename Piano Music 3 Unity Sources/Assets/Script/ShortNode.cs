using Hawki.EventObserver;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShortNode : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Button btnShortNode;
    [SerializeField] private GameObject maskNode;
    [SerializeField] private GameObject errorMaskNode;
    [SerializeField] private Image imgShortNode;
    [SerializeField] private float speedFall;
    [SerializeField] private ParticleSystem vfxLava;

    private bool isRealNode;
    private int id;

    // Start is called before the first frame update
    void Start()
    {
        //btnShortNode.onClick.AddListener(onClickShortNode);
    }

    private void OnEnable()
    {
        StartCoroutine(MoveDownCoroutine());
    }
    private void OnDisable()
    {
        resetNode();
    }

    public void InitData(int id, bool isRealNode, float speedFall)
    {
        resetNode();
        this.speedFall = speedFall;
        this.id = id;
        gameObject.SetActive(true);
        this.isRealNode = isRealNode;
        if(isRealNode)
        {
            Utilities.SetAlpha(255f, imgShortNode);
        }   
        else
        {
            Utilities.SetAlpha(0f, imgShortNode);
        }    
    }
    private IEnumerator MoveDownCoroutine()
    {
        while (true)
        {
            Vector3 movement = new Vector3(0f, speedFall, 0f);
            transform.position -= movement * Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("EndTaskBar"))
        {          
            if (!GamePlayController.Instance.IsClicked(id) && isRealNode)
            {
                EventObs.Instance.ExcuteEvent(EventName.LOSE_GAME, new LoseGameEvent { });
            }
            gameObject.SetActive(false);
        }
    }
    private void resetNode()
    {
        gameObject.transform.localPosition = Vector3.zero;
        maskNode.SetActive(false);
        errorMaskNode.SetActive(false);       
        StopAllCoroutines();
    }    
     
    private void onClickShortNode()
    {


    }    
    public void PauseNode()
    {
        StopAllCoroutines();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isRealNode)
        {
            Debug.LogError("Click node " + id.ToString());
            if (GamePlayController.Instance.TryClickNode(id))
            {
                Debug.Log("mask node " + id.ToString());
                maskNode.SetActive(true);
                vfxLava.Play();
            }
        }
        else
        {
            errorMaskNode.SetActive(true);
            EventObs.Instance.ExcuteEvent(EventName.LOSE_GAME, new LoseGameEvent { });
        }
    }
}
