using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Security.Policy;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using UnityEngine;
using Mono.Cecil;
using HarmonyLib;
using MonoMod.Utils;
using Random = UnityEngine.Random;

namespace Better_Slide_Racking
{
    [BepInPlugin("yes", "Mod", "1.0.0")]
    
    public class Mod : BaseUnityPlugin
    {
        private void Awake()
        {
            Debug.Log("Bep in plugin works");
            Harmony.CreateAndPatchAll(typeof(Mod));
            Debug.Log("Patched all");
        }
        
        
        [HarmonyPatch(typeof(FVRFireArmChamber), "BeginInteraction")]
        [HarmonyPrefix]
        private static bool BudgiePatch(FVRFireArmChamber __instance, FVRFireArmRound ___m_round, FVRViveHand hand)
        {
            
            Debug.Log("");
            Debug.Log("");
            Debug.Log("");
            Debug.Log("");
            if (__instance == null) Debug.Log("Instance is null!");
            Debug.Log("The object you have grabbed is called: " + __instance.GameObject.name);
            Debug.Log("values: " + __instance.IsManuallyChamberable + " " +  __instance.IsAccessible + " " + __instance.IsFull + " ");
            if(___m_round == null) Debug.Log("round is null!");
            Debug.Log("");
            Debug.Log("");
            
            
            if(__instance.IsManuallyChamberable && __instance.IsAccessible && __instance.IsFull & ___m_round != null)
            {
                Debug.Log("Spawning bullet");
                var fvrfireArmRound = __instance.EjectRound(hand.Input.Pos, Vector3.zero, Vector3.zero, false);
                if (fvrfireArmRound != null)
                {
                    fvrfireArmRound.BeginInteraction(hand);
                    hand.ForceSetInteractable(fvrfireArmRound);
                    __instance.SetRound(null);
                    Debug.Log("interaction complete");
                }
            }

            return false;
        }
    }
    
}
