using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropHandler : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private GameObject abilityUtil;
    private AbilityDecoder abilityDecoder;
    [SerializeField]
    private bool AcceptDice;

    private void Start()
    {
        abilityDecoder = abilityUtil.GetComponent<AbilityDecoder>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!AcceptDice)
        {
            if (transform.childCount == 0 && !(eventData.pointerDrag.GetComponent<EffectDto>().isDice))
            {
                GameObject Dropped = eventData.pointerDrag;
                if (Dropped != null)
                {
                    Dropped.GetComponent<DragHandler>().postDragParent = transform;
                }
                if (Dropped.GetComponent<EffectDto>().preStand)
                {
                    TriggerSPCEffect(Dropped);
                }
            }
        }
        else
        {
            if (transform.childCount == 0 && (eventData.pointerDrag.GetComponent<EffectDto>().isDice))
            {
                GameObject Dropped = eventData.pointerDrag;
                if (Dropped != null)
                {
                    Dropped.GetComponent<DragHandler>().postDragParent = transform;
                }
                if (Dropped.GetComponent<EffectDto>().preStand)
                {
                    TriggerSPCEffect(Dropped);
                }
            }
        }
        
    }

    public void TriggerSPCEffect()
    {
        if(transform.childCount == 0)
        {
           string s = transform.GetChild(0).GetComponent<EffectDto>().effect;
            abilityDecoder.Use(s);
            transform.GetChild(0).GetComponent<EffectDto>().Used=true;
        }
        
    }
    public void TriggerSPCEffect(GameObject child)
    {
        if (child != null)
        {
            abilityDecoder.Use(child.GetComponent<EffectDto>().effect);
            child.GetComponent<EffectDto>().Used = true;
        }
            
    }

}
