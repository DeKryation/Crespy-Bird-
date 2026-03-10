using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Set game state.
public enum GameState
{
    Intro,
    Playing,
    Dead,
    Won
}

public static class GameStateManager
{
    //Get and set the value.
    public static GameState GameState { get; set; }

    static GameStateManager ()
    {
        //Set the default value.
        GameState = GameState.Intro;
    }



}

