using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public Session session;
    public Game currentGame;

    public enum Menu
    {
        MainMenu,
        NewGame,
        Continue
    }

    private SceneController sceneController;
    // Reference to the SceneController to actually do the loading and unloading of scenes.


    protected void Start()
    {
        sceneController = FindObjectOfType<SceneController>();
        session.Load();
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

            for (int i = 0; i < session.gameSlots.Length; i++)
            {
                Game game = session.gameSlots[i];
                string buttonLabel;

                if (game.party.Count == 0)
                    buttonLabel = "New game";
                else
                    buttonLabel = game.getGameDesc();

                if (GUILayout.Button(buttonLabel))
                {
                    currentGame.currentScene = "BalaFireFestival";
                    session.Save(i);
                    Debug.Log("New save created at slot " + i);
                    sceneController.FadeAndLoadScene(currentGame.currentScene);
                }
            }
        }
        else if (currentMenu == Menu.Continue)
        {
            
            GUILayout.Box("Select Save File");
            GUILayout.Space(10);
			
            foreach (Game game in session.gameSlots)
            {
                if (game.party.Count > 0)
                {
                    if (GUILayout.Button(game.getGameDesc()))
                    {
                        //pas sur qu'on puisse faire ça, a mon avis il faut recopier les infos et overwrite, voir faire un load de json dans le current
                        currentGame = game;
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
