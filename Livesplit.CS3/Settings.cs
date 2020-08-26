using System;
using System.Windows.Forms;

namespace Livesplit.CS3
{
    public partial class Settings : UserControl
    {
        public bool SkipBattleAnimations = true;
 
        public Settings()
        {
            InitializeComponent();
 
            skipButtonBox.Click += (garbage, garbage2) => SkipBattleAnimations = skipButtonBox.Checked;
 
            Load += LoadLayout;
        }
 
        private void LoadLayout(object sender, EventArgs e)
        {
            skipButtonBox.Checked = SkipBattleAnimations;
        }

        private void battleIDSplitsCollection_SelectedIndexChanged(object sender, EventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}