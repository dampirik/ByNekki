using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public delegate void ChangeFrame(FrameInfo currentFrame);

    public class GUILogic : MonoBehaviour
    {
        public event ChangeState ChangeState;

        public event ChangeFrame ChangeFrame;

        public GameObject FramePrefab;

        private GameObject _stopGameObject;

        private GameObject _playGameObject;

        private GameObject _framesContent;

        private GUIState _currentState;

        private readonly List<FrameInfo> _frames;

        private int _selectedIndex;

        public GUILogic()
        {
            _currentState = GUIState.None;
            _frames = new List<FrameInfo>(); 
        }

        // Use this for initialization
        void Start()
        {
            _stopGameObject = gameObject.Find("StopState", true);
            _playGameObject = gameObject.Find("PlayState", true);

            _framesContent = GameObject.Find("FramesContent");

            _frames.Clear();

            SetState(GUIState.Play);
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.A))
            {
                if (_selectedIndex - 1 >= 0)
                {
                    _frames[_selectedIndex].IsActive = false;

                    _selectedIndex--;
                    SetActiveFrame();
                }
            }
            else if(Input.GetKeyDown(KeyCode.D))
            {
                if (_selectedIndex + 1 < _frames.Count)
                {
                    _frames[_selectedIndex].IsActive = false;

                    _selectedIndex++;
                    SetActiveFrame();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (_selectedIndex < 0)
                    return;

                var frame = _frames[_selectedIndex];

                _frames.RemoveAt(_selectedIndex);
                Destroy(frame.gameObject);

                if (_frames.Count > 0)
                {
                    _selectedIndex = _selectedIndex - 1 >= 0 ? _selectedIndex - 1 : 0;
                    SetActiveFrame();
                }
            }
        }

        public void SetState(int state)
        {
            SetState((GUIState) state);
        }

        public void SetState(GUIState state)
        {
            if (_currentState == GUIState.None)
            {
                _stopGameObject.SetActive(false);
                _playGameObject.SetActive(false);
            }
            else if (_currentState == GUIState.Play)
            {
                _playGameObject.SetActive(false);
            }
            else if (_currentState == GUIState.Stop)
            {
                _stopGameObject.SetActive(false);
            }

            var oldState = _currentState;
            _currentState = state;

            if (_currentState == GUIState.Play)
            {
                _playGameObject.SetActive(true);
            }
            else if (_currentState == GUIState.Stop)
            {
                _stopGameObject.SetActive(true);
            }

            var @event = ChangeState;
            if (@event != null)
            {
                @event(oldState, _currentState);
            }
        }

        public void AddFrame()
        {
            var frame = (GameObject) Instantiate(FramePrefab, Vector3.zero, Quaternion.identity);
            frame.transform.SetParent(_framesContent.transform);

            frame.transform.localScale = new Vector3(1f, 1f, 1f);
            var position = frame.transform.localPosition;
            frame.transform.localPosition = new Vector3(position.x, position.y, 0);

            var frameInfo = frame.GetComponent<FrameInfo>();
            _frames.Add(frameInfo);

            if (_frames.Count == 1)
            {
                _selectedIndex = 0;
                SetActiveFrame();
            }
        }
        
        private void SetActiveFrame()
        {
            var frame = _frames[_selectedIndex];

            frame.IsActive = true;

            var @event = ChangeFrame;
            if (@event != null)
            {
                @event(frame);
            }
        }
    }
}
