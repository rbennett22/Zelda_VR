using UnityEngine;
using Immersio.Utility;

public class SavedGamesScreen : MonoBehaviour
{
    public GameObject saveEntryViewPrefab;
    public Transform saveEntryViewsContainer;

    public int capacity;
    public float entryHeight;


    SaveEntryView[] _entryViews;
    SaveEntryView SelectedEntryView { get { return GetEntryViewAtIndex(CursorIndex); } }
    SaveEntryView GetEntryViewAtIndex(int idx)
    {
        return (idx >= _entryViews.Length) ? null : _entryViews[idx];
    }

    MenuCursor _cursor;


    public int CursorIndex
    {
        get { return _cursor.CursorY; }
        set { _cursor.CursorY = value; }
    }
    void CursorIndexChanged(MenuCursor sender, Index2 prevIdx, Index2 newIdx)
    {
        if (sender != _cursor)
        {
            return;
        }

        SetEntryViewsSelectedState(prevIdx.y, false);
        SetEntryViewsSelectedState(CursorIndex, true);
    }

    void SetEntryViewsSelectedState(int idx, bool value)
    {
        SaveEntryView v = GetEntryViewAtIndex(idx);
        if (v == null)
        {
            return;
        }

        v.UpdateSelectedState(value);
    }


    void Awake()
    {
        InstantiateSaveEntryViews();
        InstantiateMenuCursor();

        CursorIndex = 0;
        SetEntryViewsSelectedState(CursorIndex, true);
    }

    void InstantiateSaveEntryViews()
    {
        _entryViews = new SaveEntryView[capacity];

        for (int i = 0; i < capacity; i++)
        {
            InstantiateSaveEntryView(i);
        }
    }
    void InstantiateSaveEntryView(int idx)
    {
        GameObject g = Instantiate(saveEntryViewPrefab) as GameObject;
        SaveEntryView v = g.GetComponent<SaveEntryView>();

        Transform t = v.transform;
        t.SetParent(saveEntryViewsContainer);
        t.localPosition = new Vector3(0, -idx * entryHeight, -0.1f);
        t.localRotation = Quaternion.identity;

        v.InitWithEntryData(SaveManager.Instance.LoadEntryData(idx));

        _entryViews[idx] = v;
    }

    void InstantiateMenuCursor()
    {
        MenuCursor c = gameObject.AddComponent<MenuCursor>();

        c.numColumns = 1;
        c.numRows = capacity;
        c.onIndexChanged_Callback = CursorIndexChanged;

        _cursor = c;
    }


    void Update()
    {
        UpdateCursor();

        if (GetSelectButtonDown())
        {
            PlaySelectSound();
            LoadEntry(CursorIndex);
        }
        else if (Application.isEditor && GetEraseButtonDown())      // TODO
        {
            EraseEntry(CursorIndex);
        }
    }
    bool GetSelectButtonDown()
    {
        return ZeldaInput.GetButtonDown(ZeldaInput.Button.Start);
    }
    bool GetEraseButtonDown()
    {
        return ZeldaInput.GetButtonDown(ZeldaInput.Button.Extra);
    }

    void UpdateCursor()
    {
        float moveVert = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveVertical);
        Vector2 dir = new Vector2(0, -moveVert);

        if (_cursor.TryMoveCursor(dir))
        {
            PlaySelectSound();
        }
    }


    void LoadEntry(int idx)
    {
        print("LoadEntry: " + idx);

        SaveEntryView v = GetEntryViewAtIndex(idx);
        if (v == null)
        {
            return;
        }

        CommonObjects.PlayerController_C.SetHaltUpdateMovement(false);

        Player p = CommonObjects.Player_C;
        p.RegisteredName = v.PlayerName;
        p.DeathCount = v.PlayerDeathCount;

        SaveManager.Instance.LoadGame(idx);
    }

    void EraseEntry(int idx)
    {
        print("EraseEntry: " + idx);

        SaveEntryView v = GetEntryViewAtIndex(idx);
        if (v == null)
        {
            return;
        }

        bool success = SaveManager.Instance.DeleteGame(CursorIndex);
        if (success)
        {
            PlayDeleteSound();

            Destroy(v.gameObject);
            InstantiateSaveEntryView(CursorIndex);
            SetEntryViewsSelectedState(CursorIndex, true);
        }
        else
        {
            PlayErrorSound();
        }
    }


    void PlaySelectSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.select);
    }
    void PlayDeleteSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.flame);
    }
    void PlayErrorSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.shield);
    }
}