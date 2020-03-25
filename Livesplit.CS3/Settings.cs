using System.Windows.Forms;

namespace Livesplit.CS3
{
    //Taken from Livesplit.Salt from Seanpr96 as a placeholder until split and therefore settings become a thing
    public partial class Settings : UserControl
    {
        public bool RandomizeSkins { get; set; } = true;

        public Settings()
        {
            InitializeComponent();

            rndSkinsBox.Click += (garbage, garbage2) => RandomizeSkins = rndSkinsBox.Checked;

            Load += LoadLayout;
        }

        private void LoadLayout(object sender, System.EventArgs e)
        {
            rndSkinsBox.Checked = RandomizeSkins;
        }
    }
}