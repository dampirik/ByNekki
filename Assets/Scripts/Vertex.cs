using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Vertex : MonoBehaviour
    {
        private static int _index;

        public float Radius = 1;

        public int Id { get; private set; }

        private double _radiusPow2;
        
        public Vertex()
        {
            Id = _index++;
        }
        
        void Start()
        {
            _radiusPow2 = Math.Pow(Radius, 2);
        }

        public bool Collision(Vector3 point)
        {
            var delta = Math.Pow((transform.position.x - point.x), 2) + Math.Pow((transform.position.y - point.y), 2);

            if (delta <= _radiusPow2)
                return true;

            return false;
        }
    }
}
