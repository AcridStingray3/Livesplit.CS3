using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace Livesplit.CS3
{
    public partial class Settings : UserControl
    {
        public bool SkipBattleAnimations = true;

        // I kinda wanna yeet this to its own file, since this is already a sealed class, just for a scalability where you don't even need to open the main settings file. Bit overkill though
        private readonly Dictionary<string, Enum> displayedSettings = new Dictionary<string, Enum> // Since Serialization happens on a loop because Livesplit is bad, it's better to write "_" instead of spaces so that String.Replace() is used less often
        {
            ["Split_Prologue_Mechs"] = BattleEnums.PrologueMechs,
            ["Split_Prologue_Stahlritter"] = BattleEnums.PrologueStahlritter, 
            
            ["Split_Prologue_Battle_Tutorial"] = BattleEnums.FirstBattle,
            ["Split_Prologue_Arts_Tutorial"] = BattleEnums.ArtsTutorial,
            ["Split_Prologue_Juna_Tutorial"] = BattleEnums.JunaTutorial,
            ["Split_Prologue_Link_Tutorial"] = BattleEnums.LinkTutorial,
            ["Split_Prologue_S-Break_Tutorial"] = BattleEnums.SbreakTutorial,
            ["Split_Prologue_Magic_Knight"] = BattleEnums.PrologueMagicKnight,
            
            ["Split_Prologue"] = ChapterEnums.SpringOnceAgain,
        
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
            
            ["Split_Chapter_1"] = ChapterEnums.Reunion,
            
            // Chapter 2 
            ["Split_Chapter_2_Keep_First_Fight"] = BattleEnums.Ch2KeepFirstFight,
            ["Split_Chapter_2_Stratos_Diver"] = BattleEnums.StratosDiver,
            
            ["Split_Chapter_2"] = ChapterEnums.ConflictInCrossbell,
            
            
            ["Split_Chapter_3"] = ChapterEnums.PulseOfSteel,
            
            
            ["Split_Chapter_4"] = ChapterEnums.RadiantHeimdallr,
            
            
        };

        public readonly HashSet<Enum> currentSplitSettings;

        public Settings()
        {
            InitializeComponent();
 
            skipButtonBox.Click += (garbage, garbage2) => SkipBattleAnimations = skipButtonBox.Checked;

            Load += LoadLayout;

            currentSplitSettings = new HashSet<Enum>();
            
        }
        
        private void SplitsCollection_ItemCheckChanged(object sender, ItemCheckEventArgs e)
        {
            Enum setting = displayedSettings[SplitsCollection.Items[e.Index].ToString().Replace(' ', '_')];
            if (e.NewValue == CheckState.Checked)
                currentSplitSettings.Add(setting);
            else
                currentSplitSettings.Remove(setting);
        }

        private void LoadLayout(object sender, EventArgs e)
        {
            skipButtonBox.Checked = SkipBattleAnimations;
            for (int i = 0; i < SplitsCollection.Items.Count; ++i)
            {
                if (currentSplitSettings.Contains(displayedSettings[ ((string)SplitsCollection.Items[i]).Replace(' ', '_')])) 
                    SplitsCollection.SetItemChecked(i, true);
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
                element.InnerText = currentSplitSettings.Contains(displayedSettings[battleSplit]).ToString();
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
                    currentSplitSettings.Add(displayedSettings[battleSplit]);
            }
        }
    }
}