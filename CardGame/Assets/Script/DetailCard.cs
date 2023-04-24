using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailCard : MonoBehaviour
{
    public Card _card;
    public Image background;
    //public Image stars;
    public Text starsnum;
    //public Text name;
    //public Image icon;
    public Text description;
    public Text LeadNumberbuff;
    public Text value;
    private Sprite sp;
    public void Show(int starnum)
    {
        transform.SetAsLastSibling();
        //background
        //stars
        //name.text = _card.Name;
        //icon
        if (_card is not null)
        {
            
            if (_card is SoldierCard)
            {
               sp = Resources.Load<Sprite>("CardImage/0");
            }
            else
            {
                sp = Resources.Load<Sprite>("CardImage/"+_card.ID);
            }
            
            GetComponent<Image>().sprite = sp;
        }

        description.text = _card.Text;
        if (_card is Civil_Officials)
        {
            starsnum.text = starnum.ToString();
            LeadNumberbuff.text = "加成";
            value.text = Setting.CPersonATKBuff[starnum].ToString();
        }
        else if (_card is General)
        {
            starsnum.text = starnum.ToString();
            LeadNumberbuff.text = "率领人数";
            value.text = Setting.GPersonLeadNumber[starnum].ToString();

        }
        else
        {
            LeadNumberbuff.text = "";
            value.text = "";
        }
    }
}
