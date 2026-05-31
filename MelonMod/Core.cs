using Il2Cpp;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;
using UnityEngine.EventSystems;

[assembly: MelonInfo(typeof(Grime2Mod.Core), "Grime2Mod", "1.0.0", "jw11-modder", null)]
[assembly: MelonGame("Clover Bite", "GRIME II")]

namespace Grime2Mod
{
    public class Core : MelonMod
    {
        public static MelonMod Instance { get; private set; }
        private static MelonPreferences_Category MultiplierFloatCategory;
        private static MelonPreferences_Category MultiplierIntCategory;
        private static MelonPreferences_Category ToggleCategory;

        private static MelonPreferences_Entry<bool> configNoPlayerDamage;
        private static MelonPreferences_Entry<bool> configNoPlayerCooldown;
        private static MelonPreferences_Entry<bool> configInfiniteCharge;
        private static MelonPreferences_Entry<bool> configInfiniteBreath;
        private static MelonPreferences_Entry<bool> configInfiniteForce;
        private static MelonPreferences_Entry<bool> configInfiniteItems;
        private static MelonPreferences_Entry<bool> configInfinitePaint;
        private static MelonPreferences_Entry<bool> configFreeReset;

        private static MelonPreferences_Entry<float> configPlayerDamageMultiplier;
        private static MelonPreferences_Entry<float> configPlayerMovementMultiplier;
        private static MelonPreferences_Entry<float> configCurrencyMultiplier;
        private static MelonPreferences_Entry<float> configPlayerExperienceMultiplier;
        private static MelonPreferences_Entry<float> configPlayerHuntPointsMultiplier;

        private static MelonPreferences_Entry<KeyCode> configMenuToggle;

        private static MelonPreferences_Category ModConfCategory;
        private static List<MelonPreferences_Category> CustomCategoryList = new List<MelonPreferences_Category>();

        public static bool showCheatsPopup = false;

        private static GUIStyle JModStyleT = new();
        private static GUIStyle JModStyleH = new();
        private static GUIStyle JModStyleP = new();
        private static GUIStyle JModStylePV = new();
        private static GUIStyle JModStyleB = new();
        private static GUIStyle JModStyleBlank = new();

        private static Color JModColor = new(0.0f, 0.85f, 0.85f);

        private static Rect jModWindowRect;
        private static Rect _screenRect;

        private static CursorLockMode lastLockMode;
        private static bool lastVisibleState;

        public static GameObject CanvasRoot { get; private set; }

        private static EventSystem jModEventSys;
        private static EventSystem lastEventSys;
        private static BaseInputModule lastInputModule;

        public void LogHandler(string log, LogType level)
        {
            switch (level)
            {
                case LogType.Log:
                    base.LoggerInstance.Msg(log);
                    return;
                case LogType.Warning:
                case LogType.Assert:
                    base.LoggerInstance.Warning(log);
                    return;
                case LogType.Exception:
                case LogType.Error:
                    base.LoggerInstance.Error(log);
                    return;
            }
        }
        public static void Log(object message)
            => Log(message, LogType.Log);

        public static void LogWarning(object message)
            => Log(message, LogType.Warning);

        public static void LogError(object message)
            => Log(message, LogType.Error);

        internal static void Log(object message, LogType logType)
        {
            string log = message?.ToString() ?? "";

            switch (logType)
            {
                case LogType.Log:
                case LogType.Assert:
                    Instance.LoggerInstance.Msg(log); break;

                case LogType.Warning:
                    Instance.LoggerInstance.Warning(log); break;

                case LogType.Error:
                case LogType.Exception:
                    Instance.LoggerInstance.Error(log); break;
            }
        }
        public override void OnInitializeMelon()
        {
            Instance = this;
            MultiplierFloatCategory = MelonPreferences.CreateCategory("FloatMultipliers");
            MultiplierIntCategory = MelonPreferences.CreateCategory("IntMultipliers");
            ToggleCategory = MelonPreferences.CreateCategory("Toggles");

            configNoPlayerDamage = ToggleCategory.CreateEntry<bool>("configNoPlayerDamage", false, "Disable damage to player");
            configNoPlayerCooldown = ToggleCategory.CreateEntry<bool>("configNoPlayerCooldown", false, "No usables cooldown for player");
            configFreeReset = ToggleCategory.CreateEntry<bool>("configFreeReset", false, "No attribute reset cost");
            configInfiniteCharge = ToggleCategory.CreateEntry<bool>("configInfiniteCharge", false, "Weapon charge doesn't decrease");
            configInfiniteBreath = ToggleCategory.CreateEntry<bool>("configInfiniteBreath", false, "Breath charges always max");
            configInfinitePaint = ToggleCategory.CreateEntry<bool>("configInfinitePaint", false, "Paint charges always max");
            configInfiniteForce = ToggleCategory.CreateEntry<bool>("configInfiniteForce", false, "Force (stamina) is always max");
            configInfiniteItems = ToggleCategory.CreateEntry<bool>("configInfiniteItems", false, "Stackable items amount doesn't decrease");
            
            configPlayerDamageMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerDamageMultiplier", 1f, "Player damage multiplier", validator: new ValueRange<float>(1f, 20f));
            configPlayerMovementMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerMovementMultiplier", 1f, "Player movement speed multiplier", validator: new ValueRange<float>(1f, 5f));
            configCurrencyMultiplier = MultiplierFloatCategory.CreateEntry<float>("configCurrencyMultiplier", 1f, "Stackable items gain multiplier", validator: new ValueRange<float>(1f, 20f));
            configPlayerExperienceMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerExperienceMultiplier", 1f, "Experience gain multiplier", validator: new ValueRange<float>(1f, 20f));
            configPlayerHuntPointsMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerHuntPointsMultiplier", 1f, "Hunt points gain multiplier", validator: new ValueRange<float>(1f, 10f));

            JModStyleH.alignment = TextAnchor.MiddleCenter;
            JModStyleH.fontSize = 20;
            JModStyleH.fontStyle = FontStyle.Bold;
            JModStyleH.normal.textColor = JModColor;

            JModStyleP.fontSize = 16;
            JModStyleP.normal.textColor = JModColor;

            JModStylePV.fontSize = 16;
            JModStylePV.fontStyle = FontStyle.Bold;
            JModStylePV.normal.textColor = JModColor;
            JModStylePV.alignment = TextAnchor.MiddleCenter;

            ModConfCategory = MelonPreferences.CreateCategory("JModConfiguration");
            configMenuToggle = ModConfCategory.CreateEntry("ToggleKey", KeyCode.F7, "Main Menu Toggle Key");
            CustomCategoryList.Clear();
            foreach (var category in MelonPreferences.Categories)
            {
                switch (category.Identifier)
                {
                    case "FloatMultipliers":
                        {
                            MultiplierFloatCategory = category;
                            Log("Float Multipliers loaded!");
                            break;
                        }
                    case "IntMultipliers":
                        {
                            MultiplierIntCategory = category;
                            Log("Int Multipliers loaded!");
                            break;
                        }
                    case "Toggles":
                        {
                            ToggleCategory = category;
                            Log("Toggles loaded!");
                            break;
                        }
                    case "JModConfiguration":
                        {
                            break;
                        }
                    default:
                        {
                            CustomCategoryList.Add(category);
                            Log("Custom category: " + category.DisplayName + " loaded!");
                            break;
                        }

                }
            }

            Log("Menu key: " + configMenuToggle.Value.ToString());

            CanvasRoot = new GameObject("JModCanvas");
            UnityEngine.Object.DontDestroyOnLoad(CanvasRoot);
            CanvasRoot.hideFlags |= HideFlags.HideAndDontSave;
            CanvasRoot.layer = 5;
            CanvasRoot.transform.position = new Vector3(0f, 0f, 1f);

            CanvasRoot.SetActive(false);

            jModEventSys = CanvasRoot.AddComponent<EventSystem>();
            jModEventSys.enabled = false;

            CanvasRoot.SetActive(true);

            Log("Grime II Mod Initialized.");
        }

        //configNoPlayerDamage
        [HarmonyPatch(typeof(CharacterCombatHandler), nameof(CharacterCombatHandler.TakeDamage))]
        class CharacterCombatHandlerPatch1
        {
            static bool Prefix(ref CharacterCombatHandler __instance, ref float damageAmount)
            {
                if (!configNoPlayerDamage.Value || !__instance.isPlayer)
                {
                    return true;
                }
                if (__instance.isPlayer)
                {
                    //Log("Damage taken by player original value prefix: " + damageAmount);
                    damageAmount = 0;
                }
                return true;
            }

        }

        //configNoPlayerCooldown
        [HarmonyPatch(typeof(CharacterHandler), nameof(CharacterHandler.Update))]
        class CharacterHandlerPatch1
        {
            static bool Prefix(ref CharacterHandler __instance)
            {
                if (!configNoPlayerCooldown.Value || !__instance.isPlayer)
                {
                    return true;
                }
                if (__instance.getCurrentConsumable != null)
                    if (__instance.getCurrentConsumable.cooldownTime != 0)
                    {
                        //Log("Item cooldown original value prefix: " + __instance.getCurrentConsumable.cooldownTime);
                        __instance.getCurrentConsumable.cooldownTime = 0;
                        Data_Item_Usable_Consumable consumable = __instance.getCurrentConsumable.TryCast<Data_Item_Usable_Consumable>();
                        if (consumable != null)
                            consumable.consumedOnUse = false;
                    }
                return true;
            }

        }

        // Il2Cpp.CharacterScript_Player_MoldHandler
        [HarmonyPatch(typeof(CharacterScript_Player_MoldHandler), nameof(CharacterScript_Player_MoldHandler.GetUsableCooldown))]
        class Player_MoldHandlerPatch1
        {
            static void Postfix(ref CharacterScript_Player_MoldHandler __instance, ref float __result, Data_Item_Usable usable)
            {
                if (!configNoPlayerCooldown.Value)
                {
                    return;
                }

                if (__result > 0 && __instance.usableCooldowns != null)
                {
                    //Log("Consumable cooldown original value: " + __result);
                    if (__instance.usableCooldowns.ContainsKey(usable))
                        __instance.usableCooldowns[usable] = 0f;
                }
            }
        }

        // Il2Cpp.PlayerData_Inventory
        [HarmonyPatch(typeof(PlayerData_Inventory), nameof(PlayerData_Inventory.RemoveConsumableCharge))]
        class PlayerData_InventoryPatch1
        {
            static bool Prefix(ref PlayerData_Inventory __instance, ref int amount)
            {
                if (!configNoPlayerCooldown.Value)
                {
                    return true;
                }

                //Log("Consumable charge original value (-): " + amount);
                amount = 0;

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerData_Inventory), nameof(PlayerData_Inventory.RemoveStackableCharge))]
        class PlayerData_InventoryPatch2
        {
            static bool Prefix(ref PlayerData_Inventory __instance, ref int amount)
            {
                if (!configNoPlayerCooldown.Value)
                {
                    return true;
                }

                //Log("Stackable charge original value (-): " + amount);
                amount = 0;

                return true;
            }
        }


        //configInfiniteCharge
        // Il2Cpp.PlayerData_Inventory
        [HarmonyPatch(typeof(PlayerData_Inventory), nameof(PlayerData_Inventory.SetWeaponCharges))]
        class PlayerData_InventoryPatch3
        {
            static bool Prefix(ref PlayerData_Inventory __instance, ref float newAmount)
            {
                if (!configInfiniteCharge.Value)
                {
                    return true;
                }
                int maxcharge;
                Data_Item_Equipable_Weapon weapon = __instance.equippedItems.mainWeapon?.itemMeta?.item?.TryCast<Data_Item_Equipable_Weapon>();
                maxcharge = (weapon.maxCharges > 1) ? weapon.maxCharges : 1;
                if (newAmount < maxcharge)
                    newAmount = maxcharge;
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerData_Inventory), nameof(PlayerData_Inventory.ModifyMainWeaponCharge))]
        class PlayerData_InventoryPatch4
        {
            static bool Prefix(ref PlayerData_Inventory __instance, ref float amount)
            {
                if (!configInfiniteCharge.Value)
                    return true;
                if (amount < 0f)
                    amount = 0f;
                return true;
            }

        }

        //configInfiniteBreath
        //configInfinitePaint
        //configFreeReset

        [HarmonyPatch(typeof(CharacterScript_Player_AttributesHandler), nameof(CharacterScript_Player_AttributesHandler.Update))]
        class Player_AttributesHandlerPatch1
        {
            static bool Prefix(ref CharacterScript_Player_AttributesHandler __instance)
            {
                if (!configInfiniteBreath.Value && !configInfinitePaint.Value && !configFreeReset.Value)
                    return true;
                if ((__instance.currentBreath < __instance.maximumBreathBars * __instance.breathPerBar) && configInfiniteBreath.Value)
                    __instance.currentBreath = __instance.maximumBreathBars * __instance.breathPerBar;

                if ((__instance.currentPaint < __instance.maximumPaintBars * __instance.paintPerBar) && configInfinitePaint.Value)
                    __instance.currentPaint = __instance.maximumPaintBars * __instance.paintPerBar;

                if (__instance.resetAttributesCost > 0 && configFreeReset.Value)
                    __instance.resetAttributesCost = 0;
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerData_Attributes), nameof(PlayerData_Attributes.ResetAllAttributes))]
        class PlayerData_AttributesResetPatch1
        {
            static bool Prefix(ref PlayerData_Attributes __instance)
            {
                if (!configFreeReset.Value)
                    return true;
                CharacterScript_Player_AttributesHandler.instance?.resetAttributesCost = 0;
                return true;
            }
            static void Postfix(ref PlayerData_Attributes __instance)
            {
                if (!configFreeReset.Value)
                    return;
                CharacterScript_Player_AttributesHandler.instance?.resetAttributesCost = 0;
            }

        }



        //configInfiniteForce
        // Il2Cpp.CharacterScript_Player_AttributesHandler
        [HarmonyPatch(typeof(CharacterScript_Player_AttributesHandler), nameof(CharacterScript_Player_AttributesHandler.PauseForceRegen))]
        class Player_AttributesHandlerPatch2
        {
            static bool Prefix(ref float duration, ref CharacterScript_Player_AttributesHandler __instance)
            {
                if (!configInfiniteForce.Value)
                    return true;
                if (duration != 0f)
                    duration = 0f;
                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterScript_Player_AttributesHandler), nameof(CharacterScript_Player_AttributesHandler.AdjustForce))]
        class Player_AttributesHandlerPatch3
        {
            static bool Prefix(ref float amount, ref float regenStartDelayTime, ref CharacterScript_Player_AttributesHandler __instance)
            {
                if (!configInfiniteForce.Value)
                    return true;
                regenStartDelayTime = 0f;
                if (amount < 0f)
                    amount = 0f;
                return true;
            }
        }

        //configPlayerDamageMultiplier

        [HarmonyPatch(typeof(CharacterCombatHandler), nameof(CharacterCombatHandler.TakeHit))]
        class CharacterCombatHandlerPatch2
        {
            static bool Prefix(ref HitParameters hitParams, ref CharacterCombatHandler __instance)
            {
                if (configPlayerDamageMultiplier.Value <= 1)
                    return true;
                if (!__instance.isPlayer && hitParams != null)
                    hitParams.hitPower *= configPlayerDamageMultiplier.Value;
                return true;
            }
        }


        //configInfiniteItems

        //configCurrencyMultiplier
        
        [HarmonyPatch(typeof(PlayerData_Inventory), nameof(PlayerData_Inventory.ModifyStackableItem))]
        class Data_ItemPatch1
        {
            static bool Prefix(ref PlayerData_Inventory __instance, ref int amount)
            {
                if (configCurrencyMultiplier.Value <= 1 && !configInfiniteItems.Value)
                    return true;
                if (amount > 0 && configCurrencyMultiplier.Value > 1)
                    amount = Mathf.RoundToInt(amount * configCurrencyMultiplier.Value);
                if (amount < 0 && configInfiniteItems.Value)
                    amount = 0;
                return true;
            }
        }

        //configPlayerExperienceMultiplier

        [HarmonyPatch(typeof(Gameplay_PickableItem), nameof(Gameplay_PickableItem.Pickup))]
        class Gameplay_PickableItemPatch1
        {
            static bool Prefix(ref Gameplay_PickableItem __instance)
            {
                if (configPlayerExperienceMultiplier.Value <= 1)
                    return true;
                if (__instance.pickupType == Gameplay_PickableItem.PickupType.Exp && __instance.amount != 0)
                    __instance.amount = Mathf.RoundToInt(__instance.amount * configPlayerExperienceMultiplier.Value);
                return true;
            }
        }

        // Il2Cpp.Data_Character
        [HarmonyPatch(typeof(Data_Character), nameof(Data_Character.Init))]
        class Data_CharacterPatch1
        {
            static void Postfix(ref Data_Character __instance)
            {
                if (configPlayerExperienceMultiplier.Value <= 1)
                    return;
                if (__instance.expGiven != 0)
                    __instance.expGiven = Mathf.RoundToInt(__instance.expGiven * configPlayerExperienceMultiplier.Value);
            }
        }

        //configPlayerHuntPointsMultiplier

        [HarmonyPatch(typeof(PlayerData_Attributes), nameof(PlayerData_Attributes.ModifyHuntPoints))]
        class PlayerData_AttributesHuntPatch1
        {
            static bool Prefix(ref PlayerData_Attributes __instance, ref int amount)
            {
                if (configPlayerHuntPointsMultiplier.Value <= 1)
                    return true;
                if (amount > 0)
                    amount = Mathf.RoundToInt(amount * configPlayerHuntPointsMultiplier.Value);
                return true;
            }
        }

        //configPlayerMovementMultiplier

        [HarmonyPatch(typeof(CharacterMovementHandler), nameof(CharacterMovementHandler.UpdateGroundCharacterMovement))]
        class CharacterMovementHandlerPatch1
        {
            static void Postfix(ref CharacterMovementHandler __instance)
            {
                if (configPlayerMovementMultiplier.Value <= 1)
                    return;
                if (__instance.isPlayer)
                    if (__instance.characterMovementModule?.rootMotionMultiplier < configPlayerMovementMultiplier.Value)
                        __instance.characterMovementModule?.rootMotionMultiplier = configPlayerMovementMultiplier.Value;
            }
        }

        // MAIN MOD CLASSES

        public override void OnUpdate()
        {
            if (Event.current != null)
                if ((Event.current.keyCode == (configMenuToggle.Value)) && (Event.current.type == EventType.KeyDown))
                {
                    SwitchMenu();
                    //Log("Menu switched!");
                }
        }

        public override void OnGUI()
        {
            ShowMenu();
        }


        public static void SwitchMenu()
        {
            if (!showCheatsPopup)
            {
                lastLockMode = Cursor.lockState;
                lastVisibleState = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                lastEventSys = EventSystem.current;
                lastInputModule = EventSystem.current.currentInputModule;
                lastEventSys.enabled = false;
                lastInputModule.DeactivateModule();
                jModEventSys.enabled = true;
                jModEventSys.m_CurrentInputModule?.ActivateModule();
            }
            else
            {
                Cursor.lockState = lastLockMode;
                Cursor.visible = lastVisibleState;
                Event.current.Use();
                jModEventSys.enabled = false;
                jModEventSys.currentInputModule?.DeactivateModule();
                lastEventSys.enabled = true;
                lastInputModule.ActivateModule();
                lastEventSys.m_CurrentInputModule = lastInputModule;
                Event.current.Use();
                MelonPreferences.Save();
            }
            showCheatsPopup = !showCheatsPopup;
        }

        public static void ShowMenu()
        {
            if (showCheatsPopup)
            {
                JModStyleT = GUI.skin.GetStyle("toggle");
                JModStyleT.fontSize = 16;
                JModStyleT.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
                JModStyleT.onNormal.textColor = JModColor;

                JModStyleB = GUI.skin.GetStyle("box");
                JModStyleB.alignment = TextAnchor.UpperCenter;
                JModStyleB.fontSize = 24;
                JModStyleB.fontStyle = FontStyle.Bold;
                JModStyleB.normal.textColor = JModColor;

                jModWindowRect = new Rect(Screen.width / 2 - 425, Screen.height / 2 - 425, 850, 850);
                _screenRect = new Rect(0, 0, Screen.width, Screen.height);

                GUI.BeginGroup(jModWindowRect);
                for (int i = 0; i < 5; i++)
                    GUI.Box(new Rect(0, 0, 850, 850), "", JModStyleB);

                GUI.Box(new Rect(0, 0, 850, 850), "MOD OPTIONS", JModStyleB);

                var yAxis = 40;
                var xAxis = 20;
                GUI.Label(new Rect(xAxis, yAxis, 810, 20), "Toggle Mod Options", JModStyleH);
                yAxis += 35;
                ShowBoolMenu(ref xAxis, ref yAxis, ref ToggleCategory);
                yAxis += 10;
                GUI.Label(new Rect(xAxis, yAxis, 810, 20), "Multipliers", JModStyleH);
                yAxis += 45;
                ShowFloatMenu(ref xAxis, ref yAxis, ref MultiplierFloatCategory);
                ShowIntMenu(ref xAxis, ref yAxis, ref MultiplierIntCategory);
                yAxis += 15;

                if (GUI.Button(new Rect(325, 810, 200, 35), "Save settings and close"))
                {
                    SwitchMenu();
                    Event.current.Use();
                    Input.ResetInputAxes();
                }

                Vector2 mousePosition = Input.mousePosition;
                mousePosition.y = Screen.height - mousePosition.y;

                if (GUI.Button(_screenRect, string.Empty, JModStyleBlank))
                {
                    Event.current.Use();
                }

                if (jModWindowRect.Contains(mousePosition) && !((Event.current.keyCode == (configMenuToggle.Value)) && (Event.current.type == EventType.KeyDown)))
                {
                    Event.current.Use();
                }
                GUI.EndGroup();
            }
        }

        public static void ShowBoolMenu(ref int xAxis, ref int yAxis, ref MelonPreferences_Category cat)
        {
            foreach (MelonPreferences_Entry<bool> toggle in cat.Entries)
            {
                toggle.Value = GUI.Toggle(new Rect(xAxis, yAxis, 800, 20), toggle.Value, toggle.DisplayName, JModStyleT);
                xAxis = 20;
                yAxis += 35;
            }
        }

        public static void ShowFloatMenu(ref int xAxis, ref int yAxis, ref MelonPreferences_Category cat)
        {
            foreach (MelonPreferences_Entry<float> mult in cat.Entries)
            {
                string multLabel = mult.DisplayName;
                ValueRange<float> range;
                if (mult.Validator != null)
                    range = (ValueRange<float>)mult.Validator;
                else
                    range = new ValueRange<float>(1f, 20f);
                float step;
                if (range.MaxValue < 10)
                    step = 0.1f;
                else
                    step = 0.5f;
                multLabel += " (" + range.MinValue.ToString() + " - " + range.MaxValue.ToString() + ")";
                GUI.Label(new Rect(xAxis, yAxis, 680, 20), multLabel, JModStyleP);

                if (GUI.Button(new Rect(xAxis + 680, yAxis, 40, 20), " - "))
                {
                    if (mult.Value > range.MinValue)
                        mult.Value -= step;
                }
                GUI.Label(new Rect(xAxis + 730, yAxis, 40, 20), mult.Value.ToString("0.0"), JModStylePV);
                if (GUI.Button(new Rect(xAxis + 780, yAxis, 40, 20), " + "))
                {
                    if (mult.Value < range.MaxValue)
                        mult.Value += step;
                }

                yAxis += 35;
            }
        }
        public static void ShowIntMenu(ref int xAxis, ref int yAxis, ref MelonPreferences_Category cat)
        {
            foreach (MelonPreferences_Entry<int> mult in cat.Entries)
            {
                string multLabel = mult.DisplayName;
                ValueRange<int> range;
                if (mult.Validator != null)
                    range = (ValueRange<int>)mult.Validator;
                else
                    range = new ValueRange<int>(1, 20);
                multLabel += " (" + range.MinValue + " - " + range.MaxValue + ")";
                GUI.Label(new Rect(xAxis, yAxis, 680, 20), multLabel, JModStyleP);
                if (GUI.Button(new Rect(xAxis + 680, yAxis, 40, 20), " - "))
                {
                    if (mult.Value > range.MinValue)
                        mult.Value -= 1;
                }
                GUI.Label(new Rect(xAxis + 730, yAxis, 40, 20), mult.Value.ToString(), JModStylePV);
                if (GUI.Button(new Rect(xAxis + 780, yAxis, 40, 20), " + "))
                {
                    if (mult.Value < range.MaxValue)
                        mult.Value += 1;
                }
                yAxis += 35;
            }
        }

    }
}
