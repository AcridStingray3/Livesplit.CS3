namespace Livesplit.CS3
{
    public enum BattleEnums : ushort
    {
        // Juno
        PrologueMechs = 100,
        PrologueStahlritter = 101,
        
        // Prologue
        FirstBattle = 10,
        ArtsTutorial = 16,
        JunaTutorial = 15,
        LinkTutorial = 11,
        SbreakTutorial = 12,
        PrologueMagicKnight = 102,
        
        // Chapter 1
        Ch1KeepFirstFight = 210,
        Ch1KeepSecondFight = 211,
        Rontes = 212,
        
        MechTutorial = 1000,
        Ash = 1001,
        
        ForestFirstFight = 213,
        ForestSpiders = 214,
        FirstArchaisms = 202,
        FirstZephyrantes = 203,
        FirstClown = 204,
        FirstAmbush = 218,
        SecondAmbush = 219,
        ThirdAmbush = 220,
        
        Danghorns = 215,
        Mothros = 216,
        FirstHamelRoad = 205,
        DuvalieShirley = 207,
        PreBlueAion = 1002,
        BlueAion = 1003,
        
        
        // Chapter 2
        Ch2KeepFirstFight = 310,
        StratosDiver = 311,
        
    }

    public enum ChapterEnums : int
    {
        SpringOnceAgain = 0,
        Reunion = 1,
        ConflictInCrossbell = 2,
        PulseOfSteel = 3,
        RadiantHeimdallr = 4,
        ForWhomTheBellTolls = 5  // Never gonna be used because final split is on final boss, but completion's sake ig
    }
}