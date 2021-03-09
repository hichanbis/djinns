using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Linq;

public class BattleUI : MonoBehaviour
{
    public AbilityCollection abilityCollection;
    private BattleManager battleManager;
    public BattleUnits battleUnits;

    public GameObject playersInfoPanelTemplate;
    public GameObject playerActionsPanelTemplate;
    public GameObject playerMagicsPanelTemplate;
    public GameObject playerInfoPanelTemplate;
    public GameObject actionButtonTemplate;
    public GameObject magicActionButtonTemplate;
    public GameObject targetsPanelTemplate;
    public GameObject targetButtonTemplate;
    public GameObject cursorRootTemplate;

    public GameObject cursorRoot;

    private GameObject playersInfoPanel;
    private Dictionary<string, GameObject> playerActionsPanels;
    private Dictionary<string, GameObject> playerMagicsPanels;
    private List<BattleScript> playerUnits;
    private GameObject targetsPanel;


    private UnityAction playerUnitsExistListener;
    private UnityAction playerMustChooseAbilityListener;
    private UnityAction UpdatePlayersInfoListener;
    private UnityAction winListener;
    private UnityAction loseListener;
    private UnityAction cancelTargetListener;
    private UnityAction displayMagicsPanelListener;

    public GameObject battleCanvas, victoryCanvas, gameOverCanvas;

    private void Awake()
    {
   
    }

    // Use this for initialization
    void Start()
    {
        battleManager = BattleManager.Instance;

        battleCanvas.SetActive(true);
        victoryCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);

        cursorRoot = Instantiate(cursorRootTemplate, battleCanvas.transform, false);
        cursorRoot.SetActive(false);

        playerActionsPanels = new Dictionary<string, GameObject>();
        playerMagicsPanels = new Dictionary<string, GameObject>();

        playerUnitsExistListener = new UnityAction(InitializePlayerPanels);
        EventManager.StartListening(BattleEventMessages.InitBattle.ToString(), playerUnitsExistListener);

        playerMustChooseAbilityListener = new UnityAction(ActivateCurrentPlayerActionsPanel);
        EventManager.StartListening(BattleEventMessages.PlayerChoiceExpected.ToString(), playerMustChooseAbilityListener);

        UpdatePlayersInfoListener = new UnityAction(UpdatePlayersInfo);
        EventManager.StartListening(BattleEventMessages.PlayerHpChanged.ToString(), UpdatePlayersInfoListener);

        winListener = new UnityAction(DisplayWinCanvas);
        EventManager.StartListening(BattleEventMessages.Victory.ToString(), winListener);

        loseListener = new UnityAction(DisplayGameOverCanvas);
        EventManager.StartListening(BattleEventMessages.Failure.ToString(), loseListener);

        cancelTargetListener = new UnityAction(CancelTarget);
        EventManager.StartListening(BattleEventMessages.CancelTarget.ToString(), cancelTargetListener);

        displayMagicsPanelListener = new UnityAction(DisplayMagicsPanel);
        EventManager.StartListening(BattleEventMessages.DisplayMagicsPanel.ToString(), displayMagicsPanelListener);
    }

    void InitializePlayerPanels()
    {
        playerUnits = battleUnits.playerUnits;
        InstantiatePlayersInfoPanel();

        InstantiatePlayerActionsPanels();
        InstantiatePlayerMagicsPanels();
    }

    void InstantiatePlayersInfoPanel()
    {
        playersInfoPanel = Instantiate(playersInfoPanelTemplate, battleCanvas.transform, false) as GameObject;
        for (int i = 0; i < playerUnits.Count; i++)
        {
            GameObject playerInfo = Instantiate(playerInfoPanelTemplate, playersInfoPanel.transform, false) as GameObject;
            playerInfo.name = playerUnits[i].name + "PlayerInfo";
        }
        UpdatePlayersInfo();
    }

    void UpdatePlayerInfo(BattleScript playerUnit)
    {
        Character playerChar = playerUnit.character;
        Transform playerInfoTr = playersInfoPanel.transform.Find(playerUnit.name + "PlayerInfo").transform;
        playerInfoTr.Find("PlayerName").GetComponentInChildren<Text>().text = playerChar.id;
        playerInfoTr.Find("HpMp").transform.Find("Hp").GetComponentInChildren<Text>().text = playerChar.stats.hpNow.GetValue().ToString() + " / " + playerChar.stats.hp.GetValue().ToString();
        playerInfoTr.Find("HpMp").transform.Find("Mp").GetComponentInChildren<Text>().text = playerChar.stats.mpNow.GetValue().ToString() + " / " + playerChar.stats.mp.GetValue().ToString();
    }

    void UpdatePlayersInfo()
    {
        
        playerUnits = battleUnits.playerUnits;
        for (int i = 0; i < playerUnits.Count; i++)
        {
            UpdatePlayerInfo(playerUnits[i]);
        }
    }

    void InstantiatePlayerActionsPanels()
    {
        foreach (BattleScript playerUnit in playerUnits)
        {
            GameObject playerActionsPanel = Instantiate(playerActionsPanelTemplate, battleCanvas.transform, false) as GameObject;
            playerActionsPanel.name = playerUnit.name + "ActionsPanel";

            AddActionButtonToTheActionsPanel("Attack", playerActionsPanel);
            if (playerUnit.KnowsMagic())
                AddActionButtonToTheActionsPanel("Magic", playerActionsPanel);

            AddActionButtonToTheActionsPanel("Guard", playerActionsPanel);
            playerActionsPanels.Add(playerUnit.name, playerActionsPanel);
        }
    }

    void InstantiatePlayerMagicsPanels()
    {
        foreach (BattleScript playerUnit in playerUnits)
        {
            GameObject playerMagicsPanel = Instantiate(playerMagicsPanelTemplate, battleCanvas.transform, false) as GameObject;
            playerMagicsPanel.name = playerUnit.name + "MagicsPanel";
            List<Ability> abilities = playerUnit.GetComponent<BattleScript>().character.abilities;
            List<Ability> magics = abilities.Where(a => a.abilityType.Equals(AbilityType.Magic)).ToList();
            foreach (Ability magic in magics)
            {
                AddMagicActionButtonToTheMagicsPanel(magic, playerMagicsPanel);
            }
            playerMagicsPanels.Add(playerUnit.name, playerMagicsPanel);
        }
    }

    void AddMagicActionButtonToTheMagicsPanel(Ability magic, GameObject playerMagicsPanel)
    {
        GameObject magicActionButton = Instantiate(magicActionButtonTemplate, playerMagicsPanel.transform, false) as GameObject;
        magicActionButton.name = magic.name;
        magicActionButton.transform.Find("MagicName").GetComponent<Text>().text = magic.name;
        magicActionButton.transform.Find("MagicMpCost").GetComponent<Text>().text = magic.mpCost + "";
        magicActionButton.GetComponent<Button>().onClick.AddListener(() => ClickedAbility(magic));
        if (!magic.targetType.Equals(TargetType.Self))
            magicActionButton.GetComponent<Button>().onClick.AddListener(() => DisplayTargetsPanel(magic.targetType));

    }

    void AddActionButtonToTheActionsPanel(string actionName, GameObject playerActionsPanel)
    {
        GameObject actionButton = Instantiate(actionButtonTemplate, playerActionsPanel.transform, false) as GameObject;
        actionButton.name = actionName;
        actionButton.transform.Find("ActionName").GetComponent<Text>().text = actionName;

        //Depending on the command
        if (actionName.Equals("Guard"))
            actionButton.GetComponent<Button>().onClick.AddListener(() => ClickedAbility(abilityCollection.GetAbilityFromId("Guard")));
        else if (actionName.Equals("Magic"))
            actionButton.GetComponent<Button>().onClick.AddListener(() => DisplayMagicsPanel());
        else if (actionName.Equals("Attack"))
        {
            actionButton.GetComponent<Button>().onClick.AddListener(() => ClickedAbility(abilityCollection.GetAbilityFromId("Attack")));
            //actionButton.GetComponent<Button>().onClick.AddListener(() => DisplayTargetsPanel(TargetType.Opposite));
        }

    }

    void ActivateCurrentPlayerActionsPanel()
    {
        foreach (KeyValuePair<string, GameObject> entry in playerActionsPanels)
        {
            if (entry.Key.Equals(battleUnits.currentChoosingUnit.name))
            {
                entry.Value.SetActive(true);
                EventSystem.current.SetSelectedGameObject(entry.Value.GetComponentsInChildren<Button>().First<Button>().gameObject);
                cursorRoot.SetActive(true);
                cursorRoot.transform.SetAsLastSibling();

            }
            else
                entry.Value.SetActive(false);
        }

    }

    void DeActivateCurrentPlayerActionsPanel()
    {
        foreach (KeyValuePair<string, GameObject> entry in playerActionsPanels)
        {
            if (entry.Key.Equals(battleUnits.currentChoosingUnit.name))
            {
                entry.Value.SetActive(false);
                cursorRoot.SetActive(false);
            }
        }
    }


    GameObject GetPlayerMagicsPanel(string playerName)
    {
        GameObject foundPanel = null;
        foreach (KeyValuePair<string, GameObject> entry in playerMagicsPanels)
        {
            if (entry.Key.Equals(playerName))
            {
                foundPanel = entry.Value;
                break;
            }
        }
        return foundPanel;
    }


    void ActivateCurrentPlayerMagicsPanel()
    {
        foreach (KeyValuePair<string, GameObject> entry in playerMagicsPanels)
        {
            if (entry.Key.Equals(battleUnits.currentChoosingUnit.name))
            {
                entry.Value.SetActive(true);
                EventSystem.current.SetSelectedGameObject(entry.Value.GetComponentsInChildren<Button>().First<Button>().gameObject);
                cursorRoot.SetActive(true);
                cursorRoot.transform.SetAsLastSibling();
            }
            else
                entry.Value.SetActive(false);
        }
    }

    void DeActivateCurrentPlayerMagicsPanel()
    {
        foreach (KeyValuePair<string, GameObject> entry in playerMagicsPanels)
        {
            if (entry.Key.Equals(battleUnits.currentChoosingUnit.name))
                entry.Value.SetActive(false);
        }
    }

    private void DisplayMagicsPanel()
    {
        GameObject currentPlayerMagicsPanel = GetPlayerMagicsPanel(battleUnits.currentChoosingUnit.name);
        DisableMagicsBasedOnMp(currentPlayerMagicsPanel);
        cursorRoot.SetActive(true);
        cursorRoot.transform.SetAsLastSibling();
        currentPlayerMagicsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(currentPlayerMagicsPanel.GetComponentsInChildren<Button>().First<Button>().gameObject);

    }

    private void DisableMagicsBasedOnMp(GameObject currentPlayerMagicsPanel)
    {
        foreach (Button button in currentPlayerMagicsPanel.GetComponentsInChildren<Button>())
        {
            Ability ab = abilityCollection.GetAbilityFromName(button.name);
            if (battleUnits.currentChoosingUnit.character.GetStat(StatName.mpNow).GetValue() < ab.mpCost)
                button.interactable = false;
        }
    }

    private void DisplayTargetsPanel(TargetType targetType)
    {

        DeActivateCurrentPlayerActionsPanel();
        DeActivateCurrentPlayerMagicsPanel();

        InstantiateTargetsPanel(targetType);
        cursorRoot.SetActive(true);
        cursorRoot.transform.SetAsLastSibling();

    }

    //if all on grise le tout mais on attend le submit quand même
    void InstantiateTargetsPanel(TargetType targetType)
    {
        targetsPanel = Instantiate(targetsPanelTemplate, battleCanvas.transform, false) as GameObject;
        List<BattleScript> targets = null;

        if (targetType.Equals(TargetType.Opposite) || targetType.Equals(TargetType.AllOpposite))
            targets = battleUnits.enemyUnits;
        else
            targets = battleUnits.playerUnits;

        if (targetType.Equals(TargetType.AllOpposite) || targetType.Equals(TargetType.AllSame))
        {
            GameObject targetButton = Instantiate(targetButtonTemplate, targetsPanel.transform, false) as GameObject;
            String targetName = targetType.Equals(TargetType.AllSame) ? "All Players" : "All Enemies";
            targetButton.name = targetName;
            targetButton.transform.Find("TargetName").GetComponent<Text>().text = targetName;
            targetButton.GetComponent<Button>().onClick.AddListener(() => ClickedTarget(targets));
            targetButton.GetComponent<Button>().Select();
        }

        foreach (BattleScript target in targets)
        {
            GameObject targetButton = Instantiate(targetButtonTemplate, targetsPanel.transform, false) as GameObject;
            targetButton.name = target.name;
            targetButton.transform.Find("TargetName").GetComponent<Text>().text = target.name;
            if (targetType.Equals(TargetType.Opposite) || targetType.Equals(TargetType.Same))
                targetButton.GetComponent<Button>().onClick.AddListener(() => ClickedTarget(new List<BattleScript>() { target }));
            else if (targetType.Equals(TargetType.AllOpposite) || targetType.Equals(TargetType.AllSame))
            {
                targetButton.GetComponent<Button>().interactable = false;
                ColorBlock cb = targetButton.GetComponent<Button>().colors;
                cb.disabledColor = cb.highlightedColor;
                targetButton.GetComponent<Button>().colors = cb;
            }
        }

        /*if (targetType.Equals(TargetType.AllOpposite) || targetType.Equals(TargetType.AllSame))
        {
            currentActionTargets = targets;
            targetsPanel.GetComponent<CanvasGroup>().interactable = false;
        }
        else*/

        EventSystem.current.SetSelectedGameObject(targetsPanel.GetComponentsInChildren<Button>().First<Button>().gameObject);
        battleManager.SetCurrentTargetFromName(targetsPanel.GetComponentsInChildren<Button>().First<Button>().gameObject.name);
    }

    private void ClickedTarget(List<BattleScript> targetUnits)
    {
        Destroy(targetsPanel);
        cursorRoot.SetActive(false);
        battleUnits.targetUnits = targetUnits;
    }


    private void ClickedAbility(Ability ability)
    {
        DeActivateCurrentPlayerActionsPanel();
        DeActivateCurrentPlayerMagicsPanel();
        battleManager.currentUnitAction.ability = ability;
    }

    private void DisplayWinCanvas()
    {
        battleCanvas.SetActive(false);
        victoryCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(victoryCanvas.GetComponentsInChildren<Button>().First<Button>().gameObject);
        gameOverCanvas.SetActive(false);
    }

    private void DisplayGameOverCanvas()
    {
        battleCanvas.SetActive(false);
        victoryCanvas.SetActive(false);
        gameOverCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(gameOverCanvas.GetComponentsInChildren<Button>().First<Button>().gameObject);
    }

    public void ClickedOkVictory()
    {
        battleCanvas.SetActive(false);
        victoryCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        battleManager.victoryAcknowledged = true;
    }

    public void ClickedRestart()
    {
        battleCanvas.SetActive(false);
        victoryCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        battleManager.restartBattle = true;
    }

    public void ClickedBackToMainMenu()
    {
        battleCanvas.SetActive(false);
        victoryCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        battleManager.backToMainMenu = true;
    }

    public void CancelMagic()
    {
        DeActivateCurrentPlayerMagicsPanel();
        ActivateCurrentPlayerActionsPanel();

    }

    public void CancelTarget()
    {
        Destroy(targetsPanel);
    }

    void Update()
    {
    }


}

