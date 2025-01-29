using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler,IPointerClickHandler
{
    [HideInInspector]public Transform postDragParent;
    [SerializeField]
    private Image image;
    private Transform Content;
    private EffectDto effects;
    private PlayerHandler playerHandler;
    private SpecialCardsList SpecialCards;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (effects.Used)
        {
            return;
        }
        image.raycastTarget = false;
        
        //canvasGroup.interactable = false;
        if (postDragParent != null&&postDragParent.name.Contains("SPCSlot"))
        {
            postDragParent = Content;
        }
        else
        {
            postDragParent = transform.parent;
        }
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (effects.Used)
        {
            return;
        }
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //if (effects.Used)
        //{
        //    return;
        //}
        transform.SetParent(postDragParent);
        image.raycastTarget = true;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(Content.name);
        if (Content.name.Equals("Shop"))
        {
            string s = SpecialCards.GetName(gameObject);
            playerHandler.AddSpecialCard(s);
            GlobalData.money -= effects.Price;
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        effects = GetComponent<EffectDto>();
        Content = transform.parent;
        playerHandler = GameObject.Find("Player").GetComponent<PlayerHandler>();
        SpecialCards =GameObject.Find("GameHandler").GetComponent<SpecialCardsList>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
