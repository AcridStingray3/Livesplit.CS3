using System;
using System.Reflection;
using LiveSplit.Model;
using LiveSplit.UI.Components;

namespace Livesplit.CS3
{
    public class CS3Factory : IComponentFactory
    {
        // ReSharper disable once UnusedMember.Global
      
        public IComponent Create(LiveSplitState state)
        {
            return new CS3Component(state, ComponentName);
        }

        public string UpdateName => ComponentName;
        public string XMLURL => UpdateURL + "Components/LiveSplit.CS3.Updates.xml";
        public string UpdateURL => "https://raw.githubusercontent.com/AcridStingray3/LiveSplit.CS3/master/Livesplit.CS3/";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public string ComponentName => "The Legend of Heroes: Trails of Cold Steel 3 Autosplitter v" + Version;
        public string Description => "The Legend of Heroes: Trails of Cold Steel 3 Autosplitter";
        public ComponentCategory Category => ComponentCategory.Control;
    }
    
}