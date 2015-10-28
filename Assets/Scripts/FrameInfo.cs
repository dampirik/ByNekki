using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class FrameInfo : MonoBehaviour
    {
        public Color ActiveColor = Color.red;
        
        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set {
                _isActive = value;

                if (_isStarter)
                    gameObject.GetComponent<Image>().color = _isActive ? ActiveColor : _startColor;
            }
        }

        private Color _startColor;
        private bool _isStarter;

        public List<Vertex> Vertices { get; private set; }

        public FrameInfo()
        {
            Vertices = new List<Vertex>(10);
        }

        void Start()
        {
            _startColor = gameObject.GetComponent<Image>().color;

            _isStarter = true;

            if (IsActive)
            {
                gameObject.GetComponent<Image>().color = ActiveColor;
            }
        }
    }
}
