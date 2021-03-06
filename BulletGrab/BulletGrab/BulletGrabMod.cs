using System;
using System.IO;
using Deli.Setup;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace BulletGrab
{
    public class BulletGrabMod : DeliBehaviour
    {

        private void Awake()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory + "\\Deli\\Mods\\BetterHands.deli";

            if (!File.Exists(dir))
            {
                Debug.Log("Better hands mod doesn't exist, exiting ...");
                return;
            }

            Harmony.CreateAndPatchAll(typeof(BulletGrabMod));
            Harmony.CreateAndPatchAll(typeof(OptionsCollector));
        }
        

        [HarmonyPatch(typeof(FVRFireArmChamber), "EjectRound")]
        [HarmonyPostfix]
        private static void EjectRoundPatch(FVRFireArmRound __result, FVRFireArmChamber __instance)
        {
            if (__instance == null) return;
            
            var weapon = __instance.Firearm;
            FVRViveHand handDoingTheAction;

            switch (weapon)
            {
                case Handgun handgun:
                    handDoingTheAction = handgun.Slide.m_hand;
                    
                    PlaceBulletInHand(__result, handDoingTheAction);
                    break;

                case BoltActionRifle boltAction:
                    handDoingTheAction = boltAction.BoltHandle.m_hand;

                    PlaceBulletInHand(__result, handDoingTheAction);
                    break;
                
                case ClosedBoltWeapon closedBolt:
                    handDoingTheAction = closedBolt.Bolt.m_hand;
                    
                    PlaceBulletInHand(__result, handDoingTheAction);
                    break;

                default:
                    Debug.Log("this type of weapon has not been implemented : " + __instance.Firearm.GetType());
                    break;
            }
        }

        private static void PlaceBulletInHand(FVRFireArmRound round, FVRViveHand hand)
        {
            if (hand == null) return;
            if (!PlayerIsPressingDown(hand)) return;
            
            var desiredQBSlot = hand.PoseOverride.GetComponentInChildren<FVRQuickBeltSlot>();
            
            if (desiredQBSlot == null) return;

            if (desiredQBSlot.CurObject == null)
            {
                round.ForceObjectIntoInventorySlot(desiredQBSlot);
                SM.PlayHandlingReleaseIntoSlotSound(round.HandlingReleaseIntoSlotSound, round.transform.position);
                return;
            }
            
            /* If there is something in the hand slot, it will try to group them if they are of the same
                type of round*/

            if (!(desiredQBSlot.CurObject is FVRFireArmRound palmedRound)) return;
            
            /* Take from FVRFireArmBullet.UpdateInteraction(); */
            
            if (palmedRound.RoundType == round.RoundType && palmedRound.ProxyRounds.Count < palmedRound.MaxPalmedAmount)
            {
                    Debug.Log("The bullets are the same type");
                    
                    palmedRound.AddProxy(round.RoundClass, round.ObjectWrapper);
                    palmedRound.UpdateProxyDisplay();
                    SM.PlayHandlingReleaseIntoSlotSound(round.HandlingReleaseIntoSlotSound, round.transform.position);
                    Destroy(round.gameObject);
            }
        }

        private static bool PlayerIsPressingDown(FVRViveHand hand)
        {
            return hand.Input.GripPressed && hand.Input.TriggerPressed;
        }
    }
}
