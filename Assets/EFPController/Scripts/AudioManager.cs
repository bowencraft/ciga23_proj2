using UnityEngine;
using UnityEngine.Audio;
using EFPController.Utils;

namespace EFPController
{

    public class AudioManager
    {

        public static void PlaySFX(AudioSource source, AudioClip clip, float spatialBlend = -1f, float rolloffDistanceMin = -1f, float rolloffDistanceMax = -1f, float volume = -1f, float reverbZoneMix = -1f)
        {
            if (clip == null || source == null) return;
            source.clip = clip;
            if (volume >= 0f) source.volume = volume;
            if (spatialBlend >= 0f) source.spatialBlend = spatialBlend;
            if (rolloffDistanceMin >= 0f) source.minDistance = rolloffDistanceMin;
            if (rolloffDistanceMax >= 0f) source.maxDistance = rolloffDistanceMax;
            if (reverbZoneMix >= 0f) source.reverbZoneMix = reverbZoneMix;
            source.Play();
        }

        public static AudioSource CreateSFX(AudioClip clip, Vector3 position, float spatialBlend = 1f, float rolloffDistanceMin = 1f, float rolloffDistanceMax = 50f, float volume = 1f, float reverbZoneMix = 1f)
        {
            GameObject impactSFXInstance = new GameObject();
            impactSFXInstance.transform.position = position;
            DestroyAfter timedSelfDestruct = impactSFXInstance.AddComponent<DestroyAfter>();
            timedSelfDestruct.lifeTime = clip.length;
            AudioSource source = impactSFXInstance.AddComponent<AudioSource>();
            PlaySFX(source, clip, spatialBlend, rolloffDistanceMin, rolloffDistanceMax, volume, reverbZoneMix);
            return source;
        }

    }

}