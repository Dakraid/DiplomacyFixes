﻿using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class CooldownBehavior : CampaignBehaviorBase
    {
        private CooldownManager _cooldownManager;

        public CooldownBehavior()
        {
            this._cooldownManager = new CooldownManager();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.MakePeace.AddNonSerializedListener(this, RegisterDeclareWarCooldown);
            Events.PeaceProposalSent.AddNonSerializedListener(this, RegisterPeaceProposalCooldown);
        }

        private void RegisterDeclareWarCooldown(IFaction faction1, IFaction faction2)
        {
            _cooldownManager.UpdateLastWarTime(faction1, faction2, CampaignTime.Now);
        }

        private void RegisterPeaceProposalCooldown(Kingdom kingdom)
        {
            _cooldownManager.UpdateLastPeaceProposalTime(kingdom, CampaignTime.Now);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_cooldownManager", ref _cooldownManager);
            _cooldownManager.Sync();
        }
    }
}
