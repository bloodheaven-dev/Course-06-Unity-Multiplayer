using UnityEngine;

public class Projectile : MonoBehaviour
{

    public int TeamIndex {  get; private set; }

    public void Init(int teamIndex)
    {
        TeamIndex = teamIndex;
    }
}
