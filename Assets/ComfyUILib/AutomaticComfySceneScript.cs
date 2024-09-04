using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class GameAction
{
    public int index;
    public string name;
    public bool value;
    public UnityEvent eventToCall;
}

public class AutomaticComfySceneScript : MonoBehaviour
{
    public List<GameAction> gameActions;

    public int IndexToStart = -1;
    private void Start()
    {
        StartCoroutine(StartGameAction());
    }

    /// <summary>
    /// Does the given index's action at the beginning of the game. If there is no need for it, set IndexToStart = -1
    /// </summary>
    IEnumerator StartGameAction()
    {
        while (GameManager.getInstance() == null)
        {
            yield return new WaitForSeconds(2);
        }

        if (IndexToStart < 0) yield break;
        if (IndexToStart >= gameActions.Count) yield break;

        DoGameAction(IndexToStart);

        yield break;
    }

    /// <summary>
    /// Helper function for starting the events at a delay
    /// </summary>
    /// <param name="curInd">Index of the action to invoke</param>
    IEnumerator InvokeEventAction(int curInd) {
        yield return new WaitForSeconds(1f);
        gameActions[curInd].eventToCall?.Invoke();

        yield break;
    }

    /// <summary>
    /// Called from various scripts in the game. Called by functions that symbolize the end of a part of the linear narrative.
    /// </summary>
    /// <param name="curInd">Index of the action to invoke</param>
    public void DoGameAction(int curInd)
    {
        if (curInd >= gameActions.Count || curInd >= gameActions.Count) return;
        if (gameActions[curInd].value) return;
        if (curInd != 0)
        {
            if (!gameActions[curInd - 1].value) return;
        }

        StartCoroutine(InvokeEventAction(curInd));        
        gameActions[curInd].value = true;
    }
}
