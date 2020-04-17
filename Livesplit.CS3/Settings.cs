using System;
using System.Windows.Forms;

namespace Livesplit.CS3
{
    public partial class Settings : UserControl
    {
        public bool SkipBattleAnimations { get; set; } = true;
 
        public Settings()
        {
            InitializeComponent();
 
            skipButtonBox.Click += (garbage, garbage2) => SkipBattleAnimations = skipButtonBox.Checked;
 
            Load += LoadLayout;
        }
 
        private void LoadLayout(object sender, System.EventArgs e)
        {
            skipButtonBox.Checked = SkipBattleAnimations;
        }
    }
}