using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Doozy.Engine.UI;
using static SurvivalChaos.UpgradeScriptableData;
using Mirror;
using static SurvivalChaos.UpgradeScriptableData.StatUpgrade;

namespace SurvivalChaos
{
    public class SelectionUI : MonoBehaviour
    {
        [Header("General UI")]
        [SerializeField] TextMeshProUGUI generalNameText;
        //attack & defense
        [SerializeField] TextMeshProUGUI attackText;
        [SerializeField] TextMeshProUGUI attackLevelText;
        [SerializeField] TextMeshProUGUI defenseText;
        [SerializeField] TextMeshProUGUI defenseLevelText;

        //health & mana
        [SerializeField] TextMeshProUGUI healthText;
        [SerializeField] Image healthBar;
        [SerializeField] TextMeshProUGUI manaText;
        [SerializeField] Image manaBar;

        [Header("Views")]
        public UIView view_selectionButtonHolder;

        [Header("UI")]
        [SerializeField] GameObject selectionUI;
        [SerializeField] UI_GeneralSelection generalUI;
        [SerializeField] UI_MainBase mainBaseUI;
        [SerializeField] UI_Barracks barracksUI;
        [SerializeField] UI_Unit unitUI;
        [SerializeField] UI_Turret turretUI;

        [Header("GeneralUI")]
        [SerializeField] Button repairButton;
        [SerializeField] Button upgradeButton;
        [SerializeField] TextMeshProUGUI levelText;

        [Header("Barracks")]
        [SerializeField] UI_SendButton[] sendButtons; 

        [Header("MainBase")]
        [SerializeField] GameObject spellButtonHolder;
        [SerializeField] GameObject upgradeButtonHolder;

        [Header("GeneralRef")]
        [SerializeField] GameObject upgradeButtonPrefab;
        [SerializeField] GameObject statIconPrefab;
        [SerializeField] GameObject statIconsHolder;

        //[SerializeField] UI_UpgradeProgressBar ui_UpgradeProgressBar;

        //[Header("Turret Buttons")]

        Selectable selectable;
        GameNetworkManagerOld gameNetworkManager;

        //finish creating staticons pool
        private void Awake()
        {
            gameNetworkManager = ((GameNetworkManagerOld)NetworkManager.singleton);

            //create statIcons 
            var statTypes = Enum.GetValues(typeof(StatType));

            for (int i = 0; i < statTypes.Length; i++)
            {
                var statIconInstance = Instantiate(statIconPrefab, statIconsHolder.transform);

                var statIcon = statIconInstance.GetComponent<UI_StatIcon>();

                statIcon.statType = (StatType)statTypes.GetValue(i);

                statIconInstance.SetActive(false);
            }
        }

        private void OnEnable()
        {
            SelectionManager.OnObjectSelected += ActivateUI;
            SelectionManager.OnObjectDeselected += DeactivateUI;

            //MainBase.OnMainBaseSpawned += CreateMainBaseUpgradeButtons;
            MainBase.OnMainBaseSpawned += CreateMainBaseUpgradeButtons;
/*            UpgradesController.OnUpgrade += OnUpgradeUpdateButton;
            UpgradesController.NextUpgradeFailed += OnNextUpgradeFailed;
            UpgradesController.OnReachedMaxUpgrades += OnReachMaxUpgrades;
*/
            OnStartDeactivateUI();
        }

        private void OnDisable()
        {
            SelectionManager.OnObjectSelected -= ActivateUI;
            SelectionManager.OnObjectDeselected -= DeactivateUI;

            //MainBase.OnMainBaseSpawned -= CreateMainBaseUpgradeButtons;
            MainBase.OnMainBaseSpawned -= CreateMainBaseUpgradeButtons;
/*            UpgradesController.OnUpgrade -= OnUpgradeUpdateButton;
            UpgradesController.NextUpgradeFailed -= OnNextUpgradeFailed;
            UpgradesController.OnReachedMaxUpgrades -= OnReachMaxUpgrades;
*/
        }

        //general UI
        private void ActivateUI(Selectable selectable)
        {
            this.selectable = selectable;

            var rect = (RectTransform)view_selectionButtonHolder.transform;
            var view_selectionButtonHolderSizeDeltaX = 0f;

            //subscribe the object update events
            selectable.OnObjectHealthChanged += UI_SetObjectHealth;
            selectable.OnObjectManaChanged += UI_SetObjectMana;
            selectable.OnObjectDeath += DeactivateUI;

            //selectable.OnObjectUpgraded += SelectionUpgraded;

            ResetModelRenderCamPriorities();
            OnStartDeactivateUI();
            selectionUI.SetActive(true);
            generalUI.gameObject.SetActive(true);

            //PopulateGeneralUIInfo(selectable);
            PopulateGeneralUIInfo2(selectable);

            //pass logic with auth, visual without
            //seperate so its not aids on ur eyes
            switch (selectable)
            {
                case MainBase mainBase:
                    levelText.text = $"Level:{mainBase.GetUpgradeLevel()}";

                    //events
                    mainBase.upgradeController.Event_UpgradeTimerStarted += OnUpgradeTimerStarted;
                    mainBase.upgradeController.Event_UpgradeTimerUpdated += OnUpgradeTimerUpdated;
                    mainBase.upgradeController.Event_OnUpgrade += OnUpgrade;
                    mainBase.upgradeController.SyncList_UpgradeLevels.Callback += OnUpgradeLevelsUpdated;
                    mainBase.upgradeController.SyncDict_ActiveUpgradeData.Callback += OnSyncUpgradeDataSetsDictUpdated;

                    UI_SelectionRender.instance.buildingsModelRenderCamera.Priority = 0;

                    //make a UIView
                    mainBaseUI.gameObject.SetActive(true);

                    view_selectionButtonHolder.Show();
                    view_selectionButtonHolderSizeDeltaX = 383.9334f;

                    if (!mainBase.hasAuthority) return;

                    upgradeButton.onClick.AddListener(mainBase.CmdUpgradeObject);

                    break;
                
                case Barracks barracks:
                    levelText.text = $"Level:{barracks.GetUpgradeLevel()}";

                    UI_SelectionRender.instance.buildingsModelRenderCamera.Priority = 0;

                    barracksUI.gameObject.SetActive(true);

                    view_selectionButtonHolder.Show();
                    view_selectionButtonHolderSizeDeltaX = 397.3292f;

                    if (!barracks.hasAuthority) return;

                    upgradeButton.onClick.AddListener(barracks.ServerUpgradeObject);
                    repairButton.onClick.AddListener(barracks.CmdRepairBarracksOverTime);

                    foreach (var button in sendButtons)
                    {
                        foreach (var unit in (barracks.scriptableObjectData as BarracksScriptableData).units)
                        {
                            if (button.sendType == unit.scriptableObjectData.identifierType)
                            {
                                button.GetComponent<Button>().onClick.AddListener(() => barracks.TrySendUnit(button.sendType));
                                button.selectableReference = unit;
                            }
                        }
                    }

                    break;

                case Turret turret:
                    levelText.gameObject.SetActive(false);

                    UI_SelectionRender.instance.buildingsModelRenderCamera.Priority = 0;

                    turretUI.gameObject.SetActive(true);

                    view_selectionButtonHolder.Show();
                    view_selectionButtonHolderSizeDeltaX = 420f;

                    repairButton.gameObject.SetActive(false);
                    upgradeButton.gameObject.SetActive(false);

                    break;

                case Unit unit:
                    levelText.gameObject.SetActive(false);

                    //if (!unit.hasAuthority) return;
                    UI_SelectionRender.instance.unitsModelRenderCamera.Priority = 0;
                    view_selectionButtonHolder.Hide();

                    unitUI.gameObject.SetActive(true);

                    repairButton.gameObject.SetActive(false);
                    upgradeButton.gameObject.SetActive(false);

                    break;
            }

            LeanTween.value(rect.gameObject, rect.sizeDelta.x, view_selectionButtonHolderSizeDeltaX, .15f).setEase(LeanTweenType.easeInSine).setOnUpdate((Vector2 val) => {
                ((RectTransform)view_selectionButtonHolder.transform).sizeDelta = new Vector2(val.x, rect.sizeDelta.y);
            });
        }

        //might not need
        private void UI_SetObjectInfo(string text, object value)
        {
            text = value.ToString();
        }

        private void PopulateGeneralUIInfo2(Selectable selectable)
        {
            repairButton.gameObject.SetActive(true);
            upgradeButton.gameObject.SetActive(true);

            foreach (var item in selectable.SyncDict_ActiveStatData)
            {
                var statType = item.Key;
                var value = item.Value;

                for (int i = 0; i < statIconsHolder.transform.childCount; i++)
                {
                    var child = statIconsHolder.transform.GetChild(i);
                    var statIcon = child.GetComponent<UI_StatIcon>();

                    //data from the selected object
                    var selectableStatIcons = selectable.scriptableObjectData.statIcons;

                    if (statIcon.statType != statType) continue;

                    statIcon.statText.text = value.ToString();

                    statIcon.iconImage.sprite = selectableStatIcons.iconData[i].icon;

                    statIcon.gameObject.SetActive(true);
                }
            }
        }

        private void PopulateGeneralUIInfo(Selectable selectable)
        {
            repairButton.gameObject.SetActive(true);
            upgradeButton.gameObject.SetActive(true);

            UI_SetObjectName(selectable.GetName());
            UI_SetObjectAttack(selectable.GetAttack());
            UI_SetObjectAttackLevel(selectable.GetAttackLevel());
            UI_SetObjectDefense(selectable.GetDefense());
            UI_SetObjectDefenseLevel(selectable.GetDefenseLevel());
            UI_SetObjectHealth(selectable.GetCurrentHealth(), selectable.GetMaxHealth());
            UI_SetObjectMana(selectable.GetCurrentMana(), selectable.GetMaxMana());
        }
        private void DeactivateUI(Selectable selectable)
        {
            //should only unsub when selectable becomes destroyed
            selectable.OnObjectHealthChanged -= UI_SetObjectHealth;
            selectable.OnObjectManaChanged -= UI_SetObjectMana;
            selectable.OnObjectDeath -= DeactivateUI;
            //selectable.OnObjectUpgraded -= SelectionUpgraded;

            switch (selectable)
            {
                case MainBase mainBase:

                    mainBase.upgradeController.Event_UpgradeTimerStarted -= OnUpgradeTimerStarted;
                    mainBase.upgradeController.Event_UpgradeTimerUpdated -= OnUpgradeTimerUpdated;
                    mainBase.upgradeController.Event_OnUpgrade -= OnUpgrade;

                    break;
            }

            foreach (var button in sendButtons)
                button.GetComponent<Button>().onClick.RemoveAllListeners();

            foreach (Transform child in statIconsHolder.transform)
                child.gameObject.SetActive(false);

            repairButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.RemoveAllListeners();

            if (barracksUI.gameObject.activeInHierarchy) barracksUI.gameObject.SetActive(false);
            if (unitUI.gameObject.activeInHierarchy) unitUI.gameObject.SetActive(false);

            selectionUI.SetActive(false);
            generalUI.gameObject.SetActive(false);

            this.selectable = null;
        }
        private void OnStartDeactivateUI()
        {
            mainBaseUI.gameObject.SetActive(false);
            turretUI.gameObject.SetActive(false);
            barracksUI.gameObject.SetActive(false);
            unitUI.gameObject.SetActive(false);
            selectionUI.SetActive(false);
            generalUI.gameObject.SetActive(false);

            foreach (var button in sendButtons)
            {
                button.GetComponent<Button>().onClick.RemoveAllListeners();
            }

            repairButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.RemoveAllListeners();
        }

        private void UI_SetObjectName(string name)
        {
            generalNameText.text = name;
        }
        private void UI_SetObjectAttack(int attack)
        {
            attackText.text = attack.ToString();
        }
        private void UI_SetObjectAttackLevel(int attackLevel)
        {
            attackLevelText.text = attackLevel.ToString();
        }
        private void UI_SetObjectDefense(int defense)
        {
            defenseText.text = defense.ToString();
        }
        private void UI_SetObjectDefenseLevel(int defenseLevel)
        {
            defenseLevelText.text = defenseLevel.ToString();
        }
        private void UI_SetObjectHealth(float health, float maxHealth)
        {
            healthText.text = health.ToString();

            var normalizedHealth = GetNormalizedValue(health, maxHealth);

            healthBar.fillAmount = normalizedHealth;
        }
        private void UI_SetObjectMana(float mana, float maxMana)
        {
            manaText.text = mana.ToString();

            var normalizedMana = GetNormalizedValue(mana, maxMana);

            manaBar.fillAmount = normalizedMana;
/*            if (normalizedMana == -1)
            {
                manaText.gameObject.SetActive(false);
                manaBar.gameObject.SetActive(false);
            }
*/        }
        private void ResetModelRenderCamPriorities()
        {
            UI_SelectionRender.instance.unitsModelRenderCamera.Priority = -1;
            UI_SelectionRender.instance.buildingsModelRenderCamera.Priority = -1;
        }

        //upgrade UI
/*        private void CreateMainBaseUpgradeButtons(MainBase mainBase)
        {
            //itterate over array of upgrade types
            for (int i = 0; i < mainBase.upgradeController.upgradesData.Length; i++)
            {
                var upgradeData = mainBase.upgradeController.upgradesData[i];

                var buttonInstance = Instantiate(upgradeButtonPrefab, upgradeButtonHolder.transform);
                var button = buttonInstance.GetComponent<Button>();
                var upgradeButton = buttonInstance.GetComponent<UI_UpgradeButton>();

                var upgradeIncrement = mainBase.upgradeController.upgradeLevels[i];

                //upgradeButton.upgrades.AddRange(upgradeData.upgrades);
                upgradeButton.orderId = i;
                //upgradeButton.reference = mainBase;

                button.onClick.AddListener(() => mainBase.upgradeController.TryUpgrade(i));

                for (int j = 0; j < upgradeData.upgrades.Length; j++)
                {
                    var upgrade = upgradeData.upgrades[j];

                    if(j < mainBase.upgradeController.upgradeLevels.Count)
                    {
                        if(upgradeIncrement == j)
                        {
                            if(mainBase.GetUpgradeLevel() < upgrade.prerequisiteData.UpgradeLevel)
                            {
                                OnNextUpgradeFailed(upgradeButton);
                            }
                            //UpdateUpgradeButtonVisuals(upgradeButton);
                        }
                    }
                }
            }
        }
*/        
        private void CreateMainBaseUpgradeButtons(MainBase mainBase)
        {
            //create buttons for the ammount of upgrades and give the button its order id
            for (int i = 0; i < mainBase.upgradeController.upgradesData.Length; i++)
            {
                var buttonInstance = Instantiate(upgradeButtonPrefab, upgradeButtonHolder.transform);
                var button = buttonInstance.GetComponent<Button>();
                var upgradeButton = buttonInstance.GetComponent<UI_UpgradeButton>();

                var statUpgrade = mainBase.upgradeController.upgradesData[i].upgrades[mainBase.upgradeController.SyncList_UpgradeLevels[i]];

                upgradeButton.orderId = i;

                UpdateUpgradeButtonVisuals(upgradeButton, statUpgrade);

                button.onClick.AddListener(() => mainBase.upgradeController.TryUpgrade(upgradeButton.orderId));
                //print($"{button.onClick} | {button}");
            }
        }

        //should update to next upgrades image
        private void UpdateUpgradeButtonVisuals(UI_UpgradeButton upgradeButton, StatUpgrade upgrade)
        {
            upgradeButton.iconImage.sprite = upgrade.Icon;
        }
        private void OnNextUpgradeFailed(UI_UpgradeButton upgradeButton)
        {
            //on object upgrade, should call event that turns on all these buttons with == or <
            upgradeButton.GetComponent<Button>().interactable = false;
        }


        //
        /*private void UpdateUpgradeButtonVisuals(UI_UpgradeButton upgradeButton)
        {
            var upgradeIncrements = (upgradeButton.reference as MainBase).upgradeController.upgradeLevels;
            var upgradeIncrement = upgradeIncrements[upgradeButton.orderId];

            if (upgradeButton.gameObject.activeSelf)
            {
                var upgrade = upgradeButton.upgrades[upgradeIncrement];

                //upgradeButton.countText.text = (upgradeIncrement).ToString();
                upgradeButton.iconImage.sprite = upgrade.Icon;
            }
        }*/
        private void OnUpgradeUpdateButton(UI_UpgradeButton upgradeButton)
        {
            //UpdateUpgradeButtonVisuals(upgradeButton);
        }
        //sets/updates all button.interactable to true when newUpgradeLevel is set on network 
        /*private void SelectionUpgraded(int newUpgradeLevel)
        {
            levelText.text = $"Level:{newUpgradeLevel}";

            for (int i = 0; i < upgradeButtonHolder.transform.childCount; i++)
            {
                var child = upgradeButtonHolder.transform.GetChild(i);
                var button = child.GetComponent<Button>();
                var upgradeButton = child.GetComponent<UI_UpgradeButton>();
                var upgradeIncrements = (upgradeButton.reference as MainBase).upgradeController.upgradeLevels;
                var upgradeIncrement = upgradeIncrements[upgradeButton.orderId];

                if (button.interactable == false)
                {
                    if (upgradeButton.upgrades[upgradeIncrement].prerequisiteData.UpgradeLevel == newUpgradeLevel)
                    {
                        button.interactable = true;
                    }
                }
            }
        }*/
        //


        float tempMaxUpgradeTimer;
        private void OnUpgradeTimerUpdated(int upgradeTimer)
        {
            tempMaxUpgradeTimer = upgradeTimer;
        }

        private void OnUpgradeTimerStarted(bool upgradeTimerStarted, ActiveUpgradeData activeUpgradeData)
        {
            //called when the timer is finished to turn all buttons back on
            if (upgradeTimerStarted == false)
            {
                for (int i = 0; i < upgradeButtonHolder.transform.childCount; i++)
                {
                    var child = upgradeButtonHolder.transform.GetChild(i);

                    var button = child.GetComponent<Button>();
                    var upgradeButton = child.GetComponent<UI_UpgradeButton>();

                    button.interactable = true;

                    if (upgradeButton.orderId == activeUpgradeData.orderId)
                    {
                        if(activeUpgradeData.nextStatUpgrade == null)
                        {
                            OnReachMaxUpgrades(upgradeButton);
                            continue;
                        }

                        //update to the next upgrade icon
                        UpdateUpgradeButtonVisuals(upgradeButton, activeUpgradeData.nextStatUpgrade);
                    }
                }

                return;
            }

            var maxUpgradeTimer = tempMaxUpgradeTimer;

            //turn off all upgrade buttons at start of upgrade
            foreach (Transform child in upgradeButtonHolder.transform)
            {
                var button = child.GetComponent<Button>();
                var upgradeButton = child.GetComponent<UI_UpgradeButton>();

                button.interactable = false;

                if (upgradeButton.orderId == activeUpgradeData.orderId)
                {
                    LeanTween.value(upgradeButton.gameObject, tempMaxUpgradeTimer, 0, tempMaxUpgradeTimer).setOnUpdate((float val) =>
                    {
                        upgradeButton.coolDownTimer.fillAmount = val / maxUpgradeTimer;
                    });
                }
            }
        }
        //might not need to call cuz have synclist update event
        private void OnUpgrade(ActiveUpgradeData activeUpgradeData)
        {

        }

        private void OnSyncUpgradeDataSetsDictUpdated(SyncDictionary<IdentifierType, int[]>.Operation op, IdentifierType key, int[] items)
        {
            print($"On Sync Upgrade Data Sets Dict Updated: {op} | updating key - {key} | items - {items}");

            switch (op)
            {
                case SyncIDictionary<IdentifierType, int[]>.Operation.OP_ADD:
                    // entry added
                    break;
                case SyncIDictionary<IdentifierType, int[]>.Operation.OP_SET:
                    // entry changed
                    break;
                case SyncIDictionary<IdentifierType, int[]>.Operation.OP_REMOVE:
                    // entry removed
                    break;
                case SyncIDictionary<IdentifierType, int[]>.Operation.OP_CLEAR:
                    // Dictionary was cleared
                    break;
            }
        }

        private void OnUpgradeLevelsUpdated(SyncList<int>.Operation op, int index, int oldUpgradeLevel, int newUpgradeLevel)
        {
            //print($"Updating upgradeLevels SyncList: {op} | updating index - {index} | with newUpgradeLevel - {newUpgradeLevel}");

            switch (op)
            {
                case SyncList<int>.Operation.OP_ADD:
                    break;
                case SyncList<int>.Operation.OP_CLEAR:
                    break;
                case SyncList<int>.Operation.OP_INSERT:
                    break;
                case SyncList<int>.Operation.OP_REMOVEAT:
                    break;
                case SyncList<int>.Operation.OP_SET:
                    break;
                default:
                    break;
            }
        }

        private void OnReachMaxUpgrades(UI_UpgradeButton upgradeButton)
        {
            print($"Reached max upgrades!");
            upgradeButton.gameObject.SetActive(false);
        }

        //utility
        public static float GetNormalizedValue(float value, float maxValue)
        {
            return maxValue != 0 ? value / maxValue : -1;
        }
    }
}