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

        private void Awake()
        {
            Debug.Log("Bullet Grab Mod works!");

            Harmony.CreateAndPatchAll(typeof(BulletGrabMod));
        }

        [HarmonyPatch(typeof(FVRFireArmChamber), "EjectRound")]
        [HarmonyPostfix]
        private static void EjectRoundPatch(FVRFireArmRound __result)
        {
            if (__result == null)
            {
                Debug.Log("round is null");
                return;
            }
            
            Debug.Log("Ejected Round");

            CheckIfPlayerIsGrabbing(__result);
        }

        private static void CheckIfPlayerIsGrabbing(FVRFireArmRound round)
        {
            var roundPosition = round.transform.position;
            var hands = GM.CurrentMovementManager.Hands;
            var leftHandDistance = (hands[0].transform.position - roundPosition).sqrMagnitude;
            var rightHandDistance =  (hands[1].transform.position - roundPosition).sqrMagnitude;

            var leftHandIsGrabbing = false;
            var rightHandIsGrabbing = false;
            var closestHand = hands[0];

            Debug.Log("Length of hands : " + hands.Length);
            
            if (hands[0].Input.GripPressed && hands[0].Input.TouchpadNorthPressed && hands[0].Input.TouchpadPressed)
            {
                Debug.Log("Left is grabbing");
                leftHandIsGrabbing = true;
            }

            if (hands[1].Input.GripPressed && hands[1].Input.TouchpadNorthPressed && hands[1].Input.TouchpadPressed)
            {
                Debug.Log("Right is grabbing");
                rightHandIsGrabbing = true;
                closestHand = hands[1];
            }

            if (closestHand.CurrentInteractable != null)
            {
                Debug.Log("Object already in main hand");
                closestHand = closestHand.OtherHand;
            }

            if (closestHand.CurrentInteractable != null)
            {
                Debug.Log("Other hand is already holding something");
                Debug.Log("Leaving method");
                return;
            }

            Debug.Log(leftHandDistance >= rightHandDistance
                ? "Right hand is closer to bullet"
                : "Left hand is closer to bullet");

            if (leftHandIsGrabbing && closestHand == hands[0])
            {
                Debug.Log("Sending bullet to left hand");
                round.BeginInteraction(closestHand);
            }
            else if (rightHandIsGrabbing && closestHand == hands[1])
            {
                Debug.Log("Sending bullet to left hand");
                round.BeginInteraction(closestHand);
            }
            else
            {
                Debug.Log("No available hand");
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