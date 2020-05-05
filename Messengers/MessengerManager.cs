﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes.Messengers
{
    [SaveableClass(4)]
    class MessengerManager : IMissionListener
    {
        [SaveableField(1)]
        private List<Messenger> _messengers;

        public MBReadOnlyList<Messenger> Messengers { get; private set; }

        public bool CanStartMessengerConversation { get; private set; }
        private Messenger _activeMessenger;

        private PlayerEncounter savedPlayerEncounter;
        internal MessengerManager()
        {
            _messengers = new List<Messenger>();
            Messengers = new MBReadOnlyList<Messenger>(_messengers);
            CanStartMessengerConversation = true;
        }

        public void SendMessenger(Hero targetHero)
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=zv12jjyW}Messenger Sent").ToString(), GetMessengerSentText(Hero.MainHero.MapFaction, targetHero.MapFaction, targetHero, Settings.Instance.MessengerTravelTime).ToString(), true, false, GameTexts.FindText("str_ok", null).ToString(), "", delegate ()
            {
            }, null, ""), false);
            _messengers.Add(new Messenger(targetHero, CampaignTime.Now));
        }

        public void MessengerArrived()
        {
            foreach (Messenger messenger in Messengers.ToList())
            {
                if (messenger.DispatchTime.ElapsedDaysUntilNow >= Settings.Instance.MessengerTravelTime)
                {
                    if (MessengerArrived(messenger))
                    {
                        break;
                    }
                }
            }
        }

        private bool MessengerArrived(Messenger messenger)
        {
            if (CanStartMessengerConversation && IsTargetHeroAvailable(messenger.TargetHero) && PartyBase.MainParty != null && PlayerEncounter.Current == null)
            {
                CanStartMessengerConversation = false;
                _activeMessenger = messenger;
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=uy86VZX2}Messenger Arrived").ToString(), GetMessengerArrivedText(Hero.MainHero.MapFaction, messenger.TargetHero.MapFaction, messenger.TargetHero).ToString(), true, false, GameTexts.FindText("str_ok", null).ToString(), "", delegate ()
                {
                    StartDialogue(messenger.TargetHero);
                }, null, ""), true);
                return true;
            }
            else if (messenger.TargetHero.IsDead)
            {
                _messengers.Remove(_activeMessenger);
            }

            return false;
        }

        internal void Sync()
        {
            Messengers = new MBReadOnlyList<Messenger>(_messengers);
            CanStartMessengerConversation = true;
        }

        public void StartDialogue(Hero targetHero)
        {
            // We have null checked both of these at this point
            PartyBase heroParty = PartyBase.MainParty;
            PartyBase targetParty = targetHero.PartyBelongedTo.Party;

            PlayerEncounter.Start();
            PlayerEncounter.Current.SetupFields(heroParty, targetParty);
            Mission conversationMission = (Mission)Campaign.Current.CampaignMissionManager.OpenConversationMission(
                new ConversationCharacterData(Hero.MainHero.CharacterObject, heroParty, true, false, false, false),
                new ConversationCharacterData(targetHero.CharacterObject, targetParty, true, false, false, false),
                "", "");
            conversationMission.AddListener(this);
        }

        private TextObject GetMessengerArrivedText(IFaction faction1, IFaction faction2, Hero targetHero)
        {
            TextObject textObject = new TextObject("{=YnRmSele}The messenger from {FACTION1_NAME} has arrived at {HERO_NAME} of {FACTION2_NAME}.");
            textObject.SetTextVariable("FACTION1_NAME", faction1.Name.ToString());
            textObject.SetTextVariable("FACTION2_NAME", faction2.Name.ToString());
            textObject.SetTextVariable("HERO_NAME", targetHero.Name.ToString());
            return textObject;
        }

        private TextObject GetMessengerSentText(IFaction faction1, IFaction faction2, Hero targetHero, int travelDays)
        {
            TextObject textObject = new TextObject("{=qNWMZP0z}The messenger from {FACTION1_NAME} will arrive at {HERO_NAME} of {FACTION2_NAME} in {TRAVEL_TIME} days.");
            textObject.SetTextVariable("FACTION1_NAME", faction1.Name.ToString());
            textObject.SetTextVariable("FACTION2_NAME", faction2.Name.ToString());
            textObject.SetTextVariable("HERO_NAME", targetHero.Name.ToString());
            textObject.SetTextVariable("TRAVEL_TIME", travelDays);
            return textObject;
        }

        public void SendMessengerWithInfluenceCost(Hero targetHero, float influenceCost)
        {
            DiplomacyCostManager.deductInfluenceFromPlayerClan(influenceCost);
            SendMessenger(targetHero);
        }

        public static bool IsTargetHeroAvailable(Hero opposingLeader)
        {
            bool unavailable = opposingLeader.IsDead
                || opposingLeader.IsOccupiedByAnEvent()
                || opposingLeader.IsPrisoner
                || opposingLeader.IsHumanPlayerCharacter
                || opposingLeader.PartyBelongedTo?.Party == null;
            return !unavailable;
        }

        public static bool CanSendMessengerWithInfluenceCost(Hero opposingLeader, float influenceCost)
        {
            return Clan.PlayerClan.Influence >= influenceCost && IsTargetHeroAvailable(opposingLeader);
        }

        public void OnEquipItemsFromSpawnEquipmentBegin(Agent agent, Agent.CreationType creationType)
        {
        }

        public void OnEquipItemsFromSpawnEquipment(Agent agent, Agent.CreationType creationType)
        {
        }

        public void OnEndMission()
        {
            _messengers.Remove(_activeMessenger);
            _activeMessenger = null;
            CanStartMessengerConversation = true;
            
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.Finish();
            }
        }

        public void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
        {
        }

        public void OnConversationCharacterChanged()
        {
        }

        public void OnResetMission()
        {
        }
    }
}
