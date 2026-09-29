using Fusion;
using UnityEngine;
using System.Linq;

public class GameTimer : NetworkBehaviour
{
    [Networked]
    private float startTime { get; set; }

    [Networked]
    private float stopTime { get; set; }

    [Networked]
    private bool isRunning { get; set; }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
            return;

        startTime = (float)Runner.SimulationTime;
        isRunning = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (Runner.ActivePlayers.Count() <= 1 && isRunning)
        {
            stopTime = (float)Runner.SimulationTime;
            isRunning = false;
        }
    }

    public float GetElapsedTime()
    {
        if (isRunning)
            return (float)Runner.SimulationTime - startTime;

        return stopTime - startTime;
    }
}