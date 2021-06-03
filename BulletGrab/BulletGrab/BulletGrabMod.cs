using System;
using Deli.Setup;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace BulletGrab
{
    public class BulletGrabMod : DeliBehaviour
    {
        private static FVRQuickBeltSlot[] _quickBeltSlots;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(BulletGrabMod));
            Harmony.CreateAndPatchAll(typeof(OptionsCollector));
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
            
            Debug.Log("");
            Debug.Log("all of this hands children : ");
            
            foreach (Transform child in handHoldingSlide.PoseOverride)
            {
                Debug.Log("");
                Debug.Log(child.name);
                Debug.Log("their children : ");
                foreach (Transform subChildren in child)
                {
                    Debug.Log(subChildren.name);
                    if (subChildren == GM.CurrentPlayerBody.Torso.Find("QuickBeltSlot_BackPack")) Debug.Log("!!!! THIS IS A QUICK BELT SLOT");
                    if (subChildren.GetComponent<FVRQuickBeltSlot>() != null) Debug.Log("!!! THIS IS A QUICK BELT SLOT");
                }
                
            }
            

            Debug.Log("Forcing object into slot");
            // Add logic
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
            
            Debug.Log("Forcing object into slot");
            // Add logic
        }
        
        private static bool PlayerIsPressingDown(FVRViveHand hand)
        {
            if (OptionsCollector.ControlMode == ControlOptions.CoreControlMode.Standard)
                return hand.Input.GripPressed && hand.Input.TriggerPressed;
            
            return hand.Input.BYButtonPressed;
        }

        private static FVRQuickBeltSlot[] TryGetMagPalmSlots(FVRViveHand hand)
        {
            var array = new FVRQuickBeltSlot[2];
            
            if (hand.IsThisTheRightHand)
            {
                var handSlot = hand.PoseOverride.GetComponentInChildren<FVRQuickBeltSlot>();
                if (handSlot != null)
                {
                    Debug.Log("Found right hand quick belt slot");
                    array[0] = handSlot;
                }
                
                var otherHandSlot = hand.OtherHand.PoseOverride.GetComponentInChildren<FVRQuickBeltSlot>();
                if (otherHandSlot != null)
                {
                    Debug.Log("found left hand quick belt slot");
                    array[1] = handSlot;
                }
            }
            else
            {
                var handSlot = hand.PoseOverride.GetComponentInChildren<FVRQuickBeltSlot>();
                if (handSlot != null)
                {
                    Debug.Log("found left hand quick belt slot");
                    array[1] = handSlot;
                }
                
                var otherHandSlot = hand.OtherHand.PoseOverride.GetComponentInChildren<FVRQuickBeltSlot>();
                if (otherHandSlot != null)
                {
                    Debug.Log("Found right hand quick belt slot");
                    array[0] = handSlot;
                }
            }

            return array;
        }
        
    }
}