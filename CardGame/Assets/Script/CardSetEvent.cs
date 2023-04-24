using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Windows;
using System.IO;
using File = System.IO.File;
using UnityEngine.SceneManagement;

public class CardSetEvent : MonoBehaviour
{
   
    public int MinCardNum = 20;
    public int MaxCardNum = 100;
    public Text CardNum;
    public int SoldierNum = 0;
    public int SelectState = 0; //0=人物选择，1=故事选择
    public GameObject SoldierCard;
    public GameObject ShowDetailCard;
    public GameObject SelectedCardPrefab;
    public GameObject CardContent;
    public List<int> SelectedPoolList = new List<int>();
    private Stack<GameObject> CardPool = new Stack<GameObject>();
    public GameObject SelectedCityCardPrefab;
    public List<GameObject> LibraryGOList;
    public List<GameObject> CityCardPool;
    private List<GameObject> SelectedCityCardList=new List<GameObject>();
    private List<int> SelectedCityCardIDList = new List<int>();
    public int SelectedNum = 0;
    private int MaxSelectedNum = 100;
    public List<CityCard> CityCardList = new List<CityCard>();
    public Dictionary<int, Civil_Officials> CPersonCardDict=new Dictionary<int, Civil_Officials>();
    public Dictionary<int, General> GPersonCardDict=new Dictionary<int, General>();
    public Dictionary<int, StoryCard> StoryCardDict=new Dictionary<int, StoryCard>();
    public TextAsset CardAsset;
    public Button PrePage;
    public Button NextPage;
    private int Page;
    private int CurPage=1;
    private Dictionary<string, List<int>> CityPersonDict = new Dictionary<string, List<int>>();
    public Dictionary<int, List<int>> PersonStoryDict = new Dictionary<int, List<int>>();
    public Dictionary<int, List<int>> StoryPersonDict = new Dictionary<int, List<int>>();
    private Dictionary<string, int> PersontoidDict = new Dictionary<string, int>();
    private int PersonPage;
    private int StoryPage;
    private int CanUsetStoryNum=0;
    private int CurPersonStoryPage=1;
    private int CanUsePersonNum = 0;
    public List<GameObject> CardChoice;
    public List<GameObject> CardGOList;
    private List<int> CanUsePersonIDList = new List<int>();
    private List<Card> CanUsePersonList = new List<Card>(); //可用人物卡列表
    private List<int> CanUseStoryIDList = new List<int>();
    private List<Card> CanUseStoryList = new List<Card>();
    public Button PSNextButton;
    public Button PSPreButton;
    public Dictionary<int, GameObject> SelectedIDCardDict = new Dictionary<int, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        SelectedCityCardList.Clear();
        SelectedPoolList.Clear();
        Debug.Log(Application.persistentDataPath);
        Debug.Log(Application.dataPath);
        CardChoice[0].SetActive(true);
        CardChoice[1].SetActive(false);
        LoadCard();
        ShowCityCard(CurPage);
        ShowSoldierCard();
        UpdateCardNum();
    }
    public void ToCardChoice()
    {
        if (SelectedCityCardList.Count != 3) return;
        foreach (var item in SelectedCityCardList)
        {
            SelectedCityCardIDList.Add(item.GetComponent<SelectedCityCardDisplay>()._card.ID);
        }
        CardChoice[0].SetActive(false);
        CardChoice[1].SetActive(true);
        UpdateCanUsePersonCard();
        ShowPersonCard(CurPersonStoryPage);
    }
    public void ToPersonSelect()
    {
        SelectState = 0;
        CurPersonStoryPage = 1;
        ShowPersonCard(CurPersonStoryPage);

    }
    public void ToStorySelect()
    {
        SelectState = 1;
        if (SelectedPoolList.Count == 0)
        {
            return;}
        CurPersonStoryPage = 1;
        UpdateCanUseStoryCard();
        ShowStoryCard(CurPersonStoryPage);

    }
    void LoadCard()
    {
        string[] CardData = CardAsset.text.Split('\n');
        foreach (var item in CardData)
        {
            if (item.Length!=0&&item[0]=='#') continue;
            string[] SingleCard = item.Split(',');
            //City:_id, _text, _name
            //CPerson:int _id, string _text,string _name, string _area, string _title, int _stars, float _atkBuff
            //GPerson:int _id, string _text, string _name, string _area, string _title, int _stars, int _leadNumber
            //StoryCard:int _id, string _text, string _name, string _matchPerson(复数), string _matchTitle
            switch (SingleCard[0])
            {
                case "City":
                    CityCard cityCard = new CityCard(Int32.Parse(SingleCard[1]), SingleCard[3], SingleCard[2]);
                    CityCardList.Add(cityCard);
                    break;
                case "CPerson":
                    Civil_Officials civilOfficials = new Civil_Officials(Int32.Parse(SingleCard[1]), SingleCard[3],
                        SingleCard[2], SingleCard[4], SingleCard[5], Int32.Parse(SingleCard[6]),Setting.CPersonATKBuff[0]);
                    CPersonCardDict.Add(Int32.Parse(SingleCard[1]),civilOfficials);
                    if (!CityPersonDict.ContainsKey(civilOfficials.Area))
                    {
                        CityPersonDict.Add(civilOfficials.Area,new List<int>(){civilOfficials.ID});
                    }
                    else
                    {
                        CityPersonDict[civilOfficials.Area].Add(civilOfficials.ID);
                    }
                    PersontoidDict.Add(civilOfficials.Name,civilOfficials.ID);
                    break;
                case "GPerson":
                    General general = new General(Int32.Parse(SingleCard[1]), SingleCard[3],
                        SingleCard[2], SingleCard[4], SingleCard[5], Int32.Parse(SingleCard[6]),
                        Setting.GPersonLeadNumber[0]);
                    GPersonCardDict.Add(Int32.Parse(SingleCard[1]),general);
                    if (!CityPersonDict.ContainsKey(general.Area))
                    {
                        CityPersonDict.Add(general.Area,new List<int>(){general.ID});
                    }
                    else
                    {
                        CityPersonDict[general.Area].Add(general.ID);
                    }
                    PersontoidDict.Add(general.Name,general.ID);
                    break;
                case "StoryCard":
                    StoryCard storyCard = new StoryCard(Int32.Parse(SingleCard[1]), SingleCard[3],
                        SingleCard[2], SingleCard[4], SingleCard[5]);
                    StoryCardDict.Add(Int32.Parse(SingleCard[1]),storyCard);
                    break;
                default:
                    break;
                    
            }
            
        }

        foreach (var item in StoryCardDict)
        {
            string[] Persons;
            if (item.Value.MatchPerson[0] == '"')
            {
                Persons = item.Value.MatchPerson.Split('"')[1].Split('，');
            }
            else Persons = new string[] { item.Value.MatchPerson };
            for(int i=0;i<Persons.Length;i++){
             if(!PersonStoryDict.ContainsKey(PersontoidDict[Persons[i]]))
             {
              PersonStoryDict.Add(PersontoidDict[Persons[i]],new List<int>(){item.Value.ID});
             }
             else
             {
                 PersonStoryDict[PersontoidDict[Persons[i]]].Add(item.Value.ID);
             }
            }
            if (Persons.Length == 1)
            {
               StoryPersonDict.Add(item.Key,new List<int>(){PersontoidDict[Persons[0]]}); 
            }
            else
            {
                StoryPersonDict.Add(item.Key, new List<int>() { PersontoidDict[Persons[0]] });
                for (int i = 1; i < Persons.Length; i++)
                {
                    StoryPersonDict[item.Key].Add(PersontoidDict[Persons[i]]);
                }
            }

            
        }
        Page =Convert.ToInt32(Math.Ceiling(Convert.ToDouble(CityCardList.Count)/14.0));
    }
    void ShowCityCard(int Index)
    {
        int MinIndex = (Index - 1) * 14;
        int MaxIndex = Index * 14 - 1;
        int CurLibraryCardIndex = 0;
        for (int i = MinIndex; i <= MaxIndex; i++)
        {
            if (i<CityCardList.Count)
            {
                LibraryGOList[CurLibraryCardIndex].GetComponent<CityCardLibraryDisplay>()._card = CityCardList[i];
                LibraryGOList[CurLibraryCardIndex].GetComponent<CityCardLibraryDisplay>().Show();
                LibraryGOList[CurLibraryCardIndex].SetActive(true);
                CurLibraryCardIndex++;
            }
            else
            {
                LibraryGOList[CurLibraryCardIndex].GetComponent<CityCardLibraryDisplay>()._card = null;
                LibraryGOList[CurLibraryCardIndex].SetActive(false);
                CurLibraryCardIndex++;
            }
        }
        PageButtonCheck();
    }
    public bool CheckExist(int id)
    {
        if (SelectedCityCardList.Count == 0) return true;
        if (SelectedCityCardList.Count == 3) return false;
        foreach (var item in SelectedCityCardList)
        {
            Debug.Log(item.name);
            if (item.GetComponent<SelectedCityCardDisplay>()._card.ID == id)
            {
                return false;
            }
            
        }
        return true;
    }
    public void NextPageButton()
    {
        CurPage++;
        ShowCityCard(CurPage);
        PageButtonCheck();

    }
    public void PrePageButton()
    {
        CurPage--;
        ShowCityCard(CurPage);
        PageButtonCheck();
    }
    void PageButtonCheck()
    {
        if (CurPage == 1)
        {
            PrePage.enabled = false;
        }
        if (CurPage < Page)
        {
             NextPage.enabled = true;
        }
        if (CurPage == Page)
        {
            NextPage.enabled = false;
        }

        if (CurPage > 1)
        {
            PrePage.enabled = true;
        }

       
    }
    public void AddSelectedCityCard(CityCard Card)
    {
        GameObject GO = GameObject.Instantiate(SelectedCityCardPrefab);
        GO.GetComponent<SelectedCityCardDisplay>()._card = Card;
        SelectedCityCardList.Add(GO);
    }
    public void SelectedCityCardShow()
    {
        
        int Index = 0;
        foreach (var item in SelectedCityCardList)
        {
   
            item.transform.SetParent(CityCardPool[Index++].transform);
            item.transform.localPosition = new Vector3(0, 0, 0);
            item.GetComponent<SelectedCityCardDisplay>().Show();
        }
    }
    public void DeleteSelectedCityCard(int ID)
    {
        for (int Index = 0;Index<SelectedCityCardList.Count;Index++)
        {
            if (SelectedCityCardList[Index].GetComponent<SelectedCityCardDisplay>()._card.ID == ID)
            {
                Destroy(SelectedCityCardList[Index]);
                SelectedCityCardList.RemoveAt(Index);
                
            }
        }
        SelectedCityCardShow();
    }
    public void ShowPersonCard(int Index)
    {
        int MinIndex = (Index - 1) * 8;
        int MaxIndex = Index * 8 - 1;
        int CurLibraryCardIndex = 0;
        for (int i = MinIndex; i <= MaxIndex; i++)
        {
            if (i<CanUsePersonNum)
            {
                CardGOList[CurLibraryCardIndex].GetComponent<PersonStoryCardDisplay>()._card =CanUsePersonList[i];
                CardGOList[CurLibraryCardIndex].GetComponent<PersonStoryCardDisplay>().Show();
                CardGOList[CurLibraryCardIndex].SetActive(true);
                CurLibraryCardIndex++;
            }
            else
            {
                CardGOList[CurLibraryCardIndex].GetComponent<PersonStoryCardDisplay>()._card = null;
                CardGOList[CurLibraryCardIndex].SetActive(false);
                CurLibraryCardIndex++;
            }
        }
    }
    void UpdateCanUsePersonCard()
    {
        foreach (var item in SelectedCityCardList)
        {
            foreach (var VARIABLE in CityPersonDict[item.GetComponent<SelectedCityCardDisplay>()._card.Name])
            {
                CanUsePersonIDList.Add(VARIABLE);
            }
            
        }
        for (int i = 0; i < CanUsePersonIDList.Count; i++)
        {
            if (CPersonCardDict.ContainsKey(CanUsePersonIDList[i]))
            {
                CanUsePersonList.Add(CPersonCardDict[CanUsePersonIDList[i]]);
            }
            else
            {
                CanUsePersonList.Add(GPersonCardDict[CanUsePersonIDList[i]]);
            }
            
        }
        CanUsePersonNum = CanUsePersonIDList.Count;
        PersonPage =Convert.ToInt32(Math.Ceiling(Convert.ToDouble(CanUsePersonNum)/8.0));
        PSButtonCheck();
    }
    public GameObject GetSelectedCardPrefab(Card card)
    {
        CardContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (SelectedPoolList.Count+1) * 70);
        if (CardPool.Count != 0)
        {
            CardPool.Peek().GetComponent<SelectedCardDisplay>()._card = card;
            CardPool.Peek().SetActive(true);
            return CardPool.Pop();
        }
        else{
            GameObject GO = GameObject.Instantiate(SelectedCardPrefab);
            GO.GetComponent<SelectedCardDisplay>()._card = card;
            return GO;
        }

        
    }
    public void DeleteSelectedCard(GameObject go)
    {
        CardContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, SelectedPoolList.Count * 70);
        go.SetActive(false);
        go.transform.SetParent(transform);
        CardPool.Push(go);
        
    }

    public void UpdateCanUseStoryCard()
    {
        CanUseStoryIDList.Clear();
        CanUseStoryList.Clear();
        for (int i = 0; i < SelectedPoolList.Count; i++)
        {
            if (PersonStoryDict.ContainsKey(SelectedPoolList[i]))
            {
                List<int> list = PersonStoryDict[SelectedPoolList[i]];
                CanUseStoryIDList.AddRange(list);
            }
        }
        foreach (var item in CanUseStoryIDList)
        {
            if (!CanUseStoryList.Contains(StoryCardDict[item]))
            {
                CanUseStoryList.Add(StoryCardDict[item]);
            }
        }
        CanUsetStoryNum = CanUseStoryList.Count;
        StoryPage =Convert.ToInt32(Math.Ceiling(Convert.ToDouble(CanUsetStoryNum)/8.0));
        PSButtonCheck();
        if (CanUsetStoryNum == 0)
        {
            SelectState = 0;
            ToPersonSelect();
        }
    }

    public void ResetPSPage()
    {
        CurPersonStoryPage = 1;
    }

    public void ShowStoryCard(int Index)
    {
        int MinIndex = (Index - 1) * 8;
        int MaxIndex = Index * 8 - 1;
        int CurLibraryCardIndex = 0;
        for (int i = MinIndex; i <= MaxIndex; i++)
        {
            if (i<CanUsetStoryNum)
            {
                CardGOList[CurLibraryCardIndex].GetComponent<PersonStoryCardDisplay>()._card =CanUseStoryList[i];
                CardGOList[CurLibraryCardIndex].GetComponent<PersonStoryCardDisplay>().Show();
                CardGOList[CurLibraryCardIndex].SetActive(true);
                CurLibraryCardIndex++;
            }
            else
            {
                CardGOList[CurLibraryCardIndex].GetComponent<PersonStoryCardDisplay>()._card = null;
                CardGOList[CurLibraryCardIndex].SetActive(false);
                CurLibraryCardIndex++;
            }
        }
        
        
    }

    void PSButtonCheck()
    {
        if (SelectState == 0)
        {
            if (CurPersonStoryPage == 1)
            {
                PSPreButton.enabled = false;
            }
            if (CurPersonStoryPage < PersonPage)
            {
                PSNextButton.enabled = true;
            }
            if (CurPersonStoryPage == PersonPage)
            {
                PSNextButton.enabled = false;
            }

            if (CurPersonStoryPage > 1)
            {
                PSPreButton.enabled = true;
            }

            
        }
        else
        {
            if (CurPersonStoryPage == 1)
            {
                PSPreButton.enabled = false;
            }
            if (CurPersonStoryPage < StoryPage)
            {
                PSNextButton.enabled = true;
            }
            if (CurPersonStoryPage == StoryPage)
            {
                PSNextButton.enabled = false;
            }

            if (CurPersonStoryPage > 1)
            {
                PSPreButton.enabled = true;
            }

            
        }
    }

    public void PSNext()
    {
        if (SelectState == 0)
        {
            ShowPersonCard(++CurPersonStoryPage);
            PSButtonCheck();
        }
        else
        {
            ShowStoryCard(++CurPersonStoryPage);
            PSButtonCheck();
        }

    }

    public void PSPre()
    {
        if (SelectState == 0)
        {
            ShowPersonCard(--CurPersonStoryPage);
            PSButtonCheck();
        }
        else
        {
            ShowStoryCard(--CurPersonStoryPage);
            PSButtonCheck();
        }
    }

    public void ShowSoldierCard()
    {
        SelectedPoolList.Add(0);
        CardContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, SelectedPoolList.Count * 70);
        SoldierCard.GetComponent<SoldierCardDisplay>()._card = new SoldierCard(0, "", "士兵卡");
        SoldierCard.GetComponent<SoldierCardDisplay>().Show();
    }

    public void UpdateCardNum()
    {
        int num = SoldierNum + SelectedPoolList.Count - 1;
        CardNum.text = num.ToString() + "/"+MaxCardNum.ToString();
    }

    public void FinishCardSetCreate()
    {
        
        string SavePath = Application.persistentDataPath + "/CardSet.csv";
        Debug.Log(SavePath);
        List<string> CardSet = new List<string>();
        int num = SoldierNum + SelectedPoolList.Count - 1;
        if (num < MinCardNum) {return;}
        if(num > MaxCardNum) {return;}
        CardSet.Add("0,"+SoldierNum.ToString());
        Debug.Log(SelectedCityCardList.Count);
        foreach (var item in SelectedCityCardIDList)
        {
            CardSet.Add(item.ToString()+",1");
        }
        foreach (var item in SelectedPoolList)
        {
            if (item != 0)
            {
                CardSet.Add(item.ToString() + ",1");
            }
        }
        File.WriteAllLines(SavePath,CardSet);
        SceneManager.LoadScene("Scenes/Menu");


    }
}
