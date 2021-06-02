using System;
using System.CodeDom;
using System.Collections;
using System.Threading;
using Deli.Setup;
using FistVR;
using HarmonyLib;
using UnityEngine;
using BetterHands;
using BetterHands.MagPalming;

namespace BulletGrab
{
    public class BulletGrabMod : DeliBehaviour
    {
        private static ControlOptions.CoreControlMode _controlMode;
        private static MagPalm _magPalm;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(BulletGrabMod));
        }

        [HarmonyPatch(typeof(FVRFireArmChamber), "EjectRound")]
        [HarmonyPostfix]
        private static void EjectRoundPatch(FVRFireArmRound __result, FVRFireArmChamber __instance)
        {
            if (__result == null) return;
            
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

        private static void HandgunMethod(FVRPhysicalObject round, Handgun handgun)
        {
            var handHoldingSlide = handgun.Slide.m_hand;

            if (handHoldingSlide == null) return;
            if (!PlayerIsPressingDown(handHoldingSlide)) return;
            
            var slot = handHoldingSlide.transform == GM.CurrentPlayerBody.RightHand 
                ? _magPalm.QBList[_magPalm.HandSlots[0]] 
                : _magPalm.QBList[_magPalm.HandSlots[1]];

            Debug.Log("Forcing object into slot");
            round.ForceObjectIntoInventorySlot(slot);
        }
        
        private static void BoltActionMethod(FVRPhysicalObject round, BoltActionRifle boltAction)
        {
            Debug.Log("Bolt action method");
            var handHoldingBolt = boltAction.BoltHandle.m_hand;

            if (handHoldingBolt) return;
            if (!PlayerIsPressingDown(handHoldingBolt))
            {
                Debug.Log("Player is not pressing down on bolt");
                return;
            }

            var slot = handHoldingBolt.transform == GM.CurrentPlayerBody.RightHand 
                ? _magPalm.QBList[_magPalm.HandSlots[0]] 
                : _magPalm.QBList[_magPalm.HandSlots[1]];

            Debug.Log("Forcing object into slot");
            round.ForceObjectIntoInventorySlot(slot);
        }
        
        private static bool PlayerIsPressingDown(FVRViveHand hand)
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

        [HarmonyPatch(typeof(MagPalmInput), "FVRViveHand_Update")]
        [HarmonyPostfix]
        private static void MagPalmPatch(ref MagPalm ___MP)
        {
            Debug.Log("Mag Palm patch worked");   
            if (___MP == null) Debug.Log("mp is null");

            _magPalm = ___MP;
        }
    }
}