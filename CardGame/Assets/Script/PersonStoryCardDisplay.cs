using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


enum CardClass
{
    Civil_Officials,General,StoryCard
}
public class PersonStoryCardDisplay : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    public Card _card;
    public Image background;
    public Image stars;
    public Text starsnum;
    public Text name;
    public Image icon;
    public Text description;
    public Text LeadNumberbuff;
    public Text value;
    private CardClass _cardClass;
    public float MaxSize = 1.1f;
    private float MinSize = 1.0f;
    private float CurSize = 1.0f;
    public void Show()
    {
        //background
        //stars
        Sprite sp=Resources.Load<Sprite>( "CardImage/" + _card.ID);
        GetComponent<Image>().sprite = sp;
        starsnum.text = "0";
        name.text = _card.Name;
        //icon
        description.text = _card.Text;
        if (_card is Civil_Officials)
        {
            LeadNumberbuff.text = "加成";
            value.text = Setting.CPersonATKBuff[0].ToString();
            _cardClass = CardClass.Civil_Officials;
        }
        else if (_card is General)
        {
            LeadNumberbuff.text = "率领人数";
            value.text = Setting.GPersonLeadNumber[0].ToString();
            _cardClass = CardClass.General;
        }
        else
        {
            _cardClass = CardClass.StoryCard;
            LeadNumberbuff.text = "";
            value.text = "";
        }
    }
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (_card is StoryCard)
        {
            
            if (!CheckPerson())
            {
                return;
            }
        }
        if (!Setting.GetEventSystem().SelectedPoolList.Contains(_card.ID))
        {
            GameObject GO = Setting.GetEventSystem().GetSelectedCardPrefab(_card);
            GO.transform.SetParent(Setting.GetEventSystem().CardContent.transform);
            Setting.GetEventSystem().SelectedPoolList.Add(_card.ID);
            GO.GetComponent<SelectedCardDisplay>().show();
            Setting.GetEventSystem().SelectedIDCardDict.Add(_card.ID,GO);
        }
        Setting.GetEventSystem().UpdateCardNum();

    }
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
      
        StopCoroutine(nameof(Narrow));
        StartCoroutine(nameof(Enlarge));
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {

        StopCoroutine(nameof(Enlarge));
        StartCoroutine(nameof(Narrow));
    }
    IEnumerator Enlarge()
    {
        
        while (CurSize <= MaxSize)
        {
            
            CurSize += (MaxSize - MinSize) * 0.1f;
            transform.localScale = new Vector3(CurSize, CurSize, 1);
            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator Narrow()
    {
        while (CurSize >= MinSize)
        {
            
            CurSize -= (MaxSize - MinSize) * 0.1f;
            transform.localScale = new Vector3(CurSize, CurSize, 1);
            yield return new WaitForSeconds(0.02f);
        }
    }

    bool CheckPerson()
    {
        
            List<int> list = Setting.GetEventSystem().StoryPersonDict[_card.ID];
            foreach (var item in list)
            {
                Debug.Log(item);
                if (Setting.GetEventSystem().SelectedPoolList.Contains(item))
                {
                    return true;
                }
            }

            return false;

    }



}
