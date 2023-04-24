using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject BattleEventSystem;
    public GameObject BattleEventSystemPre;
    void Start()
    {
        DontDestroyOnLoad(this);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
