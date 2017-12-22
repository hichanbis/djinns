using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public Game[] gameSlots = new Game[10];
    public Game currentGame;

    public enum Menu
    {
        MainMenu,
        NewGame,
        Continue
    }


    void Awake()
    {
        if (FindObjectOfType(typeof(EventManager)) == null)
        {
            Debug.Log("No EventManager found, it is likely the persistent scene is unloaded so it is debug mode");
            StartCoroutine(LoadDebugPersistentScene());
            Debug.Log("Ok persistent scene loaded go debug");
        }

    }

    IEnumerator LoadDebugPersistentScene()
    {
        yield return SceneManager.LoadSceneAsync("Persistent", LoadSceneMode.Additive);
    }

    private SceneController sceneController;
    // Reference to the SceneController to actually do the loading and unloading of scenes.


    protected void Start()
    {
        sceneController = FindObjectOfType<SceneController>();

        for (int i = 0; i < gameSlots.Length; i++)
        {
            gameSlots[i] = Game.TryToLoad(i);
        }

        
    }

    public Menu currentMenu;

    void OnGUI()
    {

        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        if (currentMenu == Menu.MainMenu)
        {

            GUILayout.Box("Djinns");
            GUILayout.Space(10);

            if (GUILayout.Button("New Game"))
            {
                currentMenu = Menu.NewGame;
            }
                
            if (GUILayout.Button("Continue"))
            {
                currentMenu = Menu.Continue;
            }

            if (GUILayout.Button("Quit"))
            {
                Application.Quit();
            }
        }
        else if (currentMenu == Menu.NewGame)
        {
            GUILayout.Box("Select Slot to overwrite");
            GUILayout.Space(10);

            for (int i = 0; i < gameSlots.Length; i++)
            {
                Game game = gameSlots[i];
                string buttonLabel;

                if (game == null)
                    buttonLabel = "New game";
                else
                    buttonLabel = game.getGameDesc();

                if (GUILayout.Button(buttonLabel))
                {
                    currentGame.currentScene = "BalaFireFestival";
                    currentGame.Save(i);
                    Debug.Log("New save created at slot " + i);
                    sceneController.FadeAndLoadScene(currentGame.currentScene);
                }
            }
        }
        else if (currentMenu == Menu.Continue)
        {
            
            GUILayout.Box("Select Save File");
            GUILayout.Space(10);
			
            for (int i = 0; i < gameSlots.Length; i++)
            {
                Game game = gameSlots[i];
                if (game != null)
                {
                    if (GUILayout.Button("Slot " + i + ": " + game.getGameDesc()))
                    {
                        //pas sur qu'on puisse faire ça, a mon avis il faut recopier les infos et overwrite, voir faire un load de json dans le current

                        currentGame.Load(i);
                        sceneController.FadeAndLoadScene(currentGame.currentScene);
                    }
                }

            }

            GUILayout.Space(10);
            if (GUILayout.Button("Cancel"))
            {
                currentMenu = Menu.MainMenu;
            }
			
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

    }
}
