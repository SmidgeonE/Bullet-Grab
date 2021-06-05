using System;
using Deli.Setup;
using FistVR;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

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
            var weapon = __instance.Firearm;
            FVRViveHand handDoingTheAction;
            
            _quickBeltSlots = TryGetMagPalmSlots();

            if (_quickBeltSlots[0] == null || _quickBeltSlots[1] == null) return;

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

            Console.WriteLine("Ejected Round");
        }

        private static void PlaceBulletInHand(FVRFireArmRound round, FVRViveHand hand)
        {
            if (hand == null) return;
            if (!PlayerIsPressingDown(hand)) return;
            
            var desiredQBSlot = hand.IsThisTheRightHand ? _quickBeltSlots[0] : _quickBeltSlots[1];

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
            if (OptionsCollector.ControlMode == ControlOptions.CoreControlMode.Standard)
                return hand.Input.GripPressed && hand.Input.TriggerPressed;

            return hand.Input.BYButtonPressed;
        }

        private static FVRQuickBeltSlot[] TryGetMagPalmSlots()
        {
            var hand = GM.CurrentPlayerBody.RightHand.GetComponent<FVRViveHand>();
            var array = new FVRQuickBeltSlot[2];
            FVRQuickBeltSlot currentHandSlot;
            FVRQuickBeltSlot otherHandSlot;

            if (hand.IsThisTheRightHand) {
                currentHandSlot = hand.PoseOverride.GetComponentInChildren<FVRQuickBeltSlot>();
                if (currentHandSlot != null) array[0] = currentHandSlot;

                otherHandSlot = hand.OtherHand.PoseOverride.GetComponentInChildren<FVRQuickBeltSlot>();
                if (otherHandSlot != null) array[1] = otherHandSlot;
            }
            else {
                currentHandSlot = hand.PoseOverride.GetComponentInChildren<FVRQuickBeltSlot>();
                if (currentHandSlot != null) array[1] = currentHandSlot;

                otherHandSlot = hand.OtherHand.PoseOverride.GetComponentInChildren<FVRQuickBeltSlot>();
                if (otherHandSlot != null) array[0] = otherHandSlot;
            }

            return array;
        }
    }
}
