using UnityEngine;
using Fusion;

public class PlayerLobyData : NetworkBehaviour
{
    [Networked] public int CharacterIndex { get; set; }
    [Networked] public int TeamIndex { get; set; }
    [Networked] public NetworkBool isReady { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CharacterIndex = 0;
            TeamIndex = 0;
            isReady = false;
        }

        Debug.Log($"LobbyData Spawned / InputAuthoriy : {Object.InputAuthority}");
    }
}
