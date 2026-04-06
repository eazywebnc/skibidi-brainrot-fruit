using UnityEngine;
using SkibidiBrainrotFruit.Core;
using SkibidiBrainrotFruit.Obstacles;

namespace SkibidiBrainrotFruit.Effects
{
    public class NearMissDetector : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<Obstacle>(out _)) return;

            float distance = Vector3.Distance(
                transform.position,
                other.ClosestPoint(transform.position)
            );

            if (distance < _config.NearMissDistance)
            {
                GameEvents.TriggerNearMiss();
            }
        }
    }
}
