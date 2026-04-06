using UnityEngine;

namespace SkibidiBrainrotFruit.Track
{
    public class TrackChunk : MonoBehaviour
    {
        [SerializeField] private float _length = 30f;

        public float Length => _length;

        public float EndZ => transform.position.z + _length;
    }
}
