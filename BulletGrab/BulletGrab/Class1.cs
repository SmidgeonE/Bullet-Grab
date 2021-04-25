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
        private static bool _isPlayerHoldingChamber;
        private static FVRViveHand _handHoldingChamber;
        
        private void Awake()
        {
            Debug.Log("Bullet Grab Mod works!");

            Harmony.CreateAndPatchAll(typeof(BulletGrabMod));
        }

        [HarmonyPatch(typeof(FVRFireArmChamber), "BeginInteraction")]
        [HarmonyPrefix]
        private static void BeginInteractionPatch()
        {
            Debug.Log("Beginning interaction with fire arm chmaber");
            _isPlayerHoldingChamber = true;
        }
        
        [HarmonyPatch(typeof(FVRInteractiveObject), "EndInteraction")]
        [HarmonyPostfix]
        private static void EndInteractionPatch(FVRInteractiveObject __instance)
        {
            Debug.Log("Type of this object : " + __instance.GetType().Name);
            if (__instance.GetType() == typeof(FVRFireArmChamber))
            {
                Debug.Log("Ending chamber interaction");
                _isPlayerHoldingChamber = false;
                _handHoldingChamber = null;
                return;
            }
            
            Debug.Log("This object is not a fvrfirearmchamber!");
        }

        [HarmonyPatch(typeof(FVRFireArmChamber), "EjectRound")]
        [HarmonyPostfix]
        private static void EjectionPatch()
        {
            if (!_isPlayerHoldingChamber)
            {
                Debug.Log("Player is not holding the chamber");
                return;
            }

            if (_handHoldingChamber == null)
            {
                Debug.Log("Hand is null");
                return;
            }
            
            Debug.Log("Ejecting Round!");
            var handPosition = _handHoldingChamber.transform.position;

            Debug.Log("Hand position is : " + handPosition.x + " " + handPosition.y + " " + handPosition.z);

            var colliders = new Collider[5];
            var size = Physics.OverlapSphereNonAlloc(handPosition, 0.1f, colliders, Physics.AllLayers);

            Debug.Log("");
            Debug.Log("Number of colliders: " + size);
            Debug.Log("Collider's names around the hand:");
            
            foreach (var collider in colliders)
            {
                Debug.Log(collider.gameObject.name); 
                Debug.Log("Distance : " + Vector3.Distance(collider.transform.position, handPosition));
                Debug.Log("");
            }
        }
    }
}