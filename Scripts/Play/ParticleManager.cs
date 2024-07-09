using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;

    [System.Serializable]
    public class ParticleEffect
    {
        public string name;
        public int poolSize = 1;
    }

    [SerializeField] private List<ParticleEffect> particleEffects;
    private Dictionary<string, ObjectPool<ParticleSystem>> particlePools;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeParticlePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeParticlePools()
    {
        particlePools = new Dictionary<string, ObjectPool<ParticleSystem>>();

        foreach (var effect in particleEffects)
        {
            ParticleSystem prefab = Resources.Load<ParticleSystem>(Data.path_particles + effect.name);
            if (prefab != null)
            {
                ObjectPool<ParticleSystem> pool = new ObjectPool<ParticleSystem>(prefab, effect.poolSize, effect.poolSize * 2);
                particlePools.Add(effect.name, pool);
            }
            else
            {
                Debug.LogWarning("Particle prefab not found at: " + Data.path_particles + effect.name);
            }
        }
    }

    public void PlayParticleEffect(string effectName, Vector3 position, Quaternion rotation)
    {

        if (particlePools.ContainsKey(effectName))
        {
            ParticleSystem particle = particlePools[effectName].GetObject();
            particle.transform.position = position;
            particle.transform.rotation = rotation;
            particle.gameObject.SetActive(true);
            particle.Play();

            StartCoroutine(DisableParticleAfterDuration(effectName, particle));
        }
        else
        {
            ParticleSystem particle = Instantiate(Resources.Load<ParticleSystem>(string.Format("{0}{1}", Data.path_particles, effectName)));//Have to Instantiate the particle before it can be used as it is loaded as a prefab.
            particle.transform.position = position;
            particle.transform.rotation = rotation;
            particle.gameObject.SetActive(true);
            particle.Play();
            Destroy(particle.gameObject, particle.main.startLifetime.constantMax);
        }
    }

    private IEnumerator DisableParticleAfterDuration(string effectName, ParticleSystem particle)
    {
        yield return new WaitForSeconds(particle.main.startLifetime.constantMax);

        particle.Stop();
        particle.gameObject.SetActive(false);
        particlePools[effectName].ReturnObject(particle);
    }
    private IEnumerator DisableLoopingParticle(string effectName, ParticleSystem particle)
    {
        // This coroutine waits until the particle system is manually stopped
        yield return new WaitUntil(() => !particle.isPlaying);

        particle.gameObject.SetActive(false);
        if (particlePools.ContainsKey(effectName))
        {
            particlePools[effectName].ReturnObject(particle);
        }
        else
        {
            Destroy(particle.gameObject);
        }
    }
    public void StopAllParticleEffects(string effectName)
    {
        if (particlePools.ContainsKey(effectName))
        {
            List<ParticleSystem> particlesToStop = new List<ParticleSystem>();

            foreach (var activeParticle in particlePools[effectName].ActiveObjects)
            {
                particlesToStop.Add(activeParticle);
            }

            foreach (var particle in particlesToStop)
            {
                particle.Stop();
                particle.gameObject.SetActive(false);
                particlePools[effectName].ReturnObject(particle);
            }
        }
        else
        {
            Debug.LogWarning("Particle effect " + effectName + " not found!");
        }
    }
}
