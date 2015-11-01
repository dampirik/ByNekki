using System.IO;
using System.Xml;
using System.Linq;
using UnityEngine;
using System.Xml.Serialization;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

        private readonly List<VertexData> _vertices;

        private int _selectedIndex;

        private VertexData _activeVertex;

        private int _startSelectFrameAnimation;

        private float _timeAnimation;
        private float _nextStepTimeAnimation;

        public MainLogic()
        {
            _currentState = GUIState.None;
            _frames = new List<FrameInfo>();
            _vertices = new List<VertexData>(10);
        }
        
        public void Export()
        {
            if (_frames.Count <= 0)
                return;

            var data = new SaveData
                       {
                           Frames = new List<Frame>(_frames.Count)
                       };
            for (var i = 0; i < _frames.Count; i++)
            {
                var frameInfo = _frames[i];
                var frame = new Frame
                                {
                                    Id = i + 1,
                                    Vertices = new List<Vertex>(frameInfo.Changes.Count)
                                };
                foreach (var change in frameInfo.Changes)
                {
                    frame.Vertices.Add(new Vertex
                    {
                                         Id = change.Key,
                                         Position = change.Value.CurrentPosition
                                     });
                }
                data.Frames.Add(frame);
            }

            var path = Application.dataPath + "/" + "default.anim";
            using (var stream = File.Open(path, FileMode.Create))
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration = true
                };
                // Use the constructor that takes a type and XmlRootAttribute.
                var serializer = new XmlSerializer(typeof(SaveData));
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    serializer.Serialize(writer, data);
                }
            }

            Debug.Log("Export");
        }
        
        public void Import()
        {
            foreach (var frame in _frames)
            {
                Destroy(frame.gameObject);
            }
            _frames.Clear();
            _selectedIndex = -1;
            
            foreach (var vertex in _vertices)
            {
                Destroy(vertex.gameObject);
            }
            _vertices.Clear();

            SaveData data;

            var path = Application.dataPath + "/" + "default.anim";
            if (!File.Exists(path))
                return;

            using (var stream = File.Open(path, FileMode.Open))
            {
                var settings = new XmlReaderSettings
                {
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true
                };
                // Use the constructor that takes a type and XmlRootAttribute.
                var serializer = new XmlSerializer(typeof(SaveData));
                using (var reader = XmlReader.Create(stream, settings))
                {
                    var obj = serializer.Deserialize(reader);

                    data = obj as SaveData;
                }
            }

            if (data == null)
                return;

            for (var i = 0; i < data.Frames.Count; i++)
            {
                AddFrame();
                var frameInfo = _frames[i];
                foreach (var vertex in data.Frames[i].Vertices)
                {
                    frameInfo.Changes.Add(vertex.Id, new ChangeItem(vertex.Id)
                                                     {
                                                         CurrentPosition = vertex.Position
                                                     });
                }
            }


            foreach (var changeItem in _frames[0].Changes)
            {
                var item = (GameObject)Instantiate(VertexPrefab, Vector3.zero, Quaternion.identity);
                item.transform.parent = _currentFrame.transform;
                item.transform.position = Vector3.zero;

                var vertex = item.GetComponent<VertexData>();
                vertex.SetId(changeItem.Key);
                _vertices.Add(vertex);
            }

            _selectedIndex = -1;
            ChangeActiveFrame(0);
            
            Debug.Log("Import");
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

            var frame = _frames[_selectedIndex];
            if (_timeAnimation > _nextStepTimeAnimation)
            {
                var nextIndex = _selectedIndex + 1;

                if (_selectedIndex >= _frames.Count - 1)
                {
                    nextIndex = 0;
                    _nextStepTimeAnimation = 0;
                    _timeAnimation = 0;

                    foreach (var vertex in _vertices)
                    {
                        vertex.transform.position = Vector3.zero;
                    }
                }

                _frames[_selectedIndex].IsActive = false;
                
                _selectedIndex = nextIndex;

                frame = _frames[_selectedIndex];
                frame.IsActive = true;
                
                foreach (var change in frame.Changes)
                {
                    var vertex = _vertices.FirstOrDefault(s => s.Id == change.Key);
                    if (vertex == null)
                    {
                        Debug.LogError("vertex == null id ==" + change.Key + " frame == " + _selectedIndex);
                        continue;
                    }

                    vertex.StartPosition = vertex.transform.position;
                    vertex.EndPosition = change.Value.CurrentPosition;
                }
                
                _nextStepTimeAnimation += frame.FrameTime;
            }

            foreach (var change in frame.Changes)
            {
                var vertex = _vertices.FirstOrDefault(s => s.Id == change.Key);
                if (vertex == null)
                {
                    Debug.LogError("vertex == null id ==" + change.Key + " frame == " + _selectedIndex);
                    continue;
                }

                var t = 1f;
                if (frame.FrameTime > 0f)
                {
                    t -= (_nextStepTimeAnimation - _timeAnimation)/frame.FrameTime;
                }

                vertex.transform.position = Vector3.Lerp(vertex.StartPosition, vertex.EndPosition, t);
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

                var oldIndex = _selectedIndex;

                if (_frames.Count > 0)
                {
                    ChangeActiveFrame(_selectedIndex - 1 >= 0 ? _selectedIndex - 1 : 0);
                }

                var frame = _frames[oldIndex];
                _frames.RemoveAt(oldIndex);
                Destroy(frame.gameObject);
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

                    vertex = item.GetComponent<VertexData>();
                    _vertices.Add(vertex);

                    foreach (var frame in _frames)
                    {
                        ChangeItem change;
                        if (!frame.Changes.TryGetValue(vertex.Id, out change))
                        {
                            change = new ChangeItem(vertex.Id);
                            frame.Changes.Add(vertex.Id, change);
                        }
                        change.CurrentPosition = vertex.transform.position;
                    }
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

                LoadChangeByFrame(_selectedIndex > 0 ? _selectedIndex - 1 : 0);

                var frame = _frames[_selectedIndex];
                foreach (var change in frame.Changes)
                {
                    var vertex = _vertices.FirstOrDefault(s => s.Id == change.Key);
                    if (vertex == null)
                    {
                        Debug.LogError("vertex == null id ==" + change.Key + " frame == " + _selectedIndex);
                        continue;
                    }

                    vertex.StartPosition = vertex.transform.position;
                    vertex.EndPosition = change.Value.CurrentPosition;
                }
                
                _playStateMenu.SetActive(true);
            }
            else if (_currentState == GUIState.Stop)
            {
                _selectedIndex = _startSelectFrameAnimation;

                foreach (var frame in _frames)
                {
                    frame.IsActive = false;
                }

                _frames[_selectedIndex].IsActive = true;

                LoadChangeByFrame(_selectedIndex);

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

            foreach (var vertex in _vertices)
            {
                ChangeItem change;
                if (!frameInfo.Changes.TryGetValue(vertex.Id, out change))
                {
                    change = new ChangeItem(vertex.Id);
                    frameInfo.Changes.Add(vertex.Id, change);
                }
                change.CurrentPosition = vertex.transform.position;
            }

            if (_frames.Count == 1)
            {
                frameInfo.FrameTime = 0;
                ChangeActiveFrame(0);
            }
        }
        
        private void ChangeActiveFrame(int nextIndex)
        {
            Debug.Log("ChangeActiveFrame " + (nextIndex + 1) + " из " + _frames.Count);

            if (_selectedIndex >= 0)
                _frames[_selectedIndex].IsActive = false;

            SaveChangeByFrame();

            _selectedIndex = nextIndex;

            var frame = _frames[_selectedIndex];

            frame.IsActive = true;

            LoadChangeByFrame(_selectedIndex);

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

            foreach (var vertex in _vertices)
            {
                ChangeItem item;
                if (!frame.Changes.TryGetValue(vertex.Id, out item))
                {
                    item = new ChangeItem(vertex.Id);
                    frame.Changes.Add(vertex.Id, item);
                }
                item.CurrentPosition = vertex.transform.position;
            }
        }

        private void LoadChangeByFrame(int selectedIndex)
        {
            var frame = _frames[selectedIndex];
            foreach (var change in frame.Changes)
            {
                var vertex = _vertices.FirstOrDefault(s => s.Id == change.Key);
                if (vertex == null)
                {
                    Debug.LogError("vertex == null id ==" + change.Key + " frame == " + selectedIndex);
                    continue;
                }

                vertex.transform.position = change.Value.CurrentPosition;
            }
        }
    }
}
