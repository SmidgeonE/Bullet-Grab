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
            var weapon = __instance.Firearm;
            _quickBeltSlots = TryGetMagPalmSlots(GM.CurrentPlayerBody.RightHand.GetComponent<FVRViveHand>());

            if (_quickBeltSlots[0] == null || _quickBeltSlots[1] == null) return;

            switch (weapon)
            {
                case Handgun handgun:
                    var handHoldingSlide = handgun.Slide.m_hand;

                    if (handHoldingSlide == null) return;
                    if (!PlayerIsPressingDown(handHoldingSlide)) return;

                    PlaceBulletInHand(__result, handHoldingSlide);
                    break;

                case BoltActionRifle boltAction:
                    var handHoldingBolt = boltAction.BoltHandle.m_hand;

                    if (handHoldingBolt) return;
                    if (!PlayerIsPressingDown(handHoldingBolt))
                    {
                        Debug.Log("Player is not pressing down on bolt");
                        return;
                    }

                    PlaceBulletInHand(__result, handHoldingBolt);
                    break;

                default:
                    Debug.Log("this type of weapon has not been implemented : " + __instance.Firearm.GetType());
                    break;
            }

            Console.WriteLine("Ejected Round");
        }

        private static void PlaceBulletInHand(FVRPhysicalObject round, FVRViveHand hand)
        {
            Console.WriteLine("Placing bullet in hand");
            var desiredQBSlot = hand.IsThisTheRightHand ? _quickBeltSlots[0] : _quickBeltSlots[1];

            if (desiredQBSlot.CurObject != null)
            {
                Debug.Log("The current held object is not null, returning");
                return;
            }

            round.ForceObjectIntoInventorySlot(desiredQBSlot);
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
