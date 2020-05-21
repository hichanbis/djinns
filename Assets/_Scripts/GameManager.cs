using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public GameProgress gameProgress;

    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

    }

    void SaveGame()
    {
        gameProgress.Save(10);
    }

    void LoadGame()
    {
        gameProgress.Load(10);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("QuickSave"))
        {
            SaveGame();
        }

        if (Input.GetButtonDown("QuickLoad"))
        {
            LoadGame();
        }
    }
}
