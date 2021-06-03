using HarmonyLib;
using FistVR;


namespace BulletGrab
{
    public static class OptionsCollector
    {
        public static ControlOptions.CoreControlMode ControlMode;
        
        /* These patches get the current control mode */
        [HarmonyPatch(typeof(GameOptions), "InitializeFromSaveFile")]
        [HarmonyPrefix]
        private static void InitialOptionsGrabPatch(GameOptions __instance)
        {
            ControlMode = __instance.ControlOptions.CCM;
        }
                
        [HarmonyPatch(typeof(GameOptions), "SaveToFile")]
        [HarmonyPrefix]
        private static void UpdateControlOptionsPatch(GameOptions __instance)
        { 
            ControlMode = __instance.ControlOptions.CCM;
        }
    }
}