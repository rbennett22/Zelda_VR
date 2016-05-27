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

        return TryMoveCursor(Index2.GetDirectionForVector2(dir));
    }
    public bool TryMoveCursor(Index2.Direction dir)
    {
        if (_cursorCooldownActive)
        {
            return false;
        }

        Index2 newIndex = _cursorIndex.GetAdjacentIndex(dir);

        if (wraps)
        {
            if (newIndex.x < 0) { newIndex.x += numColumns; }
            else if (newIndex.x >= numColumns) { newIndex.x -= numColumns; }

            if (newIndex.y < 0) { newIndex.y += numRows; }
            else if (newIndex.y >= numRows) { newIndex.y -= numRows; }
        }

        CursorIndex = newIndex;

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