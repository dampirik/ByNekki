using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class RibData : MonoBehaviour
    {
        public VertexData VertexA;

        public VertexData VertexB;

        public Vector2 Size;

        private Vector3 _oldPositionA;

        private Vector3 _oldPositionB;

        void Start()
        {
            _oldPositionA = VertexA.transform.position;
            _oldPositionB = VertexB.transform.position;

            Create();
        }
        
        void Update()
        {
            if (_oldPositionA != VertexA.transform.position ||
                _oldPositionB != VertexB.transform.position)
            {
                _oldPositionA = VertexA.transform.position;
                _oldPositionB = VertexB.transform.position;

                Create();
            }
        }

        private void Create()
        {
            transform.position = VertexA.transform.position;

            var vectorToTarget = VertexB.transform.position - VertexA.transform.position;
            var angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            var size = (float)Math.Sqrt(Math.Pow(vectorToTarget.x, 2) + Math.Pow(vectorToTarget.y, 2));

            transform.localScale = new Vector3(size, 1, 1);
        }
    }
}
