using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public  enum CardState
 {
     Drawing,OnHand,Selected,Destory,OnTable
 }
public class HandCardDisplay : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerClickHandler
{
  
    // Start is called before the first frame update
    public Image background;
    public Image stars;
    public Text starsnum;
    public Text name;
    public Image icon;
    public Text description;
    public Text LeadNumberbuff;
    public Text value;
    public Card _card;
    private Vector3 handpos = new Vector3();
    private float showpos;
    private float destroypos;
    public CardState cardstate=CardState.Drawing;
    public bool isselected=false;
    public GameObject parent;
    private bool dragend = true;
    private Vector3 gap = new Vector3();
    public int owner = 0; //0=player.1=enemy
    private bool EnemyUsing=false;
    public int index=0;
    public int ShowIndex=0;
    void Start()
    {
        if (!Setting.GetBattleEventSystem().HandCardLine)
        {
            Debug.Log("Handcardline miss");
        }
        showpos = Setting.GetBattleEventSystem().HandCardLine.transform.position.y + 200;
        destroypos=Setting.GetBattleEventSystem().HandCardLine.transform.position.y + 300;
       
    }



    // Update is called once per frame
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (owner == 0)
        {
            if (cardstate == CardState.OnHand)
            {
                StopCoroutine(nameof(MoveBack));
                StartCoroutine(nameof(MoveUP));

            }
        }

    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (owner == 0)
        {
            if (cardstate == CardState.OnHand && dragend == true)
            {
                StopCoroutine(nameof(MoveUP));
                StartCoroutine(nameof(MoveBack));
            }
        }
    }
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (owner == 0)
        {
            if (Setting.GetBattleEventSystem().roundstate == RoundState.ChangeCard)
            {
                if (isselected == false && cardstate == CardState.OnHand)
                {
                    Setting.GetBattleEventSystem().UpdateCardUIIndex();
                    isselected = true;
                    StopCoroutine(nameof(MoveBack));
                    StartCoroutine(nameof(MoveUP));
                    cardstate = CardState.Selected;
                }
                else
                {
                    StopCoroutine(nameof(MoveUP));
                    StartCoroutine(nameof(MoveBack));
                    isselected = false;
                    cardstate = CardState.OnHand;
                }
            }
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (Setting.GetBattleEventSystem().roundstate == RoundState.Battle&&cardstate == CardState.Drawing)
        {
            dragend = false;
            Vector3 pos = new Vector3();
            RectTransformUtility.ScreenPointToWorldPointInRectangle(GetComponent<RectTransform>(), eventData.position,
                eventData.pressEventCamera, out pos);
            transform.position = new Vector2(pos.x+gap.x,pos.y+gap.y);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<Image>().raycastTarget = true;
        if (Setting.GetBattleEventSystem().roundstate == RoundState.Battle&&cardstate==CardState.Drawing)
        {
            GameObject MatchCard;
            if (_card is PersonCard)
            {
                Debug.Log(eventData.pointerEnter);
                if (Setting.GetBattleEventSystem().PlayerPersonBlock.Contains(eventData.pointerEnter))
                {
                    Debug.Log(Setting.GetBattleEventSystem().CheckCondition(eventData.pointerEnter,gameObject,out MatchCard));
                    if (Setting.GetBattleEventSystem().CheckCondition(eventData.pointerEnter,gameObject,out MatchCard))
                    {
                        Setting.GetBattleEventSystem().PutPersonCard(eventData.pointerEnter,gameObject);
                        Setting.GetBattleEventSystem().DeleteCardFromHand(gameObject);
                    }
                    else
                    { 
                        Setting.GetBattleEventSystem().UpdateCardUIIndex();
                        StartCoroutine(nameof(MoveTo),parent);
                    }
                    
                }
                else
                {    Setting.GetBattleEventSystem().UpdateCardUIIndex();
                    StartCoroutine(nameof(MoveTo),parent);
                    //cardstate = CardState.OnHand;
                }
            }
            else if(_card is StoryCard)
            {
                Debug.Log((_card as StoryCard).MatchPerson);
                if (Setting.GetBattleEventSystem().CheckCardPos(gameObject)&&Setting.GetBattleEventSystem().CheckPerson(_card as StoryCard,out MatchCard))
                {
                    Setting.GetBattleEventSystem().AddPersonStar(MatchCard);
                    Setting.GetBattleEventSystem().DeleteCardFromHand(gameObject);
                }
                else
                {   Setting.GetBattleEventSystem().UpdateCardUIIndex();
                    StartCoroutine(nameof(MoveTo),parent);
                    Debug.Log("未出牌");
                   // cardstate = CardState.OnHand;
                }
            }
            else if (_card is SoldierCard)
            {
                if (Setting.GetBattleEventSystem().CheckCardPos(gameObject))
                {
                    Setting.GetBattleEventSystem().AddSlodierNum(1);
                    Setting.GetBattleEventSystem().DeleteCardFromHand(gameObject);
                }
                else
                {   Setting.GetBattleEventSystem().UpdateCardUIIndex();
                    StartCoroutine(nameof(MoveTo),parent);
                    Debug.Log("未出牌");
                    //cardstate = CardState.OnHand;
                }
            }
            else
            {
                
            }

        }
        
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Setting.GetBattleEventSystem().roundstate == RoundState.Battle)
        {
            Debug.Log("stop");
            StopCoroutine(nameof(MoveTo));
        }
        if (Setting.GetBattleEventSystem().roundstate == RoundState.Battle&&(cardstate==CardState.Drawing||cardstate==CardState.OnHand))
        {
            cardstate = CardState.Drawing;
            GetComponent<Image>().raycastTarget = false;
            gap = transform.position-Input.mousePosition;
            Debug.Log(gap);
            Vector3 pos = new Vector3();
            RectTransformUtility.ScreenPointToWorldPointInRectangle(GetComponent<RectTransform>(), eventData.position,
                eventData.pressEventCamera, out pos);
            transform.position = pos+gap;
            
        }
    }
    public void Show()
    {
        
        if ((owner == 1&&cardstate!=CardState.OnTable))
        {
            Sprite sp = Resources.Load<Sprite>("CardImage/" + "back");
            GetComponent<Image>().sprite = sp;
            starsnum.text = "0";
            name.text = _card.Name;
            //icon
            description.text = _card.Text;            
            starsnum.gameObject.SetActive(false);
            name.gameObject.SetActive(false);
            //icon
            description.gameObject.SetActive(false);
        }
        else
        {
            
            starsnum.gameObject.SetActive(true);
            //name.gameObject.SetActive(true);
            //icon
            description.gameObject.SetActive(true);
            if (_card is not null)
            {
                Sprite sp = Resources.Load<Sprite>("CardImage/" + _card.ID);
                GetComponent<Image>().sprite = sp;
            }
            starsnum.text = "0";
            name.text = _card.Name;
            //icon
            description.text = _card.Text;  
            //background
            //stars
            if (_card is Civil_Officials)
            {
                LeadNumberbuff.text = "加成";
                value.text = Setting.CPersonATKBuff[0].ToString();
            }
            else if (_card is General)
            {
                LeadNumberbuff.text = "率领人数";
                value.text = Setting.GPersonLeadNumber[0].ToString();
            }
            else if(_card is SoldierCard)
            {
                Sprite sp = Resources.Load<Sprite>("CardImage/0");
                GetComponent<Image>().sprite = sp;
                LeadNumberbuff.text = "";
                value.text = "";
            }
        }
    }
    public void EnemyShow()
    {if (_card is not null)
        {
            Sprite sp = Resources.Load<Sprite>("CardImage/" + _card.ID);
            GetComponent<Image>().sprite = sp;
        }
        starsnum.text = "0";
        name.text = _card.Name;
        //icon
        description.text = _card.Text;  
        //background
        //stars
        if (_card is Civil_Officials)
        {
            LeadNumberbuff.text = "加成";
            value.text = Setting.CPersonATKBuff[0].ToString();
        }
        else if (_card is General)
        {
            LeadNumberbuff.text = "率领人数";
            value.text = Setting.GPersonLeadNumber[0].ToString();
        }
        else if(_card is SoldierCard)
        {
            Sprite sp = Resources.Load<Sprite>("CardImage/0");
            GetComponent<Image>().sprite = sp;
            LeadNumberbuff.text = "";
            value.text = "";
        }
        
    }
    public void EnemyUse()
    {
        EnemyUsing = true;
        EnemyShow();
        transform.SetAsLastSibling();
        StartCoroutine(EnemyUseIE());
    }
    public IEnumerator EnemyUseIE()
    {
        Vector2 endpos = Setting.GetBattleEventSystem().GameOver.transform.position;
        while ((new Vector2(transform.position.x, transform.position.y)-
                   endpos).sqrMagnitude > 400)
        {
            transform.position = Vector3.Lerp(transform.position, endpos, Setting.MoveSpeed);
            yield return new WaitForFixedUpdate();
        }
        yield return null;
        EnemyUsing = false;
        Setting.GetBattleEventSystem().DeleteCard(gameObject);
        
    }
    public IEnumerator MoveUP()
    {
        //index = transform.GetSiblingIndex();
        //transform.SetParent(Setting.GetBattleEventSystem().HandCardUp.transform);
        //transform.SetSiblingIndex(index);
        if (isselected == true)
        {
            Setting.GetBattleEventSystem().UpdateCardUIIndex();
        }
        else
        {
            transform.SetAsLastSibling();
        }
        
        while (transform.position.y<=showpos)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y+Setting.CardSpeed, 0);
            yield return new WaitForSeconds(0.02f);
            //yield return Time.fixedDeltaTime;
        }
    }
    public IEnumerator MoveBack()
    {
        //transform.SetParent(Setting.GetBattleEventSystem().HandCardPoint.transform);
        //transform.SetSiblingIndex(index);
        Setting.GetBattleEventSystem().UpdateCardUIIndex();
        while (transform.position.y>=handpos.y+10)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y-Setting.CardSpeed, 0);
            yield return new WaitForSeconds(0.02f);
        }

        //transform.position = parent.transform.position;
    }
    public IEnumerator CardDraw(GameObject endpos)
    {
        
        cardstate = CardState.Drawing;
        Debug.Log("Enemy");
        RectTransform backrect;
        if (owner == 0)
        {
            backrect = Setting.GetBattleEventSystem().CardBack.GetComponent<RectTransform>();
            RectTransform rect = GetComponent<RectTransform>();
            float x = 0f;
            while (backrect.localRotation.eulerAngles.y < 90)
            {
                backrect.localRotation = Quaternion.Euler(0, x, 0);
                x = x + 6f;
                yield return new WaitForSeconds(0.01f);
            }
            backrect.localRotation = quaternion.Euler(0, 0, 0);
            x = 0;
            gameObject.SetActive(true);
            rect.localRotation = Quaternion.Euler(0, 270, 0);
            while (rect.localRotation.eulerAngles.y > 1)
            {

                rect.localRotation = Quaternion.Euler(0, 270 + x, 0);
                x = x + 6f;
                yield return new WaitForSeconds(0.01f);
            }
        }

        yield return StartCoroutine(MoveTo(endpos));
    }
    public IEnumerator MoveTo(GameObject endpos)
    {
        if (owner == 0)
        {
            Setting.GetBattleEventSystem().UpdateCardNum();
            if (Setting.GetBattleEventSystem().roundstate == RoundState.DrawCard ||
                Setting.GetBattleEventSystem().roundstate == RoundState.ChangeCard)
            {

                transform.position = Setting.GetBattleEventSystem().DrawCardStartPoint.transform.position;
            }
        }
        else
        {
            //transform.position = Setting.GetBattleEventSystem().DrawCardStartPoint.transform.position;
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Vector3 End = endpos.transform.position;
        while (Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                   End) > 5)
        {
            transform.position = Vector3.Lerp(transform.position, End, Setting.MoveSpeed);
           yield return new WaitForSeconds(0.01f);
        }
        transform.position = End;
        handpos = endpos.transform.position;
        cardstate = CardState.OnHand;
        if (dragend == false) dragend = true;

    }
    public IEnumerator SelectedDestroy()
    {
        parent = null;
        while (transform.position.y<destroypos)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + Setting.CardSpeed, 0);
            yield return new WaitForSeconds(0.02f);
        }
        Setting.GetBattleEventSystem().DeleteCard(gameObject);
    }
    public IEnumerator ScaleToBlock()
    {
        GetComponent<Image>().raycastTarget = false;
        Vector2 target = Setting.GetBattleEventSystem().PlayerCardZone.GetComponent<GridLayoutGroup>().cellSize;
        Vector2 start=GetComponent<RectTransform>().rect.size;
        while (GetComponent<RectTransform>().localScale.x > (target/start).x)
        {
            GetComponent<RectTransform>().localScale -= (Vector3)(target / start) / 25;
            yield return new WaitForSeconds(0.02f);
        }
    }
    public void EnemyToTable()
    {
        cardstate = CardState.OnTable;
        Show();
        StartCoroutine(nameof(ScaleToBlock));
    }

    public void AIDelete()
    {
        EnemyUsing = true;
        EnemyShow();
        transform.SetAsLastSibling();
        StartCoroutine(AIDeleteIE());
    }
    public IEnumerator AIDeleteIE()
    {
        float endy = transform.position.y - 300;
        Debug.Log(transform.position.y);
        Debug.Log(endy);
        while (transform.position.y>endy)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - Setting.CardSpeed*0.5f, 0);
            yield return new WaitForSeconds(0.02f);
        }
        EnemyUsing = false;
        Setting.GetBattleEventSystem().DeleteCard(gameObject);
    }

    public void UpdateSelfIndex()
    {
        int Finindex = 0;
        Dictionary<int, GameObject> CardDict = Setting.GetBattleEventSystem().player.PlayerHandCard;
        foreach (var VARIABLE in CardDict)
        {
            if (VARIABLE.Value)
            {
                if (VARIABLE.Value.GetComponent<HandCardDisplay>().index < index)
                {
                    Finindex++;
                }
            }
        }

        ShowIndex = Finindex;
    }
}
