using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPanel : NetworkBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if(SceneManager.GetActiveScene().name == "RoomScene")
            DontDestroyOnLoad(gameObject);
    }
}
