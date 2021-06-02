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
            
            switch (weapon)
            {
                case Handgun handgun:
                    HandgunMethod(__result, handgun);
                    break;
                
                case BoltActionRifle boltAction:
                    BoltActionMethod(__result, boltAction);
                    break;
            }
            
            Debug.Log("Ejected Round");
        }

        private static void HandgunMethod(FVRFireArmRound round, Handgun handgun)
        {
            var handHoldingSlide = handgun.Slide.m_hand;

            if (handHoldingSlide == null) return;
            if (!HandIsGrabbingBullet(handHoldingSlide)) return;
                
            handgun.Slide.ForceBreakInteraction();
            handHoldingSlide.RetrieveObject(round);
        }
        
        private static void BoltActionMethod(FVRFireArmRound round, BoltActionRifle boltAction)
        {
            var handHoldingBolt = boltAction.BoltHandle.m_hand;

            if (handHoldingBolt) return;
            if (!HandIsGrabbingBullet(handHoldingBolt)) return;

            boltAction.BoltHandle.ForceBreakInteraction();
            handHoldingBolt.RetrieveObject(round);
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