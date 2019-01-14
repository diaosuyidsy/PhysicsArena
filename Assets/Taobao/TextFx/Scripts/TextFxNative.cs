using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
#if !UNITY_WINRT
using System.Xml;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using Boomlagoon.TextFx.JSON;


namespace TextFx
{

	[RequireComponent (typeof (MeshFilter))]
	[RequireComponent (typeof (MeshRenderer))]
	[ExecuteInEditMode]
	[AddComponentMenu("TextFx/TextFxNative")]
	public class TextFxNative : MonoBehaviour, TextFxAnimationInterface
	{
		// Interface class for accessing the required text vertex data in the required format
		public class TextFxTextDataHandler : TextFxAnimationManager.GuiTextDataHandler
		{
			Vector3[] m_posData;
			Color[] m_colData;
			
			public int NumVerts { get { return m_posData.Length; } }
			public int NumVertsPerLetter { get { return 4; } }
			public int ExtraVertsPerLetter { get { return 0; } }
			
			public TextFxTextDataHandler(Vector3[] posData, Color[] colData)
			{
				m_posData = posData;
				m_colData = colData;
			}
			
			public Vector3[] GetLetterBaseVerts(int letterIndex)
			{
				return new Vector3[]{m_posData[letterIndex * 4], m_posData[letterIndex * 4 + 1], m_posData[letterIndex * 4 + 2], m_posData[letterIndex * 4 + 3]};
			}
			
			public Color[] GetLetterBaseCols(int letterIndex)
			{
				return new Color[]{m_colData[letterIndex * 4], m_colData[letterIndex * 4 + 1], m_colData[letterIndex * 4 + 2], m_colData[letterIndex * 4 + 3]};
			}

			public Vector3[] GetLetterExtraVerts(int letterIndex)
			{
				return null;
			}

			public Color[] GetLetterExtraCols(int letterIndex)
			{
				return null;
			}
		}
		

		const float FONT_SCALE_FACTOR = 10f;
		const float BASE_LINE_HEIGHT = 1.05f;

		
		public string m_text = "";
		public Font m_font;
		int m_font_texture_width = 0;
		int m_font_texture_height = 0;
		public TextAsset m_font_data_file;
		public Material m_font_material;
		public Vector2 m_px_offset = new Vector2(0,0);
		public float m_character_size = 1;
		public bool m_use_colour_gradient = false;
		public Color m_textColour = Color.white;
		public VertexColour m_textColourGradient = new VertexColour (Color.white);
		public TextDisplayAxis m_display_axis = TextDisplayAxis.HORIZONTAL;
		public TextAnchor m_text_anchor = TextAnchor.MiddleCenter;
		public TextAlignment m_text_alignment = TextAlignment.Left;
		public float m_line_height_factor = 1;
		public float m_max_width = 0;
		public bool m_override_font_baseline = false;
		public float m_font_baseline_override;

		public string AssetNameSuffix { get { return ""; } }
		public float MovementScale { get { return 1f; } }
		public int LayerOverride { get { return -1; } }
		public TEXTFX_IMPLEMENTATION TextFxImplementation { get { return TEXTFX_IMPLEMENTATION.NATIVE; } }
		public TextAlignment TextAlignment { get { return m_text_alignment; } }
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

		[SerializeField]
		Vector3[] m_mesh_verts;
		[SerializeField]
		Vector2[] m_mesh_uvs;
		[SerializeField]
		Color[] m_mesh_cols;
		[SerializeField]
		Vector3[] m_mesh_normals;
		[SerializeField]
		int[] m_mesh_triangles;

		[SerializeField]
		float m_font_baseline;
		[SerializeField]
		CustomFontCharacterData m_custom_font_data;
		[SerializeField]
		string m_current_font_data_file_name = "";
		[SerializeField]
		string m_current_font_name = "";
#if !UNITY_4_6 && !UNITY_4_5 && !UNITY_4_3 && !UNITY_4_2 && !UNITY_4_1 && !UNITY_4_0_1 && !UNITY_4_0
		[SerializeField]
		System.Action<Font> m_fontRebuildCallback = null;
#endif
		
		CombineInstance[] m_mesh_combine_instance;
		Transform m_transform_reference;
		Renderer m_renderer = null;
		MeshFilter m_mesh_filter = null;
		Mesh m_mesh;
		float m_total_text_width = 0, m_total_text_height = 0;
		float m_line_height = 0;

		
		// Getter/Setters
		float LineHeightFactor { get { return m_line_height_factor * BASE_LINE_HEIGHT; } }
		float FontScale { get { return FONT_SCALE_FACTOR / m_character_size; } }
		public float FontBaseLine { get { return m_override_font_baseline ? m_font_baseline_override : m_font_baseline / FontScale; } }
		public bool IsFontBaseLineSet { get { return m_override_font_baseline || m_font_baseline != 0; } }
		public Vector3 Position { get { return m_transform != null ? m_transform.position : transform.position; } }
		public Vector3 Scale { get { return m_transform != null ? m_transform.localScale : transform.localScale; } }
		public Quaternion Rotation { get { return m_transform != null ? m_transform.rotation : transform.rotation; } }
		public string Text { get { return m_text; } set { SetText(value); } }

		public float LineHeight { get { return m_line_height; } }
		
		
		public Transform m_transform
		{
			get
			{
				return m_transform_reference;
			}
		}
		public bool IsFontDataAssigned
		{
			get
			{
				if(m_font != null)
				{
					return true;
				}
				
				if(m_font_data_file != null && m_font_material != null)
				{
					return true;
				}
				
				return false;
			}
		}
		
		public void ClearFontCharacterData()
		{
			if(m_custom_font_data != null)
			{
				m_custom_font_data.m_character_infos.Clear();
				m_custom_font_data = null;
			}
		}
		
		void OnEnable()
	    {	
			if (m_animation_manager == null)
				m_animation_manager = new TextFxAnimationManager ();

			m_animation_manager.SetParentObjectReferences (gameObject, transform, this);

//			if(m_mesh != null)
//				return;
			
			// Set component variable references
	        m_mesh_filter = gameObject.GetComponent<MeshFilter>();
			m_transform_reference = transform;
	        
	        if (m_mesh_filter.sharedMesh != null)
	        {
				// Check for two effects sharing the same SharedMesh instance (occurs when a MeshFilter component is duplicated)
				TextFxNative[] objects = GameObject.FindObjectsOfType(typeof(TextFxNative)) as TextFxNative[];

				foreach (TextFxNative effect_manager in objects)
	            {
	                MeshFilter otherMeshFilter = effect_manager.m_mesh_filter;
	                if (otherMeshFilter != null)
	                {
	                    if (otherMeshFilter.sharedMesh == m_mesh_filter.sharedMesh && otherMeshFilter != m_mesh_filter)
	                    {
							// Found shared SharedMesh instance; initialising a new one
	                        m_mesh_filter.mesh = new Mesh();
							
							m_mesh = m_mesh_filter.sharedMesh;
							
							// Reset Text with new meshes
							SetText(m_text);
	                    }
	                }
	            }
				
				m_mesh = m_mesh_filter.sharedMesh;
	        }
	        else
	        {
				m_mesh = new Mesh();
	            m_mesh_filter.mesh = m_mesh;

				if(IsFontDataAssigned)
				{
					// Reset Text with new meshes
					SetText(m_text);
				}
	        }

			if(m_font != null)
			{
#if !UNITY_4_6 && !UNITY_4_5 && !UNITY_4_3 && !UNITY_4_2 && !UNITY_4_1 && !UNITY_4_0_1 && !UNITY_4_0
				m_fontRebuildCallback = (Font rebuiltFont) => {
					FontImportDetected(rebuiltFont);
				};
				
				Font.textureRebuilt += m_fontRebuildCallback;
#else
				m_font.textureRebuildCallback += FontTextureRebuilt;
#endif
				
				// Make sure dynamic fonts add all the letters required for this animation
				m_font.RequestCharactersInTexture(m_text);

#if UNITY_EDITOR
				if(!Application.isPlaying)
				{
					// Force Texture Rebuild to avoid dynamic font texture size changes since playing. 
					SetText(m_text);
				}
#endif
			}
	    }

#if !UNITY_4_6 && !UNITY_4_5 && !UNITY_4_3 && !UNITY_4_2 && !UNITY_4_1 && !UNITY_4_0_1 && !UNITY_4_0
		// Called by TextfxFontChangeListener when a Font OnPostprocessAllAssets() call is triggered.
		// Checks if imported font is same as one being used.
		void FontImportDetected(Font changedFont)
		{
			if(m_font == null)
			{
				return;
			}
			
			if(changedFont.Equals(m_font))
			{
				m_current_font_name = "";
				
				SetText(m_text);
			}
		}
#else
		// Called by TextfxFontChangeListener when a Font OnPostprocessAllAssets() call is triggered.
		// Checks if imported font is same as one being used.
		public void FontImportDetected(string font_name)
		{
			if(m_font == null)
			{
				return;
			}
			
			if(font_name.Equals(m_font.name.ToLower()))
			{
				m_current_font_name = "";
				
				SetText(m_text);
				
				m_font.textureRebuildCallback += FontTextureRebuilt;
			}
		}
		
		void FontTextureRebuilt()
		{
			SetText (m_text);
		}
#endif

		void OnDisable()
		{
			if(m_font != null)
			{
#if !UNITY_4_6 && !UNITY_4_5 && !UNITY_4_3 && !UNITY_4_2 && !UNITY_4_1 && !UNITY_4_0_1 && !UNITY_4_0
				Font.textureRebuilt -= FontImportDetected;
#else
				m_font.textureRebuildCallback -= FontTextureRebuilt;
#endif
			}
		}


		void Start()
		{
			if(!Application.isPlaying)
			{
				return;
			}

			m_animation_manager.OnStart();
		}

		void Update()
		{
			if(!Application.isPlaying || !m_animation_manager.Playing)
			{
				return;
			}

			m_animation_manager.UpdateAnimation ();

			UpdateTextFxMesh ();
		}

		public void ForceUpdateCachedVertData()
		{
			m_animation_manager.PopulateDefaultMeshData(true);

			Vector3[] vertsToUse = m_animation_manager.MeshVerts;

			for (int idx = 0; idx < vertsToUse.Length; idx++) {
				m_mesh_verts [idx] = vertsToUse [idx];
			}
		}

		// Interface Method: To redraw the mesh with the provided mesh vertex positions
		public void UpdateTextFxMesh()
		{
			UpdateMesh (true);
		}

        // Interface Method: Returns the number of verts used for the current rendered text
        public int NumMeshVerts
        {
            get
            {
                return m_mesh.vertexCount;
            }
        }

        // Interface Method: Access to a specified vert from the current state of the rendered text
        public Vector3 GetMeshVert(int index)
        {
            return m_mesh.vertices[index];
        }

        // Interface Method: Access to a specified vert colour from the current state of the rendered text
        public Color GetMeshColour(int index)
        {
            return m_mesh.colors[index];
        }

		// Interface Method: To set the text of the text renderer
		public void SetText(string new_text)
		{
			SetTextMesh (new_text);
		}

		// Interface Method: To set the text colour of the text renderer
		public void SetColour (Color colour)
		{
			m_textColour = colour;

			// Recall to set the text mesh
			SetText(m_text);
		}

        string GetHumanReadableCharacterString(char character)
		{
			if(character.Equals('\n'))
				return "[NEW LINE]";
			else if(character.Equals(' '))
				return "[SPACE]";
			else if(character.Equals('\r'))
				return "[CARRIAGE RETURN]";
			else if(character.Equals('\t'))
				return "[TAB]";
			else
				return "" + character;
		}
		
		bool GetCharacterInfo(char m_character, ref CustomCharacterInfo char_info)
		{
			if(m_character.Equals('\n') || m_character.Equals('\r'))
			{
				return true;
			}
			
			if(m_font != null)
			{
				if(!m_current_font_name.Equals(m_font.name))
				{
					// Recalculate font's baseline value
					// Checks through all available alpha characters and uses the most common bottom y_axis value as the baseline for the font.

#pragma warning disable 
					Dictionary<float, int> baseline_values = new Dictionary<float, int>();
					float baseline;
					foreach(CharacterInfo character in m_font.characterInfo)
					{
						// only check alpha characters (a-z, A-Z)
						if((character.index >= 97 && character.index < 123) || (character.index >= 65 && character.index < 91))
						{
							baseline = -character.vert.y - character.vert.height;
							if(baseline_values.ContainsKey(baseline))
								baseline_values[baseline] ++;
							else
								baseline_values[baseline] = 1;
						}
					}
#pragma warning restore
					
					// Find most common baseline value used by the letters
					int idx=0;
					int highest_num=0, highest_idx=-1;
					float most_common_baseline = -1;
					foreach(int num in baseline_values.Values)
					{
						if(highest_idx == -1 || num > highest_num)
						{
							highest_idx = idx;
							highest_num = num;
						}
						idx++;
					}
					
					// Retrieve the most common value and use as baseline value
					idx=0;
					foreach(float baseline_key in baseline_values.Keys)
					{
						if(idx == highest_idx)
						{
							most_common_baseline = baseline_key;
							break;
						}
						idx++;
					}
					
					m_font_baseline = most_common_baseline;
					
					// Set font name to current, to ensure this check doesn't happen each time
					m_current_font_name = m_font.name;
				}
				
				CharacterInfo font_char_info = new CharacterInfo();
				m_font.GetCharacterInfo(m_character, out font_char_info);

#pragma warning disable				
				char_info.flipped = font_char_info.flipped;
				char_info.uv = font_char_info.uv;
				char_info.vert = font_char_info.vert;
				char_info.width = font_char_info.width;
				
				// Scale char_info values
				char_info.vert.x /= FontScale;
				char_info.vert.y /= FontScale;
				char_info.vert.width /= FontScale;
				char_info.vert.height /= FontScale;
				char_info.width /= FontScale;
				
				if(font_char_info.width == 0)
				{
					// Invisible character info returned because character is not contained within the font
					Debug.LogWarning("Character '" + GetHumanReadableCharacterString(m_character) + "' not found. Check that font '" + m_font.name + "' supports this character.");
				}

#pragma warning restore				

				return true;
			}
			
			if(m_font_data_file != null)
			{
				if(m_custom_font_data == null || !m_font_data_file.name.Equals(m_current_font_data_file_name))
				{
					// Setup m_custom_font_data for the custom font.
#if !UNITY_WINRT
					if(m_font_data_file.text.Substring(0,5).Equals("<?xml"))
					{
						// Text file is in xml format
						
						m_current_font_data_file_name = m_font_data_file.name;
						m_custom_font_data = new CustomFontCharacterData();
						
						XmlTextReader reader = new XmlTextReader(new StringReader(m_font_data_file.text));
						
						int texture_width = 0;
						int texture_height = 0;
						int uv_x, uv_y;
						float width, height, xoffset, yoffset, xadvance;
						CustomCharacterInfo character_info;
						
						while(reader.Read())
						{
							if(reader.IsStartElement())
							{
								if(reader.Name.Equals("common"))
								{
									texture_width = int.Parse(reader.GetAttribute("scaleW"));
									texture_height = int.Parse(reader.GetAttribute("scaleH"));
									
									m_font_baseline = int.Parse(reader.GetAttribute("base"));
								}
								else if(reader.Name.Equals("char"))
								{
									uv_x = int.Parse(reader.GetAttribute("x"));
									uv_y = int.Parse(reader.GetAttribute("y"));
									width = float.Parse(reader.GetAttribute("width"));
									height = float.Parse(reader.GetAttribute("height"));
									xoffset = float.Parse(reader.GetAttribute("xoffset"));
									yoffset = float.Parse(reader.GetAttribute("yoffset"));
									xadvance = float.Parse(reader.GetAttribute("xadvance"));
									
									character_info = new CustomCharacterInfo();
									character_info.flipped = false;
									character_info.uv = new Rect((float) uv_x / (float) texture_width, 1 - ((float)uv_y / (float)texture_height) - (float)height/(float)texture_height, (float)width/(float)texture_width, (float)height/(float)texture_height);
									character_info.vert = new Rect(xoffset,-yoffset,width, -height);
									character_info.width = xadvance;
									
									m_custom_font_data.m_character_infos.Add( int.Parse(reader.GetAttribute("id")), character_info);
								}
							}
						}
					}
					else
#endif
					if(m_font_data_file.text.Substring(0,4).Equals("info"))
					{
						// Plain txt format
						m_current_font_data_file_name = m_font_data_file.name;
						m_custom_font_data = new CustomFontCharacterData();
						
						int texture_width = 0;
						int texture_height = 0;
						int uv_x, uv_y;
						float width, height, xoffset, yoffset, xadvance;
						CustomCharacterInfo character_info;
						string[] data_fields;
						
						string[] text_lines = m_font_data_file.text.Split(new char[]{'\n'});
						
						foreach(string font_data in text_lines)
						{
							if(font_data.Length >= 5 && font_data.Substring(0,5).Equals("char "))
							{
								// character data line
								data_fields = ParseFieldData(font_data, new string[]{"id=", "x=", "y=", "width=", "height=", "xoffset=", "yoffset=", "xadvance="});
								uv_x = int.Parse(data_fields[1]);
								uv_y = int.Parse(data_fields[2]);
								width = float.Parse(data_fields[3]);
								height = float.Parse(data_fields[4]);
								xoffset = float.Parse(data_fields[5]);
								yoffset = float.Parse(data_fields[6]);
								xadvance = float.Parse(data_fields[7]);
								
								character_info = new CustomCharacterInfo();
								character_info.flipped = false;
								character_info.uv = new Rect((float) uv_x / (float) texture_width, 1 - ((float)uv_y / (float)texture_height) - (float)height/(float)texture_height, (float)width/(float)texture_width, (float)height/(float)texture_height);
								character_info.vert = new Rect(xoffset,-yoffset +1,width, -height);
								character_info.width = xadvance;
								
								m_custom_font_data.m_character_infos.Add( int.Parse(data_fields[0]), character_info);
							}
							else if(font_data.Length >= 6 && font_data.Substring(0,6).Equals("common"))
							{
								data_fields = ParseFieldData(font_data, new string[]{"scaleW=", "scaleH=", "base="});
								texture_width = int.Parse(data_fields[0]);
								texture_height = int.Parse(data_fields[1]);
								
								m_font_baseline = int.Parse(data_fields[2]);
							}
						}
					}
					
				}
				
				if(m_custom_font_data.m_character_infos.ContainsKey((int) m_character))
				{
					((CustomCharacterInfo) m_custom_font_data.m_character_infos[(int)m_character]).ScaleClone(FontScale, ref char_info);
					
					return true;
				}
			}
			
			return false;
		}

		string[] ParseFieldData(string data_string, string[] fields)
		{
			string[] data_values = new string[fields.Length];
			int count = 0, data_start_idx, data_end_idx;
			
			foreach(string field_name in fields)
			{
				data_start_idx = data_string.IndexOf(field_name) + field_name.Length;
				data_end_idx = data_string.IndexOf(" ", data_start_idx);
				
				data_values[count] = data_string.Substring(data_start_idx, data_end_idx - data_start_idx);
				
				count++;
			}
			
			return data_values;
		}


		void SetTextMesh(string new_text)
		{
			if (this == null || this.Equals (null))
			{
				// NULL instance; lost reference to Textfx component, likely because it's been deleted in the scene, but the editor panel still thinks its active
				// This check stops an editor console error message
				return;
			}

			if(m_renderer == null)
			{
				m_renderer = this.GetComponent<Renderer>();
			}
			
			bool setup_correctly = false;
			
			// Automatically assign the font material to the renderer if its not already set
			if((m_renderer.sharedMaterial == null || m_renderer.sharedMaterial != m_font_material) && m_font_material != null)
			{
				m_renderer.sharedMaterial = m_font_material;
			}
			else if(m_font != null)
			{	
				if(m_renderer.sharedMaterial == null || m_renderer.sharedMaterial != m_font_material)
				{
					m_font_material = m_font.material;
					m_renderer.sharedMaterial = m_font_material;
				}
				
				if(m_renderer.sharedMaterial != null)
				{
					setup_correctly = true;
				}
			}
			
			if(!setup_correctly && (m_renderer.sharedMaterial == null || m_font_data_file == null))
			{
				// Incorrectly setup font information
				m_font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
				m_font_material = m_font.material;
				m_renderer.sharedMaterial = m_font_material;
				m_font_data_file = null;
			}
			
			m_text = new_text;
			
			// Remove all carriage return char's from new_text
			new_text = new_text.Replace("\r", "");
			
			string raw_chars = m_text.Replace(" ", "");
			raw_chars = raw_chars.Replace("\n", "");
			raw_chars = raw_chars.Replace("\r", "");
			raw_chars = raw_chars.Replace("\t", "");
			
			int text_length = new_text.Length;
			int num_letters = raw_chars.Length;
			
			
			m_mesh_verts = new Vector3[num_letters * 4];
			m_mesh_uvs = new Vector2[num_letters * 4];
			m_mesh_cols = new Color[num_letters * 4];
			m_mesh_normals = new Vector3[num_letters * 4];
			m_mesh_triangles = new int[num_letters * 2 * 3];

			int[] letter_line_numbers = new int[num_letters];
			List<float> line_widths = new List<float> ();
			List<CustomCharacterInfo> cached_last_word_char_infos = new List<CustomCharacterInfo> ();
		
			CustomCharacterInfo char_info = new CustomCharacterInfo();
			CustomCharacterInfo last_char_info = null;
			
			if(m_font != null)
			{
				// Make sure font contains all characters required
				m_font.RequestCharactersInTexture(m_text);
				
				if(m_font_material.mainTexture.width != m_font_texture_width || m_font_material.mainTexture.height != m_font_texture_height)
				{
					// Font texture size has changed
					m_font_texture_width = m_font_material.mainTexture.width;
					m_font_texture_height = m_font_material.mainTexture.height;
					SetText(m_text);
					return;
				}
			}

			// Calculate bounds of text mesh
			char character;
			float y_max=0, y_min=0, x_max=0, x_min=0;
			float text_width = 0, text_height = 0;
			int line_letter_idx = 0;
			float line_height_offset = 0;
			float total_text_width = 0, total_text_height = 0;
			float line_width_at_last_space = 0;
			float space_char_offset = 0;
			int last_letter_setup_idx = -1;
			float last_space_y_max = 0;
			float last_space_y_min = 0;
			Rect uv_data;
			
			float letter_offset = 0;
			int letter_count = 0;
			int line_idx = 0;
			int word_idx = 0;

			m_line_height = 0;
			
			Action AddNewLineData = new Action( () =>
			{
				if(m_display_axis == TextDisplayAxis.HORIZONTAL)
				{
					float height = Mathf.Abs(y_max - y_min ) * LineHeightFactor;

					// Check if line is the tallest so far
					if(height > m_line_height)
						m_line_height = height;

					if(last_char_info != null)
					{
						// Re-adjust width of last letter since its the end of the text line
						text_width += - last_char_info.width + last_char_info.vert.width + last_char_info.vert.x;
					}

					line_widths.Add(text_width);
					line_height_offset += height;
					
					if(text_width > total_text_width)
					{
						total_text_width = text_width;
					}
					total_text_height += height;
				}
				else
				{
					float width = Mathf.Abs( x_max - x_min ) * LineHeightFactor;

					// Check if line is the tallest so far
					if(width > m_line_height)
						m_line_height = width;


					line_widths.Add(width);
					line_height_offset += width;
					
					total_text_width += width;
					if(text_height < total_text_height)
					{
						total_text_height = text_height;
					}
				}
				
				line_letter_idx = 0;
				text_width = 0;
				line_width_at_last_space = 0;
				space_char_offset = 0;
				last_space_y_max = 0;
				last_space_y_min = 0;
				text_height = 0;
				last_char_info = null;
			});
			
			for(int letter_idx=0; letter_idx < text_length; letter_idx++)
			{
				character = new_text[letter_idx];
				
				if(GetCharacterInfo(character, ref char_info))
				{
					if(character.Equals('\t'))
					{
						continue;
					}
					else if(character.Equals(' '))
					{
						if(m_display_axis == TextDisplayAxis.HORIZONTAL)
						{
							// Record the state of the line dims at this point incase the next word is forced onto next line by bound box
							line_width_at_last_space = text_width;
							space_char_offset = char_info.width;
							last_space_y_max = y_max;
							last_space_y_min = y_min;

							// Update index of word currently rendering, and reset list of words char_info's
							last_letter_setup_idx = letter_count;
							cached_last_word_char_infos = new List<CustomCharacterInfo>();


							text_width += char_info.width;
						}
						else
						{
							char_info.vert.height = -char_info.width;
						}
						
						// Add space width to offset value
						letter_offset += m_display_axis == TextDisplayAxis.HORIZONTAL ? char_info.width : -char_info.width;
						
						//Increment word count
						word_idx++;
					}
					else if(character.Equals('\n'))
					{
						AddNewLineData.Invoke();

						last_letter_setup_idx = -1;

						letter_offset = 0;
						line_idx++;
						
						//Increment word count
						word_idx++;
					}
					else
					{
						if(m_display_axis == TextDisplayAxis.HORIZONTAL)
						{
							// Add character info to current words cache list
							if(last_letter_setup_idx >= 0)
								cached_last_word_char_infos.Add(new CustomCharacterInfo(char_info));

							if(line_letter_idx == 0 || char_info.vert.y > y_max)
							{
								y_max = char_info.vert.y;
							}
							if(line_letter_idx == 0 || char_info.vert.y + char_info.vert.height < y_min)
							{
								y_min = char_info.vert.y + char_info.vert.height;
							}
							
							// increment the text width by the letter progress width, and then full mesh width for last letter or end of line.
							text_width += (letter_idx == text_length - 1)
												? char_info.vert.width + char_info.vert.x :
													char_info.width;
							
							// Handle bounding box if setup
							if(m_max_width > 0 && last_letter_setup_idx >= 0)
							{
								float actual_line_width = (letter_idx == text_length - 1) ? text_width : text_width - char_info.width + char_info.vert.width + char_info.vert.x;
								
								if(actual_line_width > m_max_width)
								{
									// Line exceeds bounding box width
									float new_line_text_width = text_width - line_width_at_last_space - space_char_offset;
									float new_line_y_min = last_space_y_min;
									float new_line_y_max = last_space_y_max;									
										
									// Set line width to what it was at the last space (which is now the end of this line)
									text_width = line_width_at_last_space;
									y_max = last_space_y_max;
									y_min = last_space_y_min;

									letter_offset = 0;
									line_idx++;

									AddNewLineData.Invoke();
									
									// Setup current values
									text_width = new_line_text_width;
									y_min = new_line_y_min;
									y_max = new_line_y_max;


									

									CustomCharacterInfo character_info;
									Vector3 new_base_offset;

									// Need to update the vert positions of the letters now on a new line
									for(int past_letter_idx = last_letter_setup_idx; past_letter_idx < letter_count; past_letter_idx++)
									{
										character_info = cached_last_word_char_infos[past_letter_idx - last_letter_setup_idx];

										letter_line_numbers[past_letter_idx] = line_idx;


										new_base_offset = m_display_axis == TextDisplayAxis.HORIZONTAL
																	? new Vector3(letter_offset + character_info.vert.x, 0, 0)
																	: new Vector3(character_info.vert.x, letter_offset, 0);

										// Setup letter mesh vert positions
										m_mesh_verts[past_letter_idx * 4 + (character_info.flipped ? 3 : 0)] = new Vector3(character_info.vert.width, (FontBaseLine + character_info.vert.y), 0) + new_base_offset;
										m_mesh_verts[past_letter_idx * 4 + (character_info.flipped ? 0 : 1)] = new Vector3(0, (FontBaseLine + character_info.vert.y), 0) + new_base_offset;
										m_mesh_verts[past_letter_idx * 4 + (character_info.flipped ? 1 : 2)] = new Vector3(0, character_info.vert.height + FontBaseLine + character_info.vert.y, 0) + new_base_offset;
										m_mesh_verts[past_letter_idx * 4 + (character_info.flipped ? 2 : 3)] = new Vector3(character_info.vert.width, character_info.vert.height + FontBaseLine + character_info.vert.y, 0) + new_base_offset;


										letter_offset += m_display_axis == TextDisplayAxis.HORIZONTAL
																? character_info.width + (m_px_offset.x / FontScale)
																: character_info.vert.height + (-m_px_offset.y / FontScale);
									}


									last_letter_setup_idx = -1;
								}
							}
						}
						else
						{
							if(line_letter_idx == 0 || char_info.vert.x + char_info.vert.width > x_max)
							{
								x_max = char_info.vert.x + char_info.vert.width;
							}
							if(line_letter_idx == 0 || char_info.vert.x < x_min)
							{
								x_min = char_info.vert.x;
							}
							
							text_height += char_info.vert.height;
						}
						
						


						Vector3 base_offset = m_display_axis == TextDisplayAxis.HORIZONTAL
													? new Vector3(letter_offset + char_info.vert.x, 0, 0)
													: new Vector3(char_info.vert.x, letter_offset, 0);

						// Setup letter mesh vert positions
						m_mesh_verts[letter_count * 4 + (char_info.flipped ? 3 : 0)] = new Vector3(char_info.vert.width, (FontBaseLine + char_info.vert.y), 0) + base_offset;
						m_mesh_verts[letter_count * 4 + (char_info.flipped ? 0 : 1)] = new Vector3(0, (FontBaseLine + char_info.vert.y), 0) + base_offset;
						m_mesh_verts[letter_count * 4 + (char_info.flipped ? 1 : 2)] = new Vector3(0, char_info.vert.height + FontBaseLine + char_info.vert.y, 0) + base_offset;
						m_mesh_verts[letter_count * 4 + (char_info.flipped ? 2 : 3)] = new Vector3(char_info.vert.width, char_info.vert.height + FontBaseLine + char_info.vert.y, 0) + base_offset;

						// Set letter mesh uv offsets
						uv_data = char_info.uv;
						m_mesh_uvs[letter_count * 4 + (char_info.flipped ? 3 : 0)] = new Vector2(uv_data.x + uv_data.width, uv_data.y + uv_data.height);
						m_mesh_uvs[letter_count * 4 + (char_info.flipped ? 2 : 1)] = new Vector2(uv_data.x, uv_data.y + uv_data.height);
						m_mesh_uvs[letter_count * 4 + (char_info.flipped ? 1 : 2)] = new Vector2(uv_data.x, uv_data.y);
						m_mesh_uvs[letter_count * 4 + (char_info.flipped ? 0 : 3)] = new Vector2(uv_data.x + uv_data.width, uv_data.y);

						m_mesh_normals[letter_count * 4] = m_mesh_normals[letter_count * 4 + 1] = m_mesh_normals[letter_count * 4 + 2] = m_mesh_normals[letter_count * 4 + 3] = Vector3.back;
						
						// {2,1,0, 3,2,0};
						m_mesh_triangles[letter_count * 6] = letter_count * 4 + 2;
						m_mesh_triangles[letter_count * 6 + 1] = letter_count * 4 + 1;
						m_mesh_triangles[letter_count * 6 + 2] = letter_count * 4 + 0;
						
						m_mesh_triangles[letter_count * 6 + 3] = letter_count * 4 + 3;
						m_mesh_triangles[letter_count * 6 + 4] = letter_count * 4 + 2;
						m_mesh_triangles[letter_count * 6 + 5] = letter_count * 4 + 0;


						// Set base colours
						m_mesh_cols[letter_count * 4 + (char_info.flipped ? 3 : 0)] = m_use_colour_gradient ? m_textColourGradient.top_right : m_textColour;
						m_mesh_cols[letter_count * 4 + (char_info.flipped ? 0 : 1)] = m_use_colour_gradient ? m_textColourGradient.top_left : m_textColour;
						m_mesh_cols[letter_count * 4 + (char_info.flipped ? 1 : 2)] = m_use_colour_gradient ? m_textColourGradient.bottom_left : m_textColour;
						m_mesh_cols[letter_count * 4 + (char_info.flipped ? 2 : 3)] = m_use_colour_gradient ? m_textColourGradient.bottom_right : m_textColour;

						// Preserve current line index for this letter
						letter_line_numbers[letter_count] = line_idx;


						letter_count ++;
						
						letter_offset += m_display_axis == TextDisplayAxis.HORIZONTAL ? 
												char_info.width + (m_px_offset.x / FontScale) : 
												char_info.vert.height + (-m_px_offset.y / FontScale);
						
						
						last_char_info = char_info;
						
					}
				}
				
				line_letter_idx++;
			}
			
			if(m_display_axis == TextDisplayAxis.HORIZONTAL)
			{
				float height = Mathf.Abs(y_max - y_min );

				line_widths.Add(text_width);
				
				if(text_width > total_text_width)
				{
					total_text_width = text_width;
				}
				total_text_height += height;

				if(m_line_height == 0)
				{
					// Only one line
					m_line_height = total_text_height;
				}
			}
			else
			{
				float width = Mathf.Abs( x_max - x_min );

				line_widths.Add( width );
				
				total_text_width += width;
				
				if(text_height < total_text_height)
				{
					total_text_height = text_height;
				}
			}
			
			m_total_text_width = m_max_width > 0 ? m_max_width : total_text_width;
			m_total_text_height = total_text_height * (m_display_axis == TextDisplayAxis.HORIZONTAL ? 1 : -1);


			
			// Apply line height offsetting and anchoring/aligment text offset per each line of text
			Vector3[] text_offsets = new Vector3[line_widths.Count];

			for(int idx = 0; idx < line_widths.Count; idx ++)
				text_offsets[idx] = CalculateTextPositionalOffset (m_text_anchor, m_display_axis, m_text_alignment, line_widths[idx]);

			for(int letter_idx = 0; letter_idx < num_letters; letter_idx++)
			{
				Vector3 new_line_offset = m_display_axis == TextDisplayAxis.HORIZONTAL
												? new Vector3(0, -letter_line_numbers[letter_idx] * LineHeight, 0)
												: new Vector3(letter_line_numbers[letter_idx] * LineHeight, 0, 0);

				m_mesh_verts[letter_idx * 4] += text_offsets[letter_line_numbers[letter_idx]] + new_line_offset;
				m_mesh_verts[letter_idx * 4 + 1] += text_offsets[letter_line_numbers[letter_idx]] + new_line_offset;
				m_mesh_verts[letter_idx * 4 + 2] += text_offsets[letter_line_numbers[letter_idx]] + new_line_offset;
				m_mesh_verts[letter_idx * 4 + 3] += text_offsets[letter_line_numbers[letter_idx]] + new_line_offset;
			}


			m_animation_manager.UpdateText (m_text, new TextFxTextDataHandler(m_mesh_verts, m_mesh_cols), false);


			if(//!Application.isPlaying &&
			   !m_animation_manager.Playing
//#if UNITY_EDITOR
//			   && !TextFxAnimationManager.ExitingPlayMode		// To stop text being auto rendered in to default position when exiting play mode in editor.
//#endif
			   )
			{
                // Update animated mesh values to latest
                m_animation_manager.UpdateMesh(true, true, delta_time: 0);

				if(m_animation_manager.CheckCurveData())
				{
					// Update mesh values to latest using new curve offset values
					ForceUpdateCachedVertData();

//					// Reset the verts data to reflect the curves offset
//					verts.Clear();
//
//					Vector3[] vertsToUse = m_animation_manager.MeshVerts;
//
//					for (int idx = 0; idx < vertsToUse.Length; idx ++)
//					{
//						verts.Add (vertsToUse[idx]);
//					}
				}

                // Render state of newly set text
				UpdateMesh( Application.isPlaying );
			}
		}


		// Calculates the amount that the whole text block needs to be offset in order to adhere to the anchor and text-alignment settings
		Vector3 CalculateTextPositionalOffset(TextAnchor anchor, TextDisplayAxis display_axis, TextAlignment alignment, float line_width)
		{
			Vector3 text_positional_offset = Vector3.zero;
			
			// Handle text y offset
			if(anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleRight)
			{
				text_positional_offset += new Vector3(0, (m_total_text_height / 2) - FontBaseLine, 0);
			}
			else if(anchor == TextAnchor.LowerLeft || anchor == TextAnchor.LowerCenter || anchor == TextAnchor.LowerRight)
			{
				text_positional_offset += new Vector3(0, m_total_text_height - m_line_height, 0);
			}
			else
			{
				text_positional_offset += new Vector3(0, -FontBaseLine, 0);
			}
			
			
			float alignment_offset = 0;
			if(display_axis == TextDisplayAxis.HORIZONTAL)
			{
				if(alignment == TextAlignment.Center)
				{
					alignment_offset = (m_total_text_width - line_width) / 2;
				}
				else if(alignment == TextAlignment.Right)
				{
					alignment_offset = (m_total_text_width - line_width);
				}
			}
			else
			{
				if(alignment == TextAlignment.Center)
				{
					text_positional_offset -= new Vector3(0, (m_total_text_height - m_line_height) / 2, 0);
					
				}
				else if(alignment == TextAlignment.Right)
				{
					text_positional_offset -= new Vector3(0, (m_total_text_height - m_line_height), 0);
				}
			}
			
			// Handle text x offset
			if(anchor == TextAnchor.LowerRight || anchor == TextAnchor.MiddleRight || anchor == TextAnchor.UpperRight)
			{
				text_positional_offset -= new Vector3(m_total_text_width - alignment_offset, 0, 0);
			}
			else if(anchor == TextAnchor.LowerCenter || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.UpperCenter)
			{
				text_positional_offset -= new Vector3((m_total_text_width/2) - alignment_offset, 0, 0);
			}
			else
			{
				text_positional_offset += new Vector3(alignment_offset, 0, 0);
			}
			
			return text_positional_offset;
		}





		public void SetFont(Font font)
		{
			m_font_data_file = null;
			m_font = font;
			m_font_material = null;

#if !UNITY_4_6 && !UNITY_4_5 && !UNITY_4_3 && !UNITY_4_2 && !UNITY_4_1 && !UNITY_4_0_1 && !UNITY_4_0
			if (m_fontRebuildCallback == null)
			{
				m_fontRebuildCallback = (Font rebuiltFont) => {
					FontImportDetected(rebuiltFont);
				};
				
				Font.textureRebuilt += m_fontRebuildCallback;
			}
#else
			m_font.textureRebuildCallback += FontTextureRebuilt;
#endif

			SetText(m_text);
		}

		public void SetFont(TextAsset font_data, Material font_material)
		{
			m_font = null;
			m_font_data_file = font_data;
			m_font_material = font_material;
			
			SetText(m_text);
		}
		

		// animationCall denotes whether the text is being rendered in its current animated state, or in its default state
		void UpdateMesh(bool animationCall)
		{
			if (m_mesh_uvs == null)
			{
				//
				Debug.LogError("UpdateMesh() - No mesh UV data available");
				return;
			}

			if (m_mesh == null)
				OnEnable ();

			// Clear triangles array to prevent errors to do with referencing vertex indexes which aren't present anymore
			m_mesh.triangles = null; //new int[0];

			// Assign latest verts and uvs from SetText()
			m_mesh.vertices = animationCall && m_animation_manager.MeshVerts != null ? m_animation_manager.MeshVerts : m_mesh_verts;
			m_mesh.colors = animationCall && m_animation_manager.MeshColours != null ? m_animation_manager.MeshColours : m_mesh_cols;

			m_mesh.uv = m_mesh_uvs;
			m_mesh.triangles = m_mesh_triangles;
			m_mesh.normals = m_mesh_normals;

			if(OnMeshUpdateCall != null)
				OnMeshUpdateCall();
		}

		void OnDestroy()
		{
			// Destroy shared mesh instance.
			if(Application.isPlaying)
			{
				Destroy(m_mesh);
			}
			else
			{
				DestroyImmediate(m_mesh);
			}
	    }
		
	#if UNITY_EDITOR	
		void OnDrawGizmos()
		{
			if(m_max_width > 0)
			{
				Gizmos.color = Color.red;
				
				Vector3 position_offset = Vector3.zero;
				if(m_text_anchor == TextAnchor.LowerLeft || m_text_anchor == TextAnchor.MiddleLeft || m_text_anchor == TextAnchor.UpperLeft)
				{
					position_offset += new Vector3((m_max_width > 0 ? m_max_width : m_total_text_width) / 2, 0, 0);
				}
				else if(m_text_anchor == TextAnchor.LowerRight || m_text_anchor == TextAnchor.MiddleRight || m_text_anchor == TextAnchor.UpperRight)
				{
					position_offset -= new Vector3((m_max_width > 0 ? m_max_width : m_total_text_width) / 2, 0, 0);
				}
				
				if(m_text_anchor == TextAnchor.LowerCenter || m_text_anchor == TextAnchor.LowerLeft || m_text_anchor == TextAnchor.LowerRight)
				{
					position_offset += new Vector3(0, m_total_text_height / 2, 0);
				}
				else if(m_text_anchor == TextAnchor.UpperLeft || m_text_anchor == TextAnchor.UpperCenter || m_text_anchor == TextAnchor.UpperRight)
				{
					position_offset -= new Vector3(0, m_total_text_height / 2, 0);
				}
				
				if(m_max_width > 0)
				{
					// Left edge limit
					Gizmos.DrawWireCube(transform.position + position_offset - new Vector3(m_max_width/2, 0, 0), new Vector3(0.01f, m_total_text_height, 0));
					// Right edge limit
					Gizmos.DrawWireCube(transform.position + position_offset + new Vector3(m_max_width/2, 0, 0), new Vector3(0.01f, m_total_text_height, 0));
				}
			}
	    }
	#endif
	}
}