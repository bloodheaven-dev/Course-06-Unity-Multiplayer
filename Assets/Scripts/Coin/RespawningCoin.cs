using System;
using UnityEngine;

public class RespawningCoin : Coin
{
    public event Action<RespawningCoin> OnCollected;

    Vector3 previousPosition;

    void Update()
    {
        //if (!IsClient) return;

        Vector3 currentPosition = transform.position;

        if (previousPosition != currentPosition)
        {
            Show(true);
        }

        previousPosition = currentPosition;
    }

    public override int Collect()
    {
        if(!IsServer)
        {
            Show(false);
            return 0;
        }
        if (isCollected) return 0;

        isCollected = true;
        OnCollected?.Invoke(this);

        return coinValue;
    }

    public void Reset()
    {
        isCollected = false;
    }
}
