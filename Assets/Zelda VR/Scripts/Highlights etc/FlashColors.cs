using UnityEngine;
using UnityEngine.UI;

public class FlashColors : MonoBehaviour
{
    public Color normalColor = Color.gray;
    public Color flashColor = Color.white;
    public float speed = 30.0f;
    public bool useSharedMaterial;


    Renderer _renderer;
    Graphic _graphic;
    float _time;
    Color _origColor;


    void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _graphic = GetComponentInChildren<Graphic>();

        if (_renderer != null)
        {
            Material m = useSharedMaterial ? _renderer.sharedMaterial : _renderer.material;
            _origColor = m.color;
        }
        else if (_graphic != null)
        {
            _origColor = _graphic.color;
        }

        SetColor(normalColor);
    }

    void OnEnable()
    {
        _time = 0;
        SetColor(normalColor);
    }
    void OnDisable()
    {
        SetColor(_origColor);
    }


    void Update()
    {
        _time += Time.deltaTime;

        const float PhaseShift = -Mathf.PI / 2;
        float t = (Mathf.Sin(PhaseShift + speed * _time) + 1) * 0.5f;
        Color c = Color.Lerp(normalColor, flashColor, t);
        SetColor(c);
    }

    void SetColor(Color color)
    {
        if (_renderer != null)
        {
            Material m = useSharedMaterial ? _renderer.sharedMaterial : _renderer.material;
            m.SetColor("_Color", color);
        }

        if (_graphic != null)
        {
            _graphic.color = color;
        }
    }
}