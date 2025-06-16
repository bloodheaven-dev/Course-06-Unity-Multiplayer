using System;
using Unity.Collections;
using Unity.Netcode;

public struct LeaderboardState : INetworkSerializable, IEquatable<LeaderboardState>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int Coins;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Coins);
    }

    public bool Equals(LeaderboardState other)
    {
        return
            //
            ClientId == other.ClientId &&
            PlayerName == other.PlayerName &&
            Coins == other.Coins;
        //
    }
}