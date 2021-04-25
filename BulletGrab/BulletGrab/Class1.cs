using System;
using System.CodeDom;
using System.Collections;
using System.Threading;
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
            Harmony.CreateAndPatchAll(typeof(BulletGrabMod));
        }

        [HarmonyPatch(typeof(FVRFireArmChamber), "EjectRound")]
        [HarmonyPostfix]
        private static void EjectRoundPatch(FVRFireArmRound __result, FVRFireArmChamber __instance)
        {
            if (__result == null) return;

            _hands = GM.CurrentMovementManager.Hands;
            var weapon = __instance.Firearm;
            
            if (weapon is Handgun) HandgunMethod(__result);
            
            Debug.Log("Ejected Round");
        }

        private static void HandgunMethod(FVRFireArmRound round)
        {
            if (_hands[0].CurrentInteractable is HandgunSlide handgunSlideLeftHand)
            {
                if (!HandIsGrabbingBullet(_hands[0])) return;
                
                handgunSlideLeftHand.ForceBreakInteraction();
                _hands[0].RetrieveObject(round);
            }
            else if (_hands[1].CurrentInteractable is HandgunSlide handgunSlideRightHand)
            {
                if (!HandIsGrabbingBullet(_hands[1])) return;
                
                handgunSlideRightHand.ForceBreakInteraction();
                _hands[1].RetrieveObject(round);
            }
        }

        private static bool HandIsGrabbingBullet(FVRViveHand hand)
        {
            if (_controlMode == ControlOptions.CoreControlMode.Standard)
                return hand.Input.GripPressed && hand.Input.TriggerPressed;

            return hand.Input.BYButtonPressed;
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