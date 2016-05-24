using UnityEngine;
using System.Collections;

public class Spell_Whistle : Spell_Base 
{
    const float WHISTLE_MELODY_DURATION = 4.0f;
    const float THRESHOLD_DIST_TO_DUNGEON_7 = 7;


    public override void Cast(GameObject target)
    {
        base.Cast(target);

        StartCoroutine("UseWhistle_CR");
    }

    int _nextWarpDungeonNum = 1;
    IEnumerator UseWhistle_CR()
    {
        // TODO

        if (PlayerC.IsJinxed)
        {
            PlayerC.DeactivateJinx();
        }

        PlayerC.ParalyzeAllNearbyEnemies(WHISTLE_MELODY_DURATION);
        PlayerC.ActivateParalyze(WHISTLE_MELODY_DURATION);

        Music.Instance.Stop();
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.whistle);

        yield return new WaitForSeconds(WHISTLE_MELODY_DURATION);

        Music.Instance.PlayAppropriateMusic();

        if (HandleDigdogger())
        {
            yield break;
        }
        if (HandleDungeon7Entrance())
        {
            yield break;
        }

        Warp();
    }

    bool HandleDigdogger()
    {
        if (!WorldInfo.Instance.IsInDungeon)
        {
            return false;
        }
        DungeonRoom room = PlayerC.OccupiedDungeonRoom();
        if (room == null)
        {
            return false;
        }

        EnemyAI_Digdogger digdogger = null;
        foreach (var enemy in room.Enemies)
        {
            digdogger = enemy.GetComponent<EnemyAI_Digdogger>();
            if (digdogger != null)
            {
                break;
            }
        }
        if (digdogger == null || digdogger.HasSplit)
        {
            return false;
        }

        digdogger.SplitIntoBabies();

        return true;
    }

    bool HandleDungeon7Entrance()
    {
        if (!WorldInfo.Instance.IsOverworld)
        {
            return false;
        }

        GameObject dungeon7 = GameObject.FindGameObjectWithTag("Dungeon7Entrance");
        Vector3 playerPos = PlayerC.PlayerController.transform.position;

        float dist = Vector3.Distance(dungeon7.transform.position, playerPos);
        if (dist >= THRESHOLD_DIST_TO_DUNGEON_7)
        {
            return false;
        }

        Dungeon7Entrance ent = dungeon7.GetComponent<Dungeon7Entrance>();
        ent.EmptyLake();

        return true;
    }

    void Warp()
    {
        if (!WorldInfo.Instance.IsOverworld)
        {
            return;
        }

        Locations.Instance.WarpToOverworldDungeonEntrance(_nextWarpDungeonNum);

        // Determine next dungeon to warp to (if Whistle is blown again);
        bool canWarpToDungeon = false;
        int count = 0;
        do
        {
            if (++_nextWarpDungeonNum > 8)
            {
                _nextWarpDungeonNum = 1;
            }

            canWarpToDungeon = Inventory.Instance.HasTriforcePieceForDungeon(_nextWarpDungeonNum);
        } while (!canWarpToDungeon && ++count < WorldInfo.NUM_DUNGEONS);
    }
}