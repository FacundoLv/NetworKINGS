using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{
    public AnimationCurve speedCurve;

    private ObjectPool<Pipe> pipePool;

    private Queue<Pipe> pipes;

    private Pipe tail = null;

    private void Awake()
    {
        if (!GameServer.Instance.IsServer)
        {
            Destroy(gameObject);
            return;
        }

        pipePool = new ObjectPool<Pipe>(NewPipe, Pipe.TurnOn, Pipe.TurnOff);

        pipes = new Queue<Pipe>();

        GetPipe(2);
    }

    private void Update()
    {
        float currentSpeed = speedCurve.Evaluate(Time.time);

        foreach (Pipe pipe in pipes)
        {
            pipe.Speed = currentSpeed;
        }
    }

    private void GetPipe(int pipeAmount = 1)
    {
        for (int i = 0; i < pipeAmount; i++)
        {
            var newPipe = pipePool
                .GetObject()
                .SetPosition(new Vector3(0, 0, tail?.MaxBounds.z - 0.5f ?? 0f));

            tail = newPipe;

            pipes.Enqueue(newPipe);
        }
    }

    private Pipe NewPipe()
    {
        var pipe = PhotonNetwork
            .Instantiate("Pipes/SimplePipe", Vector3.zero, Quaternion.identity)
            .GetComponent<Pipe>();

        pipe.OnPassed += LoopPipe;

        return pipe;
    }

    private void LoopPipe(Pipe pipe)
    {
        pipes.Dequeue();
        pipePool.ReturnObject(pipe);

        GetPipe();
    }
}
