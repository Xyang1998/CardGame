using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
public class AIController : MonoBehaviour
{
    public Player enemy;
    private int MaxHandCardNum = 10;
    private BattleEventSystem BattleEventSystem;
    private int RandomSlodierNum=0;//当最高星武将带领最大人数也无法攻下城池时，保存一定兵力。
    private List<GameObject> Attacklist;
    public Queue<IEnumerator> Queue=new Queue<IEnumerator>();
    // Start is called before the first frame update
    private void Awake()
    {
        
        BattleEventSystem = Setting.GetBattleEventSystem();
        StopAllCoroutines();
    }

    void Start()
    {
        
        
    }

    private void OnDestroy()
    {
        Attacklist.Clear();
        Queue.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool CheckCity(PersonCard card)
    {
        for (int i = 0; i < BattleEventSystem.EnemyCityBlock.Count; i++)
        {
            if (BattleEventSystem.EnemyCityBlock[i].GetComponent<CityBlock>().cardGO.GetComponent<CityCardLibraryDisplay>()._card.Name == card.Area) return true;
        }

        return false;
    }
    public bool CheckPerson(StoryCard card)
    {
        Debug.Log(card.MatchPerson);
        string[] matchpersons;
        if (card.MatchPerson[0] != '"')
        {
            matchpersons = new string[] { card.MatchPerson };
        }
        else{ matchpersons= card.MatchPerson.Split('"')[1].Split('，');}
        foreach (var VARIABLE in matchpersons)
        {
            foreach (var item in enemy.PlayerOnTableCard)
            {
                if (item.GetComponent<HandCardDisplay>()._card.Name == VARIABLE)
                {
                            return true;
                }
            }
        }
        
        return false;
    }
    public bool CheckCanUseBlock()
    {
        if (enemy.PlayerOnTableCard.Count >= 6)
        {
            return false;
            
        }
        else
        {
            return true;
        }
    }
    public void Defence()
    {
        Debug.Log("防御开始");
        List<int> NumList = new List<int>();
        int PlayerMaxLeaderNum=0;
        int CurUseNum = 0;
        int num=0;
        foreach (var item in BattleEventSystem.player.PlayerOnTableCard)
        {
            if (item.GetComponent<HandCardDisplay>()._card is General)
            {
                int temp = Setting.GPersonLeadNumber[Int32.Parse(item.GetComponent<HandCardDisplay>().starsnum.text)];
                if (temp >
                    PlayerMaxLeaderNum)
                {
                    PlayerMaxLeaderNum = temp;
                }
            }
        }
        int MinCanDefenceNum =
            Convert.ToInt32((PlayerMaxLeaderNum*(1 + BattleEventSystem.player.PlayterMoraleSum))*BattleEventSystem.EnemyCityBlock.Count/(1+enemy.PlayterMoraleSum));
        if (MinCanDefenceNum <= enemy.PlayerSlodierNum)
        {
            for (int i = 0; i < BattleEventSystem.EnemyCityBlock.Count; i++)
            {
                int temp = Convert.ToInt32(PlayerMaxLeaderNum * (1 + BattleEventSystem.player.PlayterMoraleSum) /
                                           (1 + enemy.PlayterMoraleSum));
                temp = temp == 0 ? 1 : temp;
                if (CurUseNum + temp <= enemy.PlayerSlodierNum)
                {
                    CurUseNum += temp;
                    Debug.Log(temp);
                    NumList.Add(temp);
                }
            }
            if (CurUseNum < enemy.PlayerSlodierNum)
            {
                num =Convert.ToInt32(enemy.PlayerSlodierNum - CurUseNum);
                int index = Random.Range(0, BattleEventSystem.EnemyCityBlock.Count + 1);
                if (index < NumList.Count)
                {
                    NumList[index] += num;
                }
            }
        }
        else
        {
            for (int i = 0; i < BattleEventSystem.EnemyCityBlock.Count; i++)
            {
                num = Convert.ToInt32(enemy.PlayerSlodierNum / BattleEventSystem.EnemyCityBlock.Count);
                CurUseNum += num;
                if (CurUseNum <= enemy.PlayerSlodierNum)
                {
                    NumList.Add(num);
                }
            }

            if (CurUseNum < enemy.PlayerSlodierNum)
            {
                num =Convert.ToInt32(enemy.PlayerSlodierNum - CurUseNum);
                int index = Random.Range(0, BattleEventSystem.EnemyCityBlock.Count + 1);
                if (index < NumList.Count)
                {
                    NumList[index] += num;
                }
            }
        }
        Debug.Log("敌方士兵总数："+enemy.PlayerSlodierNum);
        foreach (var VARIABLE in NumList)
        {
            Debug.Log("Numlist:"+VARIABLE);
        }

        int Index=0;
        foreach (var city in BattleEventSystem.EnemyCityBlock)
        {
            if (Index <= BattleEventSystem.EnemyCityBlock.Count - 1)
            { 
                if (Index < NumList.Count)
                {
                    city.GetComponent<CityBlock>().SlodierNum = NumList[Index];
                    city.GetComponent<CityBlock>().cardGO.GetComponent<CityCardLibraryDisplay>().Description.text="驻守人数:"+ city.GetComponent<CityBlock>().SlodierNum;
                    enemy.SlodierInCityNum+= NumList[Index];
                    Index++;
                }
                
            }
        }
        Debug.Log("敌方驻守数量"+enemy.SlodierInCityNum);
        UpdateSlodierNum();
    }
    public void LoadEnemyCardSet(string[] CardData, string EnemyCardSet)
    {
        string[] playercardset = EnemyCardSet.Split('\n');
        foreach (var item in playercardset)
        {
            if (item.Length != 0)
            {
                string[] SingleCard = item.Split(',');
                enemy.PlayerCardSetDict.Add(Int32.Parse(SingleCard[0]), Int32.Parse(SingleCard[1]));
            }
        }

        for (int i = 0; i < enemy.PlayerCardSetDict[0]; i++)
        {
            SoldierCard soldiercard = new SoldierCard(-i, "", "士兵卡");
            enemy.PlayerSetList.Add(soldiercard);
        }

        foreach (var item in CardData)
        {
            if (item.Length == 0 || item[0] == '#') continue;
            string[] SingleCard = item.Split(',');
            if (enemy.PlayerCardSetDict.ContainsKey(Int32.Parse(SingleCard[1])))
            {
                switch (SingleCard[0])
                {
                    case "City":
                        CityCard cityCard = new CityCard(Int32.Parse(SingleCard[1]), SingleCard[3], SingleCard[2]);
                        enemy.PlayerCityList.Add(cityCard);
                        break;
                    case "CPerson":
                        Civil_Officials civilOfficials = new Civil_Officials(Int32.Parse(SingleCard[1]), SingleCard[3],
                            SingleCard[2], SingleCard[4], SingleCard[5], Int32.Parse(SingleCard[6]),
                            Setting.CPersonATKBuff[0]);
                        enemy.PlayerSetList.Add(civilOfficials);
                        break;
                    case "GPerson":
                        General general = new General(Int32.Parse(SingleCard[1]), SingleCard[3],
                            SingleCard[2], SingleCard[4], SingleCard[5], Int32.Parse(SingleCard[6]),
                            Setting.GPersonLeadNumber[0]);
                        enemy.PlayerSetList.Add(general);
                        break;
                    case "StoryCard":
                        StoryCard storyCard = new StoryCard(Int32.Parse(SingleCard[1]), SingleCard[3],
                            SingleCard[2], SingleCard[4], SingleCard[5]);
                        enemy.PlayerSetList.Add(storyCard);
                        break;
                    default:
                        break;
                }
            }
        }

        Setting.GetBattleEventSystem().ShuffleCardSet(enemy.PlayerSetList);
    }
    public List<GameObject> CreateCube(int num,GameObject HandLine)
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < num; i++)
        {  
            GameObject go = BattleEventSystem.GetGO();
            go.transform.SetParent(HandLine.transform);
            list.Add(go);
        }
        return list;
    }
    public IEnumerator DrawCard(int num,List<GameObject> list)
    {
        for(int i=1;i<=num;i++)
        {
            if (enemy.PlayerSetList.Count > 0)
            {
                GameObject CurCard = BattleEventSystem.GetCard(enemy.PlayerSetList[enemy.PlayerSetList.Count - 1]);
                enemy.PlayerSetList.RemoveAt(enemy.PlayerSetList.Count - 1);
                CurCard.GetComponent<HandCardDisplay>().owner = 1;
                CurCard.GetComponent<HandCardDisplay>().Show();
                if (!BattleEventSystem)
                {
                    Debug.Log("BattleEventSystem MISS");
                }
                CurCard.transform.SetParent(BattleEventSystem.Canvas.transform);
                if (enemy.PlayerHandCard.Count == MaxHandCardNum)
                {
                   
                }
                else
                {
                    CurCard.GetComponent<HandCardDisplay>().parent = list[i - 1];
                    CurCard.transform.position = BattleEventSystem.EnemyHandCardStartPoint.transform.position;
                    Queue.Enqueue(CurCard.GetComponent<HandCardDisplay>().CardDraw(list[i - 1]));
                    enemy.PlayerHandCard.Add(CurCard.GetComponent<HandCardDisplay>()._card.ID,CurCard);
                } 
            }
            else break;
        }
        UpdateCardNum();
        yield return null;
        
    }
    public GameObject CheckCard()
    {
        foreach (var item in enemy.PlayerHandCard)
        {
            if (item.Value.GetComponent<HandCardDisplay>()._card is SoldierCard)
            {
                return item.Value;
            }

            if (item.Value.GetComponent<HandCardDisplay>()._card is Civil_Officials||item.Value.GetComponent<HandCardDisplay>()._card is General)
            {
                if (CheckCity(item.Value.GetComponent<HandCardDisplay>()._card as PersonCard)&&CheckCanUseBlock())
                {
                    return item.Value;
                }
            }
        }

        foreach (var item in enemy.PlayerHandCard)
        {
            if (item.Value.GetComponent<HandCardDisplay>()._card is StoryCard)
            {
                if (CheckPerson(item.Value.GetComponent<HandCardDisplay>()._card as StoryCard))
                {
                    return item.Value;
                }
            }
        }

        return null;
    }
    public bool CheckOneCard(GameObject card)
    {
        if (card.GetComponent<HandCardDisplay>()._card is SoldierCard)
        {
            return true;
        }
        else if(card.GetComponent<HandCardDisplay>()._card is Civil_Officials||card.GetComponent<HandCardDisplay>()._card is General)
        {
            if (CheckCity(card.GetComponent<HandCardDisplay>()._card as PersonCard)&&CheckCanUseBlock())
            {
                return true;
            }
        }
        else if (card.GetComponent<HandCardDisplay>()._card is StoryCard)
        {
            if (CheckPerson(card.GetComponent<HandCardDisplay>()._card as StoryCard))
            {
                return true;
            }
        }
        else if (card.GetComponent<HandCardDisplay>()._card is PersonCard)
        {
            if (enemy.PlayerOnTableCard.Count < 6) return true;
        }

        return false;
    }
    public void UseCard(GameObject UseCard)
    {
        if (UseCard.GetComponent<HandCardDisplay>()._card is SoldierCard)
        {
            enemy.PlayerSlodierNum += 1;
            enemy.CanUsePlayerSlodierNum += 1;
            if (UseCard.GetComponent<HandCardDisplay>().parent != null)
            {
                BattleEventSystem.DeleteGO(UseCard.GetComponent<HandCardDisplay>().parent);
            }

            UseCard.GetComponent<HandCardDisplay>().parent = null;
            enemy.PlayerHandCard.Remove(UseCard.GetComponent<HandCardDisplay>()._card.ID);
            UseCard.GetComponent<HandCardDisplay>().EnemyUse();
            //BattleEventSystem.DeleteCard(UseCard);
            UpdateSlodierNum();
        }
        else if (UseCard.GetComponent<HandCardDisplay>()._card is PersonCard)
        {
            foreach (var item in BattleEventSystem.EnemyPeronBlock)
            {
                if (item.GetComponent<PersonBlock>().cardGO == null)
                {
                    if (UseCard.GetComponent<HandCardDisplay>().parent != null)
                    {
                        BattleEventSystem.DeleteGO(UseCard.GetComponent<HandCardDisplay>().parent);
                    }
                    item.GetComponent<PersonBlock>().cardGO = UseCard;
                    UseCard.transform.SetParent(item.transform);
                    UseCard.GetComponent<HandCardDisplay>().parent = null;
                    UseCard.GetComponent<HandCardDisplay>().owner = 1;
                    UseCard.transform.position = item.transform.position;
                    UseCard.GetComponent<HandCardDisplay>().EnemyToTable();
                    enemy.PlayerOnTableCard.Add(UseCard);
                    enemy.PlayerHandCard.Remove(UseCard.GetComponent<HandCardDisplay>()._card.ID);
                    break;
                }
            }
        }
        else if(UseCard.GetComponent<HandCardDisplay>()._card is StoryCard)
        {
            AddPersonStar(FindMatchPerson(UseCard));
            Debug.Log("使用故事卡"+UseCard.GetComponent<HandCardDisplay>()._card.Name);
            if (UseCard.GetComponent<HandCardDisplay>().parent != null)
            {
                BattleEventSystem.DeleteGO(UseCard.GetComponent<HandCardDisplay>().parent);
            }
            UseCard.GetComponent<HandCardDisplay>().parent = null;
            UpdateEnemyMorale();
            enemy.PlayerHandCard.Remove(UseCard.GetComponent<HandCardDisplay>()._card.ID);
            UseCard.GetComponent<HandCardDisplay>().EnemyUse();
            //BattleEventSystem.DeleteCard(UseCard);
        }
    }
    public List<GameObject> CheckAttackBlock()
    {
        List<GameObject> list = new List<GameObject>();
        foreach (var item in BattleEventSystem.EnemyPeronBlock)
        {
            if(item.GetComponent<PersonBlock>().cardGO==null)continue;
            if (item.GetComponent<PersonBlock>().AttackCount > 0&&item.GetComponent<PersonBlock>().cardGO.GetComponent<HandCardDisplay>()._card is General)
            {
                list.Add(item);
                
            }
        }

        return list;
    }
    public List<GameObject> CreateQueue(List<GameObject> CanAttackBlocks)
    {
        //带领人数最多首选
        int leadernum = 0;
        int maxleadernum = 0;
        int index = 0;
        for (int j = 0; j < CanAttackBlocks.Count; j++)
        {
            maxleadernum=Setting.GPersonLeadNumber[
                Int32.Parse(CanAttackBlocks[j].GetComponent<PersonBlock>().cardGO.GetComponent<HandCardDisplay>()
                    .starsnum.text)];
            for (int i = j+1; i < CanAttackBlocks.Count; i++)
            {
                leadernum = Setting.GPersonLeadNumber[
                    Int32.Parse(CanAttackBlocks[i].GetComponent<PersonBlock>().cardGO.GetComponent<HandCardDisplay>()
                        .starsnum.text)];
                if (leadernum > maxleadernum)
                {
                    maxleadernum = leadernum;
                    index = i;
                }
            }
            GameObject Temp;
            Temp = CanAttackBlocks[j];
            CanAttackBlocks[j] = CanAttackBlocks[index];
            CanAttackBlocks[index] = Temp;
        }
        

        return CanAttackBlocks;


    }
    public void Attack(GameObject go)
    {
        if (enemy.CanUsePlayerSlodierNum == 0) return;
        GameObject CanTake = null;
        int leadernum=Int32.Parse(go.GetComponent<PersonBlock>().cardGO.GetComponent<HandCardDisplay>().value.text);
        foreach (var item in BattleEventSystem.PlayerCityBlock)
        {
            if (enemy.CanUsePlayerSlodierNum * (1 + enemy.PlayterMoraleSum) >
                item.GetComponent<CityBlock>().SlodierNum * (1 + BattleEventSystem.player.PlayterMoraleSum))
            {
                CanTake = item;
                break;
            }
        }

        if (CanTake != null)
        {
           
            if ( leadernum*
                (1 + enemy.PlayterMoraleSum) > CanTake.GetComponent<CityBlock>().SlodierNum *
                (1 + BattleEventSystem.player.PlayterMoraleSum))
            {
                int num = 0;
                for (; num <= leadernum; num++)
                {
                    if (num * (1 + enemy.PlayterMoraleSum) > CanTake.GetComponent<CityBlock>().SlodierNum *
                        (1 + BattleEventSystem.player.PlayterMoraleSum)) //不一定带最大人数
                    {
                        
                        Debug.Log("敌方攻击，武将："+go.GetComponent<PersonBlock>().cardGO.GetComponent<HandCardDisplay>()._card.Name+",攻打："+CanTake.GetComponent<CityBlock>().cardGO.GetComponent<CityCardLibraryDisplay>()._card.Name+",使用士兵:"+num);
                        go.GetComponent<PersonBlock>().AttackCount--;
                        float playerloss=Convert.ToInt32(CanTake.GetComponent<CityBlock>().SlodierNum);
                        float enemyloss=Convert.ToInt32(CanTake.GetComponent<CityBlock>().SlodierNum*(1+BattleEventSystem.player.PlayterMoraleSum)/((1 + enemy.PlayterMoraleSum)));
                        enemyloss = enemyloss > leadernum ? leadernum : enemyloss;
                        BattleEventSystem.player.PlayerSlodierNum -= playerloss;
                        BattleEventSystem.player.SlodierInCityNum -= playerloss;
                        BattleEventSystem.enemy.PlayerSlodierNum -= enemyloss;
                        BattleEventSystem.enemy.CanUsePlayerSlodierNum -= num;
                        BattleEventSystem.Arrow.GetComponent<Arrow>().Show(go.transform.position);
                        BattleEventSystem.Arrow.GetComponent<Arrow>().LengthenToEnd(CanTake.transform.position,CanTake,0);
                        return;
                    }
                }
            }
        }
        else
        {
            int ran = Random.Range(0, 2);
            if (ran == 0) //50%不攻击
            {
                go.GetComponent<PersonBlock>().AttackCount--;
                return;
            }
            else   //50%攻击
            {
                int RanTarget = Random.Range(0, BattleEventSystem.PlayerCityBlock.Count);
                if (RandomSlodierNum == 0)
                {
                    RandomSlodierNum = Random.Range(Convert.ToInt32(enemy.CanUsePlayerSlodierNum) / 2,
                        Convert.ToInt32(enemy.CanUsePlayerSlodierNum)+1);
                }

                int MaxNum = Convert.ToInt32(enemy.CanUsePlayerSlodierNum) - RandomSlodierNum > leadernum
                    ? leadernum
                    : Convert.ToInt32(enemy.CanUsePlayerSlodierNum) - RandomSlodierNum;
                int AttackSlodierNum =Random.Range(MaxNum/2,MaxNum);
                int AttackTarget = Random.Range(0, BattleEventSystem.PlayerCityBlock.Count);
                GameObject AttackTargetGO = BattleEventSystem.PlayerCityBlock[AttackTarget];
                float PlayerDefenceNum = AttackTargetGO.GetComponent<CityBlock>().SlodierNum;
                float playerATK = AttackTargetGO.GetComponent<CityBlock>().SlodierNum *
                                (1 + BattleEventSystem.player.PlayterMoraleSum);
                float enemyATK = AttackSlodierNum * (1 + enemy.PlayterMoraleSum);
                if (playerATK == enemyATK)
                {
                    int playerloss =Convert.ToInt32( enemyATK / (1 + BattleEventSystem.player.PlayterMoraleSum)>PlayerDefenceNum?PlayerDefenceNum:enemyATK / (1 + BattleEventSystem.player.PlayterMoraleSum));
                    int enemyloss = Convert.ToInt32(playerATK / (1 + enemy.PlayterMoraleSum)>AttackSlodierNum?AttackSlodierNum:playerATK / (1 + enemy.PlayterMoraleSum));
                    if (playerloss < enemyloss) return;
                    Debug.Log("敌方攻击，武将："+go.GetComponent<PersonBlock>().cardGO.GetComponent<HandCardDisplay>()._card.Name+",攻打："+AttackTargetGO.GetComponent<CityBlock>().cardGO.GetComponent<CityCardLibraryDisplay>()._card.Name+",使用士兵:"+AttackSlodierNum);
                    BattleEventSystem.player.PlayerSlodierNum -= playerloss;
                    BattleEventSystem.enemy.PlayerSlodierNum -= enemyloss;
                    BattleEventSystem.enemy.CanUsePlayerSlodierNum -= AttackSlodierNum;
                    BattleEventSystem.Arrow.GetComponent<Arrow>().Show(go.transform.position);
                    BattleEventSystem.Arrow.GetComponent<Arrow>().LengthenToEnd(AttackTargetGO.transform.position,AttackTargetGO,1,playerloss);

                }
                else
                {
                    Debug.Log("敌方攻击，武将："+go.GetComponent<PersonBlock>().cardGO.GetComponent<HandCardDisplay>()._card.Name+",攻打："+AttackTargetGO.GetComponent<CityBlock>().cardGO.GetComponent<CityCardLibraryDisplay>()._card.Name+",使用士兵:"+AttackSlodierNum);
                    int playerloss = Convert.ToInt32(enemyATK / (1 + BattleEventSystem.player.PlayterMoraleSum)>PlayerDefenceNum?PlayerDefenceNum:enemyATK / (1 + BattleEventSystem.player.PlayterMoraleSum));
                    int enemyloss = Convert.ToInt32(AttackSlodierNum);
                    BattleEventSystem.player.PlayerSlodierNum -= playerloss;
                    BattleEventSystem.enemy.PlayerSlodierNum -= enemyloss;
                    BattleEventSystem.enemy.CanUsePlayerSlodierNum -= AttackSlodierNum;
                    BattleEventSystem.Arrow.GetComponent<Arrow>().Show(go.transform.position);
                    BattleEventSystem.Arrow.GetComponent<Arrow>().LengthenToEnd(AttackTargetGO.transform.position,AttackTargetGO,1,playerloss);
                }
            }
        }
    }
    public void UpdateEnemyMorale()
    {
        BattleEventSystem.EnemyMorale.text = "士气加成：" + enemy.PlayterMoraleSum.ToString();
    }
    public int CardChange()
    {
        List<GameObject> list = new List<GameObject>();//调换卡牌IDlist
        int changenum = 0;
        foreach (var item in enemy.PlayerHandCard)
        {
            if (!CheckOneCard(item.Value))
            {
                list.Add(item.Value);
            }
        }
        foreach (var item in list)
        {
            int a = Random.Range(0, 2);
            if (a == 0) continue;
            else if (a == 1&&enemy.PlayerSetList.Count>0)
            {
                if (item.GetComponent<HandCardDisplay>()._card is StoryCard)
                {
                    int b = Random.Range(0, 10);
                    if(b>=5)continue;
                }
                changenum++;
                enemy.PlayerHandCard.Remove(item.GetComponent<HandCardDisplay>()._card.ID);
                Debug.Log("AI弃牌："+item.GetComponent<HandCardDisplay>()._card.Name);
                BattleEventSystem.DeleteGO(item.GetComponent<HandCardDisplay>().parent);
                item.GetComponent<HandCardDisplay>().AIDelete();
                //item.GetComponent<HandCardDisplay>()._card = enemy.PlayerSetList[enemy.PlayerSetList.Count - 1];
                //item.GetComponent<HandCardDisplay>().Show();
                //enemy.PlayerSetList.RemoveAt(enemy.PlayerSetList.Count-1);
                //UpdateCardNum();
                //enemy.PlayerHandCard.Add(item.GetComponent<HandCardDisplay>()._card.ID,item);

            }
        }
        //AfterDelete();
        return changenum;
    }
    public IEnumerator AIStart()
    {
        ResetSlodierNum();
        yield return new WaitForSeconds(2);
        //换牌阶段
        foreach (var VARIABLE in enemy.PlayerHandCard)
        {
            Debug.Log(VARIABLE.Key+"+"+VARIABLE.Value.GetComponent<HandCardDisplay>()._card.Name);
        }
        
        int num=CardChange();
        yield return BattleEventSystem.AIChange(num);
        Debug.Log("换牌结束");
        yield return new WaitForSeconds(2);
        //出牌阶段
        while (CheckCard()!=null)
        {
            GameObject CurCard = CheckCard();
            Debug.Log(CurCard.GetComponent<HandCardDisplay>()._card.Name);
            UseCard(CurCard);
            foreach (var VARIABLE in enemy.PlayerHandCard)
            {
                Debug.Log(VARIABLE.Key+"+"+VARIABLE.Value.GetComponent<HandCardDisplay>()._card.Name);
            }
            yield return 0;
            AfterDelete();
            yield return new WaitForSeconds(2);
            
        }
        Debug.Log("出牌结束");
        //攻击阶段
        Attacklist = CheckAttackBlock();
        Attacklist = CreateQueue(Attacklist);
        while (Attacklist.Count!=0)
        {
            Attack(Attacklist[0]);
            Attacklist.RemoveAt(0);
            yield return new WaitForSeconds(3);
        }
        Debug.Log("攻击结束");
        //防御阶段
        Defence();
        yield return new WaitForSeconds(2);
        Debug.Log("防御结束");
        //结束
        BattleEventSystem.TurnNum++;
        BattleEventSystem.ResetAttckCount();
        BattleEventSystem.NextTurn();
        BattleEventSystem.TurnButton.GetComponent<RoundButtonScript>().ImageChange();

    }
    public void AfterDelete()//手牌替换或出牌时更新现有手牌位置
    {
        foreach (var item in enemy.PlayerHandCard)
        {
            //Debug.Log(item.Value.GetComponent<HandCardDisplay>()._card.ID);
            if (item.Value.GetComponent<HandCardDisplay>().parent != null)
            {
                item.Value.transform.position = item.Value.GetComponent<HandCardDisplay>().parent.transform.position;
            }
        }
    }
    public void ResetSlodierNum()
    {
        foreach (var item in BattleEventSystem.EnemyCityBlock)
        {
            item.GetComponent<CityBlock>().ResetSlodierNum();
        }

        enemy.SlodierInCityNum = 0;
        enemy.CanUsePlayerSlodierNum = enemy.PlayerSlodierNum;
        UpdateSlodierNum();
    }
    public void UpdateSlodierNum()
    {
        BattleEventSystem.EnemySlodierNum.text ="驻守士兵数量："+enemy.SlodierInCityNum+"/士兵总量:"+enemy.PlayerSlodierNum.ToString();
        BattleEventSystem.EnemyCanUseSlodierNum.text = "可用进攻：" + enemy.CanUsePlayerSlodierNum;
    }
    public void UpdateCardNum()
    {
        BattleEventSystem.EnemyCardNumText.text = "剩余卡牌：" + enemy.PlayerSetList.Count;
    }
    public GameObject FindMatchPerson(GameObject UseCard)
    {
        string[] matchpersons;
        if ((UseCard.GetComponent<HandCardDisplay>()._card as StoryCard).MatchPerson[0] != '"')
        {
            matchpersons = new string[] { (UseCard.GetComponent<HandCardDisplay>()._card as StoryCard).MatchPerson };
        }
        else{matchpersons= (UseCard.GetComponent<HandCardDisplay>()._card as StoryCard).MatchPerson.Split('"')[1].Split('，');}
        foreach (var VARIABLE in matchpersons)
        {
            foreach (var item in enemy.PlayerOnTableCard)
            {
                if (VARIABLE==
                    item.GetComponent<HandCardDisplay>()._card.Name)
                {
                    return item;
                }
            }
        }
        
        return null;
    }
    public void AddPersonStar(GameObject Card)
    {
        if (Card.GetComponent<HandCardDisplay>()._card is Civil_Officials)
        {
            enemy.PlayterMoraleSum -= Setting.CPersonATKBuff[Int32.Parse(Card.GetComponent<HandCardDisplay>().starsnum.text)];
            Card.GetComponent<HandCardDisplay>().starsnum.text =
                (Int32.Parse(Card.GetComponent<HandCardDisplay>().starsnum.text) + 1).ToString();
            enemy.PlayterMoraleSum += Setting.CPersonATKBuff[Int32.Parse(Card.GetComponent<HandCardDisplay>().starsnum.text)];
            UpdateEnemyMorale();
        }
        else if(Card.GetComponent<HandCardDisplay>()._card is General)
        {
            Card.GetComponent<HandCardDisplay>().starsnum.text =
                (Int32.Parse(Card.GetComponent<HandCardDisplay>().starsnum.text) + 1).ToString();
            Card.GetComponent<HandCardDisplay>().value.text =
                Setting.GPersonLeadNumber[Int32.Parse(Card.GetComponent<HandCardDisplay>().starsnum.text)].ToString();
        }
    }
}
