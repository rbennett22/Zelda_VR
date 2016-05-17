using Uniblocks;
using UnityEditor;
using UnityEngine;

public class OverworldUniblockGeneration_Editor : MonoBehaviour
{
    const string BLOCKS_FOLDER = "Assets/__BLOCKS";


    [MenuItem(ZeldaEditorMenu.MENU_NAME + "/Create Overworld Uniblock Prefabs")]
    static void CreateOverworldUniblockPrefabs()
    {
        GameObject prefab = Selection.activeObject as GameObject;

        ZeldaVRSettings s = ZeldaVRSettings.Instance;
        int total = 0;
        int hexNum = 0;
        int yMin = s.tileMapSideLengthInTiles - s.tileMapHeightInTiles_WithoutFiller;

        for (int y = s.tileMapSideLengthInTiles - 1; y >= yMin; y--)
        {
            for (int x = 0; x < s.tileMapWidthInTiles_WithoutFiller; x++)
            {
                total++;

                string blockName = "block_" + total;
                string path = BLOCKS_FOLDER + "/" + blockName + ".prefab";

                GameObject clone = PrefabUtility.CreatePrefab(path, prefab);
                Voxel v = clone.GetComponent<Voxel>();

                hexNum = x + (s.tileMapSideLengthInTiles - 1 - y) * s.tileMapSideLengthInTiles;
                v.VName = (hexNum).ToString("X2");  // (hex format)

                v.VTransparency = Transparency.solid;
                v.VColliderType = ColliderType.cube;

                if (v.VTexture.Length == 0)
                    v.VTexture = new Vector2[1];
                v.VTexture[0] = new Vector2(x, y);
            }
        }

        print("Created " + total + " blocks");
    }
    [MenuItem(ZeldaEditorMenu.MENU_NAME + "/Create Overworld Uniblock Prefabs", true)]
    static bool ValidateCreateOverworldUniblockPrefabs()
    {
        GameObject g = Selection.activeObject as GameObject;
        if (g == null) { return false; }

        PrefabType pType = PrefabUtility.GetPrefabType(g);
        return (pType == PrefabType.Prefab) && (g.GetComponent<Voxel>() != null);
    }
}