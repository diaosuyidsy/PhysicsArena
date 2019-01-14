using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TextFx
{
	[System.Serializable]
	public struct AnimationStateVariables
	{
		public bool m_active;
		public bool m_waiting_to_sync;
		public bool m_started_action;			// triggered when action starts (after initial delay)
		public float m_break_delay;
		public float m_timer_offset;
		public int m_action_index;
		public bool m_reverse;
		public int m_action_index_progress;		// Used to track progress through a loop cycle
		public int m_prev_action_index;
		public float m_linear_progress;
		public float m_action_progress;
		private List<ActionLoopCycle> m_active_loop_cycles;

        public List<ActionLoopCycle> ActiveLoopCycles {
            get {
                if (m_active_loop_cycles == null)
                    m_active_loop_cycles = new List<ActionLoopCycle>();
                return m_active_loop_cycles;
            }
            set {
                m_active_loop_cycles = value;
            }
        }
		
		public AnimationStateVariables Clone()
		{
			List<ActionLoopCycle> loopCycles = null;
			if (m_active_loop_cycles != null)
			{
				loopCycles = new List<ActionLoopCycle> ();

				if (m_active_loop_cycles.Count > 0)
				{
					for (int idx = 0; idx < m_active_loop_cycles.Count; idx++)
					{
						loopCycles.Add (m_active_loop_cycles [idx].Clone ());
					}
				}
			}

			return new AnimationStateVariables(){
				m_active = m_active,
				m_waiting_to_sync = m_waiting_to_sync,
				m_started_action = m_started_action,
				m_break_delay = m_break_delay,
				m_timer_offset = m_timer_offset,
				m_action_index = m_action_index,
				m_reverse = m_reverse,
				m_action_index_progress = m_action_index_progress,
				m_prev_action_index = m_prev_action_index,
				m_linear_progress = m_linear_progress,
				m_action_progress = m_action_progress,
				m_active_loop_cycles = loopCycles
			};
		}
		
		public void Reset(int starting_action_index = 0)
		{
			m_active = false;
			m_waiting_to_sync = false;
			m_started_action = false;
			m_break_delay = 0;
			m_timer_offset = 0;
			m_action_index = starting_action_index;
			m_reverse = false;
			m_action_index_progress = 0;
			m_prev_action_index = starting_action_index - 1;
			m_linear_progress = 0;
			m_action_progress = 0;
            if(m_active_loop_cycles != null)
			    m_active_loop_cycles.Clear();
		}
	}

	[System.Serializable]
	public class LetterSetup
	{
		public enum VertexPosition
		{
			TopLeft,
			TopRight,
			BottomRight,
			BottomLeft
		}

		[System.NonSerialized]
		TextFxAnimationManager m_animation_manager_ref;		// A reference to the animation manager that created this LetterSetup 

		[System.NonSerialized]
		LetterAction m_current_letter_action = null;

		[SerializeField]
		Vector3[] m_base_vertices;						// These will always be the vert positions provided by the text rendering system

		[SerializeField]
		Vector3[] m_base_extra_vertices;

		[SerializeField]
		bool m_using_curved_data = false;

		[SerializeField]
		Vector3[] m_curve_base_vertices;				// Curve positioned versions of the base verts

		[SerializeField]
		Vector3[] m_curve_base_extra_vertices;

		[SerializeField]
		VertexColour m_base_colours;

		[SerializeField]
		Color[] m_base_extra_colours;

		[SerializeField]
		int[] m_base_indexes;	// The indexes of the provided base vertices for the expected ordering TL, TR, BR, BL

		[SerializeField]
		bool m_visible_character = true;

		[SerializeField]
		bool m_stub_instance = false;

		[SerializeField]
		int m_mesh_index = -1;	// Of the list of letter meshes making up the text, this is the index of the mesh for this letter

		[SerializeField]
		float m_width;

		[SerializeField]
		float m_height;

		
		public AnimationProgressionVariables m_progression_variables;
		AnimationStateVariables m_anim_state_vars;

		float m_action_timer;
		float m_action_delay;
		bool m_ignore_action_delay = false;
		float m_action_duration;
		AnimatePerOptions m_last_animate_per;
		[SerializeField]
		Vector3[] m_current_animated_vertices;
		[SerializeField]
		Color[] m_current_animated_colours;
		// Cached action start/end states for this letter
		Vector3 m_local_scale_from, m_local_scale_to;
		Vector3 m_global_scale_from, m_global_scale_to;
		Vector3 m_local_rotation_from, m_local_rotation_to;
		Vector3 m_global_rotation_from, m_global_rotation_to;
		Vector3 m_position_from, m_position_to;
		Vector3 m_anchor_offset_from, m_anchor_offset_to;
		VertexColour m_colour_from, m_colour_to;
		VertexColour m_letter_colour;
		Vector3 m_letter_position, m_letter_scale, m_letter_global_scale, m_anchor_offset;
		Quaternion m_letter_rotation, m_letter_global_rotation;
		LETTER_ANIMATION_STATE m_current_state = LETTER_ANIMATION_STATE.PLAYING;
		bool m_flippedVerts = false;

		// Continue vars
		ContinueType m_continueType = ContinueType.None;
		System.Action<float> m_continueLetterCallback;
		int m_continueActionIndexTrigger = -1;		// the index of the action which this letter needs to reach the end of, to start continuing
		int m_continuedLoopStartIndex = -1; 		/* the index of the action which starts the loop which is currently being continued on from.
														If a letter is waiting to sync at the start of this loop, it should be forced to continue instead */
		bool m_fastTrackLoops = false;

		[SerializeField]
		Vector3 m_anchor_offset_middle_center;

		[SerializeField]
		Vector3 m_active_anchor_offset_upper_left,
				m_active_anchor_offset_upper_center,
				m_active_anchor_offset_upper_right,
				m_active_anchor_offset_middle_left,
				m_active_anchor_offset_middle_center,
				m_active_anchor_offset_middle_right,
				m_active_anchor_offset_lower_left,
				m_active_anchor_offset_lower_center,
				m_active_anchor_offset_lower_right;

		
		// Getter / Setters
		public Vector3[] BaseVertices { get { return m_using_curved_data ? m_curve_base_vertices : m_base_vertices; } }
		public Vector3 BaseVerticesTL { get { return (m_using_curved_data ? m_curve_base_vertices : m_base_vertices) [m_base_indexes [0]]; } }
		public Vector3 BaseVerticesTR { get { return (m_using_curved_data ? m_curve_base_vertices : m_base_vertices) [m_base_indexes [1]]; } }
		public Vector3 BaseVerticesBR { get { return (m_using_curved_data ? m_curve_base_vertices : m_base_vertices) [m_base_indexes [2]]; } }
		public Vector3 BaseVerticesBL { get { return (m_using_curved_data ? m_curve_base_vertices : m_base_vertices) [m_base_indexes [3]]; } }
        public Color LetterColourTL {  get { return m_letter_colour.top_left; } }
        public Color LetterColourTR { get { return m_letter_colour.top_right; } }
        public Color LetterColourBR { get { return m_letter_colour.bottom_right; } }
        public Color LetterColourBL { get { return m_letter_colour.bottom_left; } }
        int NumExtraVerts {  get { return m_current_animated_vertices.Length == 4 ? 0 : m_current_animated_vertices.Length - 4; } }
        bool AnimatedMeshDataAvailable { get { return m_current_animated_vertices != null && m_current_animated_vertices.Length > 0; } }
		public Color LetterMeshColourTL { get { return AnimatedMeshDataAvailable ? m_current_animated_colours[NumExtraVerts + m_base_indexes[0]] : Color.white; } }
		public Color LetterMeshColourTR { get { return AnimatedMeshDataAvailable ? m_current_animated_colours[NumExtraVerts + m_base_indexes[1]] : Color.white; } }
		public Color LetterMeshColourBR { get { return AnimatedMeshDataAvailable ? m_current_animated_colours[NumExtraVerts + m_base_indexes[2]] : Color.white; } }
		public Color LetterMeshColourBL { get { return AnimatedMeshDataAvailable ? m_current_animated_colours[NumExtraVerts + m_base_indexes[3]] : Color.white; } }
		public Vector3[] BaseExtraVertices { get { return m_using_curved_data ? m_curve_base_extra_vertices : m_base_extra_vertices; } }
		public Vector3 BaseVerticesCenter { get { return m_anchor_offset_middle_center; } }
		public Vector3 ActiveBaseVerticesCenter { get { return m_active_anchor_offset_middle_center; } }
		public VertexColour BaseColour { get { return m_base_colours; } }
		public Color[] BaseExtraColours { get { return m_base_extra_colours; } }
		public AnimationStateVariables AnimStateVars { get { return m_anim_state_vars; } }
		public List<ActionLoopCycle> ActiveLoopCycles { get { return m_anim_state_vars.ActiveLoopCycles; } }
		public bool WaitingToSync { get { return m_anim_state_vars.m_waiting_to_sync; } }
		public int ActionIndex { get { return m_anim_state_vars.m_action_index; } }
		public bool InReverse { get { return m_anim_state_vars.m_reverse; } }
		public int ActionProgress { get { return m_anim_state_vars.m_action_index_progress; } }
		public bool Active { get { return m_anim_state_vars.m_active; } set { m_anim_state_vars.m_active = value; } }
		public int LetterIdx { get { return m_progression_variables != null ? m_progression_variables.LetterValue : -1; } }
		public bool VisibleCharacter { get { return m_visible_character; } set { m_visible_character = value; } }
		public bool StubInstance { get { return m_stub_instance; } }
		public int MeshIndex { get { return m_mesh_index; } }
		public LETTER_ANIMATION_STATE CurrentAnimationState { get { return m_current_state; } }
		public Vector3[] CurrentAnimatedVerts { get { return m_current_animated_vertices; } }
		public Color[] CurrentAnimatedCols { get { return m_current_animated_colours; } }
		public float Width { get { return m_width; } }
		public float Height { get { return m_height; } }
		public float WidthScaled { get { return m_width / m_animation_manager_ref.MovementScale; } }
		public float HeightScaled { get { return m_height / m_animation_manager_ref.MovementScale; } }

		public Vector3 Center { get { return 
				((m_animation_manager_ref != null ? m_animation_manager_ref.Transform.rotation : Quaternion.identity) * Vector3.Scale(CenterLocal, (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.lossyScale : Vector3.one)))
					+ (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.position : Vector3.zero); 
			} }
		public Vector3 CenterLocal { get { 
				if(m_current_animated_vertices != null && m_current_animated_vertices.Length == 4)
					return (m_current_animated_vertices [0] + m_current_animated_vertices [1] + m_current_animated_vertices [2] + m_current_animated_vertices [3]) / 4f;
				else
					return m_active_anchor_offset_middle_center;
			} }
		public Vector3 TopLeft { get { return
				((m_animation_manager_ref != null ? m_animation_manager_ref.Transform.rotation : Quaternion.identity) * Vector3.Scale(TopLeftLocal, (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.lossyScale : Vector3.one)))
					+ (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.position : Vector3.zero); } }
		public Vector3 TopLeftLocal { get { return GetAnimatedVertexPosition(VertexPosition.TopLeft); } }
		public Vector3 TopRight { get { return
				((m_animation_manager_ref != null ? m_animation_manager_ref.Transform.rotation : Quaternion.identity) * Vector3.Scale(TopRightLocal, (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.lossyScale : Vector3.one)))
					+ (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.position : Vector3.zero); } }
		public Vector3 TopRightLocal { get { return GetAnimatedVertexPosition(VertexPosition.TopRight); } }
		public Vector3 BottomLeft { get { return
				((m_animation_manager_ref != null ? m_animation_manager_ref.Transform.rotation : Quaternion.identity) * Vector3.Scale(BottomLeftLocal, (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.lossyScale : Vector3.one)))
					+ (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.position : Vector3.zero); } }
		public Vector3 BottomLeftLocal { get { return GetAnimatedVertexPosition(VertexPosition.BottomLeft); } }
		public Vector3 BottomRight { get { return
				((m_animation_manager_ref != null ? m_animation_manager_ref.Transform.rotation : Quaternion.identity) * Vector3.Scale(BottomRightLocal, (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.lossyScale : Vector3.one)))
					+ (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.position : Vector3.zero); } }
		public Vector3 BottomRightLocal { get { return GetAnimatedVertexPosition(VertexPosition.BottomRight); } }

		public Quaternion Rotation { get { return (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.rotation : Quaternion.identity) * m_letter_rotation; } }
		public Quaternion RotationLocal { get { return m_letter_rotation; } }
		public Vector3 Scale { get { return Vector3.Scale(m_letter_scale, (m_animation_manager_ref != null ? m_animation_manager_ref.Transform.lossyScale : Vector3.one)); } }
		public Vector3 ScaleLocal { get { return m_letter_scale; } }

		public Vector3 Normal { get { return Vector3.Cross (m_current_animated_vertices [1] - m_current_animated_vertices [0], m_current_animated_vertices [2] - m_current_animated_vertices [0]); } }
		public Vector3 UpVector { get { return GetAnimatedVertexPosition (VertexPosition.TopLeft) - GetAnimatedVertexPosition (VertexPosition.BottomLeft); } }

        public Color BaseMeshColour(int meshVertIndex)
        {
            return m_base_colours[m_base_indexes[ meshVertIndex ] ];
        }

        Vector3 GetAnimatedVertexPosition(VertexPosition position)
		{
			switch (position)
			{
				case VertexPosition.TopLeft:
					return m_current_animated_vertices != null ? m_current_animated_vertices[m_base_indexes[0]] : m_base_vertices[m_base_indexes[0]];
				case VertexPosition.TopRight:
					return m_current_animated_vertices != null ? m_current_animated_vertices[m_base_indexes[1]] : m_base_vertices[m_base_indexes[1]];
				case VertexPosition.BottomRight:
					return m_current_animated_vertices != null ? m_current_animated_vertices[m_base_indexes[2]] : m_base_vertices[m_base_indexes[2]];
				case VertexPosition.BottomLeft:
					return m_current_animated_vertices != null ? m_current_animated_vertices[m_base_indexes[3]] : m_base_vertices[m_base_indexes[3]];
				default:
					return Vector3.zero;
			}
		}


		
		public LetterSetup(TextFxAnimationManager animation_manager_ref, LetterSetup cloneFrom = null)
		{
			if(cloneFrom == null)
			{
				m_anim_state_vars = new AnimationStateVariables();
				m_anim_state_vars.ActiveLoopCycles = new List<ActionLoopCycle>();
			}
			else
			{
				m_anim_state_vars = cloneFrom.AnimStateVars.Clone();
			}

			m_animation_manager_ref = animation_manager_ref;

			m_flippedVerts = m_animation_manager_ref.AnimationInterface.FlippedMeshVerts;
		}

		public void SetWordLineIndex(int word_idx, int line_num)
		{
			if(m_progression_variables == null)
			{
				// letter index is set when text data has been prepared 
				m_progression_variables = new AnimationProgressionVariables(-1, word_idx, line_num);
			}
			else
			{
				m_progression_variables.WordValue = word_idx;
				m_progression_variables.LineValue = line_num;
			}
		}

		public void SetAsStubInstance()
		{
			m_stub_instance = true;

			m_mesh_index = -1;
		}

		public void SetLetterData(Vector3[] mesh_verts, Color[] mesh_cols, Vector3[] extra_mesh_verts, Color[] extra_mesh_cols, int mesh_index)
		{
			m_base_vertices = mesh_verts;
            m_base_extra_vertices = extra_mesh_verts;

			m_base_colours = new VertexColour(Color.white);
			if (mesh_cols != null)
			{
				m_base_colours[0] = mesh_cols[0];
				m_base_colours[1] = mesh_cols[1];
				m_base_colours[2] = mesh_cols[2];
				m_base_colours[3] = mesh_cols[3];
			}

			m_base_extra_colours = extra_mesh_cols;

			m_stub_instance = false;

			m_using_curved_data = false;

			m_mesh_index = mesh_index;

			m_rotationOffsetQuat = Quaternion.identity;

			PreCalculateMeshInformation (m_flippedVerts);
        }

		Quaternion m_rotationOffsetQuat;
		Quaternion m_rotationOffsetQuatInverse;

		public void OffsetLetterData(Vector3 positionOffset, Vector3 rotationOffset)
		{
			Vector3 baseCenterPosition = ( m_base_vertices [0] + m_base_vertices [1] + m_base_vertices [2] + m_base_vertices [3] ) / 4f;
			m_rotationOffsetQuat = Quaternion.Euler (rotationOffset);
			m_rotationOffsetQuatInverse = Quaternion.Inverse (m_rotationOffsetQuat);
			
			m_curve_base_vertices = new Vector3[4];

			// Set curved base vert values with the provided positional and rotational offset values
			for (int idx = 0; idx < m_base_vertices.Length; idx++)
			{
				m_curve_base_vertices [idx] = m_rotationOffsetQuat * (m_base_vertices [idx] - baseCenterPosition) + positionOffset;
			}

			if (m_base_extra_vertices != null)
			{
				m_curve_base_extra_vertices = new Vector3[m_base_extra_vertices.Length];

				for (int idx = 0; idx < m_base_extra_vertices.Length; idx++)
				{
					m_curve_base_extra_vertices [idx] = m_rotationOffsetQuat * (m_base_extra_vertices [idx] - baseCenterPosition) + positionOffset;
				}
			}

			m_using_curved_data = true;

			 
			// Calculate Anchor Offset Data
			Vector3 half_width = (m_curve_base_vertices [m_base_indexes [1]] - m_curve_base_vertices [m_base_indexes [0]]) / 2f;
			Vector3 half_height = (m_curve_base_vertices [m_base_indexes [0]] - m_curve_base_vertices [m_base_indexes [3]]) / 2f;

			m_active_anchor_offset_middle_center = ( m_curve_base_vertices [0] + m_curve_base_vertices [1] + m_curve_base_vertices [2] + m_curve_base_vertices [3] ) / 4f;

			m_active_anchor_offset_upper_left = m_active_anchor_offset_middle_center - half_width + half_height;
			m_active_anchor_offset_upper_center = m_active_anchor_offset_middle_center + half_height;
			m_active_anchor_offset_upper_right = m_active_anchor_offset_middle_center + half_width + half_height;
			m_active_anchor_offset_middle_left = m_active_anchor_offset_middle_center -half_width;
			m_active_anchor_offset_middle_right = m_active_anchor_offset_middle_center + half_width;
			m_active_anchor_offset_lower_left = m_active_anchor_offset_middle_center - half_width - half_height;
			m_active_anchor_offset_lower_center = m_active_anchor_offset_middle_center - half_height;
			m_active_anchor_offset_lower_right = m_active_anchor_offset_middle_center + half_width - half_height;
		}

		// Return base verts to their original cached values
		public void ClearOffsetData()
		{
			m_using_curved_data = false;

			m_curve_base_vertices = null;
			m_curve_base_extra_vertices = null;

			m_rotationOffsetQuat = Quaternion.identity;
			m_rotationOffsetQuatInverse = Quaternion.identity;

			if(!StubInstance && m_visible_character)
				PreCalculateMeshInformation (m_flippedVerts);
		}

		void PreCalculateMeshInformation(bool flippedVerts = false)
		{
			// Determine vertex index ordering
			m_base_indexes = new int[4];

			List<float> xVals = new List<float> (), yVals = new List<float> ();
			List<int> xIdxs = new List<int>(), yIdxs = new List<int>();

			for (int idx = 0; idx < 4; idx ++)
			{
				if(idx == 0)
				{
					xVals.Add(m_base_vertices[0].x);
					xIdxs.Add(0);
					yVals.Add(m_base_vertices[0].y);
					yIdxs.Add(0);
				}
				else
				{
					if(m_base_vertices[idx].x <= xVals[0])
					{
						xVals.Insert(0, m_base_vertices[idx].x);
						xIdxs.Insert(0, idx);
					}
					else
					{
						xVals.Add (m_base_vertices[idx].x);
						xIdxs.Add (idx);
					}

					if(m_base_vertices[idx].y <= yVals[0])
					{
						yVals.Insert(0, m_base_vertices[idx].y);
						yIdxs.Insert(0, idx);
					}
					else
					{
						yVals.Add (m_base_vertices[idx].y);
						yIdxs.Add (idx);
					}
				}
			}

			m_base_indexes [0] = xIdxs [0] == yIdxs [2] || xIdxs [0] == yIdxs [3] ? xIdxs [0] : xIdxs [1];	// TL
			m_base_indexes [1] = xIdxs [2] == yIdxs [2] || xIdxs [2] == yIdxs [3] ? xIdxs [2] : xIdxs [3];	// TR
			m_base_indexes [2] = xIdxs [2] == yIdxs [0] || xIdxs [2] == yIdxs [1] ? xIdxs [2] : xIdxs [3];	// BR
			m_base_indexes [3] = xIdxs [0] == yIdxs [0] || xIdxs [0] == yIdxs [1] ? xIdxs [0] : xIdxs [1];  // BL

            // Calculate Anchor Offset Data
            float half_width = 0, half_height = 0;

			half_width = (m_base_vertices [m_base_indexes [1]].x - m_base_vertices [m_base_indexes [0]].x) / 2f;
			half_height = (m_base_vertices [m_base_indexes [0]].y - m_base_vertices [m_base_indexes [3]].y) / 2f;

			m_active_anchor_offset_middle_center = m_anchor_offset_middle_center = ( m_base_vertices [0] + m_base_vertices [1] + m_base_vertices [2] + m_base_vertices [3] ) / 4f;

			m_active_anchor_offset_upper_left = m_anchor_offset_middle_center + new Vector3 (-half_width, half_height, 0);
			m_active_anchor_offset_upper_center = m_anchor_offset_middle_center + new Vector3 (0, half_height, 0);
			m_active_anchor_offset_upper_right = m_anchor_offset_middle_center + new Vector3 (half_width, half_height, 0);
			m_active_anchor_offset_middle_left = m_anchor_offset_middle_center + new Vector3 (-half_width, 0, 0);
			m_active_anchor_offset_middle_right = m_anchor_offset_middle_center + new Vector3 (half_width, 0, 0);
			m_active_anchor_offset_lower_left = m_anchor_offset_middle_center + new Vector3 (-half_width, -half_height, 0);
			m_active_anchor_offset_lower_center = m_anchor_offset_middle_center + new Vector3 (0, -half_height, 0);
			m_active_anchor_offset_lower_right = m_anchor_offset_middle_center + new Vector3 (half_width, -half_height, 0);

			m_width = half_width * 2;
			m_height = half_height * 2;

			// Setup base colours array with correctly mapped colour values for this mesh
			m_base_colours = new VertexColour(m_base_colours[m_base_indexes[0]], m_base_colours[m_base_indexes[1]], m_base_colours[m_base_indexes[2]], m_base_colours[m_base_indexes[3]]);

			// Do the same for any extra colour values
			if (m_base_extra_colours != null)
			{
				Color[] orderExtraColourVerts = new Color[m_base_extra_colours.Length];

				for (int midx = 0; midx < m_base_extra_colours.Length / 4; midx ++)
				{
					for (int vidx = 0; vidx < 4; vidx++)
						orderExtraColourVerts[(midx * 4) + vidx] = m_base_extra_colours[(midx * 4) + m_base_indexes[vidx]];
				}

				m_base_extra_colours = orderExtraColourVerts;
			}
		}

		Vector3 GetAnchorOffset(TextfxTextAnchor letter_anchor )
		{
			switch (letter_anchor)
			{
				case TextfxTextAnchor.UpperLeft:
					return m_active_anchor_offset_upper_left;
				case TextfxTextAnchor.UpperCenter:
					return m_active_anchor_offset_upper_center;
				case TextfxTextAnchor.UpperRight:
					return m_active_anchor_offset_upper_right;
				case TextfxTextAnchor.MiddleLeft:
					return m_active_anchor_offset_middle_left;
				case TextfxTextAnchor.MiddleCenter:
					return m_active_anchor_offset_middle_center;
				case TextfxTextAnchor.MiddleRight:
					return m_active_anchor_offset_middle_right;
				case TextfxTextAnchor.LowerLeft:
					return m_active_anchor_offset_lower_left;
				case TextfxTextAnchor.LowerCenter:
					return m_active_anchor_offset_lower_center;
				case TextfxTextAnchor.LowerRight:
					return m_active_anchor_offset_lower_right;
				default:
					return m_active_anchor_offset_middle_center;
			}
		}

		public void Reset(LetterAnimation animation, int starting_action_index = 0)
		{
			m_anim_state_vars.Reset(starting_action_index);

			m_current_letter_action = null;

			m_continueType = ContinueType.None;

			m_fastTrackLoops = false;

			m_current_state = LETTER_ANIMATION_STATE.PLAYING;

			if(animation.NumLoops > 0)
			{
				UpdateLoopList(animation);
			}
		}
		
		public void SetMeshState(TextFxAnimationManager animation_manager_ref, int action_idx, float action_progress, LetterAnimation animation, AnimatePerOptions animate_per, ref Vector3[] mesh_verts, ref Color[] mesh_colours)
		{
			if (m_stub_instance)
				return;

			m_animation_manager_ref = animation_manager_ref;

			if(action_idx >= 0 && action_idx < animation.NumActions && animation.GetAction(action_idx).m_action_type == ACTION_TYPE.ANIM_SEQUENCE)
			{
				// Get start/end state values
				CacheLetterActionStartEndStates(animation.GetAction(action_idx),
				                                animation,
				                                animate_per);

				// Set mesh
				SetupMesh(animation.GetAction(action_idx),
				          Mathf.Clamp(action_progress, 0,1),
				          Mathf.Clamp(action_progress, 0,1),
				          ref mesh_verts,
				          ref mesh_colours);
			}
			else
			{
                // action not found for this letter. Just use base positional vertex values
                int offsetIndex = 0;
				if (m_base_extra_vertices != null)
                {
					BaseExtraVertices.CopyTo(mesh_verts, 0);
					offsetIndex = m_base_extra_vertices.Length;

                }
				BaseVertices.CopyTo(mesh_verts, offsetIndex);

                if(m_base_extra_colours != null)
                    m_base_extra_colours.CopyTo(mesh_colours, 0);

                (new Color[] { m_base_colours[m_base_indexes[0]], m_base_colours[m_base_indexes[1]], m_base_colours[m_base_indexes[2]], m_base_colours[m_base_indexes[3]] }).CopyTo(mesh_colours, offsetIndex);
            }
		}
		
		void SetNextActionIndex(LetterAnimation animation)
		{
			// based on current active loop list, return the next action index
			
			// increment action progress count
			m_anim_state_vars.m_action_index_progress++;
			
			ActionLoopCycle current_loop;
			for(int loop_idx=0; loop_idx < m_anim_state_vars.ActiveLoopCycles.Count; loop_idx++)
			{
				current_loop = m_anim_state_vars.ActiveLoopCycles[loop_idx];
				
				if((current_loop.m_loop_type == LOOP_TYPE.LOOP && m_anim_state_vars.m_action_index == current_loop.m_end_action_idx) ||
					(current_loop.m_loop_type == LOOP_TYPE.LOOP_REVERSE && ((m_anim_state_vars.m_reverse && m_anim_state_vars.m_action_index == current_loop.m_start_action_idx) || (!m_anim_state_vars.m_reverse && m_anim_state_vars.m_action_index == current_loop.m_end_action_idx)))
				)
				{
					// Reached end of loop cycle. Deduct one cycle from loop count.
					bool end_of_loop_cycle = current_loop.m_loop_type == LOOP_TYPE.LOOP || m_anim_state_vars.m_reverse;
					
					if(end_of_loop_cycle)
					{
						current_loop.m_number_of_loops--;
					}
					
					// Switch reverse status
					if(current_loop.m_loop_type == LOOP_TYPE.LOOP_REVERSE)
					{
						m_anim_state_vars.m_reverse = !m_anim_state_vars.m_reverse;
					}
					
					current_loop.FirstPass = false;
					
					if(end_of_loop_cycle && current_loop.m_number_of_loops == 0)
					{
						// loop cycle finished
						// Remove this loop from active loop list
						m_anim_state_vars.ActiveLoopCycles.RemoveAt(loop_idx);
						loop_idx--;
						
						if(current_loop.m_loop_type == LOOP_TYPE.LOOP_REVERSE)
						{
							if(!current_loop.m_finish_at_end)
							{
								// Don't allow anim to progress back through actions, skip to action beyond end of reverse loop
								m_anim_state_vars.m_action_index = current_loop.m_end_action_idx;
							}
							else
							{
								m_anim_state_vars.m_action_index = current_loop.m_start_action_idx - 1;
							}
						}
					}
					else
					{
						if(current_loop.m_number_of_loops < 0)
						{
							current_loop.m_number_of_loops = -1;
						}
						
						// return to the start of this loop again
						if(current_loop.m_loop_type == LOOP_TYPE.LOOP)
						{
							m_anim_state_vars.m_action_index = current_loop.m_start_action_idx;
						}
						
						return;
					}
				}
				else
				{
					break;
				}
			}

//			Debug.Log("SetNextActionIndex() Num Active Loops : "+ m_anim_state_vars.ActiveLoopCycles.Count);

			// Progress the action index
			m_anim_state_vars.m_action_index += (m_anim_state_vars.m_reverse ? -1 : 1);

			// check for animation reaching end
			if(m_anim_state_vars.m_action_index >= animation.NumActions)
			{
				m_anim_state_vars.m_active = false;

				m_anim_state_vars.m_action_index = animation.NumActions -1;
			}
			
			return;
		}
		
		// Only called if action_idx has changed since last time
		void UpdateLoopList(LetterAnimation animation)
		{
			// add any new loops from the next action index to the loop list
			ActionLoopCycle loop;
			for(int idx=0; idx < animation.NumLoops; idx++)
			{
				loop = animation.GetLoop(idx);
				
				if(loop.m_start_action_idx == m_anim_state_vars.m_action_index)
				{
					// add this new loop into the ordered active loop list
					int new_loop_cycle_span = loop.SpanWidth;
					
					int loop_idx = 0;
					foreach(ActionLoopCycle active_loop in m_anim_state_vars.ActiveLoopCycles)
					{
						if(loop.m_start_action_idx == active_loop.m_start_action_idx && loop.m_end_action_idx == active_loop.m_end_action_idx)
						{
							// This loop is already in the active loop list, don't re-add
							loop_idx = -1;
							break;
						}
						
						if(new_loop_cycle_span < active_loop.SpanWidth)
						{
							break;
						}
							
						loop_idx++;
					}
					
					if(loop_idx >= 0)
					{
						loop = loop.Clone(idx);

						if(m_fastTrackLoops)
						{
							// set any new loops to just run through once
							loop.m_number_of_loops = 1;
						}

						m_anim_state_vars.ActiveLoopCycles.Insert(loop_idx, loop);
					}
				}
			}
		}

		// Called to continue this letter past a BREAK action
		public void ContinueAction(float animation_timer, LetterAnimation animation, AnimatePerOptions animate_per)
		{
			if(m_anim_state_vars.m_waiting_to_sync)
			{
				m_anim_state_vars.m_break_delay = 0;
				m_anim_state_vars.m_waiting_to_sync= false;
				
				// reset timer offset to compensate for the sync-up wait time
				m_anim_state_vars.m_timer_offset = animation_timer;
				
				// Progress letter animation index to next, and break out of the loop
				int prev_action_idx = m_anim_state_vars.m_action_index;
				
				// Set next action index
				SetNextActionIndex(animation);
				
				if(m_anim_state_vars.m_active)
				{
					if(!m_anim_state_vars.m_reverse && m_anim_state_vars.m_action_index_progress > m_anim_state_vars.m_action_index)
					{
						// Repeating the action again; check for unqiue random variable requests.
						animation.GetAction(m_anim_state_vars.m_action_index).SoftReset(animation.GetAction(prev_action_idx), m_progression_variables, animate_per);
					}
					
					if(prev_action_idx != m_anim_state_vars.m_action_index)
					{
						UpdateLoopList(animation);
					}
				}
			}		
		}
		

		public void ContinueFromCurrentToAction(LetterAnimation animation,
		                                        int action_state_to_continue_to,
		                                        bool use_start_state,
		                                        int action_index_to_continue_with,
		                                        AnimatePerOptions animate_per,
		                                        float anim_timer,
		                                        float continueDuration,
		                                        int action_index_progress,
		                                        int deepestLoopDepth,
		                                        ContinueType continueType,
		                                        bool trimInterimLoops,
		                                        int[] lowestActiveLoopIterations)
		{
			m_continueLetterCallback = (float timer) => {
				LetterAction targetAction = animation.GetAction(action_state_to_continue_to);

				m_anim_state_vars.m_timer_offset = timer;
				m_current_letter_action = null;
				m_anim_state_vars.m_waiting_to_sync = false;
				m_anim_state_vars.m_reverse = false;
				m_action_delay = 0;
				m_action_duration = continueType == ContinueType.Instant ? continueDuration : 0;
				m_anim_state_vars.m_action_index = action_index_to_continue_with;
				m_anim_state_vars.m_prev_action_index = m_anim_state_vars.m_action_index;
				m_anim_state_vars.m_action_index_progress = action_index_progress;

				CacheCurrentStateToAction (targetAction, animation, use_start_state, animate_per);

				// Trim any extra loopCycles in active list
				if(deepestLoopDepth == 0)
					ActiveLoopCycles.Clear();
				else
					ActiveLoopCycles.RemoveRange(0, ActiveLoopCycles.Count - (deepestLoopDepth - 1));
			};

			m_continueType = continueType;

			if(m_continueType == ContinueType.EndOfLoop && deepestLoopDepth > 0 && ActiveLoopCycles.Count >= deepestLoopDepth)
			{
				ActionLoopCycle loopBeingContinuedOutOf = ActiveLoopCycles[ActiveLoopCycles.Count - deepestLoopDepth];
				m_continuedLoopStartIndex = loopBeingContinuedOutOf.m_start_action_idx; 
				m_continueActionIndexTrigger = loopBeingContinuedOutOf.m_end_action_idx;

				if(deepestLoopDepth == ActiveLoopCycles.Count)
				{
					// Set to last iteration of current loop, with respect to the leading letters progress
					ActiveLoopCycles[0].m_number_of_loops = 1 + (ActiveLoopCycles[0].m_number_of_loops - lowestActiveLoopIterations[ActiveLoopCycles[0].m_active_loop_index]);
				}


				if(trimInterimLoops)
				{
					// Set any active BREAK action states, to just one frame duration
					m_fastTrackLoops = true;

					// Set any remaining active loops to their last iteration
					if(deepestLoopDepth < ActiveLoopCycles.Count)
					{
//						Debug.Log("[" + m_mesh_index + "] Set any remaining active loops to their last iteration");

						ActionLoopCycle loop_cycle;
						for(int loop_idx = 0; loop_idx < ActiveLoopCycles.Count - deepestLoopDepth; loop_idx++)
						{
							loop_cycle = ActiveLoopCycles[loop_idx];

							// Set to last iteration, with respect to the leading letters progress
							loop_cycle.m_number_of_loops = 1 + (loop_cycle.m_number_of_loops - lowestActiveLoopIterations[loop_cycle.m_active_loop_index]);
//							Debug.LogError("Set loop[" + loop_cycle.m_active_loop_index + "] count  : "+ loop_cycle.m_number_of_loops);
						}
					}
				}
			}
			else if(continueType == ContinueType.Instant)
			{
				m_continueLetterCallback(anim_timer);
				m_continueLetterCallback = null;
				
				m_current_state = LETTER_ANIMATION_STATE.CONTINUING;
			}
			else
			{
				// This shouldn't happen...
			}
		}
		
		void SetCurrentLetterAction(LetterAnimation animation, int action_index , AnimatePerOptions animate_per)
		{	
			LetterAction prev_action = m_current_letter_action;
			m_current_letter_action = animation.GetAction (action_index);

			if(m_current_letter_action.m_action_type == ACTION_TYPE.ANIM_SEQUENCE)
			{
				m_action_delay = Mathf.Max(m_current_letter_action.m_delay_progression.GetValue(m_progression_variables, m_last_animate_per, m_current_letter_action.m_delay_with_white_space_influence), 0);
			}

			m_action_duration = Mathf.Max(m_current_letter_action.m_duration_progression.GetValue(m_progression_variables, m_last_animate_per), 0);
			
			// Check if action is in a loopreverse_onetime delay case. If so, set delay to 0.
			if(	m_anim_state_vars.ActiveLoopCycles != null &&
				m_anim_state_vars.ActiveLoopCycles.Count > 0 &&
				m_anim_state_vars.ActiveLoopCycles[0].m_delay_first_only &&
				!m_anim_state_vars.ActiveLoopCycles[0].FirstPass &&
				m_current_letter_action.m_delay_progression.Progression != (int) ValueProgression.Constant)
			{
				m_ignore_action_delay = true;
			}
			else
				m_ignore_action_delay = false;



			// Calculate any unique random values
			if(!m_anim_state_vars.m_reverse) // && m_anim_state_vars.m_action_index_progress > m_anim_state_vars.m_action_index)
			{
//				Debug.Log ("SetCurrentLetterAction() SoftReset action " + m_anim_state_vars.m_action_index);
				m_current_letter_action.SoftReset(prev_action, m_progression_variables, animate_per, m_anim_state_vars.m_action_index == 0);
			}
			else if(m_anim_state_vars.m_reverse)
			{
//				Debug.Log ("SetCurrentLetterAction() SoftResetStarts");
				m_current_letter_action.SoftResetStarts(prev_action, m_progression_variables, animate_per);
			}

			// Cache this actions start/end state values
			CacheLetterActionStartEndStates (m_current_letter_action,
			                                 animation,
			                                 animate_per);
		}

		public void SetPlayingState()
		{
			m_current_state = LETTER_ANIMATION_STATE.PLAYING;
		}

		void CallContinueCallback(float timer)
		{
//			Debug.Log ("CallContinueCallback " + m_mesh_index);
			m_continueLetterCallback(timer);
			m_continueLetterCallback = null;

			m_continueType = ContinueType.None;

			m_fastTrackLoops = false;
			
			m_current_state = LETTER_ANIMATION_STATE.CONTINUING;
		}

		int prev_action_idx;
		float prev_delay;
		float current_action_delay;
		bool altered_delay;
		float old_action_delay;

		// Animates the letter mesh and return the current action index in use
		public bool AnimateMesh(	TextFxAnimationManager animation_manager_ref,
                                  	bool force_render,
									float timer,
									int lowest_action_progress,
									LetterAnimation animation,
									AnimatePerOptions animate_per,
									float delta_time,
                                  	ref Vector3[] mesh_verts,
                                  	ref Color[] mesh_colours)
		{
			if(m_animation_manager_ref == null)
				m_animation_manager_ref = animation_manager_ref;

			// Do nothing if in finished continue state
			if (m_current_state == LETTER_ANIMATION_STATE.CONTINUING_FINISHED)
			{
				m_anim_state_vars.m_timer_offset = timer;
				return false;
			}

			m_last_animate_per = animate_per;

			if(animation.NumActions > 0 && m_anim_state_vars.m_action_index < animation.NumActions && ( m_anim_state_vars.m_active || force_render ))
			{
				if(m_anim_state_vars.m_action_index != m_anim_state_vars.m_prev_action_index ||
				   (m_current_state != LETTER_ANIMATION_STATE.CONTINUING && m_current_letter_action == null))
				{
					SetCurrentLetterAction(animation, m_anim_state_vars.m_action_index, animate_per);

					if(m_anim_state_vars.m_action_index != m_anim_state_vars.m_prev_action_index)
						m_anim_state_vars.m_started_action = false;
				}


				m_anim_state_vars.m_prev_action_index = m_anim_state_vars.m_action_index;
				
				if(force_render)
				{
					SetupMesh(m_current_letter_action,
					          m_anim_state_vars.m_action_progress,
					          m_anim_state_vars.m_linear_progress,
					          ref mesh_verts,
					          ref mesh_colours);
				}
				
				if(m_anim_state_vars.m_waiting_to_sync)
				{
					if(m_continueType == ContinueType.EndOfLoop && m_anim_state_vars.m_action_index == m_continuedLoopStartIndex && m_current_letter_action.m_action_type != ACTION_TYPE.BREAK)
					{
						// Currently waiting at the start of the loop that is being Continued On from
						// Force continue callback call.
						CallContinueCallback(timer);

						return false;
					}

					if(m_current_letter_action.m_action_type == ACTION_TYPE.BREAK)
					{
						if(!force_render && m_anim_state_vars.m_break_delay > 0)
						{
							m_anim_state_vars.m_break_delay -= delta_time;
							
							if(m_anim_state_vars.m_break_delay <= 0 || m_fastTrackLoops)
							{
								// Check for continue case
								if(m_continueType == ContinueType.EndOfLoop
								   && m_anim_state_vars.m_action_index == m_continueActionIndexTrigger)
								{
									CallContinueCallback(timer);
									
									return false;
								}


								ContinueAction(timer, animation, animate_per);

								m_current_state = LETTER_ANIMATION_STATE.PLAYING;

								return false;
							}
						}

						m_current_state = m_anim_state_vars.m_break_delay == 0 ? LETTER_ANIMATION_STATE.WAITING_INFINITE : LETTER_ANIMATION_STATE.WAITING;
						return false;
					}
					else if(lowest_action_progress < m_anim_state_vars.m_action_index_progress)
					{
						m_current_state = LETTER_ANIMATION_STATE.PLAYING;

						return false;
					}
					else if(!force_render)
					{
						m_anim_state_vars.m_waiting_to_sync = false;

						// reset timer offset to compensate for the sync-up wait time
						m_anim_state_vars.m_timer_offset = timer;
					}
				}
				else if(!force_render && m_current_letter_action != null
				        && (m_current_letter_action.m_action_type == ACTION_TYPE.BREAK
				    		|| (!m_anim_state_vars.m_reverse
				    			&& !m_ignore_action_delay
				    			&& m_current_letter_action.m_force_same_start_time
				    			&& lowest_action_progress < m_anim_state_vars.m_action_index_progress
				    			)
							)
						)
				{
					// Force letter to wait for rest of letters to be in sync

					m_anim_state_vars.m_waiting_to_sync = true;

					m_anim_state_vars.m_break_delay = Mathf.Max(m_current_letter_action.m_duration_progression.GetValue(m_progression_variables, animate_per), 0);
					
					m_current_state = LETTER_ANIMATION_STATE.PLAYING;

					return false;
				}


				
				if(force_render)
				{
					m_current_state = m_anim_state_vars.m_active ? LETTER_ANIMATION_STATE.PLAYING : LETTER_ANIMATION_STATE.STOPPED;

					return false;
				}


				current_action_delay = m_ignore_action_delay || m_anim_state_vars.m_reverse ? 0 : m_action_delay;
				altered_delay = false;
				
				if(m_animation_manager_ref.WhatJustChanged == ANIMATION_DATA_TYPE.ALL
				   || m_animation_manager_ref.WhatJustChanged == ANIMATION_DATA_TYPE.DELAY)
				{
					old_action_delay = m_action_delay;

					m_action_delay = Mathf.Max (m_current_letter_action.m_delay_progression.GetValue (m_progression_variables, m_last_animate_per, m_current_letter_action.m_delay_with_white_space_influence), 0);

					if(old_action_delay != m_action_delay)
					{
						// This letters delay has changed
						m_anim_state_vars.m_timer_offset += m_action_delay - old_action_delay;

						altered_delay = true;
					}
				}

				if(!altered_delay && 
				   (m_animation_manager_ref.WhatJustChanged == ANIMATION_DATA_TYPE.ALL
				   || m_animation_manager_ref.WhatJustChanged == ANIMATION_DATA_TYPE.DURATION))
				{
					// Update cached duration value
					m_action_duration = Mathf.Max(m_current_letter_action.m_duration_progression.GetValue(m_progression_variables, m_last_animate_per), 0);

					// Duration progression values have just changed. Need to alter timer_offset to preserve the previous linear progress (to stop jumping)
					m_anim_state_vars.m_timer_offset = -1 * (((m_anim_state_vars.m_reverse ? 1 - m_anim_state_vars.m_linear_progress : m_anim_state_vars.m_linear_progress) * m_action_duration) - timer + (m_anim_state_vars.m_reverse ? 0 : current_action_delay));

					// Take away the delta time so that it has actually progressed this frame
					m_anim_state_vars.m_timer_offset -= delta_time;
				}


				m_anim_state_vars.m_action_progress = 0;
				m_anim_state_vars.m_linear_progress = 0;


				m_action_timer = timer - m_anim_state_vars.m_timer_offset;
				
				if((m_anim_state_vars.m_reverse || m_action_timer > current_action_delay))
				{
					m_anim_state_vars.m_linear_progress = (m_action_timer - (m_anim_state_vars.m_reverse ? 0 : current_action_delay)) / m_action_duration;

					if(m_anim_state_vars.m_reverse)
					{
						if(m_action_timer >= m_action_duration)
						{
							m_anim_state_vars.m_linear_progress = 0;
						}
						else
						{
							m_anim_state_vars.m_linear_progress = 1 - m_anim_state_vars.m_linear_progress;
						}
					}

					if(m_visible_character && !m_anim_state_vars.m_started_action && m_current_letter_action != null)
					{
						// Trigger any action onStart audio or particle effects

						if(m_current_letter_action.AudioEffectSetups != null && m_current_letter_action.AudioEffectSetups.Count > 0)
							TriggerEffects<AudioEffectSetup>(
											m_current_letter_action.AudioEffectSetups,
							               	animate_per,
							               	PLAY_ITEM_EVENTS.ON_START,
											PlayAudioEffect
							);


						if(m_current_letter_action.ParticleEffectSetups != null && m_current_letter_action.ParticleEffectSetups.Count > 0)
							TriggerEffects<ParticleEffectSetup>(
											m_current_letter_action.ParticleEffectSetups,
							               	animate_per,
							               	PLAY_ITEM_EVENTS.ON_START,
											PlayParticleEffect
							);
						
						m_anim_state_vars.m_started_action = true;
					}


					m_anim_state_vars.m_action_progress = m_current_letter_action != null
																? EasingManager.GetEaseProgress(m_current_letter_action.m_ease_type, m_anim_state_vars.m_linear_progress)
																: EasingManager.GetEaseProgress(EasingEquation.CubicEaseOut, m_anim_state_vars.m_linear_progress); //m_anim_state_vars.m_linear_progress;


					// Maintain the CONTINUING state until its reached its end.
					if(m_current_state != LETTER_ANIMATION_STATE.CONTINUING)
						m_current_state = LETTER_ANIMATION_STATE.PLAYING;


					
					if((!m_anim_state_vars.m_reverse && m_anim_state_vars.m_linear_progress >= 1) || (m_anim_state_vars.m_reverse && m_action_timer >= m_action_duration + current_action_delay))
					{
						m_anim_state_vars.m_action_progress = m_anim_state_vars.m_reverse ? 0 : 1;
						m_anim_state_vars.m_linear_progress = m_anim_state_vars.m_reverse ? 0 : 1;
						
						if(animation.CurrentAnimationState != LETTER_ANIMATION_STATE.CONTINUING && m_visible_character && !m_anim_state_vars.m_reverse && m_anim_state_vars.m_action_index != -1 && m_current_letter_action != null)
						{
							if(m_current_letter_action.AudioEffectSetups != null && m_current_letter_action.AudioEffectSetups.Count > 0)
								TriggerEffects<AudioEffectSetup>(
												m_current_letter_action.AudioEffectSetups,
								               	animate_per,
								               	PLAY_ITEM_EVENTS.ON_FINISH,
												PlayAudioEffect );

							if(m_current_letter_action.ParticleEffectSetups != null && m_current_letter_action.ParticleEffectSetups.Count > 0)
								TriggerEffects<ParticleEffectSetup>(
												m_current_letter_action.ParticleEffectSetups,
								               	animate_per,
								               	PLAY_ITEM_EVENTS.ON_FINISH,
												PlayParticleEffect);
						}
						


						// Flag continue state finished if in a continue state...
						if(m_current_state == LETTER_ANIMATION_STATE.CONTINUING)
							m_current_state = LETTER_ANIMATION_STATE.CONTINUING_FINISHED;


//						if(m_continueType == ContinueType.EndOfLoop)
//							Debug.Log("[" + m_mesh_index + "] current action index : "+  m_anim_state_vars.m_action_index + ", triggerIndex : "+ m_continueActionIndexTrigger + ", [" + ActiveLoopCycles[0].m_active_loop_index + "] loop_num_left : "+ ActiveLoopCycles[0].m_number_of_loops);

						if(m_continueType == ContinueType.EndOfLoop
						   && m_anim_state_vars.m_action_index == m_continueActionIndexTrigger
						   && (ActiveLoopCycles.Count == 0 || ((ActiveLoopCycles[0].m_loop_type == TextFx.LOOP_TYPE.LOOP || m_anim_state_vars.m_reverse) && ActiveLoopCycles[0].m_number_of_loops <= 1) ))
						{
							// Draw this end state
							SetupMesh(m_current_letter_action,
							          m_anim_state_vars.m_action_progress,
							          m_anim_state_vars.m_linear_progress,
							          ref mesh_verts,
							          ref mesh_colours);
							
							
							CallContinueCallback(timer);
							
							return true;
						}


						prev_action_idx = m_anim_state_vars.m_action_index;
						prev_delay = current_action_delay;

						// Set next action index
						SetNextActionIndex(animation);

						
						if(m_anim_state_vars.m_active)
						{
							if(!m_anim_state_vars.m_reverse)
							{
								m_anim_state_vars.m_started_action = false;
							}
							
							// Add to the timer offset
							m_anim_state_vars.m_timer_offset += prev_delay + m_action_duration;
							
							if(prev_action_idx != m_anim_state_vars.m_action_index)
								UpdateLoopList(animation);
							else
								SetCurrentLetterAction(animation, m_anim_state_vars.m_action_index, animate_per);
						}
						else
						{
							m_current_state = LETTER_ANIMATION_STATE.STOPPED;
						}
					}
				}
				
				SetupMesh(m_current_letter_action,
				          m_anim_state_vars.m_action_progress,
				          m_anim_state_vars.m_linear_progress,
				          ref mesh_verts,
				          ref mesh_colours);

			}
			else
			{
				// no actions found for this letter. Position and colour the letter in its last animated state, or else its default state
				if (m_current_animated_vertices != null)
					m_current_animated_vertices.CopyTo (mesh_verts, 0);
				else
					BaseVertices.CopyTo(mesh_verts,0);

				if (m_current_animated_colours != null)
					m_current_animated_colours.CopyTo (mesh_colours, 0);
				else
					mesh_colours = new Color[]{ Color.white,Color.white,Color.white,Color.white };

				m_anim_state_vars.m_active = false;

				m_current_state = LETTER_ANIMATION_STATE.STOPPED;
			}

			return true;
		}

		void PlayAudioEffect(AudioEffectSetup effect_setup, AnimatePerOptions animate_per)
		{
			m_animation_manager_ref.PlayAudioClip(effect_setup, m_progression_variables, animate_per);
		}

		void PlayParticleEffect(ParticleEffectSetup effect_setup, AnimatePerOptions animate_per)
		{
			m_animation_manager_ref.PlayParticleEffect(
				this,
				effect_setup,
				m_progression_variables,
				animate_per
			);
		}
		

		void TriggerEffects<T>(List<T> effectSetups, AnimatePerOptions animate_per, PLAY_ITEM_EVENTS play_when, System.Action<T, AnimatePerOptions> play_effect_callback) where T : EffectItemSetup
		{
			if(effectSetups != null)
			{
				foreach(T effect_setup in effectSetups)
				{
					if(effect_setup.m_play_when == play_when
					   && (effect_setup.m_effect_assignment == PLAY_ITEM_ASSIGNMENT.PER_LETTER || effect_setup.m_effect_assignment_custom_letters.Contains(m_progression_variables.LetterValue)))
					{
						if(	!effect_setup.m_loop_play_once ||
							m_anim_state_vars.ActiveLoopCycles == null ||
							m_anim_state_vars.ActiveLoopCycles.Count == 0 ||
							m_anim_state_vars.ActiveLoopCycles[0].FirstPass)
						{
							play_effect_callback(effect_setup, animate_per);
						}
					}
				}
			}
		}

		void CacheCurrentStateToAction(LetterAction end_action, LetterAnimation letterAnimation, bool use_start_state, AnimatePerOptions animate_per)
		{
			if (m_stub_instance)
			{
				return;
			}

			List<LetterAction> all_letter_actions = letterAnimation.LetterActions;

			m_local_scale_from = m_letter_scale;
			m_local_scale_to = use_start_state ? end_action.m_start_scale.GetValue(all_letter_actions, m_progression_variables, animate_per, true) : end_action.m_end_scale.GetValue(all_letter_actions, m_progression_variables, animate_per, true);
			
			// Calculate Global Scale Vector
			m_global_scale_from = m_letter_global_scale;
			m_global_scale_to = use_start_state ? end_action.m_global_start_scale.GetValue(all_letter_actions, m_progression_variables, animate_per, true) : end_action.m_global_end_scale.GetValue(all_letter_actions, m_progression_variables, animate_per, true);
			
			// Calculate Local Rotation
			m_local_rotation_from = m_letter_rotation.eulerAngles;
			m_local_rotation_to = use_start_state ? end_action.m_start_euler_rotation.GetValue(all_letter_actions, m_progression_variables, animate_per, true) : end_action.m_end_euler_rotation.GetValue(all_letter_actions, m_progression_variables, animate_per, true);
			
			// Calculate Global Rotation
			m_global_rotation_from = m_letter_global_rotation.eulerAngles;
			m_global_rotation_to = use_start_state ? end_action.m_global_start_euler_rotation.GetValue(all_letter_actions, m_progression_variables, animate_per, true) : end_action.m_global_end_euler_rotation.GetValue(all_letter_actions, m_progression_variables, animate_per, true);
			
			// Calculate Position
			m_position_from = m_letter_position;
			m_position_to = use_start_state ? end_action.m_start_pos.GetValue(all_letter_actions, m_progression_variables, animate_per, true) : end_action.m_end_pos.GetValue(all_letter_actions, m_progression_variables, animate_per, true);

			// Sort out letters colour
			if (m_colour_to == null)
				m_colour_to = new VertexColour ();

			m_colour_from = m_letter_colour;

			if (use_start_state)
				end_action.m_start_colour.GetValue (ref m_colour_to, all_letter_actions, m_progression_variables, animate_per, letterAnimation.m_defaultTextColourProgression);
			else
				end_action.m_end_colour.GetValue(ref m_colour_to, all_letter_actions, m_progression_variables, animate_per, letterAnimation.m_defaultTextColourProgression);

			// Letter anchor offset
			m_anchor_offset_from = m_anchor_offset;
			m_anchor_offset_to = GetAnchorOffset ((TextfxTextAnchor) (use_start_state ? end_action.m_letter_anchor_start : end_action.m_letter_anchor_end) );
		}

		void CacheLetterActionStartEndStates(LetterAction current_action, LetterAnimation letterAnimation, AnimatePerOptions animate_per)
		{
			if (m_stub_instance || (current_action.m_action_type == ACTION_TYPE.BREAK && m_colour_from != null))
			{
				return;
			}

			// Calculate Local Scale Vector
			m_local_scale_from = current_action.m_start_scale.GetValue(letterAnimation.LetterActions, m_progression_variables, animate_per, true);
			m_local_scale_to = current_action.m_end_scale.GetValue(letterAnimation.LetterActions, m_progression_variables, animate_per, true);

			// Calculate Global Scale Vector
			m_global_scale_from = current_action.m_global_start_scale.GetValue(letterAnimation.LetterActions, m_progression_variables, animate_per, true);
			m_global_scale_to = current_action.m_global_end_scale.GetValue(letterAnimation.LetterActions, m_progression_variables, animate_per, true);

			// Calculate Local Rotation
			m_local_rotation_from = current_action.m_start_euler_rotation.GetValue(letterAnimation.LetterActions, m_progression_variables, animate_per, true);
			m_local_rotation_to = current_action.m_end_euler_rotation.GetValue(letterAnimation.LetterActions, m_progression_variables, animate_per, true);

			// Calculate Global Rotation
			m_global_rotation_from = current_action.m_global_start_euler_rotation.GetValue(letterAnimation.LetterActions, m_progression_variables, animate_per, true);
			m_global_rotation_to = current_action.m_global_end_euler_rotation.GetValue(letterAnimation.LetterActions, m_progression_variables, animate_per, true);

			// Calculate Position
			m_position_from = current_action.m_start_pos.GetValue(letterAnimation.LetterActions, m_progression_variables, animate_per, true);
			m_position_to = current_action.m_end_pos.GetValue(letterAnimation.LetterActions, m_progression_variables, animate_per, true);

			// Sort out letters colour
			if (m_colour_from == null)
				m_colour_from = new VertexColour ();
			if (m_colour_to == null)
				m_colour_to = new VertexColour ();
			current_action.m_start_colour.GetValue(ref m_colour_from, letterAnimation.LetterActions, m_progression_variables, animate_per, letterAnimation.m_defaultTextColourProgression);
			current_action.m_end_colour.GetValue(ref m_colour_to, letterAnimation.LetterActions, m_progression_variables, animate_per, letterAnimation.m_defaultTextColourProgression);

			// Letter anchor offset
			m_anchor_offset_from = GetAnchorOffset ((TextfxTextAnchor)current_action.m_letter_anchor_start);
			m_anchor_offset_to = current_action.m_letter_anchor_2_way ? GetAnchorOffset ((TextfxTextAnchor)current_action.m_letter_anchor_end) : m_anchor_offset_from;
        }
	

		void SetupMesh(LetterAction letter_action, float action_progress, float linear_progress, ref Vector3[] mesh_verts, ref Color[] mesh_colours)
		{
			if (m_stub_instance)
			{
				return;
			}

			bool usingLocalScale = true;
			bool usingLocalRotation = true;

			// Calculate Local Scale Vector
			if(letter_action != null && letter_action.m_scale_axis_ease_data.m_override_default)
			{
				m_letter_scale = new Vector3(	FloatLerp(m_local_scale_from.x, m_local_scale_to.x, EasingManager.GetEaseProgress(letter_action.m_scale_axis_ease_data.m_x_ease, linear_progress)),
					                            FloatLerp(m_local_scale_from.y, m_local_scale_to.y, EasingManager.GetEaseProgress(letter_action.m_scale_axis_ease_data.m_y_ease, linear_progress)),
					                            FloatLerp(m_local_scale_from.z, m_local_scale_to.z, EasingManager.GetEaseProgress(letter_action.m_scale_axis_ease_data.m_z_ease, linear_progress)));
			}
			else
			{
				m_letter_scale = Vector3Lerp(
												m_local_scale_from,
												m_local_scale_to,
												action_progress);
			}

			if (m_letter_scale == Vector3.one)
				usingLocalScale = false;


			// Calculate Global Scale Vector
			if(letter_action != null && letter_action.m_global_scale_axis_ease_data.m_override_default)
			{
				m_letter_global_scale = new Vector3(	FloatLerp(m_global_scale_from.x, m_global_scale_to.x, EasingManager.GetEaseProgress(letter_action.m_global_scale_axis_ease_data.m_x_ease, linear_progress)),
					                                    FloatLerp(m_global_scale_from.y, m_global_scale_to.y, EasingManager.GetEaseProgress(letter_action.m_global_scale_axis_ease_data.m_y_ease, linear_progress)),
					                                    FloatLerp(m_global_scale_from.z, m_global_scale_to.z, EasingManager.GetEaseProgress(letter_action.m_global_scale_axis_ease_data.m_z_ease, linear_progress)));
			}
			else
			{
				m_letter_global_scale = Vector3Lerp(
														m_global_scale_from,
														m_global_scale_to,
														action_progress);
			}


			
			// Calculate Local Rotation
			if(letter_action != null && letter_action.m_rotation_axis_ease_data.m_override_default)
			{
				m_letter_rotation =	Quaternion.Euler
									(
										FloatLerp(m_local_rotation_from.x, m_local_rotation_to.x, EasingManager.GetEaseProgress(letter_action.m_rotation_axis_ease_data.m_x_ease, linear_progress)),
										FloatLerp(m_local_rotation_from.y, m_local_rotation_to.y, EasingManager.GetEaseProgress(letter_action.m_rotation_axis_ease_data.m_y_ease, linear_progress)),
										FloatLerp(m_local_rotation_from.z, m_local_rotation_to.z, EasingManager.GetEaseProgress(letter_action.m_rotation_axis_ease_data.m_z_ease, linear_progress))
									);
			}
			else
			{
				m_letter_rotation = Quaternion.Euler(
											Vector3Lerp(
												m_local_rotation_from,
												m_local_rotation_to,
												action_progress)
											);
			}

			if (m_letter_rotation == Quaternion.identity)
				usingLocalRotation = false;


			// Calculate Global Rotation
			if(letter_action != null && letter_action.m_global_rotation_axis_ease_data.m_override_default)
			{
				m_letter_global_rotation =	Quaternion.Euler(
												FloatLerp(m_global_rotation_from.x, m_global_rotation_to.x, EasingManager.GetEaseProgress(letter_action.m_global_rotation_axis_ease_data.m_x_ease, linear_progress)),
												FloatLerp(m_global_rotation_from.y, m_global_rotation_to.y, EasingManager.GetEaseProgress(letter_action.m_global_rotation_axis_ease_data.m_y_ease, linear_progress)),
												FloatLerp(m_global_rotation_from.z, m_global_rotation_to.z, EasingManager.GetEaseProgress(letter_action.m_global_rotation_axis_ease_data.m_z_ease, linear_progress))
											);
			}
			else
			{
				m_letter_global_rotation = Quaternion.Euler(
												Vector3Lerp(
													m_global_rotation_from,
													m_global_rotation_to,
													action_progress)
											);
			}


			

			// Calculate Position
			if(letter_action != null && letter_action.m_position_axis_ease_data.m_override_default)
			{
				m_letter_position = new Vector3(	FloatLerp(m_position_from.x, m_position_to.x, EasingManager.GetEaseProgress(letter_action.m_position_axis_ease_data.m_x_ease, linear_progress)),
					                                FloatLerp(m_position_from.y, m_position_to.y, EasingManager.GetEaseProgress(letter_action.m_position_axis_ease_data.m_y_ease, linear_progress)),
					                                FloatLerp(m_position_from.z, m_position_to.z, EasingManager.GetEaseProgress(letter_action.m_position_axis_ease_data.m_z_ease, linear_progress)));
			}
			else
			{
				m_letter_position = Vector3Lerp(
					m_position_from, 
					m_position_to,
					action_progress);
			}


			bool renderingToCurve = m_animation_manager_ref.AnimationInterface != null ? m_animation_manager_ref.AnimationInterface.RenderToCurve : false;

			// Apply any rotation offset to this position
			if (renderingToCurve)
				m_letter_position = m_rotationOffsetQuat * m_letter_position;

			if (m_letter_colour == null)
				m_letter_colour = new VertexColour ();

			// Calculate anchor_offset
			m_anchor_offset = Vector3.Lerp( m_anchor_offset_from, m_anchor_offset_to, action_progress );

			int baseVertIdx = -1;
			int extraVertIdx = -1;


			// Compile it all together to get the final vertex positions
			for(int idx=0; idx < mesh_verts.Length; idx++)
			{
				if(mesh_verts.Length - idx > 4)
				{
					baseVertIdx = -1;
					extraVertIdx = idx;
				}
				else
				{
					baseVertIdx = 4 - (mesh_verts.Length - idx);
					extraVertIdx = -1;
				}

				mesh_verts[idx] = extraVertIdx >= 0 ? BaseExtraVertices[extraVertIdx] : BaseVertices[baseVertIdx];

				// Perform local transform operations if applicable
				if (usingLocalRotation || usingLocalScale)
				{
					// normalise vert position to the anchor point before scaling and rotating.
					mesh_verts[idx] -= m_anchor_offset;
					
					// Un-apply the curve rotation offset
					if (renderingToCurve)
					{
						mesh_verts [idx] = m_rotationOffsetQuatInverse * mesh_verts [idx];
					}
					
					// Locally Scale verts
					if (usingLocalScale)
						mesh_verts[idx] = Vector3.Scale(mesh_verts[idx], m_letter_scale);
					
					// Locally Rotate vert
					if (usingLocalRotation)
						mesh_verts[idx] = m_letter_rotation	* mesh_verts[idx];
					
					// Re-apply the curve rotation offset
					if (renderingToCurve)
					{
						mesh_verts [idx] = m_rotationOffsetQuat * mesh_verts [idx];
					}
					
					// Re-apply the letters anchor point offset
					mesh_verts[idx] += m_anchor_offset;
				}


				// Globally Scale verts
				mesh_verts[idx] = Vector3.Scale(mesh_verts[idx], m_letter_global_scale);

				// Globally Rotate vert
				mesh_verts[idx] = m_letter_global_rotation * mesh_verts[idx];
				
				// translate vert
				// Normalise translation vectors with MovementScale factor
				mesh_verts[idx] += m_letter_position * m_animation_manager_ref.MovementScale;


				// Handle mesh colours
				if(baseVertIdx >= 0)
				{
					if (m_flippedVerts)
					{
						baseVertIdx += 2;

						if (baseVertIdx > 3)
							baseVertIdx -= 4;
					}

					switch(m_base_indexes[ baseVertIdx ])
					{
						case 0:
							mesh_colours [idx] = Color.Lerp(m_colour_from.top_left, m_colour_to.top_left, action_progress);
							m_letter_colour.top_left = mesh_colours [idx];
							break;
						case 1:
							mesh_colours [idx] = Color.Lerp (m_colour_from.top_right, m_colour_to.top_right, action_progress);
							m_letter_colour.top_right = mesh_colours [idx];
							break;
						case 2:
							mesh_colours [idx] = Color.Lerp(m_colour_from.bottom_right, m_colour_to.bottom_right, action_progress);
							m_letter_colour.bottom_right = mesh_colours [idx];
							break;
						case 3:
							mesh_colours [idx] = Color.Lerp(m_colour_from.bottom_left, m_colour_to.bottom_left, action_progress);
							m_letter_colour.bottom_left = mesh_colours [idx];
							break;
					}
				}
			}

			// Set any extra vert colours, applying the alpha value of the base vert nearest to it
			if(mesh_verts.Length > 4)
			{
				// There are extra verts that need colouring
				Color vertColour;

				for(int idx=0; idx < mesh_verts.Length - 4; idx++)
				{
					vertColour = m_base_extra_colours[idx];
					switch(m_base_indexes[idx %4])
					{
					    case 0: vertColour.a *= m_letter_colour.top_left.a; break;
					    case 1: vertColour.a *= m_letter_colour.top_right.a; break;
					    case 2: vertColour.a *= m_letter_colour.bottom_right.a; break;
					    case 3: vertColour.a *= m_letter_colour.bottom_left.a; break;
					}

					mesh_colours[idx] = vertColour;
				}
			}

			if(m_current_animated_vertices == null || m_current_animated_vertices.Length != mesh_verts.Length)
				m_current_animated_vertices = new Vector3[mesh_verts.Length];

			mesh_verts.CopyTo (m_current_animated_vertices, 0);


			if(m_current_animated_colours == null || m_current_animated_colours.Length != mesh_verts.Length)
				m_current_animated_colours = new Color[mesh_verts.Length];
			
			mesh_colours.CopyTo (m_current_animated_colours, 0);
		}


		// Lerp function that handles progress value going over 1
		static Vector3 Vector3Lerp(Vector3 from_vec, Vector3 to_vec, float progress)
		{
			if(progress <= 1 && progress >= 0)
			{
				return Vector3.Lerp(from_vec, to_vec, progress);
			}
			else
			{
				return from_vec + Vector3.Scale((to_vec - from_vec), Vector3.one * progress);
			}
		}
		
		static float FloatLerp(float from_val, float to_val, float progress)
		{
			if(progress <= 1 && progress >= 0)
			{
				return Mathf.Lerp(from_val, to_val, progress);
			}
			else
			{
				return from_val + ((to_val - from_val) * progress);
			}
		}
	}
}