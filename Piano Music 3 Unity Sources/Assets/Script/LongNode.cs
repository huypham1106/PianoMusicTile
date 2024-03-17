using Hawki.EventObserver;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongNode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject maskNode;
    [SerializeField] private GameObject errorMaskNode;
    [SerializeField] private GameObject line;
    [SerializeField] private Image imgLongNode;
    [SerializeField] private float speedScroll;
    [SerializeField] private float speedFall;

    private Coroutine crScrollMask;

    private bool isDragging;
    private bool isRealNode;
    private int id;

    private void OnEnable()
    {
        StartCoroutine(MoveDownCoroutine());
    }
    private void OnDisable()
    {
        resetNode();
    }
    private void resetNode()
    {
        gameObject.transform.localPosition = Vector3.zero;
        isDragging = false;
        maskNode.transform.localScale = new Vector3(1f, 0f, 1f);
        errorMaskNode.SetActive(false);
        StopAllCoroutines();
    }
    public void InitData(int id,bool isRealNode, float speedFall, float speedScroll)
    {
        resetNode();
        this.speedFall = speedFall;
        this.speedScroll = speedScroll;
        this.id = id;
        gameObject.SetActive(true);
        this.isRealNode = isRealNode;
        if (isRealNode)
        {
            Utilities.SetAlpha(255f, imgLongNode);
            line.SetActive(true);
        }
        else
        {
            Utilities.SetAlpha(0f, imgLongNode);
            line.SetActive(false);
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
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isRealNode)
        {
            if (GamePlayController.Instance.TryClickNode(id))
            {
                Debug.LogError("co vo day");
                isDragging = true;

                if (crScrollMask != null) StopCoroutine(crScrollMask);
                crScrollMask = StartCoroutine(ScrollMask());
            }
        }
        else
        {
            errorMaskNode.SetActive(true);
            EventObs.Instance.ExcuteEvent(EventName.LOSE_GAME, new LoseGameEvent { });
        }    
        
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        if(crScrollMask != null)
        StopCoroutine(crScrollMask);
    }
    private IEnumerator ScrollMask()
    {
        while (isDragging)
        {
            if (maskNode.transform.localScale.y >= 0.99)
            {
                break;
            }
            maskNode.transform.localScale +=   new Vector3(0f, speedScroll, 0f);
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
    public void PauseNode()
    {
        StopAllCoroutines();
    }
}
