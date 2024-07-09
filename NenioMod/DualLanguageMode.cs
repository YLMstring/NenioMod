using HarmonyLib;
using Kingmaker.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Localization.Shared;
using BlueprintCore.Utils;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.TextTools;
using Kingmaker.Utility;

namespace NenioMod
{
    [HarmonyPatch(typeof(BlueprintCue), nameof(BlueprintCue.DisplayText), MethodType.Getter)]
    internal class DualLanguageMode
    {
        static void Postfix(ref string __result, ref BlueprintCue __instance)
        {
            try
            {
                if (LocalizationManager.CurrentLocale == Locale.enGB)
                {
                    return;
                }
                string en = GetEnglish(__instance.Text);
                __result = __result + "\n" + en;
            }
            catch { }
        }

        public static string GetEnglish(LocalizedString local)
        {
            string text = local.LoadString(pack, Locale.enGB);
            if (local.ShouldProcess)
            {
                return TextTemplateEngine.Process(text);
            }
            return text;
        }

        private static LocalizationPack pack = LocalizationManager.LoadPack(Locale.enGB);
    }

    [HarmonyPatch(typeof(BlueprintAnswer), nameof(BlueprintAnswer.DisplayText), MethodType.Getter)]
    internal class DualLanguageMode2
    {
        static void Postfix(ref string __result, ref BlueprintAnswer __instance)
        {
            try
            {
                if (LocalizationManager.CurrentLocale == Locale.enGB)
                {
                    return;
                }
                string en = DualLanguageMode.GetEnglish(__instance.Text);
                __result = __result + "\n" + en;
            }
            catch { }
        }
    }
}
