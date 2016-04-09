using UnityEngine;


public class LoadSelectScreen : MonoBehaviour
{

    public GameObject saveEntryPrefab;
    public Transform saveEntriesContainer;
    public int numSaveEntries;

    public float tableRowHeight;


    int _selectedEntryID;
    SaveEntry[] _entries;

    bool _inputCooldownActive = false;
    int _inputCooldownDuration = 8;
    int _inputCooldownCounter = 0;


    void Start()
    {
        _entries = new SaveEntry[numSaveEntries];

        InstantiateSaveEntries();
        SelectEntry(0);
    }

    void InstantiateSaveEntries()
    {
        for (int i = 0; i < numSaveEntries; i++)
        {
            InstantiateSaveEntry(i);
        }
    }

    void InstantiateSaveEntry(int id)
    {
        GameObject g = Instantiate(saveEntryPrefab) as GameObject;
        SaveEntry entry = g.GetComponent<SaveEntry>();

        entry.transform.parent = saveEntriesContainer;
        entry.transform.localPosition = new Vector3(0, -id * tableRowHeight, -0.1f);
        entry.transform.localRotation = Quaternion.identity;

        entry.ID = id;
        entry.InitWithEntryData(SaveManager.Instance.LoadEntryData(id));

        _entries[id] = entry;
    }


	void Update () 
    {
        UpdateSelection();

        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.Start))
        {
            LoadSelectedEntry();
        }

        /*if (ZeldaInput.GetButtonDown(ZeldaInput.Button.Extra))      // TODO
        {
            DeleteSelectedEntry();
        }*/
	}

    void UpdateSelection()
    {
        if (_inputCooldownActive)
        {
            if (++_inputCooldownCounter >= _inputCooldownDuration) { _inputCooldownActive = false; }
        }
        else
        {
            float moveVertAxis = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveVertical);
            if (moveVertAxis > 0)
            {
                PrevEntry();
            }
            else if (moveVertAxis < 0)
            {
                NextEntry();
            }
        }
    }


    void PrevEntry()
    {
        int prev = _selectedEntryID - 1;
        if (prev < 0) { prev = numSaveEntries - 1; }
        SelectEntry(prev);

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.select);
    }

    void NextEntry()
    {
        int next = _selectedEntryID + 1;
        if (next >= numSaveEntries) { next = 0; }
        SelectEntry(next);

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.select);
    }

    void SelectEntry(int id)
    {
        print("SelectEntry: " + id);

        _entries[_selectedEntryID].MarkAsSelected(false);

        _entries[id].MarkAsSelected();
        _selectedEntryID = id;

        _inputCooldownActive = true;
        _inputCooldownCounter = 0;
    }


    void LoadSelectedEntry()
    {
        print("LoadSelectedEntry: " + _selectedEntryID);

        SoundFx.Instance.PlayOneShot(SoundFx.Instance.select);

        CommonObjects.PlayerController_C.controlsEnabled = true;
        GameplayHUD.Instance.enabled = true;

        SaveEntry entry = _entries[_selectedEntryID];
        Player player = CommonObjects.Player_C;
        player.Name = entry.PlayerName;
        player.DeathCount = entry.PlayerDeathCount;

        SaveManager.Instance.LoadGame(_selectedEntryID);
    }

    void DeleteSelectedEntry()
    {
        print("DeleteSelectedEntry: " + _selectedEntryID);

        bool success = SaveManager.Instance.DeleteGame(_selectedEntryID);
        if (success)
        {
            SoundFx.Instance.PlayOneShot(SoundFx.Instance.flame);

            Destroy(_entries[_selectedEntryID].gameObject);
            InstantiateSaveEntry(_selectedEntryID);
            SelectEntry(_selectedEntryID);
        }
        else
        {
            SoundFx.Instance.PlayOneShot(SoundFx.Instance.shield);
        }
    }

}