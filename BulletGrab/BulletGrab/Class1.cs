using System;
using System.CodeDom;
using Deli.Setup;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace BulletGrab
{
    public class BulletGrabMod : DeliBehaviour
    {
        private static ControlOptions.CoreControlMode _controlMode;
        private static FVRViveHand[] _hands;

        private void Awake()
        {
            Debug.Log("Bullet Grab Mod works!");

            Harmony.CreateAndPatchAll(typeof(BulletGrabMod));
        }

        [HarmonyPatch(typeof(FVRFireArmChamber), "EjectRound")]
        [HarmonyPostfix]
        private static void EjectRoundPatch(FVRFireArmRound __result, FVRFireArmChamber __instance)
        {
            if (__result == null)
            {
                Debug.Log("round is null");
                return;
            }

            _hands = GM.CurrentMovementManager.Hands;
            var weapon = __instance.Firearm;
            
            if (weapon is Handgun)
            {
                Debug.Log("This weapon is a handgun");
                HandgunMethod(__result);
            }
            else
            {
                Debug.Log("Weapon is not handgun");
            }
            
            Debug.Log("Ejected Round");
        }

        private static void HandgunMethod(FVRFireArmRound round)
        {
            if (_hands[0].CurrentInteractable is HandgunSlide handgunSlideLeftHand)
            {
                Debug.Log("Left hand is holding handgun slide");
                handgunSlideLeftHand.ForceBreakInteraction();
                _hands[0].RetrieveObject(round);
            }
            else if (_hands[1].CurrentInteractable is HandgunSlide handgunSlideRightHand)
            {
                Debug.Log("Right hand is holding handugn slide");
                handgunSlideRightHand.ForceBreakInteraction();
                _hands[1].RetrieveObject(round);
            }
            else
            {
                Debug.Log("No hand is holding the slide.");
            }
        }










        /* These patches get the current control mode */
        [HarmonyPatch(typeof(GameOptions), "InitializeFromSaveFile")]
        [HarmonyPrefix]
        private static void InitialOptionsGrabPatch(GameOptions __instance)
        {
            _controlMode = __instance.ControlOptions.CCM;
        }
        
        [HarmonyPatch(typeof(GameOptions), "SaveToFile")]
        [HarmonyPrefix]
        private static void UpdateControlOptionsPatch(GameOptions __instance)
        {
            _controlMode = __instance.ControlOptions.CCM;
        }
    }
}