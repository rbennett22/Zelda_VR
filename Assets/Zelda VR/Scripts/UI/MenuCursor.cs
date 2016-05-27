using Immersio.Utility;
using System;
using UnityEngine;

public class MenuCursor : MonoBehaviour
{
    const float CURSOR_COOLDOWN_DURATION = 0.2f;


    public int numColumns, numRows;
    public bool wraps = true;

    public Action<MenuCursor> onIndexChanged_Callback;


    Index2 _cursorIndex = new Index2();


    public void SetCursorX(int value)
    {
        Index2 n = CursorIndex;
        n.x = value;
        CursorIndex = n;
    }
    public void SetCursorY(int value)
    {
        Index2 n = CursorIndex;
        n.y = value;
        CursorIndex = n;
    }
    public Index2 CursorIndex
    {
        get { return _cursorIndex; }
        set
        {
            Index2 clampedIndex = new Index2();
            clampedIndex.x = Mathf.Clamp(value.x, 0, numColumns - 1);
            clampedIndex.y = Mathf.Clamp(value.y, 0, numRows - 1);

            if (clampedIndex == _cursorIndex)
            {
                return;
            }
            _cursorIndex = clampedIndex;

            OnIndexChanged();
        }
    }
    void OnIndexChanged()
    {
        if (onIndexChanged_Callback != null)
        {
            onIndexChanged_Callback(this);
        }
    }


    public bool TryMoveCursor(Vector2 vec)
    {
        Vector2 dir = vec.GetNearestNormalizedAxisDirection();

        if (dir.x == 0 && dir.y == 0)
        {
            return false;
        }

        return TryMoveCursor(new IndexDirection2(dir));
    }
    public bool TryMoveCursor(IndexDirection2 dir)
    {
        if (_cursorCooldownActive)
        {
            return false;
        }

        Index2 n = _cursorIndex + dir;

        if (wraps)
        {
            if (n.x < 0) { n.x += numColumns; }
            else if (n.x >= numColumns) { n.x -= numColumns; }

            if (n.y < 0) { n.y += numRows; }
            else if (n.y >= numRows) { n.y -= numRows; }
        }

        CursorIndex = n;

        StartCursorCooldownTimer();

        return true;
    }


    void Update()
    {
        if (_cursorCooldownActive)
        {
            UpdateCursorCooldownTimer();
        }
    }


    #region Cursor Cooldown

    bool _cursorCooldownActive;
    float _cursorCooldownTimer;


    void StartCursorCooldownTimer()
    {
        _cursorCooldownTimer = CURSOR_COOLDOWN_DURATION;
        _cursorCooldownActive = true;
    }
    void UpdateCursorCooldownTimer()
    {
        _cursorCooldownTimer -= Time.unscaledDeltaTime;
        if (_cursorCooldownTimer <= 0)
        {
            _cursorCooldownActive = false;
        }
    }

    #endregion Cursor Cooldown
}