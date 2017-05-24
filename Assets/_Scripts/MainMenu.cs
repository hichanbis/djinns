using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{

    public enum Menu
    {
        MainMenu,
        Continue
    }

    private SceneController sceneController;
    // Reference to the SceneController to actually do the loading and unloading of scenes.


    protected void Start()
    {
        sceneController = FindObjectOfType<SceneController>();
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
                Game.current = new Game();
                //Session.Save();
                //Move on to game...
                sceneController.FadeAndLoadScene(Game.current.currentScene);
            }

            if (GUILayout.Button("Continue"))
            {
                Session.Load();
                currentMenu = Menu.Continue;
            }

            if (GUILayout.Button("Quit"))
            {
                Application.Quit();
            }
        }
        else if (currentMenu == Menu.Continue)
        {
			
            GUILayout.Box("Select Save File");
            GUILayout.Space(10);
			
            foreach (Game game in Session.savedGames)
            {
                if (GUILayout.Button(game.getGameDesc()))
                {
                    Game.current = game;
                    sceneController.FadeAndLoadScene(Game.current.currentScene);
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
