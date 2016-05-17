using UnityEngine;
using UnityEngine.UI;

namespace Eyefluence.Utility
{
    public class Highlight : MonoBehaviour
    {
        public Color normalColor = Color.gray;
        public Color highlightedColor = Color.white;
        public bool useSharedMaterial;


        Renderer _renderer;
        Graphic _graphic;


        void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _graphic = GetComponentInChildren<Graphic>();

            /*if (_renderer != null)
            {
                Material m = useSharedMaterial ? _renderer.sharedMaterial : _renderer.material;
                normalColor = m.color;
            }
            else if (_graphic != null)
            {
                normalColor = _graphic.color;
            }*/

            SetHighlighted(false);
        }

        void OnEnable()
        {
            SetHighlighted(true);
        }
        void OnDisable()
        {
            SetHighlighted(false);
        }


        void SetHighlighted(bool value)
        {
            Color color = value ? highlightedColor : normalColor;

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
}