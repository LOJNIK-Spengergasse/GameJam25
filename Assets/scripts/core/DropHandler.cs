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
    private AudioManager audio;

    private void Start()
    {
        audio = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
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
                PlayRandomPlaceSound();
            }
        }
        else
        {
            //animate shake
            //rise and calc on effect use
            if (transform.childCount == 0 && (eventData.pointerDrag.GetComponent<EffectDto>().isDice))
            {
                GameObject Dropped = eventData.pointerDrag;
                if (Dropped != null)
                {
                    Dropped.GetComponent<DragHandler>().postDragParent = transform;
                    transform.GetChild(0).GetComponent<EffectDto>().Used = true;
                    audio.PlaySFX(audio.rollDice);
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
            Debug.Log("Drop");
            abilityDecoder.Use(child.GetComponent<EffectDto>().effect);
            child.GetComponent<EffectDto>().Used = true;
        }
            
    }

    private void PlayRandomPlaceSound()
    {
        System.Random r = new System.Random();
        switch (r.Next(3))
        {
            case 0:
                audio.PlaySFX(audio.PlayingMagicCard);
                break;
            case 1:
                audio.PlaySFX(audio.PlayingMagicCard2);
                break;
            case 2:
                audio.PlaySFX(audio.PlayingMagicCard3);
                break;
        }
    }

}
