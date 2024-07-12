using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelect : MonoBehaviour
{
    [SerializeField] int playerId;
    [SerializeField] int characterId;

    public void SetId()
    {
        PlayerHolder.Instance.EditPlayerCharacterId(playerId, characterId);
    }
}
