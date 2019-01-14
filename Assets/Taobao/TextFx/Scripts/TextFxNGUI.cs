#if NGUI
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TextFx
{
	[AddComponentMenu("NGUI/UI/NGUI Label TextFx")]
	public class TextFxNGUI : UILabel, TextFxAnimationInterface
	{
		public class NGUITextDataHandler : TextFxAnimationManager.GuiTextDataHandler
		{
			List<Vector3> m_posData;
			List<Color> m_colData;
			List<Vector2> m_uvData;
			int m_numBaseLetters;
			int m_extraVertsPerLetter;
			int m_totalVertsPerLetter;
			int m_numExtraQuads = 0;
			
			public int NumVerts { get { return m_posData.Count; } }
			public int ExtraVertsPerLetter { get { return m_extraVertsPerLetter; } }
			public int NumVertsPerLetter { get { return m_totalVertsPerLetter; } }

			public NGUITextDataHandler(List<Vector3> posData, List<Color> colData, List<Vector2> uvData, int numBaseLetterMeshes)
			{
				m_posData = posData;
				m_colData = colData;
				m_uvData = uvData;
				m_numBaseLetters = numBaseLetterMeshes;

				if(m_posData.Count > m_numBaseLetters * 4)
				{
					// There are some extra mesh verts (used by effects)
					m_extraVertsPerLetter = (m_posData.Count - (m_numBaseLetters * 4)) / m_numBaseLetters;
					m_numExtraQuads = m_extraVertsPerLetter / 4;
				}
				else
					m_extraVertsPerLetter = 0;

				m_totalVertsPerLetter = m_numBaseLetters > 0 ? m_posData.Count / m_numBaseLetters : 0;
			}
			
			public Vector3[] GetLetterBaseVerts(int letterIndex)
			{
				Vector3[] verts = new Vector3[4];
				int vertOffset = m_numBaseLetters * m_extraVertsPerLetter;

				for(int idx=0; idx < 4; idx++)
				{
					// Fill in the base mesh verts 
					verts[idx] = m_posData[vertOffset + (letterIndex * 4) + idx];
				}
				
				return verts;
			}

			public Color[] GetLetterBaseCols(int letterIndex)
			{
				Color[] cols = new Color[4];
				
				int vertOffset = m_numBaseLetters * m_extraVertsPerLetter;

				for(int idx=0; idx < 4; idx++)
				{
                    // Fill in the base mesh verts 
                    cols[idx] = m_colData[vertOffset + (letterIndex * 4) + idx];
				}
				
				return cols;
			}

			public Vector2[] GetLetterBaseUVs(int letterIndex)
			{
				Vector2[] uvs = new Vector2[4];
				
				int vertOffset = m_numBaseLetters * m_extraVertsPerLetter;
				
				for(int idx=0; idx < 4; idx++)
				{
					// Fill in the base mesh verts 
					uvs[idx] = m_uvData[vertOffset + (letterIndex * 4) + idx];
				}
				
				return uvs;
			}

			public Vector3[] GetLetterExtraVerts(int letterIndex)
			{
				Vector3[] extraVerts = null;

				if (m_extraVertsPerLetter > 0)
				{
					extraVerts = new Vector3[m_extraVertsPerLetter];

					for(int quadIdx = 0; quadIdx < m_numExtraQuads; quadIdx++)
					{
						for(int vIdx=0; vIdx < 4; vIdx++)
						{
							// Fill in the extra mesh verts 
							extraVerts[quadIdx * 4 + vIdx] = m_posData[(quadIdx * m_numBaseLetters * 4) + (letterIndex * 4) + vIdx];
						}
					}
				}

				return extraVerts;
			}

			public Color[] GetLetterExtraCols(int letterIndex)
			{
				Color[] extraCols = null;
				
				if (m_extraVertsPerLetter > 0)
				{
					extraCols = new Color[m_extraVertsPerLetter];
					
					for(int quadIdx = 0; quadIdx < m_numExtraQuads; quadIdx++)
					{
						for(int vIdx=0; vIdx < 4; vIdx++)
						{
							// Fill in the extra mesh cols 
							extraCols[quadIdx * 4 + vIdx] = m_colData[(quadIdx * m_numBaseLetters * 4) + (letterIndex * 4) + vIdx];
						}
					}
				}
				
				return extraCols;
			}

			public Vector2[] GetLetterExtraUVs(int letterIndex)
			{
				Vector2[] extraUVs = null;
				
				if (m_extraVertsPerLetter > 0)
				{
					extraUVs = new Vector2[m_extraVertsPerLetter];
					
					for(int quadIdx = 0; quadIdx < m_numExtraQuads; quadIdx++)
					{
						for(int vIdx=0; vIdx < 4; vIdx++)
						{
							// Fill in the extra mesh cols 
							extraUVs[quadIdx * 4 + vIdx] = m_uvData[(quadIdx * m_numBaseLetters * 4) + (letterIndex * 4) + vIdx];
						}
					}
				}
				
				return extraUVs;
			}
		}


		// Editor TextFx conversion
#if UNITY_EDITOR
		[MenuItem ("Tools/TextFx/Convert NGUI Label to TextFx", false, 201)]
		static void ConvertToTextFX ()
		{
			GameObject activeGO = Selection.activeGameObject;

			UILabel uLabelText = activeGO.GetComponent<UILabel> ();
			Vector3 cachedPosition = uLabelText.cachedTransform.localPosition;
			TextFxNGUI textfxNGUI = activeGO.GetComponent<TextFxNGUI> ();
			
			if(textfxNGUI != null)
				return;

			textfxNGUI = activeGO.AddComponent<TextFxNGUI>();
			
			uLabelText.GetCloneOf<UILabel, TextFxNGUI> (ref textfxNGUI, new string[]{"mainTexture", "shader"});
			
			DestroyImmediate (uLabelText);

			uLabelText.cachedTransform.localPosition = cachedPosition;

			Debug.Log (activeGO.name + "'s UILabel component converted into a TextFxNGUI component");
		}

		[MenuItem ("Tools/TextFx/Convert NGUI Label to TextFx", true)]
		static bool ValidateConvertToTextFX ()
		{
			if(Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<UILabel>() != null)
				return true;
			else
				return false;
		}
#endif
		
		// Animation Interface Properties
		public string AssetNameSuffix { get { return "_NGUI"; } }
		public float MovementScale { get { return 26f; } }
		public int LayerOverride { get { return 5; } }          // Renders objects on the UI layer
		public TEXTFX_IMPLEMENTATION TextFxImplementation { get { return TEXTFX_IMPLEMENTATION.NGUI; } }
		public TextAlignment TextAlignment {
			get {
				switch (alignment) {
				case NGUIText.Alignment.Center:
					return TextAlignment.Center;
				case NGUIText.Alignment.Right:
					return TextAlignment.Right;
				default:
					return TextAlignment.Left;
				}
			}
		}
		public bool FlippedMeshVerts { get { return false; } }

		[SerializeField]
		TextFxAnimationManager m_animation_manager;
		public TextFxAnimationManager AnimationManager { get { return m_animation_manager; } }

		[SerializeField]
		GameObject m_gameobject_reference;
		public GameObject GameObject { get { if( m_gameobject_reference == null) m_gameobject_reference = gameObject; return m_gameobject_reference; } }

		public bool CurvePositioningEnabled { get { return true; } }

		[SerializeField]
		bool m_renderToCurve = false;
		public bool RenderToCurve { get { return m_renderToCurve; } set { m_renderToCurve = value; } }

		[SerializeField]
		TextFxBezierCurve m_bezierCurve;
		public TextFxBezierCurve BezierCurve { get { return m_bezierCurve; }
			set
			{
				m_bezierCurve = value;

				// Update the curve data based on current BezierCurve state
				m_animation_manager.CheckCurveData ();

				// Update mesh values to latest using new curve offset values
				ForceUpdateCachedVertData();

				// Update text mesh
				UpdateTextFxMesh();
			}
		}

#if UNITY_EDITOR
		public void DrawBezierInspector()
		{
			m_bezierCurve.OnInspector (RecordUndoObject);
		}

		public void OnSceneGUIBezier(Vector3 position_offset, Vector3 scale)
		{
			m_bezierCurve.OnSceneGUI (position_offset, scale, RecordUndoObject);
		}

		void RecordUndoObject(string label)
		{
			Undo.RecordObject(this, label);
		}
#endif

		public UnityEngine.Object ObjectInstance { get { return this; } }

		public System.Action OnMeshUpdateCall { get; set; }

		// TextFxNGUI specific variables

		[SerializeField]
		Vector2[] m_cachedUVs;
		[SerializeField]
		List<Vector3> m_currentVerts;
		[SerializeField]
		List<Color> m_currentColours;

		bool m_textFxUpdateGeometryCall = false;	// Set to highlight Geometry update calls triggered by TextFx
		bool m_markChangedCallMade = false;
		bool m_textFxAnimDrawCall = false;		// Denotes whether the subsequent OnFillVBO call has been triggered by TextFx or not


		protected override void OnEnable()
		{
			base.OnEnable ();

			if (m_animation_manager == null)
				m_animation_manager = new TextFxAnimationManager ();

			m_animation_manager.SetParentObjectReferences (gameObject, transform, this);
		}


		protected override void OnStart()
		{
			base.OnStart ();

			if(!Application.isPlaying)
			{
				return;
			}
			
			m_animation_manager.OnStart ();
		}


		protected override void OnUpdate ()
		{
			base.OnUpdate ();

			if(!Application.isPlaying || !m_animation_manager.Playing)
			{
				return;
			}
			
			m_animation_manager.UpdateAnimation ();

			TextFxMarkAsChanged ();
		}

		public void ForceUpdateCachedVertData()
		{
			m_animation_manager.PopulateDefaultMeshData(true);
		}

		// Interface Method: To redraw the mesh with the provided mesh vertex positions
		public void UpdateTextFxMesh()
		{
			TextFxMarkAsChanged ();
		}

		// Interface Method: To set the text of the text renderer
		public void SetText(string new_text)
		{
			text = new_text;

			m_markChangedCallMade = true;
			m_textFxAnimDrawCall = false;

			m_animation_manager.SetDataRebuildCallFrame ();
		}

		// Interface Method: To set the text colour of the text renderer
		public void SetColour (Color colour)
		{
			color = colour;

			m_animation_manager.SetDataRebuildCallFrame ();
		}

		// Interface Method: Returns the number of verts used for the current rendered text
		public int NumMeshVerts
		{
			get
			{
				return m_currentVerts != null ? m_currentVerts.Count : 0;
			}
		}

		// Interface Method: Access to a specified vert from the current state of the rendered text
		public Vector3 GetMeshVert(int index)
		{
			return m_currentVerts[index];
		}

		// Interface Method: Access to a specified vert colour from the current state of the rendered text
		public Color GetMeshColour(int index)
		{
			return m_currentColours[index];
		}

		// Wrapper to catch all TextFx related calls to UI.Text's UpdateGeometry.
		void TextFxMarkAsChanged()
		{
			m_textFxUpdateGeometryCall = true;
			
			MarkAsChanged ();
		}

		public override void MarkAsChanged()
		{
			if (m_markChangedCallMade)
				return;

			if(m_textFxUpdateGeometryCall)
			{
//				Debug.Log("TextFx Triggering Geometry Update");
				m_textFxAnimDrawCall = true;
			}
			else
			{
//				Debug.Log("System calling Geometry Update, GUI.changed " + GUI.changed);
				m_textFxAnimDrawCall = false;
			}
			
			m_textFxUpdateGeometryCall = false;

			m_markChangedCallMade = true;
			
			base.MarkAsChanged ();
		}

		bool forceNativeDrawCall = false;

#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			// NGUI inspector setting has been changed.
			m_textFxAnimDrawCall = false;

			forceNativeDrawCall = true;

			base.OnValidate();
		}
#endif

		int _numRenderedLetters;
		NGUITextDataHandler _textDataHandler;
		Vector2[] _uvSection;
		Color[] _colsToUse;

		public override void OnFill (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
		{
			if( ! m_textFxAnimDrawCall || forceNativeDrawCall)
			{
				forceNativeDrawCall = false;

				// Text has changed, need to update mesh verts and re-cache them

				base.OnFill (verts, uvs, cols);

				// Update current mesh vert/colour state
				m_currentVerts = verts;
				m_currentColours = cols;

				// Calculate num raw letter meshes
				_numRenderedLetters = m_currentVerts.Count / 4;

				if (effectStyle == Effect.Shadow)
					_numRenderedLetters /= 2;
				else if (effectStyle == Effect.Outline)
					_numRenderedLetters /= 5;
				else if (effectStyle == Effect.Outline8)
					_numRenderedLetters /= 9;

				_textDataHandler = new NGUITextDataHandler(verts, cols, uvs, _numRenderedLetters);

				// Call to update animation letter setups
				m_animation_manager.UpdateText (text, _textDataHandler, white_space_meshes: false);


				// Make sure m_cachedUVs is ordered correctly, mirroring the vert order returned by the TextFx animation system
				m_cachedUVs = new Vector2[uvs.Count];
				int baseUVIndexOffset = _numRenderedLetters * _textDataHandler.ExtraVertsPerLetter;

				for(int letterIdx=0; letterIdx < _numRenderedLetters; letterIdx++)
				{
					_uvSection = _textDataHandler.GetLetterExtraUVs(letterIdx);

					if(_uvSection != null)
						for(int eIdx=0; eIdx < _uvSection.Length; eIdx++)
							m_cachedUVs[letterIdx * _textDataHandler.ExtraVertsPerLetter + eIdx] = _uvSection[eIdx];

					_uvSection = _textDataHandler.GetLetterBaseUVs(letterIdx);

                    if (baseUVIndexOffset + (letterIdx * 4) >= m_cachedUVs.Length)
                        return;

                    for (int bIdx=0; bIdx < _uvSection.Length; bIdx++)
                    {
						m_cachedUVs[baseUVIndexOffset + (letterIdx * 4) + bIdx] = _uvSection[bIdx];
                    }
				}

				if(m_animation_manager.CheckCurveData())
				{
					// Update mesh values to latest using new curve offset values
					ForceUpdateCachedVertData();

					// Reset the verts data to reflect the curves offset
					verts.Clear();

					for (int idx = 0; idx < m_animation_manager.MeshVerts.Length; idx ++)
					{
						verts.Add (m_animation_manager.MeshVerts[idx]);
					}
				}

				if(Application.isPlaying
				   || m_animation_manager.Playing					// Animation is playing, so will now need to render this new mesh in the current animated state
#if UNITY_EDITOR
				   || TextFxAnimationManager.ExitingPlayMode		// To stop text being auto rendered in to default position when exiting play mode in editor.
#endif
				   )
				{
					// The verts need to be set to their current animated states
					// So recall OnFill, getting it to populate with whatever available animate mesh data for this frame
					m_textFxAnimDrawCall = true;

					verts.Clear();
					uvs.Clear();
					cols.Clear();

                    // Update animated mesh values to latest
                    m_animation_manager.UpdateMesh(true, true, delta_time: 0);

                    OnFill(verts, uvs, cols );
					return;
				}
				else
				{
					m_animation_manager.PopulateDefaultMeshData(true);
				}
			}
			else
			{
				if (_colsToUse == null || _colsToUse.Length != m_animation_manager.MeshColours.Length)
				{
					_colsToUse = new Color[m_animation_manager.MeshColours.Length];
				}
				
				for(int idx=0; idx < _colsToUse.Length; idx++)
					_colsToUse[idx] = m_animation_manager.MeshColours[idx];


				// Use cached vert data
				
				// Add each cached vert into the VBO buffer. Verts seem to need to be added one by one using Add(), can't just copy the list over
				for(int idx = 0; idx < m_cachedUVs.Length; idx++)
				{
					// Check incase there are more cachedUVs than animatedVerts; caused when text length has been extended, so this frames animated mesh data is short of what is required.
					// The verts without available data will be temporarily filled with blank data for this frame
					if(idx >= m_animation_manager.MeshVerts.Length)
						break;

					verts.Add(m_animation_manager.MeshVerts[idx]);
					cols.Add(_colsToUse[idx]);
					uvs.Add(m_cachedUVs[idx]);
				}


				if(m_animation_manager.MeshVerts.Length < m_cachedUVs.Length)
				{
					// Temporarily fill overlapping verts with blank data until animated vert data is available next frame.
					for(int idx = m_animation_manager.MeshVerts.Length; idx < m_cachedUVs.Length; idx++)
					{
						verts.Add( Vector3.zero );
						cols.Add( Color.clear );
						uvs.Add( Vector2.zero );
					}
				}

				m_currentVerts = verts;
				m_currentColours = cols;
			}

			m_markChangedCallMade = false;

			// Reset textFx anim call flag
			m_textFxAnimDrawCall = false;

			forceNativeDrawCall = false;

			if(OnMeshUpdateCall != null)
				OnMeshUpdateCall();
		}
	}
}
#endif