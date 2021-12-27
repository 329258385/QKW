using UnityEngine;
using System.Collections;




public class SpaceParticles : MonoBehaviour 
{	
	// Maximum number of particles in the sphere (configure to your needs for look and performance)
	public int              maxParticles = 1000;
	public float            range = 50.0f;
	public float            distanceSpawn = 0.95f;	
	public float            minParticleSize = 0.5f;
	public float            maxParticleSize = 1.0f;	
	public float            sizeMultiplier = 1.0f;	
	public float            minParticleDriftSpeed = 0.0f;
	public float            maxParticleDriftSpeed = 1.0f;
	public float            driftSpeedMultiplier = 1.0f;	
	public bool             fadeParticles = true;
	public float            distanceFade = 0.5f;

	// Private variables
	private float           _distanceToSpawn;
	private float           _distanceToFade;
	private ParticleSystem  _cacheParticleSystem;
	private Transform       _cacheTransform;
	
	void Start () {
		// Cache transform and particle system to improve performance
		_cacheTransform         = transform;
		_cacheParticleSystem    = GetComponent<ParticleSystem>();
        _distanceToSpawn        = range * distanceSpawn;
        _distanceToFade         = range * distanceFade;
		
		// Scale particles
		if (_cacheParticleSystem == null)
        {
			
		}
		
		// Spawn all new particles within a sphere in range of the object
		for (int i=0; i < maxParticles; i ++) 
        {
			ParticleSystem.Particle _newParticle = new ParticleSystem.Particle();					
			_newParticle.position                = _cacheTransform.position + (Random.insideUnitSphere * _distanceToSpawn);
			_newParticle.remainingLifetime       = Mathf.Infinity;
			Vector3 _velocity = new Vector3(
				Random.Range(minParticleDriftSpeed, maxParticleDriftSpeed) * driftSpeedMultiplier, 
				Random.Range(minParticleDriftSpeed, maxParticleDriftSpeed) * driftSpeedMultiplier, 
				Random.Range(minParticleDriftSpeed, maxParticleDriftSpeed) * driftSpeedMultiplier);			
			_newParticle.velocity               = _velocity;						
			_newParticle.startSize              = Random.Range(minParticleSize, maxParticleSize) * sizeMultiplier;								
			GetComponent<ParticleSystem>().Emit(1);
			
		}			
	}
	
	void Update () 
    {
		int _numParticles = GetComponent<ParticleSystem>().particleCount;		
		ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[_numParticles];
		GetComponent<ParticleSystem>().GetParticles(_particles);

		for (int i = 0; i < _particles.Length; i++) 
        {			
			// Calcualte distance to particle from transform position
			float _distance = Vector3.Distance(_particles[i].position, _cacheTransform.position);
            if (_distance > range) 
            {						
				// reposition (respawn) particle according to spawn distance
				_particles[i].position = Random.onUnitSphere * _distanceToSpawn + _cacheTransform.position;								
				// Re-calculate distance to particle for fading
				_distance = Vector3.Distance(_particles[i].position, _cacheTransform.position);				
				// Set a new velocity of the particle
				Vector3 _velocity = new Vector3(
					Random.Range(minParticleDriftSpeed, maxParticleDriftSpeed) * driftSpeedMultiplier, 
					Random.Range(minParticleDriftSpeed, maxParticleDriftSpeed) * driftSpeedMultiplier, 
					Random.Range(minParticleDriftSpeed, maxParticleDriftSpeed) * driftSpeedMultiplier);			
				_particles[i].velocity = _velocity;
				// Set a new size of the particle
                _particles[i].startSize = Random.Range(minParticleSize, maxParticleSize) * sizeMultiplier;						
			}
			
			// If particle fading is enabled...
            if (fadeParticles)
            {
                Color _col = _particles[i].startColor;
                if (_distance > _distanceToFade)
                {
                    // Fade alpha value of particle between fading distance and spawnin distance
                    _particles[i].startColor = new Color(_col.r, _col.g, _col.b, Mathf.Clamp01(1.0f - ((_distance - _distanceToFade) / (_distanceToSpawn - _distanceToFade))));
                }
                else
                {
                    // Particle is within range so ensure it has full alpha value
                    _particles[i].startColor = new Color(_col.r, _col.g, _col.b, 1.0f);
                }
            }
       	}        
		
		// Set the particles according to above modifications
    	GetComponent<ParticleSystem>().SetParticles(_particles, _numParticles);    	
	}
}
