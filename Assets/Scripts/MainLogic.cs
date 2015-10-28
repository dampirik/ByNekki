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

        private GameObject _stopGameObject;

        private GameObject _playGameObject;

        private GameObject _framesContent;

        private GameObject _currentFrame;

        private GUIState _currentState;

        private readonly List<FrameInfo> _frames;

        private int _selectedIndex;

        public MainLogic()
        {
            _currentState = GUIState.None;
            _frames = new List<FrameInfo>(); 
        }

        // Use this for initialization
        void Start()
        {
            _stopGameObject = gameObject.Find("StopState", true);
            _playGameObject = gameObject.Find("PlayState", true);
            _currentFrame = GameObject.Find("CurrentFrame");

            _framesContent = GameObject.Find("FramesContent");

            _frames.Clear();

            AddFrame();

            SetState(GUIState.Play);
        }

        private Vertex _activeVertex;

        void Update()
        {
            if (_frames.Count == 0)
                return;

            if(Input.GetKeyDown(KeyCode.A))
            {
                if (_selectedIndex - 1 >= 0)
                {
                    _frames[_selectedIndex].IsActive = false;
                    
                    ClearCurrentFrame();

                    _selectedIndex--;
                    SetActiveFrame();
                }
            }
            else if(Input.GetKeyDown(KeyCode.D))
            {
                if (_selectedIndex + 1 < _frames.Count)
                {
                    _frames[_selectedIndex].IsActive = false;

                    ClearCurrentFrame();

                    _selectedIndex++;
                    SetActiveFrame();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (_selectedIndex < 0)
                    return;

                var frame = _frames[_selectedIndex];

                ClearCurrentFrame();

                _frames.RemoveAt(_selectedIndex);
                Destroy(frame.gameObject);

                if (_frames.Count > 0)
                {
                    _selectedIndex = _selectedIndex - 1 >= 0 ? _selectedIndex - 1 : 0;
                    SetActiveFrame();
                }
            }

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.GetMouseButtonDown(1))
            {
                var frame = _frames[_selectedIndex];

                var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                var vertex = frame.Vertices.FirstOrDefault(item => item.Collision(mouse));

                if (vertex == null)
                {
                    var item = (GameObject)Instantiate(VertexPrefab, Vector3.zero, Quaternion.identity);
                    item.transform.parent = _currentFrame.transform;

                    vertex = new Vertex
                                 {
                                     CurrentGameObject = item,
                                     Position = new Vector3(mouse.x, mouse.y, 0)
                                 };
                    frame.Vertices.Add(vertex);
                }
                else
                {
                    Destroy(vertex.CurrentGameObject);
                    frame.Vertices.Remove(vertex);
                }
            }
            else if (Input.GetMouseButton(0) && _activeVertex != null)
            {
                var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _activeVertex.Position = new Vector3(mouse.x, mouse.y, 0);
            }
            else if (Input.GetMouseButtonUp(0) && _activeVertex != null)
            {
                _activeVertex = null;
            }
            else if (Input.GetMouseButtonDown(0) && _activeVertex == null)
            {
                var frame = _frames[_selectedIndex];
                var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var vertex = frame.Vertices.FirstOrDefault(item => item.Collision(mouse));
                _activeVertex = vertex;
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
        
        private void ClearCurrentFrame()
        {
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; ++i)
            {
                var child = transform.GetChild(i);
                Destroy(child.gameObject);
            }

            var frame = _frames[_selectedIndex];
            foreach (var vertex in frame.Vertices)
            {
                vertex.CurrentGameObject = null;
            }
        }

        private void CreateCurrentFrame()
        {
            var frame = _frames[_selectedIndex];
            foreach (var vertex in frame.Vertices)
            {
                var item = (GameObject)Instantiate(VertexPrefab, Vector3.zero, Quaternion.identity);
                item.transform.parent = _currentFrame.transform;
                item.transform.position = vertex.Position;
            }
        }

        private void SetActiveFrame()
        {
            var frame = _frames[_selectedIndex];

            frame.IsActive = true;

            CreateCurrentFrame();

            var @event = ChangeFrame;
            if (@event != null)
            {
                @event(frame);
            }
        }
    }
}
