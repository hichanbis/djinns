﻿using UnityEngine;
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
    public GameObject cursorTemplate;

    public GameObject cursor;

    private GameObject playersInfoPanel;
    private Dictionary<string, GameObject> playerActionsPanels;
    private Dictionary<string, GameObject> playerMagicsPanels;
    private List<GameObject> playerUnits;
    private GameObject targetsPanel;


    private UnityAction playerUnitsExistListener;
    private UnityAction playerMustChooseAbilityListener;
    private UnityAction rageAppliedListener;
    private UnityAction winListener;
    private UnityAction loseListener;
    private UnityAction cancelTargetListener;
    private UnityAction displayMagicsPanelListener;

    public GameObject battleCanvas, victoryCanvas, gameOverCanvas;


    // Use this for initialization
    void Start()
    {
        battleManager = BattleManager.Instance;

        battleCanvas.SetActive(true);
        victoryCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);

        cursor = Instantiate(cursorTemplate, battleCanvas.transform, false);
        cursor.SetActive(false);

        playerActionsPanels = new Dictionary<string, GameObject>();
        playerMagicsPanels = new Dictionary<string, GameObject>();

        playerUnitsExistListener = new UnityAction(InitializePlayerPanels);
        EventManager.StartListening(BattleEventMessages.InitBattle.ToString(), playerUnitsExistListener);

        playerMustChooseAbilityListener = new UnityAction(ActivateCurrentPlayerActionsPanel);
        EventManager.StartListening(BattleEventMessages.PlayerChoiceExpected.ToString(), playerMustChooseAbilityListener);

        rageAppliedListener = new UnityAction(UpdatePlayersInfo);
        EventManager.StartListening(BattleEventMessages.DamageApplied.ToString(), rageAppliedListener);

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
        playerUnits = battleManager.playerUnits;
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

    void UpdatePlayerInfo(GameObject playerUnit)
    {
        Character playerChar = playerUnit.GetComponent<BattleScript>().character;

        Transform playerInfoTr = playersInfoPanel.transform.Find(playerUnit.name + "PlayerInfo").transform;
        playerInfoTr.Find("PlayerName").GetComponentInChildren<Text>().text = playerChar.name;
        playerInfoTr.Find("HpMp").transform.Find("Hp").GetComponentInChildren<Text>().text = playerChar.GetStat(StatName.hpNow).baseValue.ToString() + " / " + playerChar.GetStat(StatName.hp).baseValue.ToString();
        playerInfoTr.Find("HpMp").transform.Find("Mp").GetComponentInChildren<Text>().text = playerChar.GetStat(StatName.mpNow).baseValue.ToString() + " / " + playerChar.GetStat(StatName.mp).baseValue.ToString();
    }

    void UpdatePlayersInfo()
    {
        playerUnits = battleManager.playerUnits;
        for (int i = 0; i < playerUnits.Count; i++)
        {
            UpdatePlayerInfo(playerUnits[i]);
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
            List<Ability> abilities = playerUnit.GetComponent<BattleScript>().character.Abilities;
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
            actionButton.GetComponent<Button>().onClick.AddListener(() => ClickedAbility(AbilityCollection.Instance.FindAbilityFromId("Guard")));
        else if (actionName.Equals("Magic"))
            actionButton.GetComponent<Button>().onClick.AddListener(() => DisplayMagicsPanel());
        else if (actionName.Equals("Attack"))
        {
            actionButton.GetComponent<Button>().onClick.AddListener(() => ClickedAbility(AbilityCollection.Instance.FindAbilityFromId("Attack")));
            actionButton.GetComponent<Button>().onClick.AddListener(() => DisplayTargetsPanel(TargetType.Opposite));
        }

    }

    void ActivateCurrentPlayerActionsPanel()
    {
        foreach (KeyValuePair<string, GameObject> entry in playerActionsPanels)
        {
            if (entry.Key.Equals(battleManager.currentChoosingUnit.name))
            {
                entry.Value.SetActive(true);
                EventSystem.current.SetSelectedGameObject(entry.Value.GetComponentsInChildren<Button>().First<Button>().gameObject);
                cursor.SetActive(true);

            }
            else
                entry.Value.SetActive(false);
        }

    }

    void DeActivateCurrentPlayerActionsPanel()
    {
        foreach (KeyValuePair<string, GameObject> entry in playerActionsPanels)
        {
            if (entry.Key.Equals(battleManager.currentChoosingUnit.name))
            {
                entry.Value.SetActive(false);
                cursor.SetActive(false);
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
            if (entry.Key.Equals(battleManager.currentChoosingUnit.name))
            {
                entry.Value.SetActive(true);
                EventSystem.current.SetSelectedGameObject(entry.Value.GetComponentsInChildren<Button>().First<Button>().gameObject);
                cursor.SetActive(true);
            }
            else
                entry.Value.SetActive(false);
        }
    }

    void DeActivateCurrentPlayerMagicsPanel()
    {
        foreach (KeyValuePair<string, GameObject> entry in playerMagicsPanels)
        {
            if (entry.Key.Equals(battleManager.currentChoosingUnit.name))
                entry.Value.SetActive(false);
        }
    }

    private void DisplayMagicsPanel()
    {
        GameObject currentPlayerMagicsPanel = GetPlayerMagicsPanel(battleManager.currentChoosingUnit.name);
        DisableMagicsBasedOnMp(currentPlayerMagicsPanel);
        cursor.SetActive(true);
        currentPlayerMagicsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(currentPlayerMagicsPanel.GetComponentsInChildren<Button>().First<Button>().gameObject);

    }

    private void DisableMagicsBasedOnMp(GameObject currentPlayerMagicsPanel)
    {
        foreach (Button button in currentPlayerMagicsPanel.GetComponentsInChildren<Button>())
        {
            Ability ab = AbilityCollection.Instance.FindAbilityFromName(button.name);
            if (battleManager.currentChoosingUnit.GetComponent<BattleScript>().character.GetStat(StatName.mpNow).baseValue < ab.mpCost)
                button.interactable = false;
        }
    }

    private void DisplayTargetsPanel(TargetType targetType)
    {
       
        DeActivateCurrentPlayerActionsPanel();
        DeActivateCurrentPlayerMagicsPanel();
        cursor.SetActive(true);
        InstantiateTargetsPanel(targetType);

    }

    //if all on grise le tout mais on attend le submit quand même
    void InstantiateTargetsPanel(TargetType targetType)
    {
        targetsPanel = Instantiate(targetsPanelTemplate, battleCanvas.transform, false) as GameObject;
        List<GameObject> targets = null;

        if (targetType.Equals(TargetType.Opposite) || targetType.Equals(TargetType.AllOpposite))
            targets = battleManager.monsterUnits;
        else
            targets = battleManager.playerUnits;

        if (targetType.Equals(TargetType.AllOpposite) || targetType.Equals(TargetType.AllSame))
        {
            GameObject targetButton = Instantiate(targetButtonTemplate, targetsPanel.transform, false) as GameObject;
            String targetName = targetType.Equals(TargetType.AllSame) ? "All Players" : "All Enemies";
            targetButton.name = targetName;
            targetButton.transform.Find("TargetName").GetComponent<Text>().text = targetName;
            targetButton.GetComponent<Button>().onClick.AddListener(() => ClickedTarget(targets));
            targetButton.GetComponent<Button>().Select();
        }

        foreach (GameObject target in targets)
        {
            GameObject targetButton = Instantiate(targetButtonTemplate, targetsPanel.transform, false) as GameObject;
            targetButton.name = target.name;
            targetButton.transform.Find("TargetName").GetComponent<Text>().text = target.name;
            if (targetType.Equals(TargetType.Opposite) || targetType.Equals(TargetType.Same))
                targetButton.GetComponent<Button>().onClick.AddListener(() => ClickedTarget(new List<GameObject>() { target }));
            else if (targetType.Equals(TargetType.AllOpposite) || targetType.Equals(TargetType.AllSame))
                targetButton.GetComponent<Button>().interactable = false;
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

    private void ClickedTarget(List<GameObject> targetUnits)
    {
        Destroy(targetsPanel);
        cursor.SetActive(false);
        battleManager.currentUnitAction.targets = targetUnits;
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

