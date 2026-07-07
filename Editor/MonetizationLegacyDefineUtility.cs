using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationEditor
{
    public static class MonetizationLegacyDefineUtility
    {
        public const string GameAnalyticsDefine = "MONETIZATION_GAMEANALYTICS";

        public static bool IsLegacyGameAnalyticsEnabled()
        {
            return HasDefine(GameAnalyticsDefine);
        }

        public static void SetLegacyGameAnalyticsEnabled(bool enabled)
        {
            SetDefine(GameAnalyticsDefine, enabled);
        }

        private static bool HasDefine(string define)
        {
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            return defines.Split(';').Any(d => d == define);
        }

        private static void SetDefine(string define, bool enabled)
        {
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group)
                .Split(';')
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .ToList();

            bool hasDefine = defines.Contains(define);
            if (enabled && !hasDefine)
            {
                defines.Add(define);
            }
            else if (!enabled && hasDefine)
            {
                defines.Remove(define);
            }
            else
            {
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines));
        }
    }
}
