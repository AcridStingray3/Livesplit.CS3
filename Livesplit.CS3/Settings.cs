using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

namespace Livesplit.CS3
{
    public partial class Settings : UserControl
    {
        public bool SkipBattleAnimations = true;

        private readonly Dictionary<string, BattleEnums> displayedSettings = new Dictionary<string, BattleEnums> // Since Serialization happens on a loop because Livesplit is bad, it's better to write "_" instead of spaces so that String.Replace() is used less often
        {
            ["Split_Prologue_Mechs"] = BattleEnums.PrologueMechs,
            ["Split_Prologue_Stahlritter"] = BattleEnums.PrologueStahlritter, 
            
            ["Split_Prologue_Battle_Tutorial"] = BattleEnums.FirstBattle,
            ["Split_Prologue_Arts_Tutorial"] = BattleEnums.ArtsTutorial,
            ["Split_Prologue_Juna_Tutorial"] = BattleEnums.JunaTutorial,
            ["Split_Prologue_Link_Tutorial"] = BattleEnums.LinkTutorial,
            ["Split_Prologue_S-Break_Tutorial"] = BattleEnums.SbreakTutorial,
            ["Split_Prologue_Magic_Knight"] = BattleEnums.PrologueMagicKnight,
        
            // Chapter 1
            ["Split_Chapter_1_Keep_First_Fight"] = BattleEnums.Ch1KeepFirstFight,
            ["Split_Chapter_1_Keep_Second_Fight"] = BattleEnums.Ch1KeepSecondFight,
            ["Split_Chapter_1_Rontes"] = BattleEnums.Rontes,
            
            ["Split_Chapter_1_Mech_Tutorial"] = BattleEnums.MechTutorial,
            ["Split_Chapter_1_Ash_Mech"] = BattleEnums.Ash,
            
            ["Split_Chapter_1_Forest_First_Fight"] = BattleEnums.ForestFirstFight,
            ["Split_Chapter_1_Forest_Spiders"] = BattleEnums.ForestSpiders,
            ["Split_Chapter_1_First_Archaisms"] = BattleEnums.FirstArchaisms,
            ["Split_Chapter_1_First_Zephyrantes"] = BattleEnums.FirstZephyrantes,
            ["Split_Chapter_1_First_Clowns"] = BattleEnums.FirstClown,
            ["Split_Chapter_1_First_Night_Ambush"] = BattleEnums.FirstAmbush,
            ["Split_Chapter_1_Second_Night_Ambush"] = BattleEnums.SecondAmbush,
            ["Split_Chapter_1_Third_Night_Ambush"] = BattleEnums.ThirdAmbush,
            
            
            ["Split_Chapter_1_Danghorns"] = BattleEnums.Danghorns,
            ["Split_Chapter_1_Mothros"] = BattleEnums.Mothros,
            ["Split_Chapter_1_First_Hamel_Road_Ambush"] = BattleEnums.FirstHamelRoad,
            ["Split_Chapter_1_Duvalie_and_Shirley"] = BattleEnums.DuvalieShirley,
            ["Split_Chapter_1_Blue_Aion_I"] = BattleEnums.PreBlueAion,
            ["Split_Chapter_1_Blue_Aion_II"] = BattleEnums.BlueAion,
            
            // Chapter 2 
            ["Split_Chapter_2_Keep_First_Fight"] = BattleEnums.Ch2KeepFirstFight,
            ["Split_Chapter_2_Stratos_Diver"] = BattleEnums.StratosDiver,
        };

        public readonly HashSet<BattleEnums> currentBattleSettings;

        public Settings()
        {
            InitializeComponent();
 
            skipButtonBox.Click += (garbage, garbage2) => SkipBattleAnimations = skipButtonBox.Checked;

            Load += LoadLayout;

            currentBattleSettings = new HashSet<BattleEnums>();
            
        }
        
        private void battleIDSplitsCollection_ItemCheckChanged(object sender, ItemCheckEventArgs e)
        {
            BattleEnums setting = displayedSettings[battleIDSplitsCollection.Items[e.Index].ToString().Replace(' ', '_')];
            if (e.NewValue == CheckState.Checked)
                currentBattleSettings.Add(setting);
            else
                currentBattleSettings.Remove(setting);
        }

        private void LoadLayout(object sender, EventArgs e)
        {
            skipButtonBox.Checked = SkipBattleAnimations;
            for (int i = 0; i < battleIDSplitsCollection.Items.Count; ++i)
            {
                if (currentBattleSettings.Contains(displayedSettings[ ((string)battleIDSplitsCollection.Items[i]).Replace(' ', '_')])) 
                    battleIDSplitsCollection.SetItemChecked(i, true);
            }
        }


        private void Settings_Load(object sender, EventArgs e)
        {
            LoadLayout(sender, e);
        }

        public XmlNode Serialize(XmlDocument document)
        {
            XmlElement xmlSettings = document.CreateElement("Settings");

            XmlElement element = document.CreateElement(nameof(SkipBattleAnimations));
            element.InnerText = SkipBattleAnimations.ToString();
            xmlSettings.AppendChild(element);

            foreach (string battleSplit in displayedSettings.Keys)
            {
                element = document.CreateElement(battleSplit);
                element.InnerText = currentBattleSettings.Contains(displayedSettings[battleSplit]).ToString();
                xmlSettings.AppendChild(element);
            }
            
            return xmlSettings;
        }

        public void Deserialize(XmlNode settings)
        {
            XmlNode skipBattleAnimsNode = settings.SelectSingleNode(".//" + nameof(SkipBattleAnimations));
            if (bool.TryParse(skipBattleAnimsNode?.InnerText, out bool skipBattleAnims))
            {
                SkipBattleAnimations = skipBattleAnims;
            }

            foreach (string battleSplit in displayedSettings.Keys)
            {
                XmlNode node = settings.SelectSingleNode(".//" + battleSplit);
                if (!bool.TryParse(node?.InnerText, out bool splitSetting)) continue;
                if (splitSetting)
                    currentBattleSettings.Add(displayedSettings[battleSplit]);
            }
        }
    }
}