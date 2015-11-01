﻿using System;
using UnityEngine;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Assets.Scripts
{
    [Serializable]
    [XmlRoot("Animation")]
    public class SaveData
    {
        [XmlArray("Frames")]
        public List<Frame> Frames;
    }

    [Serializable]
    public class Frame
    {
        public int Id { get; set; }
        [XmlArray("Vertices")]
        public List<Vertex> Vertices;
    }

    [Serializable]
    public class Vertex
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }
    }
}
