using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plugin;
using Solarmax;

public enum GameState
{
	Init,
	Game,
	GameWatch,
	Watcher,
	GameEnd,
}

public enum GameType
{
	Single = 1,
	PVP = 2,
	League = 3,
    Guide = 4,
    TestLevel = 5,
	SingleLevel = 6,
}

public class BattleData : Lifecycle2
{
	public GameObject	sceneRoot;
	public GameState    gameState = GameState.Init;
	public GameType     gameType = GameType.PVP;

	public Rand         rand;

	public string       matchId;
	public int          difficultyLevel;		//难度等级。用于单机关卡 
	public int          aiLevel;	//AI 级别。用于单机关卡
	public MapConfig    currentTable;

	public TEAM         currentTeam;
	public TEAM         winTEAM;
	public float        sliderRate;
	public bool         isFakeBattle = false;
	public bool         isReplay = false;
	public bool         teamFight = false;
	public bool         useAI = true;
    public int          BattleTeamID = 0;
    public BattleTeam   currentBattleTeam = null;

	public bool        vertical { 
		get { return false;}
		set { if (currentTable == null) return;
			currentTable.vertical = value; }
	}

	public int          currentPlayers 
	{
		get { return currentTable != null ? currentTable.player_count : 0; }
		set { if (currentTable == null) return;
			currentTable.player_count = value; }
	}
	
	public string       mapAudio 
	{
		get { return currentTable != null ? currentTable.audio : null; }
		set { if (currentTable == null) return;
 			currentTable.audio = value; }
	}

	public bool         mapEdit = false;
	public bool         silent = false;
	public int          resumingFrame = -1;

	public BattleData ()
	{
		rand = new Rand (0);

	}

	public bool Init ()
	{
		gameState       = GameState.Init;
		gameType        = GameType.PVP;
		
		matchId         = string.Empty;
		currentTable    = null;
		currentTeam     = TEAM.Neutral;
		winTEAM         = TEAM.Neutral;
		sliderRate      = 1.0f;
		isFakeBattle    = false;
		isReplay        = false;
		teamFight       = false;
		silent          = false;
		return true;
	}

	public void Tick (int frame, float interval)
	{
		
	}

	public void Destroy ()
	{
		
	}

}

