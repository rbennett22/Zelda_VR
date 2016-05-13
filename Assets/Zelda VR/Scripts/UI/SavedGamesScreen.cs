using UnityEngine;

public class SavedGamesScreen : MonoBehaviour
{
    public GameObject saveEntryPrefab;
    public Transform saveEntriesContainer;

    public int capacity;
    public float entryHeight;


    SaveEntryView[] _entries;
    SaveEntryView _selectedEntry;


    MenuCursor _cursor;


    public int CursorIndex
    {
        get { return _cursor.CursorIndex.y; }
        set { _cursor.CursorIndex.y = value; }
    }
    void OnCursorIndexChanged(MenuCursor sender)
    {
        if (sender != _cursor)
        {
            return;
        }

        SelectEntry(CursorIndex);
    }


    void Awake()
    {
        InstantiateSaveEntries();
        InstantiateMenuCursor();
    }

    void InstantiateSaveEntries()
    {
        _entries = new SaveEntryView[capacity];

        for (int i = 0; i < capacity; i++)
        {
            InstantiateSaveEntry(i);
        }
    }
    void InstantiateSaveEntry(int id)
    {
        GameObject g = Instantiate(saveEntryPrefab) as GameObject;
        SaveEntryView entryView = g.GetComponent<SaveEntryView>();

        entryView.transform.parent = saveEntriesContainer;
        entryView.transform.localPosition = new Vector3(0, -id * entryHeight, -0.1f);
        entryView.transform.localRotation = Quaternion.identity;

        entryView.InitWithEntryData(SaveManager.Instance.LoadEntryData(id));

        _entries[id] = entryView;
    }

    void InstantiateMenuCursor()
    {
        _cursor = gameObject.AddComponent<MenuCursor>();

        _cursor.numColumns = 1;
        _cursor.numRows = capacity;
        _cursor.onIndexChanged_Callback = OnCursorIndexChanged;

        CursorIndex = 0;
        OnCursorIndexChanged(_cursor);
    }


    void Update () 
    {
        UpdateCursor();

        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.Start) 
            || ZeldaInput.GetButtonDown(ZeldaInput.Button.SwordAttack))
        {
            PlaySelectSound();
            LoadSelectedEntry();
        }

        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.Extra))      // TODO
        {
            DeleteSelectedEntry();
        }
	}

    void UpdateCursor()
    {
        float moveVert = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveVertical);
        Vector2 dir = new Vector2(0, moveVert);

        if (_cursor.TryMoveCursor(dir))
        {
            PlaySelectSound();
        }
    }

    void SelectEntry(int id)
    {
        print("SelectEntry: " + id);

        // Deselect previous
        if(_selectedEntry != null)
        {
            _selectedEntry.MarkAsSelected(false);
        }

        // Select new
        _selectedEntry = _entries[id];
        if (_selectedEntry != null)
        {
            _selectedEntry.MarkAsSelected();
        }
    }


    void LoadSelectedEntry()
    {
        if (_selectedEntry == null)
        {
            return;
        }

        print("LoadSelectedEntry: " + _selectedEntry.name);

        CommonObjects.PlayerController_C.SetHaltUpdateMovement(false);

        Player player = CommonObjects.Player_C;
        player.RegisteredName = _selectedEntry.PlayerName;
        player.DeathCount = _selectedEntry.PlayerDeathCount;

        SaveManager.Instance.LoadGame(CursorIndex);
    }

    void DeleteSelectedEntry()
    {
        if(_selectedEntry == null)
        {
            return;
        }

        print("DeleteSelectedEntry: " + _selectedEntry.name);

        bool success = SaveManager.Instance.DeleteGame(CursorIndex);
        if (success)
        {
            SoundFx.Instance.PlayOneShot(SoundFx.Instance.flame);

            Destroy(_selectedEntry.gameObject);
            InstantiateSaveEntry(CursorIndex);
            SelectEntry(CursorIndex);
        }
        else
        {
            SoundFx.Instance.PlayOneShot(SoundFx.Instance.shield);
        }
    }


    void PlaySelectSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.select);
    }
}