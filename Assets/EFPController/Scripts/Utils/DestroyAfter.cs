using UnityEngine;

namespace EFPController.Utils
{

    public class DestroyAfter : MonoBehaviour
    {

        public float lifeTime;
        private float destroyTime = float.PositiveInfinity;

        void Start()
        {
            destroyTime = Time.time + lifeTime;
        }

        private void Update()
        {
            if (destroyTime < Time.time)
            {
                Destroy(gameObject);
            }
        }

    }

}