using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroup_Script : List<Player_Script>{

    public void Remove(string player_name)
    {
        int index = IndexOf(player_name);

        if (index == -1)
        {
            Debug.LogError("Couldn't remove player from name '" + player_name + "'! Reason: player not belong in this player group");
            return;
        }

        RemoveAt(index);
    }

    public int IndexOf(string player_name)
    {
        return -1;
    }
    
}
