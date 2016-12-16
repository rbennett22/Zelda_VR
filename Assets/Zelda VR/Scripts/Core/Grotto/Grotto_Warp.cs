using UnityEngine;

public class Grotto_Warp : GrottoExtension_Base
{
    bool WarpsActive {
        get { return Grotto.WarpsActive; }
        set { Grotto.WarpsActive = value; }
    }


    // TODO  (we return true here for now so that message gets displayed)
    override public bool ShouldShowTheGoods { get { return true; } }

    override public void OnPlayerEnter()
    {
        WarpsActive = true;
    }


    public void OnPlayerEnteredPortal(GrottoPortal portal)
    {
        int warpDestination = GrottoSpawnPoint.GetWarpDestinationIndexForPortal(portal);
        Locations.Instance.WarpToGrottoWarpDestination(warpDestination);


        //WarpToPortalInsideGrotto(portal);
    }

    void WarpToPortalInsideGrotto(GrottoPortal fromPortal)
    {
        Player player = CommonObjects.Player_C;
        ZeldaPlayerController pc = player.PlayerController;

        GrottoSpawnPoint warpToGrottoSP = null;
        Transform warpToLocation = null;
        Grotto destinationGrotto = null;

        if (fromPortal == Grotto.warpA)
        {
            warpToGrottoSP = GrottoSpawnPoint.warpToA;
            destinationGrotto = warpToGrottoSP.SpawnGrotto();
            warpToLocation = destinationGrotto.warpA.transform;
        }
        else if (fromPortal == Grotto.warpB)
        {
            warpToGrottoSP = GrottoSpawnPoint.warpToB;
            destinationGrotto = warpToGrottoSP.SpawnGrotto();
            warpToLocation = destinationGrotto.warpB.transform;
        }
        else if (fromPortal == Grotto.warpC)
        {
            warpToGrottoSP = GrottoSpawnPoint.warpToC;
            destinationGrotto = warpToGrottoSP.SpawnGrotto();
            warpToLocation = destinationGrotto.warpC.transform;
        }


        Vector3 eulerDiff = warpToLocation.eulerAngles - fromPortal.transform.eulerAngles;

        Transform t = pc.transform;
        Vector3 offset = t.position - fromPortal.transform.position;
        offset = Quaternion.Euler(eulerDiff) * offset;
        t.position = warpToLocation.position + offset;

        Vector3 newEuler = pc.transform.eulerAngles + eulerDiff;
        player.ForceNewRotation(newEuler);

        pc.Stop();

        OnPlayerExit();
        destinationGrotto.OnPlayerEnter();
    }
}