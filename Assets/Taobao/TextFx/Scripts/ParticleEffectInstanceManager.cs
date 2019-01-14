// A class to store all necessary information to handle the start, end and transitions of a particle effect within a TextFx animation
using UnityEngine;


namespace TextFx
{
	[System.Serializable]
	public class ParticleEffectInstanceManager
	{
		[System.NonSerialized]
		TextFxAnimationManager m_animation_manager;
		[System.NonSerialized]
		LetterSetup m_letter_setup_ref;

#if !UNITY_5_4_OR_NEWER
		ParticleEmitter m_particle_emitter;
#endif
		ParticleSystem m_particle_system;
		float m_duration = 0;
		float m_delay = 0;
		Vector3 m_position_offset;
		Quaternion m_rotation_offset;
		bool m_rotate_with_letter = true;
		bool m_follow_mesh;
		bool m_active;
		Transform m_transform;
		Quaternion rotation;

#if !UNITY_3_5 && UNITY_EDITOR
		int m_stopped_effect_particle_count = 0, old_num_particles;
		float p_system_timer = 0;
		ParticleSystem.Particle[] m_particles_array;
		ParticleSystem.Particle[] temp_array;
#endif

#if !UNITY_5_4_OR_NEWER
		public ParticleEffectInstanceManager(TextFxAnimationManager animation_manager, LetterSetup letter_setup_ref, ParticleEffectSetup effect_setup, AnimationProgressionVariables progression_vars, AnimatePerOptions animate_per, ParticleEmitter particle_emitter = null, ParticleSystem particle_system = null)
#else
		public ParticleEffectInstanceManager(TextFxAnimationManager animation_manager, LetterSetup letter_setup_ref, ParticleEffectSetup effect_setup, AnimationProgressionVariables progression_vars, AnimatePerOptions animate_per, ParticleSystem particle_system = null)
#endif
		{
#if !UNITY_5_4_OR_NEWER
			m_particle_emitter = particle_emitter;
#endif
			m_particle_system = particle_system;
			m_letter_setup_ref = letter_setup_ref;
			m_follow_mesh = effect_setup.m_follow_mesh;
			m_duration = effect_setup.m_duration.GetValue(progression_vars, animate_per);
			m_delay = effect_setup.m_delay.GetValue(progression_vars, animate_per);
			m_position_offset = effect_setup.m_position_offset.GetValue(null, progression_vars, animate_per);
			m_rotation_offset = Quaternion.Euler(effect_setup.m_rotation_offset.GetValue(null, progression_vars, animate_per));
			m_rotate_with_letter = effect_setup.m_rotate_relative_to_letter;
			m_animation_manager = animation_manager;
			m_active = false;

#if !UNITY_5_4_OR_NEWER
			if(m_particle_emitter != null)
			{
				m_transform = m_particle_emitter.transform;

				m_particle_emitter.emit = true;
				m_particle_emitter.enabled = false;

				// Set the layer of the effect if an override is specified
				if(animation_manager.AnimationInterface.LayerOverride >= 0)
					m_particle_emitter.gameObject.layer = animation_manager.AnimationInterface.LayerOverride;
			}
			else
#endif
				if(m_particle_system != null)
			{
				m_transform = m_particle_system.transform;

#if UNITY_5_5_OR_NEWER
				ParticleSystem.MainModule mainMod = m_particle_system.main;
				mainMod.playOnAwake = false;
#else
				m_particle_system.playOnAwake = false;
#endif
				if(m_delay <= 0)
					m_particle_system.Play();
	#if !UNITY_3_5 && UNITY_EDITOR
				p_system_timer = 0;
	#endif

				// Set the layer of the effect if an override is specified
				if(animation_manager.AnimationInterface.LayerOverride >= 0)
					m_particle_system.gameObject.layer = animation_manager.AnimationInterface.LayerOverride;
			}
		}
		
		void OrientateEffectToMesh()
		{
			// Position effect relative to letter mesh, according to offset and rotation settings
			Vector3 letter_mesh_normal = m_letter_setup_ref.Normal;

			if(!letter_mesh_normal.Equals(Vector3.zero))
			{
				rotation = m_rotate_with_letter
					? Quaternion.LookRotation(letter_mesh_normal * -1, m_letter_setup_ref.UpVector)
					: Quaternion.identity;
				
				m_transform.position = m_animation_manager.Transform.position + 
											(m_animation_manager.Transform.rotation * Vector3.Scale((rotation * m_position_offset) + m_letter_setup_ref.CenterLocal, m_animation_manager.Transform.lossyScale));
				
				rotation *= m_rotation_offset;
				
				m_transform.rotation = rotation;
			}
			else
			{
				m_transform.position = m_animation_manager.Transform.position + m_position_offset + m_letter_setup_ref.CenterLocal;
			}
		}
		
		// Updates particle effect. Returns true when effect is completely finished and ready to be reused.
		public bool Update(float delta_time)
		{
			if(!m_active)
			{
				if(m_delay > 0)
				{
					m_delay -= delta_time;
					if(m_delay < 0)
					{
						m_delay = 0;
					}
					
					return false;
				}
				
				m_active = true;
	#if !UNITY_3_5 && UNITY_EDITOR			
				m_stopped_effect_particle_count = -1;
	#endif
				
				// Position effect to current mesh position/orientation
				OrientateEffectToMesh();
				
#if !UNITY_5_4_OR_NEWER
				if(m_particle_emitter != null)
				{
					m_particle_emitter.emit = false;
					m_particle_emitter.enabled = true;

					if(m_duration > 0)
					{
						m_particle_emitter.emit = true;
					}
					else
					{
						m_particle_emitter.Emit();
					}
				}
				else
#endif
				{
					
					if(m_duration <= 0)
					{
#if UNITY_5_5_OR_NEWER
						ParticleSystem.MainModule mainMod = m_particle_system.main;
						m_duration = m_particle_system.main.duration + m_particle_system.main.startLifetime.constant;
						mainMod.loop = false;
#else
						m_duration = m_particle_system.duration + m_particle_system.startLifetime;
						m_particle_system.loop = false;
#endif
					}
					
					m_particle_system.Play(true);
				}
			}
			
			if(m_follow_mesh)
			{
				OrientateEffectToMesh();
			}
			
			m_duration -= delta_time;
			
			if(m_duration > 0)
			{
#if !UNITY_3_5 && UNITY_EDITOR
				// Handle manually calling to simulate the ParticleSystem effect in the editor.
				if(!Application.isPlaying && m_particle_system != null)
				{
					p_system_timer += delta_time;

					m_particle_system.Simulate(p_system_timer,true, true);
				}
#endif
				
				return false;
			}

#if !UNITY_5_4_OR_NEWER
			if(m_particle_emitter != null)
			{
				m_particle_emitter.emit = false;
				
				if(m_particle_emitter.particleCount > 0)
				{
					return false;
				}
			}
			else
#endif
			if(m_particle_system != null)
			{
				if(Application.isPlaying)
				{
					m_particle_system.Stop(true);
				}
#if !UNITY_3_5 && UNITY_EDITOR
				// Handle manually calling to simulate the ParticleSystem effect in the editor until all particles dead.
				else
				{
					p_system_timer += delta_time;

					m_particle_system.Simulate(p_system_timer,true, true);
					
					if(m_stopped_effect_particle_count == -1)
					{
						// Initialise particle array to current particle num
						m_particles_array = new ParticleSystem.Particle[m_particle_system.particleCount];
					}
					
					m_particle_system.GetParticles(m_particles_array);
					
					if(m_stopped_effect_particle_count != -1)
					{
						old_num_particles = m_stopped_effect_particle_count;
						
						m_stopped_effect_particle_count=0;
						for(int idx=0; idx < old_num_particles; idx++)
						{
#if UNITY_5_5_OR_NEWER
							if(m_particles_array[idx].remainingLifetime > 0.05f)
#else
							if(m_particles_array[idx].lifetime > 0.05f)
#endif
							{
								temp_array[m_stopped_effect_particle_count] = m_particles_array[idx];
								m_stopped_effect_particle_count++;
							}
						}
						
						// Remove any extra particles created by Simulate
						m_particle_system.SetParticles(temp_array, m_stopped_effect_particle_count);
					}
					else
					{
						m_stopped_effect_particle_count = m_particle_system.particleCount;
						temp_array = new ParticleSystem.Particle[m_stopped_effect_particle_count];
					}
				}
#endif
				
				if(m_particle_system.particleCount > 0)
				{
					return false;
				}
			}
			
			return true;
		}
		
		public void Pause(bool state)
		{
#if !UNITY_5_4_OR_NEWER
			// Pause/unpause particle effects 
			if(m_particle_emitter != null)
			{
				if(state && m_particle_emitter.enabled)
					m_particle_emitter.enabled = false;
				else if(!state && !m_particle_emitter.enabled)
					m_particle_emitter.enabled = true;
			}
			else
#endif
			if(m_particle_system != null)
			{
				if(state && !m_particle_system.isPaused)
					m_particle_system.Pause(true);
				else if(!state && m_particle_system.isPaused)
					m_particle_system.Play(true);
			}
		}
		
		public void Stop(bool force_stop)
		{
#if !UNITY_5_4_OR_NEWER
			if(m_particle_emitter != null)
			{
				m_particle_emitter.emit = false;
				
				if(force_stop)
				{
					m_particle_emitter.ClearParticles();
				}
			}
			else
#endif
				if(m_particle_system != null)
			{
				m_particle_system.Stop(true);
		
				if(force_stop)
					m_particle_system.Clear(true);
			}
		}
	}
}