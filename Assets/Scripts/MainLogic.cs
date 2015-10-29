using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public delegate void ChangeFrame(FrameInfo currentFrame);

    public class MainLogic : MonoBehaviour
    {
        public event ChangeState ChangeState;

        public event ChangeFrame ChangeFrame;

        public GameObject FramePrefab;

        public GameObject VertexPrefab;

        public GameObject RibPrefab;

        private GameObject _stopStateMenu;

        private GameObject _playStateMenu;

        private GameObject _framesContent;

        private GameObject _currentFrame;

        private GUIState _currentState;

        private readonly List<FrameInfo> _frames;

        private readonly List<Vertex> _vertices;

        private int _selectedIndex;

        private Vertex _activeVertex;

        private int _startSelectFrameAnimation;

        private float _timeAnimation;
        private float _nextStepTimeAnimation;

        public MainLogic()
        {
            _currentState = GUIState.None;
            _frames = new List<FrameInfo>();
            _vertices = new List<Vertex>(10);
        }

        // Use this for initialization
        void Start()
        {
            _stopStateMenu = gameObject.Find("StopStateMenu", true);
            _playStateMenu = gameObject.Find("PlayStateMenu", true);

            _currentFrame = GameObject.Find("CurrentFrame");

            _framesContent = GameObject.Find("FramesContent");

            _frames.Clear();

            _selectedIndex = -1;

            AddFrame();

            SetState(GUIState.Stop);
        }
        
        private void UpdateStatePlay()
        {
            _timeAnimation += Time.deltaTime;

            if (_timeAnimation > _nextStepTimeAnimation)
            {
                var nextIndex = _selectedIndex + 1;

                if (_selectedIndex >= _frames.Count - 1)
                {
                    nextIndex = 0;
                    _nextStepTimeAnimation = 0;
                    _timeAnimation = 0;
                }

                ChangeActiveFrame(nextIndex); //временно
                //TODO потом переделать на анимацю
                var frame = _frames[_selectedIndex];
                //frame.IsActive = true;
                _nextStepTimeAnimation += frame.FrameTime;
            }
        }

        private void UpdateStateStop()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (_selectedIndex - 1 >= 0)
                {
                    ChangeActiveFrame(_selectedIndex - 1);
                }
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                if (_selectedIndex + 1 < _frames.Count)
                {
                    ChangeActiveFrame(_selectedIndex + 1);
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
                    ChangeActiveFrame(_selectedIndex - 1 >= 0 ? _selectedIndex - 1 : 0);
                }
            }

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.GetMouseButtonDown(1))
            {
                var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                var vertex = _vertices.FirstOrDefault(item => item.Collision(mouse));

                if (vertex == null)
                {
                    var item = (GameObject)Instantiate(VertexPrefab, Vector3.zero, Quaternion.identity);
                    item.transform.parent = _currentFrame.transform;
                    item.transform.position = new Vector3(mouse.x, mouse.y, 0);
                    _vertices.Add(item.GetComponent<Vertex>());
                }
                else
                {
                    Destroy(vertex.gameObject);
                    _vertices.Remove(vertex);

                    foreach (var frame in _frames)
                    {
                        frame.Changes.Remove(vertex.Id);
                    }
                }
            }
            else if (Input.GetMouseButton(0) && _activeVertex != null)
            {
                var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _activeVertex.transform.position = new Vector3(mouse.x, mouse.y, 0);
            }
            else if (Input.GetMouseButtonUp(0) && _activeVertex != null)
            {
                _activeVertex = null;
            }
            else if (Input.GetMouseButtonDown(0) && _activeVertex == null)
            {
                var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var vertex = _vertices.FirstOrDefault(item => item.Collision(mouse));
                _activeVertex = vertex;
            }
        }

        void Update()
        {
            if (_frames.Count == 0)
                return;

            switch (_currentState)
            {
                case GUIState.Play:
                    UpdateStatePlay();
                    break;
                case GUIState.Stop:
                    UpdateStateStop();
                    break;
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
                _stopStateMenu.SetActive(false);
                _playStateMenu.SetActive(false);
                _startSelectFrameAnimation = 0;
            }
            else if (_currentState == GUIState.Play)
            {
                _timeAnimation = 0;
                _nextStepTimeAnimation = 0;

                _playStateMenu.SetActive(false);
            }
            else if (_currentState == GUIState.Stop)
            {
                _stopStateMenu.SetActive(false);
            }

            var oldState = _currentState;
            _currentState = state;

            if (_currentState == GUIState.Play)
            {
                _activeVertex = null;

                _startSelectFrameAnimation = _selectedIndex;

                for (var i = 0; i < _selectedIndex; i++)
                {
                    _timeAnimation += _frames[i].FrameTime;
                }
                _nextStepTimeAnimation = _timeAnimation + _frames[_selectedIndex].FrameTime;

                LoadChangeByFrame(_selectedIndex);

                _playStateMenu.SetActive(true);
            }
            else if (_currentState == GUIState.Stop)
            {
                _selectedIndex = _startSelectFrameAnimation;
                
                LoadChangeByFrame(_selectedIndex + 1);

                _stopStateMenu.SetActive(true);
            }

            var @event = ChangeState;
            if (@event != null)
            {
                @event(oldState, _currentState);
            }
        }

        public void AddFrame()
        {
            if (_currentState == GUIState.Play)
                return;

            var frame = (GameObject) Instantiate(FramePrefab, Vector3.zero, Quaternion.identity);
            frame.transform.SetParent(_framesContent.transform);

            frame.transform.localScale = new Vector3(1f, 1f, 1f);
            var position = frame.transform.localPosition;
            frame.transform.localPosition = new Vector3(position.x, position.y, 0);

            var frameInfo = frame.GetComponent<FrameInfo>();
            _frames.Add(frameInfo);

            if (_frames.Count == 1)
            {
                frameInfo.FrameTime = 0;
                ChangeActiveFrame(0);
            }
        }
        
        private void ChangeActiveFrame(int nextIndex)
        {
            Debug.Log("ChangeActiveFrame " + nextIndex + " из " + _frames.Count);

            if (_selectedIndex >= 0)
                _frames[_selectedIndex].IsActive = false;

            SaveChangeByFrame();

            _selectedIndex = nextIndex;

            var frame = _frames[_selectedIndex];

            frame.IsActive = true;

            LoadChangeByFrame(_selectedIndex + 1);

            var @event = ChangeFrame;
            if (@event != null)
            {
                @event(frame);
            }
        }

        private void SaveChangeByFrame()
        {
            if (_selectedIndex < 0)
                return;
            
            var frame = _frames[_selectedIndex];

            frame.Changes.Clear();

            if (_selectedIndex == 0)
            {
                foreach (var vertex in _vertices)
                {
                    ChangeItem item;
                    if (!frame.Changes.TryGetValue(vertex.Id, out item))
                    {
                        item = new ChangeItem(vertex.Id);
                        frame.Changes.Add(vertex.Id, item);
                    }
                    item.PositionOffset = vertex.transform.position;
                }
            }
            else
            {
                foreach (var vertex in _vertices)
                {
                    var previousPosition = GetVertexPosition(vertex.Id, _selectedIndex);
                    if (vertex.transform.position == previousPosition)
                    {
                        continue;
                    }

                    ChangeItem item;
                    if (!frame.Changes.TryGetValue(vertex.Id, out item))
                    {
                        item = new ChangeItem(vertex.Id);
                        frame.Changes.Add(vertex.Id, item);
                    }
                    item.PositionOffset = vertex.transform.position - previousPosition;
                }
            }
        }

        private Vector3 GetVertexPosition(int vertexId, int frameIndex)
        {
            var vertexPosition = Vector3.zero;
            for (var i = 0; i < frameIndex; i++)
            {
                ChangeItem item;
                if (_frames[i].Changes.TryGetValue(vertexId, out item))
                {
                    vertexPosition += item.PositionOffset;
                }
            }

            return vertexPosition;
        }

        private void LoadChangeByFrame(int selectedIndex)
        {
            foreach (var vertex in _vertices)
            {
                vertex.transform.position = Vector3.zero;
            }

            for (var i = 0; i < selectedIndex; i++)
            {
                var frame = _frames[i];

                foreach (var change in frame.Changes)
                {
                    var vertex = _vertices.FirstOrDefault(s => s.Id == change.Key);
                    if (vertex == null)
                    {
                        Debug.LogError("vertex == null id ==" + change.Key + " frame == " + i);
                        continue;
                    }

                    vertex.transform.position += change.Value.PositionOffset;
                }
            }
        }
    }
}
