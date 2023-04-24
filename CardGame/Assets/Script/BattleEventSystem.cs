using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;
using Cache = UnityEngine.Cache;
using UnityEngine.SceneManagement;


public enum RoundState
{
    DrawCard,ChangeCard,Battle,Defense,Choosing,Enemy,End
}

enum WhoseTurn
{
 player,enemy  
}

public class Player
{
    public float SlodierInCityNum = 0;
    public float PlayterMoraleSum = 0;
    public float PlayerSlodierNum = 0;//单位：万
    public float CanUsePlayerSlodierNum = 0;
    public List<GameObject> PlayerOnTableCard = new List<GameObject>(); //场上卡（不包含城池）
    public Dictionary<int, int> PlayerCardSetDict = new Dictionary<int, int>(); //卡组字典
    public List<Card> PlayerCityList = new List<Card>();  //现有城池卡
    public List<Card> PlayerSetList = new List<Card>();  //卡组
    public Dictionary<int, GameObject> PlayerHandCard = new Dictionary<int, GameObject>(); //现有手牌
}

//public class AIController
//{
   
//}

public class BattleEventSystem : MonoBehaviour
{

    private int CardIndex = 0;
    public GameObject HandCardUp;
    public GameObject HandCardPoint;
    public GameObject TurnButton;
    public GameObject GameOver;
    //public Text GameOverText;
    public GameObject EnemyCardBack;
    public GameObject CardBack;
    public GameObject Arrow;
    public InputField AttackInput;
    public InputField DefenceSlodierInput;
    public GameObject AttackConfirm;
    public GameObject DefenceSlodierConfirm;
    public GameObject DetailCard;
    public Text PlayerSlodierNum;
    public Text EnemySlodierNum;
    public Text PlayerCanUseSlodierNum;
    public Text EnemyCanUseSlodierNum;
    public Text PlayterMorale;
    public Text EnemyMorale;
    public Text PlayerCardNumText;
    public Text EnemyCardNumText;
    public UnityEvent CardTOTable = new UnityEvent();
    public List<GameObject> PlayerCityBlock;
    public List<GameObject> EnemyCityBlock;
    public GameObject PlayerCityZone;
    public GameObject EnemyCityZone;
    private TextAsset PlayerCardSet;
    private TextAsset EnemyCardSet;
    public List<GameObject> PlayerPersonBlock;
    public List<GameObject> EnemyPeronBlock;
    public GameObject PlayerCardZone;
    public GameObject Canvas;
    public GameObject CanvasPre;
    public GameObject HandCardPointPrefab;
    public GameObject EnemyHandCardStartPoint;
    public int MaxHandCardNum = 10;
    public GameObject HandCardLine;
    public GameObject EnemyHandCardLine;
    public GameObject DrawCardStartPoint;
    public GameObject HandCardPrefab;
    public GameObject CityPrefab;
    public TextAsset CardAsset;
    private Stack<GameObject> CardPool = new Stack<GameObject>();
    private Stack<GameObject> GOPool = new Stack<GameObject>();
    public int TurnNum = 0;
    private Queue<IEnumerator> IEStack = new Queue<IEnumerator>();
    public RoundState roundstate = RoundState.DrawCard;
    private int selectednum = 0;
    public Player player ;
    public Player enemy ;
    public GameObject ChoosingBlock; //攻击的格子
    public GameObject TargetBlock; //被攻击的格子
    public AIController AIController;
    private bool changed = false;
    private void Start()
    {
        StopAllCoroutines();
        player = new Player();
        enemy = new Player();
        Debug.Log(HandCardLine);
        CardPool.Clear();
        GOPool.Clear();
        IEStack.Clear();
        Debug.Log(CardPool.Count);
        Debug.Log(GOPool.Count);
        Debug.Log(Application.persistentDataPath);
        AIController = Canvas.GetComponent<AIController>();
        AIController.enemy = enemy;
        Load();
        StartCoroutine(GameStart());
        
    }
    public IEnumerator GameStart()
    {
        yield return 0;
        CardTOTable.Invoke();
        List<GameObject> EnemyHandLineBlock = AIController.CreateCube(5, EnemyHandCardLine);
        List<GameObject> PlayerHandLineBlock = CreateCube(5, HandCardLine);
        FirstTurnCantAttack();
        yield return new WaitForSeconds(0.1f);
        DrawCard(5, PlayerHandLineBlock);
        StartCoroutine(AIController.DrawCard(5, EnemyHandLineBlock));
        while (AIController.Queue.Count != 0)
        {
            yield return StartCoroutine(AIController.Queue.Dequeue());
        }
    }

    public IEnumerator AIDrawAndStart()
    {
        int num = 0;
        if (enemy.PlayerSetList.Count - 2 >= 0)
        {
            num = 2;
        }
        else if(enemy.PlayerSetList.Count-2==-1)
        {
            num = 1;
        }
        else if(enemy.PlayerSetList.Count==0)
        {
            num = 0;
        }
        List<GameObject> EnemyHandLineBlock = AIController.CreateCube(num, EnemyHandCardLine);
        yield return 0;
        AIController.AfterDelete();
        yield return StartCoroutine(AIController.DrawCard(num, EnemyHandLineBlock));
        while (AIController.Queue.Count != 0)
        {
            yield return StartCoroutine(AIController.Queue.Dequeue());
        }
        StartCoroutine(AIController.AIStart());
    }

    public IEnumerator AIChange(int num)
    { 
        List<GameObject> EnemyHandLineBlock = AIController.CreateCube(num, EnemyHandCardLine);
        yield return 0;
        AIController.AfterDelete();
        yield return null;
        yield return StartCoroutine(AIController.DrawCard(num, EnemyHandLineBlock));
        while (AIController.Queue.Count != 0)
        {
            yield return StartCoroutine(AIController.Queue.Dequeue());
        }
    }

    public void FirstTurnCantAttack()
    {
        foreach (var item in PlayerPersonBlock)
        {
            item.GetComponent<PersonBlock>().AttackCount = 0;
        }

        foreach (var item in EnemyPeronBlock)
        {
            item.GetComponent<PersonBlock>().AttackCount = 0;
        }
    }

    public void ResetAttckCount()
    {
        foreach (var item in PlayerPersonBlock)
        {
            item.GetComponent<PersonBlock>().AttackCount = 1;
        }

        foreach (var item in EnemyPeronBlock)
        {
            item.GetComponent<PersonBlock>().AttackCount = 1;
        }
    }

    public void NextTurn()
    {
        changed = false;
        ResetPlayerSlodierInCity();
        StartCoroutine(NextTurnIE());

    }

    public IEnumerator NextTurnIE()
    {
        roundstate = RoundState.DrawCard;
        int num = 0;
        if (player.PlayerSetList.Count - 2 >= 0)
        {
            num = 2;
        }
        else if(player.PlayerSetList.Count-2==-1)
        {
            num = 1;
        }
        else if(player.PlayerSetList.Count==0)
        {
            num = 0;
        }
        List<GameObject> list = CreateCube(num, HandCardLine);
        yield return 0;
        StartCoroutine(MoveAfterDelete());
        yield return 0;
        DrawCard(num, list);

    }
    public void Load()
    {
        //string[] CardData = File.ReadAllText(Application.persistentDataPath + "/CardSet.csv").Split('\n');
        string[] playercardset;
        if (File.Exists(Application.persistentDataPath + "/CardSet.csv"))
        {
            playercardset=File.ReadAllText(Application.persistentDataPath + "/CardSet.csv").Split('\n');
        }
        else
        {
            Debug.Log(Application.dataPath);
            playercardset=File.ReadAllText(Application.streamingAssetsPath + "/DefaultCardSet.csv").Split('\n');
        }
        string enemycardset;
        if (File.Exists(Application.persistentDataPath + "/EnemyCardSet.csv"))
        {
            enemycardset=File.ReadAllText(Application.persistentDataPath + "/EnemyCardSet.csv");
        }
        else
        {
            Debug.Log(Application.dataPath);
            enemycardset=File.ReadAllText(Application.streamingAssetsPath + "/DefaultCardSet.csv");
        }
        string[] CardData = CardAsset.text.Split('\n');
        //string[] playercardset = PlayerCardSet.text.Split('\n');
        foreach (var item in playercardset)
        {
            if (item.Length != 0)
            {
                string[] SingleCard = item.Split(',');
                player.PlayerCardSetDict.Add(Int32.Parse(SingleCard[0]), Int32.Parse(SingleCard[1]));
            }
        }

        for (int i = 0; i < player.PlayerCardSetDict[0]; i++)
        {
            SoldierCard soldiercard = new SoldierCard(-i, "", "士兵卡");
            player.PlayerSetList.Add(soldiercard);
        }

        foreach (var item in CardData)
        {
            if (item.Length == 0 || item[0] == '#') continue;
            string[] SingleCard = item.Split(',');
            if (player.PlayerCardSetDict.ContainsKey(Int32.Parse(SingleCard[1])))
            {
                switch (SingleCard[0])
                {
                    case "City":
                        CityCard cityCard = new CityCard(Int32.Parse(SingleCard[1]), SingleCard[3], SingleCard[2]);
                        player.PlayerCityList.Add(cityCard);
                        break;
                    case "CPerson":
                        Civil_Officials civilOfficials = new Civil_Officials(Int32.Parse(SingleCard[1]), SingleCard[3],
                            SingleCard[2], SingleCard[4], SingleCard[5], Int32.Parse(SingleCard[6]),
                            Setting.CPersonATKBuff[0]);
                        player.PlayerSetList.Add(civilOfficials);
                        break;
                    case "GPerson":
                        General general = new General(Int32.Parse(SingleCard[1]), SingleCard[3],
                            SingleCard[2], SingleCard[4], SingleCard[5], Int32.Parse(SingleCard[6]),
                            Setting.GPersonLeadNumber[0]);
                        player.PlayerSetList.Add(general);
                        break;
                    case "StoryCard":
                        StoryCard storyCard = new StoryCard(Int32.Parse(SingleCard[1]), SingleCard[3],
                            SingleCard[2], SingleCard[4], SingleCard[5]);
                        player.PlayerSetList.Add(storyCard);
                        Debug.Log("StoryCard:"+storyCard.MatchPerson);
                        break;
                    default:
                        break;
                }
            }
        }
        ShuffleCardSet(player.PlayerSetList);
        AIController.LoadEnemyCardSet(CardData, enemycardset);
        CityShow();
    }

    public void CityShow()
    {
        Vector3 scale = new Vector3();
        for (int i = 0; i < player.PlayerCityList.Count; i++)
        {
            GameObject go = GameObject.Instantiate(CityPrefab, PlayerCityBlock[i].transform);
            go.GetComponent<CityCardLibraryDisplay>()._card = player.PlayerCityList[i] as CityCard;
            go.GetComponent<CityCardLibraryDisplay>().parent = PlayerCityBlock[i];
            go.GetComponent<CityCardLibraryDisplay>().Show();
            go.GetComponent<CityCardLibraryDisplay>().Ontable = true;
            go.GetComponent<CityCardLibraryDisplay>().Description.text = "驻守人数：0";
            go.GetComponent<CityCardLibraryDisplay>().Description.fontSize = 28;
            go.GetComponent<Image>().raycastTarget = false;
            go.transform.localPosition = new Vector3(0, 0, 0);
            if (i == 0)
            {
                scale = new Vector3(
                    PlayerCityZone.GetComponent<GridLayoutGroup>().cellSize.x /
                    go.GetComponent<RectTransform>().sizeDelta.x,
                    PlayerCityZone.GetComponent<GridLayoutGroup>().cellSize.y /
                    go.GetComponent<RectTransform>().sizeDelta.y, 0);

            }

            go.GetComponent<RectTransform>().localScale = scale;
            PlayerCityBlock[i].GetComponent<CityBlock>().cardGO = go;

        }

        for (int i = 0; i < enemy.PlayerCityList.Count; i++)
        {
            GameObject go = GameObject.Instantiate(CityPrefab, EnemyCityBlock[i].transform);
            go.GetComponent<CityCardLibraryDisplay>()._card = enemy.PlayerCityList[i] as CityCard;
            go.GetComponent<CityCardLibraryDisplay>().parent = EnemyCityBlock[i];
            go.GetComponent<CityCardLibraryDisplay>().Show();
            go.GetComponent<CityCardLibraryDisplay>().Ontable = true;
            go.GetComponent<CityCardLibraryDisplay>().Description.text = "驻守人数：0";
            go.GetComponent<CityCardLibraryDisplay>().Description.fontSize = 28;
            go.GetComponent<Image>().raycastTarget = false;
            go.transform.localPosition = new Vector3(0, 0, 0);
            if (i == 0)
            {
                scale = new Vector3(
                    PlayerCityZone.GetComponent<GridLayoutGroup>().cellSize.x /
                    go.GetComponent<RectTransform>().sizeDelta.x,
                    PlayerCityZone.GetComponent<GridLayoutGroup>().cellSize.y /
                    go.GetComponent<RectTransform>().sizeDelta.y, 0);

            }

            go.GetComponent<RectTransform>().localScale = scale;
            EnemyCityBlock[i].GetComponent<CityBlock>().cardGO = go;
        }

    }

    public void ShuffleCardSet<T>(List<T> list)
    {
        int CurIndex;
        T Temp;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            CurIndex = Random.Range(0, i + 1);
            Temp = list[CurIndex];
            list[CurIndex] = list[i];
            list[i] = Temp;
        }
    }

    public void DrawCard(int Num, List<GameObject> list)
    {
        for (int i = 1; i <= Num; i++)
        {
            if (player.PlayerSetList.Count > 0)
            {
                GameObject CurCard = GetCard(player.PlayerSetList[player.PlayerSetList.Count - 1]);
                player.PlayerSetList.RemoveAt(player.PlayerSetList.Count - 1);
                CurCard.GetComponent<HandCardDisplay>().owner = 0;
                CurCard.GetComponent<HandCardDisplay>().Show();
                //CurCard.transform.SetParent(Canvas.transform);
                CurCard.transform.SetParent(HandCardPoint.transform);
                CurCard.GetComponent<HandCardDisplay>().index = CardIndex;
                CurCard.SetActive(false);
                CurCard.transform.position = DrawCardStartPoint.transform.position;
                if (player.PlayerHandCard.Count == MaxHandCardNum)
                {

                }
                else
                {
                    CurCard.GetComponent<HandCardDisplay>().parent = list[i - 1];
                    IEStack.Enqueue(CurCard.GetComponent<HandCardDisplay>().CardDraw(list[i - 1]));
                    player.PlayerHandCard.Add(CurCard.GetComponent<HandCardDisplay>()._card.ID, CurCard);
                    CurCard.GetComponent<HandCardDisplay>().index = CardIndex;
                    CurCard.GetComponent<HandCardDisplay>().ShowIndex = CardIndex;
                    CardIndex++;
                }
            }
            else break;
        }
        UpdateCardUIIndex();
        StartCoroutine(StartDraw());
    }

    public GameObject GetCard(Card card)
    {
        if (CardPool.Count != 0)
        {
            if (CardPool.Peek())
            {
                CardPool.Peek().GetComponent<HandCardDisplay>()._card = card;
                CardPool.Peek().transform.position = transform.position;
                CardPool.Peek().SetActive(true);
                return CardPool.Pop();
            }
            else
            {
                GameObject GO = GameObject.Instantiate(HandCardPrefab);
                GO.GetComponent<HandCardDisplay>()._card = card;
                return GO;
            }
        }
        else
        {
            GameObject GO = GameObject.Instantiate(HandCardPrefab);
            GO.GetComponent<HandCardDisplay>()._card = card;
            return GO;
        }
    }

    public void DeleteCard(GameObject go)
    {
        
        go.GetComponent<HandCardDisplay>().isselected = false;
        go.SetActive(false);
        go.transform.SetParent(transform);
        go.transform.position = transform.position;
        CardPool.Push(go);

    }

    public GameObject GetGO()
    {
        Debug.Log(GOPool.Count);
        if (GOPool.Count != 0)
        {
            GameObject GO = GOPool.Pop();
            //if (!GO)
            //{ 
              //  GO = GameObject.Instantiate(HandCardPointPrefab);
            //}
            GO.SetActive(true);
            return GO;
        }
        else
        {
            GameObject GO = GameObject.Instantiate(HandCardPointPrefab);
            return GO;
        }

    }

    public void DeleteGO(GameObject go)
    {
        Debug.Log("deletego");
        go.SetActive(false);
        go.transform.SetParent(transform);
        GOPool.Push(go);
    }

    public IEnumerator StartDraw()
    {
        yield return 0;
        while (IEStack.Count != 0)
        {
            yield return StartCoroutine(IEStack.Dequeue());
        }
        
        if (!(roundstate == RoundState.Battle)&&changed==false)
        {
            roundstate = RoundState.ChangeCard;
            TurnButton.GetComponent<RoundButtonScript>().ImageChange();
        }
        else
        {
            roundstate = RoundState.Battle;
        }


    }

    public List<GameObject> CreateCube(int num, GameObject HandCardLine)
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < num; i++)
        {
            GameObject go = GetGO();
            go.transform.SetParent(HandCardLine.transform);
            list.Add(go);
        }

        return list;
    }

    public IEnumerator DrawCardIE(int num, List<GameObject> list)
    {
        Debug.Log(num);
        DrawCard(num, list);
        yield return 0;
    }

    public void TurnEnd()
    {
        if (roundstate == RoundState.ChangeCard)
        {
            Debug.Log("换卡结束");
            changed = true;
            StartCoroutine(DeleteAndDraw());
            TurnButton.GetComponent<RoundButtonScript>().ImageChange();
            
        }
        else if (roundstate == RoundState.Battle)
        {
            roundstate = RoundState.Defense;
            Debug.Log("攻击结束");
            TurnButton.GetComponent<RoundButtonScript>().ImageChange();
        }
        else if (roundstate == RoundState.Defense)
        {
            TurnButton.GetComponent<RoundButtonScript>().ImageChange();
            roundstate = RoundState.Enemy;
            Debug.Log("敌方回合");
            if (TurnNum == 0)
            {
                StartCoroutine(AIController.AIStart());
            }
            else
            {
                StartCoroutine(AIDrawAndStart());
            }
        }
    }

    public IEnumerator DeleteSelectedCard()
    {
        selectednum = 0;
        List<int> list = new List<int>();
        foreach (var item in player.PlayerHandCard)
        {
            if (item.Value.GetComponent<HandCardDisplay>().isselected)
            {
                list.Add(item.Key);
                selectednum++;
                DeleteGO(item.Value.GetComponent<HandCardDisplay>().parent);
                StartCoroutine(item.Value.GetComponent<HandCardDisplay>().SelectedDestroy());
            }

        }

        foreach (var item in list)
        {
            player.PlayerHandCard.Remove(item);
        }

        yield return 0;

    }

    public IEnumerator DeleteAndDraw()
    {
        yield return StartCoroutine(DeleteSelectedCard());
        int num = 0;
        if (player.PlayerSetList.Count == 0){num = 0;}
        else
        {
            
            num = player.PlayerSetList.Count - selectednum >= 0
                ? selectednum
                : selectednum - player.PlayerSetList.Count;
        }
        roundstate = RoundState.DrawCard;
        
        List<GameObject> list = CreateCube(num, HandCardLine);
        yield return 0;
        StartCoroutine(MoveAfterDelete());
        yield return 0;
        Debug.Log(num);
        yield return StartCoroutine(DrawCardIE(num, list));
        //roundstate = RoundState.Battle;

    }

    public IEnumerator MoveAfterDelete()
    {
        yield return 0;
        foreach (var item in player.PlayerHandCard)
        {
            item.Value.transform.position = item.Value.GetComponent<HandCardDisplay>().parent.transform.position;
        }
    }

    public void PutPersonCard(GameObject block, GameObject Card)
    {
        Card.transform.SetParent(block.transform);
        block.GetComponent<PersonBlock>().cardGO = Card;
        Card.transform.position = block.transform.position;
        Card.GetComponent<HandCardDisplay>().cardstate = CardState.OnTable;
        Card.GetComponent<Image>().raycastTarget = false;
        StartCoroutine(Card.GetComponent<HandCardDisplay>().ScaleToBlock());
    }

    public bool CheckCondition(GameObject block, GameObject Card, out GameObject MatchCard) //判断城池是否在手并且是否有格子,判断场上是否有该人物
    {
        Debug.Log("check");
        Card curcard = Card.GetComponent<HandCardDisplay>()._card;
        if (curcard is PersonCard)
        {
            MatchCard = null;
            return CheckCity(curcard as PersonCard) && CheckBlock(block);
        }
        else if (curcard is StoryCard)
        {
            return CheckPerson(curcard as StoryCard, out MatchCard);
        }

        MatchCard = null;
        return false;
    }

    public bool CheckPerson(StoryCard card, out GameObject MatchCard)
    {
        Debug.Log(card.MatchPerson);
        string[] matchpersons;
        if (card.MatchPerson[0] != '"')
        {
            matchpersons = new string[] { card.MatchPerson };
        }
        else{matchpersons= card.MatchPerson.Split('"')[1].Split('，');}
        foreach (var VARIABLE in matchpersons)
        {
            Debug.Log(VARIABLE);
            foreach (var item in player.PlayerOnTableCard)
            {
                if (item.GetComponent<HandCardDisplay>()._card.Name == VARIABLE)
                {
                    MatchCard = item;
                    return true;
                }
            }
        }
        MatchCard = null;
        return false;
    }

    public bool CheckCity(PersonCard card)
    {
        for (int i = 0; i < PlayerCityBlock.Count; i++)
        {
            if (PlayerCityBlock[i].GetComponent<CityBlock>().cardGO.GetComponent<CityCardLibraryDisplay>()._card.Name ==
                card.Area) return true;
        }

        return false;

    }

    public bool CheckBlock(GameObject go)
    {
        if (go.GetComponent<PersonBlock>().cardGO == null)
        {
            return true;
        }
        else return false;
    }

    public void AddPersonStar(GameObject Card)
    {
        if (Card.GetComponent<HandCardDisplay>()._card is Civil_Officials)
        {
            player.PlayterMoraleSum -=
                Setting.CPersonATKBuff[Int32.Parse(Card.GetComponent<HandCardDisplay>().starsnum.text)];
            Card.GetComponent<HandCardDisplay>().starsnum.text =
                (Int32.Parse(Card.GetComponent<HandCardDisplay>().starsnum.text) + 1).ToString();
            player.PlayterMoraleSum +=
                Setting.CPersonATKBuff[Int32.Parse(Card.GetComponent<HandCardDisplay>().starsnum.text)];
            UpdatePlayterMorale();
        }
        else if (Card.GetComponent<HandCardDisplay>()._card is General)
        {
            Card.GetComponent<HandCardDisplay>().starsnum.text =
                (Int32.Parse(Card.GetComponent<HandCardDisplay>().starsnum.text) + 1).ToString();
            Card.GetComponent<HandCardDisplay>().value.text =
                Setting.GPersonLeadNumber[Int32.Parse(Card.GetComponent<HandCardDisplay>().starsnum.text)].ToString();
        }
    }

    public void DeleteCardFromHand(GameObject HandCard)
    {
        player.PlayerHandCard.Remove(HandCard.GetComponent<HandCardDisplay>()._card.ID);
        player.PlayerOnTableCard.Add(HandCard);
        DeleteGO(HandCard.GetComponent<HandCardDisplay>().parent);
        if (HandCard.GetComponent<HandCardDisplay>()._card is StoryCard ||
            HandCard.GetComponent<HandCardDisplay>()._card is SoldierCard)
        {
            DeleteCard(HandCard);
        }

        StartCoroutine(MoveAfterDelete());
    }

    public void UpdatePlayterMorale()
    {
        PlayterMorale.text = "士气加成：" + player.PlayterMoraleSum.ToString();

    }

    public void UpdateCardNum()
    {
        PlayerCardNumText.text = "剩余卡牌：" + player.PlayerSetList.Count.ToString();
    }

    public bool CheckCardPos(GameObject Card) //判断是否出牌
    {
        return CheckCardPosX(Card) && CheckCardPosY(Card);
    }

    public bool CheckCardPosX(GameObject Card) //判断是否出牌
    {
        Debug.Log(Card.transform.position.x);
        Debug.Log("PlayerCardZone.transform.position.x" + (PlayerCardZone.transform.position.x -
                                                           PlayerCardZone.GetComponent<RectTransform>().sizeDelta.x / 2)
            .ToString());
        Debug.Log("PlayerCardZone.GetComponent<RectTransform>().sizeDelta.x" + (PlayerCardZone.transform.position.x +
            PlayerCardZone.GetComponent<RectTransform>().sizeDelta.x / 2).ToString());
        if (Card.transform.position.x >= PlayerCardZone.transform.position.x -
            PlayerCardZone.GetComponent<RectTransform>().sizeDelta.x / 2 && Card.transform.position.x <=
            PlayerCardZone.transform.position.x + PlayerCardZone.GetComponent<RectTransform>().sizeDelta.x / 2)
        {
            return true;
        }
        else return false;
    }

    public bool CheckCardPosY(GameObject Card) //判断是否出牌
    {
        Debug.Log(Card.transform.position.y);
        Debug.Log(PlayerCardZone.transform.position.y -
                  PlayerCardZone.GetComponent<RectTransform>().sizeDelta.y / 2);
        Debug.Log(PlayerCityZone.transform.position.y + PlayerCityZone.GetComponent<RectTransform>().sizeDelta.y / 2);
        if (Card.transform.position.y >= PlayerCardZone.transform.position.y -
            PlayerCardZone.GetComponent<RectTransform>().sizeDelta.y / 2 && Card.transform.position.y <=
            PlayerCityZone.transform.position.y + PlayerCityZone.GetComponent<RectTransform>().sizeDelta.y / 2)
        {
            return true;
        }
        else return false;
    }

    public void AddSlodierNum(float num)
    {
        player.PlayerSlodierNum += num;
        player.CanUsePlayerSlodierNum = player.PlayerSlodierNum;
        UpdatePlayerSlodierNum();
    }

    public void UpdatePlayerSlodierNum()
    {
        PlayerSlodierNum.text = "驻守士兵数量：" + player.SlodierInCityNum + "/士兵总量:" + player.PlayerSlodierNum.ToString();
        PlayerCanUseSlodierNum.text = "可用进攻：" + player.CanUsePlayerSlodierNum;
    }

    public void AttackYes()
    {
        if (roundstate != RoundState.Choosing) return;
        if (ChoosingBlock.GetComponent<PersonBlock>().AttackCount == 0) return;
        if (AttackInput.text == "") return;
        if (player.CanUsePlayerSlodierNum > 0)
        {
            int UseSlodierNum = Int32.Parse(AttackInput.text);
            if (UseSlodierNum > Setting.GPersonLeadNumber[
                    Int32.Parse(ChoosingBlock.GetComponent<PersonBlock>().cardGO.GetComponent<HandCardDisplay>()
                        .starsnum
                        .text)]) return;
            if (player.CanUsePlayerSlodierNum - UseSlodierNum >= 0)
            {
                float playerATK;
                float enemyATK;
                float gapATK = 0;
                int playerloss = 0;
                int enemyloss = 0;
                playerATK = UseSlodierNum * (1 + player.PlayterMoraleSum);
                enemyATK = TargetBlock.GetComponent<CityBlock>().SlodierNum * (1 + enemy.PlayterMoraleSum);
                Debug.Log("playerATK" + playerATK);
                Debug.Log("enemyATK" + enemyATK);
                if (playerATK > enemyATK)
                {
                    playerloss = Convert.ToInt32(enemyATK / (1 + player.PlayterMoraleSum) > UseSlodierNum
                        ? UseSlodierNum
                        : enemyATK / (1 + player.PlayterMoraleSum));
                    enemyloss = Convert.ToInt32(TargetBlock.GetComponent<CityBlock>().SlodierNum);
                    TakeCity(TargetBlock.GetComponent<CityBlock>().cardGO, 0);
                }
                else if (playerATK < enemyATK)
                {
                    playerloss = Int32.Parse(AttackInput.text);
                    enemyloss = Convert.ToInt32(
                        playerATK / (1 + enemy.PlayterMoraleSum) > TargetBlock.GetComponent<CityBlock>().SlodierNum
                            ? TargetBlock.GetComponent<CityBlock>().SlodierNum
                            : playerATK / (1 + enemy.PlayterMoraleSum));
                    TargetBlock.GetComponent<CityBlock>().SlodierLoss(enemyloss);
                }
                else
                {
                    playerloss = Convert.ToInt32(enemyATK / (1 + player.PlayterMoraleSum) > UseSlodierNum
                        ? UseSlodierNum
                        : enemyATK / (1 + player.PlayterMoraleSum));
                    enemyloss =
                        Convert.ToInt32(playerATK / (1 + enemy.PlayterMoraleSum) >
                                        TargetBlock.GetComponent<CityBlock>().SlodierNum
                            ? TargetBlock.GetComponent<CityBlock>().SlodierNum
                            : playerATK / (1 + enemy.PlayterMoraleSum));
                    TargetBlock.GetComponent<CityBlock>().SlodierLoss(enemyloss);
                }
                Debug.Log("playerloss" + playerloss);
                Debug.Log("enemyloss" + enemyloss);
                player.PlayerSlodierNum -= playerloss;
                enemy.PlayerSlodierNum -= enemyloss;
                enemy.SlodierInCityNum -= enemyloss;
                player.CanUsePlayerSlodierNum -= UseSlodierNum;
                //enemy.CanUsePlayerSlodierNum -= enemyloss;
                ChoosingBlock.GetComponent<PersonBlock>().AttackCount--;
                AttackConfirm.SetActive(false);
                roundstate = RoundState.Battle;
                ChoosingBlock = null;
                TargetBlock = null;
                UpdatePlayerSlodierNum();
                AIController.UpdateSlodierNum();
            }
        }
    }

    public void AttackNo()
    {
        AttackConfirm.SetActive(false);
        roundstate = RoundState.Battle;
        ChoosingBlock = null;
        TargetBlock = null;
    }

    public void DefenceSlodierConfirmYes()
    {
        if (TargetBlock == null) return;
        if (player.PlayerSlodierNum - player.SlodierInCityNum - Int32.Parse(DefenceSlodierInput.text) >= 0)
        {
            TargetBlock.GetComponent<CityBlock>().SlodierNum += Int32.Parse(DefenceSlodierInput.text);
            TargetBlock.GetComponent<CityBlock>().cardGO.GetComponent<CityCardLibraryDisplay>().Description.text =
                "驻守人数:" + TargetBlock.GetComponent<CityBlock>().SlodierNum;
            player.SlodierInCityNum += Int32.Parse(DefenceSlodierInput.text);
            UpdatePlayerSlodierNum();
            TargetBlock = null;
            DefenceSlodierConfirm.SetActive(false);
            roundstate = RoundState.Defense;
        }
    }

    public void DefenceSlodierConfirmNo()
    {
        DefenceSlodierConfirm.SetActive(false);
        roundstate = RoundState.Defense;
    }

    public void TakeCity(GameObject city, int ToWho) //0=player,1=enemy
    {
        GameObject ToBlock = city.GetComponent<CityCardLibraryDisplay>().parent;
        Vector3 StartPos = city.transform.position;
        ToBlock.GetComponent<CityBlock>().ResetSlodierNum();
        if (ToWho == 0)
        {
            enemy.PlayerCityList.Remove(city.GetComponent<CityCardLibraryDisplay>()._card);
            EnemyCityBlock.Remove(city.GetComponent<CityCardLibraryDisplay>().parent);
            ToBlock.transform.SetParent(PlayerCityZone.transform);
            ToBlock.GetComponent<CityBlock>().owner = Owner.Player;
            PlayerCityBlock.Add(ToBlock);
            player.PlayerCityList.Add(city.GetComponent<CityCardLibraryDisplay>()._card);
        }
        else if (ToWho == 1)
        {
            player.PlayerCityList.Remove(city.GetComponent<CityCardLibraryDisplay>()._card);
            PlayerCityBlock.Remove(city.GetComponent<CityCardLibraryDisplay>().parent);
            ToBlock.transform.SetParent(EnemyCityZone.transform);
            ToBlock.GetComponent<CityBlock>().owner = Owner.Enemy;
            EnemyCityBlock.Add(ToBlock);
            enemy.PlayerCityList.Add(city.GetComponent<CityCardLibraryDisplay>()._card);
        }
        StartCoroutine(city.GetComponent<CityCardLibraryDisplay>().ToParent(StartPos));


    }

    public void ResetCityPos(int Who, GameObject Ignore)
    {
        List<GameObject> list = new List<GameObject>();
        if (Who == 0)
        {
            list = PlayerCityBlock;
        }
        else if (Who == 1)
        {
            list = EnemyCityBlock;
        }

        foreach (var item in list)
        {
            if (item.GetComponent<CityBlock>().cardGO != Ignore)
            {
                item.GetComponent<CityBlock>().cardGO.transform.position = item.GetComponent<CityBlock>().cardGO
                    .GetComponent<CityCardLibraryDisplay>().parent.transform.position;
            }
        }
    }

    public void ResetPlayerSlodierInCity()
    {
        foreach (var item in PlayerCityBlock)
        {
            item.GetComponent<CityBlock>().ResetSlodierNum();
        }

        player.SlodierInCityNum = 0;
        player.CanUsePlayerSlodierNum = player.PlayerSlodierNum;
        UpdatePlayerSlodierNum();
    }

    public void CheckVictory()
    {
        if (EnemyCityBlock.Count ==0)
        {
            roundstate = RoundState.End;
            GameOver.SetActive(true);
            Sprite sp = Resources.Load<Sprite>("other/ButtonImage/Win");
            GameOver.GetComponent<Image>().sprite = sp;
            //GameOverText.text = "你赢了";
            StopAllCoroutines();
        }
        if (PlayerCityBlock.Count == 0)
        {
            roundstate = RoundState.End;
            GameOver.SetActive(true);
            Sprite sp = Resources.Load<Sprite>("other/ButtonImage/Lose");
            GameOver.GetComponent<Image>().sprite = sp;
            //GameOverText.text = "你输了";
            StopAllCoroutines();
        }
        
    }

    public void GameOverButton()
    {
        GOPool.Clear();
        CardPool.Clear();
        StopCoroutine(AIController.AIStart());
        SceneManager.LoadScene("Scenes/Menu");
    }

    public void UpdateCardUIIndex()
    {
        foreach (var VARIABLE in player.PlayerHandCard)
        {
            VARIABLE.Value.GetComponent<HandCardDisplay>().UpdateSelfIndex();
            VARIABLE.Value.transform.SetSiblingIndex(VARIABLE.Value.GetComponent<HandCardDisplay>().ShowIndex);
        }
        
    }
}
