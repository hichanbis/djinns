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
    private BattleManager battleManager;

    public GameObject playersInfoPanelTemplate;
    public GameObject playerActionsPanelTemplate;
    public GameObject playerMagicsPanelTemplate;
    public GameObject playerInfoPanelTemplate;
    public GameObject actionButtonTemplate;
    public GameObject magicActionButtonTemplate;
    public GameObject targetsPanelTemplate;
    public GameObject targetButtonTemplate;

    private GameObject playersInfoPanel;
    private Dictionary<string, GameObject> playerActionsPanels;
    private Dictionary<string, GameObject> playerMagicsPanels;
    private List<GameObject> playerUnits;
    private GameObject targetsPanel;
    private string currentActionPanel;

    private UnityAction playerUnitsExistListener;
    private UnityAction playerMustChooseAbilityListener;
    private UnityAction rageAppliedListener;
    private UnityAction winListener;
    private UnityAction loseListener;

    private GameObject battleCanvas, victoryCanvas, gameOverCanvas;


    void Awake()
    {
        battleCanvas = GameObject.Find("BattleCanvas");
        victoryCanvas = GameObject.Find("VictoryCanvas");
        gameOverCanvas = GameObject.Find("GameOverCanvas");
    }

    // Use this for initialization
    void Start()
    {
        battleManager = BattleManager.Instance;

        battleCanvas.SetActive(true);
        victoryCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);

        playerActionsPanels = new Dictionary<string, GameObject>();
        playerMagicsPanels = new Dictionary<string, GameObject>();

        playerUnitsExistListener = new UnityAction(InitializePlayerPanels);
        EventManager.StartListening(BattleEventMessages.playerUnitsExist.ToString(), playerUnitsExistListener);

        playerMustChooseAbilityListener = new UnityAction(ActivateCurrentPlayerActionsPanel);
        EventManager.StartListening(BattleEventMessages.playerChoiceExpected.ToString(), playerMustChooseAbilityListener);

        rageAppliedListener = new UnityAction(refreshPlayersInfo);
        EventManager.StartListening(BattleEventMessages.damageApplied.ToString(), rageAppliedListener);

        winListener = new UnityAction(displayWinCanvas);
        EventManager.StartListening(BattleEventMessages.win.ToString(), winListener);

        loseListener = new UnityAction(displayGameOverCanvas);
        EventManager.StartListening(BattleEventMessages.lose.ToString(), loseListener);
    }


    void InitializePlayerPanels()
    {
        playerUnits = battleManager.playerUnits;
        InstantiatePlayersInfoPanel();
        refreshPlayersInfo();
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
    }

    void fillPlayerInfo(GameObject playerUnit)
    {
        Character playerChar = playerUnit.GetComponent<BattleScript>().Character;

        Transform playerInfoTr = playersInfoPanel.transform.Find(playerUnit.name + "PlayerInfo").transform;
        playerInfoTr.Find("PlayerName").transform.Find("PlayerNameText").GetComponent<Text>().text = playerChar.name;
        playerInfoTr.Find("HpMp").transform.Find("Hp").transform.Find("HpText").GetComponent<Text>().text = playerChar.GetStat(StatName.hpNow).baseValue.ToString() + " / " + playerChar.GetStat(StatName.hp).baseValue.ToString();
        playerInfoTr.Find("HpMp").transform.Find("Mp").transform.Find("MpText").GetComponent<Text>().text = playerChar.GetStat(StatName.mpNow).baseValue.ToString() + " / " + playerChar.GetStat(StatName.mp).baseValue.ToString();
    }

    void refreshPlayersInfo()
    {
        playerUnits = battleManager.playerUnits;
        for (int i = 0; i < playerUnits.Count; i++)
        {
            fillPlayerInfo(playerUnits[i]);
        }
    }

    void InstantiatePlayerActionsPanels()
    {
        foreach (GameObject playerUnit in playerUnits)
        {
            GameObject playerActionsPanel = Instantiate(playerActionsPanelTemplate, battleCanvas.transform, false) as GameObject;
            playerActionsPanel.name = playerUnit.name + "ActionsPanel";

            AddActionButtonToTheActionsPanel("Attack", playerActionsPanel);
            if (playerUnit.GetComponent<BattleScript>().KnowsMagic())
                AddActionButtonToTheActionsPanel("Magic", playerActionsPanel);

            AddActionButtonToTheActionsPanel("Guard", playerActionsPanel);
            playerActionsPanels.Add(playerUnit.name, playerActionsPanel);
        }
    }

    void InstantiatePlayerMagicsPanels()
    {
        foreach (GameObject playerUnit in playerUnits)
        {
            GameObject playerMagicsPanel = Instantiate(playerMagicsPanelTemplate, battleCanvas.transform, false) as GameObject;
            playerMagicsPanel.name = playerUnit.name + "MagicsPanel";
            List<Ability> abilities = playerUnit.GetComponent<BattleScript>().Character.Abilities;
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
        if (magic.targetType.Equals(TargetType.Same) || magic.targetType.Equals(TargetType.Opposite))
            magicActionButton.GetComponent<Button>().onClick.AddListener(() => DisplayTargetsPanel(magic.targetType));
    }

    void AddActionButtonToTheActionsPanel(string actionName, GameObject playerActionsPanel)
    {
        GameObject actionButton = Instantiate(actionButtonTemplate, playerActionsPanel.transform, false) as GameObject;
        actionButton.name = actionName;
        actionButton.transform.Find("ActionName").GetComponent<Text>().text = actionName;

        //Depending on the command
        if (actionName.Equals("Guard"))
            actionButton.GetComponent<Button>().onClick.AddListener(() => ClickedAbility(AbilityCollection.Instance.FindAbilityFromId("Guard")));
        else if (actionName.Equals("Magic"))
            actionButton.GetComponent<Button>().onClick.AddListener(() => DisplayMagicsPanel());
        else
        {
            actionButton.GetComponent<Button>().onClick.AddListener(() => ClickedAbility(AbilityCollection.Instance.FindAbilityFromId("Attack")));
            actionButton.GetComponent<Button>().onClick.AddListener(() => DisplayTargetsPanel(TargetType.Opposite));
        }

    }

    void ActivateCurrentPlayerActionsPanel()
    {
        foreach (KeyValuePair<string, GameObject> entry in playerActionsPanels)
        {
            if (entry.Key.Equals(battleManager.currentUnit.name))
            {
                entry.Value.SetActive(true);
                EventSystem.current.SetSelectedGameObject(entry.Value.GetComponentsInChildren<Button>().First<Button>().gameObject);
            }
            else
                entry.Value.SetActive(false);
        }
        currentActionPanel = "action";
    }

    void DeActivateCurrentPlayerActionsPanel()
    {
        foreach (KeyValuePair<string, GameObject> entry in playerActionsPanels)
        {
            if (entry.Key.Equals(battleManager.currentUnit.name))
                entry.Value.SetActive(false);
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
            if (entry.Key.Equals(battleManager.currentUnit.name))
            {
                entry.Value.SetActive(true);
                EventSystem.current.SetSelectedGameObject(entry.Value.GetComponentsInChildren<Button>().First<Button>().gameObject);
            }
            else
                entry.Value.SetActive(false);
        }
    }

    void DeActivateCurrentPlayerMagicsPanel()
    {
        foreach (KeyValuePair<string, GameObject> entry in playerMagicsPanels)
        {
            if (entry.Key.Equals(battleManager.currentUnit.name))
                entry.Value.SetActive(false);
        }
    }

    private void DisplayMagicsPanel()
    {
        GameObject currentPlayerMagicsPanel = GetPlayerMagicsPanel(battleManager.currentUnit.name);
        DisableMagicsBasedOnMp(currentPlayerMagicsPanel);
        currentPlayerMagicsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(currentPlayerMagicsPanel.GetComponentsInChildren<Button>().First<Button>().gameObject);
        currentActionPanel = "magic";
    }

    private void DisableMagicsBasedOnMp(GameObject currentPlayerMagicsPanel)
    {
        foreach (Button button in currentPlayerMagicsPanel.GetComponentsInChildren<Button>())
        {
            Ability ab = AbilityCollection.Instance.FindAbilityFromName(button.name);
            if (battleManager.currentUnit.GetComponent<BattleScript>().Character.GetStat(StatName.mpNow).baseValue < ab.mpCost)
                button.interactable = false;
        }
    }

    private void DisplayTargetsPanel(TargetType targetType)
    {
        DeActivateCurrentPlayerActionsPanel();
        DeActivateCurrentPlayerMagicsPanel();
        InstantiateTargetsPanel(targetType);
        currentActionPanel = "targets";
    }

    void InstantiateTargetsPanel(TargetType targetType)
    {
        targetsPanel = Instantiate(targetsPanelTemplate, battleCanvas.transform, false) as GameObject;
        List<GameObject> targets = targetType.Equals(TargetType.Opposite) ? battleManager.monsterUnits : battleManager.playerUnits;
        foreach (GameObject target in targets)
        {
            GameObject targetButton = Instantiate(targetButtonTemplate, targetsPanel.transform, false) as GameObject;
            targetButton.name = target.name;
            targetButton.transform.Find("TargetName").GetComponent<Text>().text = target.name;
            targetButton.GetComponent<Button>().onClick.AddListener(() => ClickedTarget(target));
        }
        EventSystem.current.SetSelectedGameObject(targetsPanel.GetComponentsInChildren<Button>().First<Button>().gameObject);
    }

    private void ClickedTarget(GameObject targetUnit)
    {
        Destroy(targetsPanel);
        battleManager.currentUnitAction.targets = new List<GameObject>() { targetUnit };
    }

    private void ClickedAbility(Ability ability)
    {
        DeActivateCurrentPlayerActionsPanel();
        DeActivateCurrentPlayerMagicsPanel();
        battleManager.currentUnitAction.ability = ability;
    }

    private void displayWinCanvas()
    {
        battleCanvas.SetActive(false);
        victoryCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(victoryCanvas.GetComponentsInChildren<Button>().First<Button>().gameObject);
        gameOverCanvas.SetActive(false);
    }

    private void displayGameOverCanvas()
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

    void Update()
    {
        if (Input.GetButtonDown("Cancel") && currentActionPanel != null)
        {
            if (currentActionPanel.Equals("magic"))
            {
                DeActivateCurrentPlayerMagicsPanel();
                ActivateCurrentPlayerActionsPanel();
            }
            else if (currentActionPanel.Equals("targets"))
            {
                Destroy(targetsPanel);
                ActivateCurrentPlayerActionsPanel();
                if (battleManager.currentUnitAction.ability.abilityType.Equals(AbilityType.Magic))
                    DisplayMagicsPanel();
                battleManager.currentUnitAction.ability = null;
            }
        }
    }


}

