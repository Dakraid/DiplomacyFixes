﻿using DiplomacyFixes.ViewModel;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(KingdomDiplomacyVM))]
    class KingdomDiplomacyVMPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("RefreshDiplomacyList")]
        public static void RefreshDiplomacyListPatch(KingdomDiplomacyVM __instance)
        {
            MBBindingList<KingdomWarItemVM> playerWars = new MBBindingList<KingdomWarItemVM>();
            MBBindingList<KingdomTruceItemVM> playerTruces = new MBBindingList<KingdomTruceItemVM>();

            MethodInfo onDiplomacyItemSelection = __instance.GetType().GetMethod("OnDiplomacyItemSelection", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo onDeclareWarMethod = __instance.GetType().GetMethod("OnDeclareWar", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo onProposePeaceMethod = __instance.GetType().GetMethod("OnDeclarePeace", BindingFlags.NonPublic | BindingFlags.Instance);

            Action<KingdomTruceItemVM> onDeclareWarAction = (Action<KingdomTruceItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomTruceItemVM>), __instance, onDeclareWarMethod);
            Action<KingdomWarItemVM> onProposePeaceAction = (Action<KingdomWarItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomWarItemVM>), __instance, onProposePeaceMethod);
            Action<KingdomDiplomacyItemVM> onItemSelectedAction = (Action<KingdomDiplomacyItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomDiplomacyItemVM>), __instance, onDiplomacyItemSelection);

            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;

            foreach (CampaignWar campaignWar in from w in FactionManager.Instance.CampaignWars
                                                orderby w.Side1[0].Name.ToString()
                                                select w)
            {
                if (campaignWar.Side1[0] is Kingdom && campaignWar.Side2[0] is Kingdom && !campaignWar.Side1[0].IsMinorFaction && !campaignWar.Side2[0].IsMinorFaction && (campaignWar.Side1[0] == playerKingdom || campaignWar.Side2[0] == playerKingdom))
                {
                    playerWars.Add(new KingdomWarItemVMExtensionVM(campaignWar, onItemSelectedAction, onProposePeaceAction, __instance.RefreshValues));
                }
            }
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom != playerKingdom && !kingdom.IsEliminated && (FactionManager.IsAlliedWithFaction(kingdom, playerKingdom) || FactionManager.IsNeutralWithFaction(kingdom, playerKingdom)))
                {
                    playerTruces.Add(new KingdomTruceItemVMExtensionVM(playerKingdom, kingdom, onItemSelectedAction, onDeclareWarAction, __instance.RefreshValues));
                }
            }

            __instance.PlayerTruces = playerTruces;
            __instance.PlayerWars = playerWars;

            MethodInfo setDefaultSelectedItem = __instance.GetType().GetMethod("SetDefaultSelectedItem", BindingFlags.NonPublic | BindingFlags.Instance);
            setDefaultSelectedItem.Invoke(__instance, new object[] { });
        }
    }
}
