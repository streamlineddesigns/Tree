using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm {

    public enum NodeType
    {
        Disjoint,
        Parent,
        Child
    }

    public enum NodeColor
    {
        Blue,
        Green,
        Purple,
        White,
        GrayScale,
        Orange,
        Yellow,
    }

    public enum ColorType
    {
        Default,
        Light,
        Dark,
        PurposelyUnassigned,
    }

    public enum NodeAction
    {
        TravelNode,
        GetEdge,
        SetEdge,
        TravelEdge
    }

    public enum ViewName 
    {
        PauseView,
        StartView,
        GameView,
        LevelCompleteView,
        LevelLostView,
        ActionView,
        ResetView,
        LevelSelectView,
        LevelSelectButtonView,
        ZoomView,
        ChapterRowView,
        CutSceneSelectButtonView,
        CutSceneView,
        FadeView,
        HeartView,
        PlayerHeartsView,
        TutorialView,
        CharacterSelectView,
        ShopView,
        NewsView,
        SettingsView,
        CreditsView,
        AccountView,
        CellsUsedView,
    }

    public enum ViewType
    {
        Screen,
        Dialog,
        Popup,
        Component
    }

    public enum GameState
    {
        GameStart,
        LevelComplete,
        LevelRestarted,
        LevelLost,
        Playing,
        Paused,
        Resumed,
        LevelExited,
    }

    public enum ClickType {
        Tap = 0,
        LongPress = 1,
        PanBegin = 2,
        PanComplete = 3,
    }

    public enum HashDataType {
        Node,
        Boid
    }

    public enum ObstacleType {
        SingleNode,
        DoubleNode,
        TripleNode,
        BetweenNode,
        OnNode
    }
    
}