using UnityEngine;
using System.Collections.Generic;


public class OverworldInfo : MonoBehaviour
{

    public Transform collectibleSPs;


    List<string> _tappedGrottos = new List<string>();


    public bool HasGrottoBeenTapped(GrottoSpawnPoint gsp)
    {
        return _tappedGrottos.Contains(gsp.name); 
    }
    public void SetGrottoHasBeenTapped(GrottoSpawnPoint gsp, bool value)
    {
        if (value == HasGrottoBeenTapped(gsp)) { return; }

        if (value) { _tappedGrottos.Add(gsp.name); }
        else { _tappedGrottos.Remove(gsp.name); }
    }  


    #region Save/Load

    public class Serializable
    {
        public string[] collectedItems;
        public string[] tappedGrottos;
    }


    public Serializable GetInfo()
    {
        Serializable s = new Serializable();

        // Collectible SPs
        List<string> temp = new List<string>();
        foreach (Transform child in collectibleSPs)
        {
            CollectibleSpawnPoint csp = child.GetComponent<CollectibleSpawnPoint>();
            if (csp.HasBeenCollected) { temp.Add(csp.name); }
        }
        s.collectedItems = temp.ToArray();

        // Grotto SPs
        s.tappedGrottos = _tappedGrottos.ToArray();

        return s;
    }

    public void InitWithInfo(Serializable s)
    {
        // Collectible SPs
        foreach (var itemName in s.collectedItems)
        {
            Transform child = collectibleSPs.FindChild(itemName);
            CollectibleSpawnPoint csp = child.GetComponent<CollectibleSpawnPoint>();
            csp.HasBeenCollected = true;
        }

        // Grotto SPs
        _tappedGrottos = new List<string>(s.tappedGrottos);
    }

    #endregion

}