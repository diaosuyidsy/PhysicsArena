using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Boomlagoon.TextFx.JSON;

namespace TextFx
{
	[System.Serializable]
	public struct BezierCurvePoint
	{
		public Vector3 m_anchor_point;
		public Vector3 m_handle_point;
		
		public Vector3 HandlePoint(bool inverse)
		{
			if(inverse)
				return m_anchor_point + (m_anchor_point - m_handle_point);
			else
				return m_handle_point;
		}
	}

	[System.Serializable]
	public struct BezierCurvePointData
	{
		public BezierCurvePoint m_anchor_point_1;
		public BezierCurvePoint m_anchor_point_2;
		public BezierCurvePoint m_anchor_point_3;
		public BezierCurvePoint m_anchor_point_4;
		public BezierCurvePoint m_anchor_point_5;
		public int m_numActiveCurvePoints;

		public Vector3 GetAnchorPoint(int index)
		{
			switch (index)
			{
			case 0:
				return m_anchor_point_1.m_anchor_point;
			case 1:
				return m_anchor_point_2.m_anchor_point;
			case 2:
				return m_anchor_point_3.m_anchor_point;
			case 3:
				return m_anchor_point_4.m_anchor_point;
			case 4:
				return m_anchor_point_5.m_anchor_point;
			default:
				return Vector3.zero;
			}
		}

		public void SetAnchorPoint(int index, Vector3 value)
		{
			switch (index)
			{
			case 0:
				m_anchor_point_1.m_anchor_point = value; break;
			case 1:
				m_anchor_point_2.m_anchor_point = value; break;
			case 2:
				m_anchor_point_3.m_anchor_point = value; break;
			case 3:
				m_anchor_point_4.m_anchor_point = value; break;
			case 4:
				m_anchor_point_5.m_anchor_point = value; break;
			}
		}

		public Vector3 GetHandlePoint(int index, bool inverse = false)
		{
			switch (index)
			{
			case 0:
				return inverse ? m_anchor_point_1.m_anchor_point + (m_anchor_point_1.m_anchor_point - m_anchor_point_1.m_handle_point) : m_anchor_point_1.m_handle_point;
			case 1:
				return inverse ? m_anchor_point_2.m_anchor_point + (m_anchor_point_2.m_anchor_point - m_anchor_point_2.m_handle_point) : m_anchor_point_2.m_handle_point;
			case 2:
				return inverse ? m_anchor_point_3.m_anchor_point + (m_anchor_point_3.m_anchor_point - m_anchor_point_3.m_handle_point) : m_anchor_point_3.m_handle_point;
			case 3:
				return inverse ? m_anchor_point_4.m_anchor_point + (m_anchor_point_4.m_anchor_point - m_anchor_point_4.m_handle_point) : m_anchor_point_4.m_handle_point;
			case 4:
				return inverse ? m_anchor_point_5.m_anchor_point + (m_anchor_point_5.m_anchor_point - m_anchor_point_5.m_handle_point) : m_anchor_point_5.m_handle_point;
			default:
				return Vector3.zero;
			}
		}

		public void SetHandlePoint(int index, Vector3 value)
		{
			switch (index)
			{
			case 0:
				m_anchor_point_1.m_handle_point = value; break;
			case 1:
				m_anchor_point_2.m_handle_point = value; break;
			case 2:
				m_anchor_point_3.m_handle_point = value; break;
			case 3:
				m_anchor_point_4.m_handle_point = value; break;
			case 4:
				m_anchor_point_5.m_handle_point = value; break;
			}
		}
	}


	[System.Serializable]
	public struct TextFxBezierCurve
	{
		const int m_gizmo_line_subdivides = 25;
		const int NUM_CURVE_SAMPLE_SUBSECTIONS = 50;
		const int MAX_NUM_ANCHOR_POINTS = 5;
		
		public BezierCurvePointData m_pointData;

		public float m_baselineOffset;		// The y-offset distance to be applied to the text. Used to align the text with the curve better
#if UNITY_EDITOR
		[SerializeField]
		bool m_editor_visible;
		public bool EditorVisible { get { return m_editor_visible; } set { m_editor_visible = value; } }
#endif
		Vector3[] m_temp_anchor_points;
		Vector3 rot;

		public void AddNewAnchor()
		{
			if(m_pointData.m_numActiveCurvePoints < 2)
			{
				m_pointData.m_numActiveCurvePoints = 2;

				m_pointData.SetAnchorPoint (0, new Vector3 (-5, 0, 0));
				m_pointData.SetHandlePoint (0, new Vector3 (-2.5f, 4f, 0));

				m_pointData.SetAnchorPoint (1, new Vector3 (5, 0, 0));
				m_pointData.SetHandlePoint (1, new Vector3 (2.5f, 4f, 0));
			}
			else if(m_pointData.m_numActiveCurvePoints < MAX_NUM_ANCHOR_POINTS)
			{
				m_pointData.SetAnchorPoint (m_pointData.m_numActiveCurvePoints, m_pointData.GetAnchorPoint (m_pointData.m_numActiveCurvePoints - 1) + new Vector3 (5,0,0));
				m_pointData.SetHandlePoint (m_pointData.m_numActiveCurvePoints, m_pointData.GetHandlePoint (m_pointData.m_numActiveCurvePoints - 1) + new Vector3 (5,0,0));

				m_pointData.m_numActiveCurvePoints++;
			}
		}

		public void AddNewAnchor(Vector3 anchorPointPos, Vector3 handlePointPos)
		{
			m_pointData.SetAnchorPoint (m_pointData.m_numActiveCurvePoints, anchorPointPos);
			m_pointData.SetHandlePoint (m_pointData.m_numActiveCurvePoints, handlePointPos);

			m_pointData.m_numActiveCurvePoints++;
		}
		
		public Vector3 GetCurvePoint(float progress, int num_anchors = 4, int curve_idx = -1, float yOffset = 0)
		{
			if(m_pointData.m_numActiveCurvePoints < 2)
				return Vector3.zero;
			
			if(m_temp_anchor_points == null || m_temp_anchor_points.Length < num_anchors)
				m_temp_anchor_points = new Vector3[num_anchors];

			if(progress < 0)
				progress = 0;

			if(curve_idx < 0)
			{
				// Work out curve idx from progress
				curve_idx = Mathf.FloorToInt(progress);
				
				progress %= 1;
			}
			
			if(curve_idx >= m_pointData.m_numActiveCurvePoints - 1)
			{
				curve_idx = m_pointData.m_numActiveCurvePoints - 2;
				progress = 1;
			}
			

			for(int idx=1; idx < num_anchors; idx++)
			{
				if(num_anchors == 4)
				{
					if(idx == 1)
						m_temp_anchor_points[idx-1] = m_pointData.GetAnchorPoint(curve_idx) + ( m_pointData.GetHandlePoint(curve_idx, curve_idx > 0) -  m_pointData.GetAnchorPoint(curve_idx)) * progress;
					else if(idx == 2)
						m_temp_anchor_points[idx-1] = m_pointData.GetHandlePoint(curve_idx, curve_idx > 0) + ( m_pointData.GetHandlePoint(curve_idx+1) - m_pointData.GetHandlePoint(curve_idx, curve_idx > 0)) * progress;
					else if(idx == 3)
						m_temp_anchor_points[idx-1] = m_pointData.GetHandlePoint(curve_idx+1) + ( m_pointData.GetAnchorPoint(curve_idx+1) -  m_pointData.GetHandlePoint(curve_idx+1)) * progress;
				}
				else
					m_temp_anchor_points[idx-1] = m_temp_anchor_points[idx-1] + (m_temp_anchor_points[idx] - m_temp_anchor_points[idx-1]) * progress;
			}
			
			if(num_anchors == 2)
			{
				// Reached the bezier curve point requested
				// Check for yOffset
				if(yOffset != 0)
				{
					// Calculate UpVector for this point on the curve
					Vector3 upVec = Vector3.Cross((m_temp_anchor_points[1] - m_temp_anchor_points[0]), Vector3.forward);

					m_temp_anchor_points[0] -= upVec.normalized * yOffset;
				}

				return m_temp_anchor_points[0];
			}
			else
				return GetCurvePoint(progress, num_anchors-1, curve_idx, yOffset);
		}
		
		public Vector3 GetCurvePointRotation(float progress, int curve_idx = -1)
		{
			if(m_pointData.m_numActiveCurvePoints < 2)
				return Vector3.zero;
			
			if(curve_idx < 0)
			{
				// Work out curve idx from progress
				curve_idx = Mathf.FloorToInt(progress);
				
				progress %= 1;
			}
			
			if(curve_idx >= m_pointData.m_numActiveCurvePoints - 1)
			{
				curve_idx = m_pointData.m_numActiveCurvePoints - 2;
				progress = 1;
			}
			
			if(progress < 0)
				progress = 0;
			
			Vector3 point_dir_vec = GetCurvePoint(Mathf.Clamp(progress + 0.01f, 0, 1), curve_idx : curve_idx) - GetCurvePoint(Mathf.Clamp(progress - 0.01f, 0, 1), curve_idx : curve_idx);
			
			if(point_dir_vec.Equals(Vector3.zero))
			{
				return Vector3.zero;
			}
			
			rot = (Quaternion.AngleAxis(-90, point_dir_vec) * Quaternion.LookRotation(Vector3.Cross(point_dir_vec, Vector3.forward), Vector3.forward)).eulerAngles;
			
			// Clamp all axis rotations to be within [-180, 180] range for more sensible looking rotation transitions
			rot.x -= rot.x < 180 ? 0 : 360;
			rot.y -= rot.y < 180 ? 0 : 360;
			rot.z -= rot.z < 180 ? 0 : 360;
			
			return rot;
		}
		
		public float[] GetLetterProgressions(TextFxAnimationManager anim_manager, ref LetterSetup[] letters, TextAlignment alignment = TextAlignment.Left)
		{
			float[] letter_progressions = new float[letters.Length];

			if (letters.Length == 0)
				return letter_progressions;
			
			if(m_pointData.m_numActiveCurvePoints < 2)
			{
				for(int idx=0; idx < letters.Length; idx++)
				{
					letter_progressions[idx] = 0;
				}
				return letter_progressions;
			}
			
			float progress_inc = 1f / NUM_CURVE_SAMPLE_SUBSECTIONS;
			float progress;
			Vector3 new_point = new Vector3();
			Vector3 last_point = new Vector3();
			int letter_idx = 0, line_number=0;
			float base_letters_offset = 0, letters_offset = 0;
			LetterSetup letter = null;
			float last_line_length = 0, line_length = 0;
			float curve_length = 0;
			float renderedTextWidth;

			// Method to handle offsetting the letter_offset value based on the text alignment setting
			System.Action offsetBasedOnTextAlignment = () => {
				if (alignment == TextAlignment.Center || alignment == TextAlignment.Right)
				{
					renderedTextWidth = (anim_manager.TextWidthScaled (letter.m_progression_variables.LineValue) < curve_length ? anim_manager.TextWidthScaled (letter.m_progression_variables.LineValue) : curve_length);

					if (alignment == TextAlignment.Center)
					{
						letters_offset += (curve_length / 2) - (renderedTextWidth / 2);
					}
					else if (alignment == TextAlignment.Right)
					{
						letters_offset += curve_length - renderedTextWidth;
					}
				}
			};


			// Grab reference to first letter setup
			letter = letters[0];
			
			// Calculate the total length of the belzier curve if the text alignment is set to center or right.
			if(alignment == TextAlignment.Center || alignment == TextAlignment.Right)
			{
				for(int curve_idx=0; curve_idx < m_pointData.m_numActiveCurvePoints - 1; curve_idx++)
				{
					curve_length += GetCurveLength(curve_idx);
				}
			}


			// Assign base letter offset value using first letters offset value
			base_letters_offset = letter.BaseVerticesCenter.x / anim_manager.AnimationInterface.MovementScale - (letter.Width / anim_manager.AnimationInterface.MovementScale)/2;
			
			// Setup letter offset value
			letters_offset = (letter.Width / anim_manager.AnimationInterface.MovementScale)/2;

			// Handle alignment-specific offset values
			offsetBasedOnTextAlignment ();


			bool reachedEndOfLetters = false;

			while (!reachedEndOfLetters)
			{
				for (int curve_idx = 0; curve_idx < m_pointData.m_numActiveCurvePoints - 1; curve_idx++)
				{
					for (int idx = 0; idx <= NUM_CURVE_SAMPLE_SUBSECTIONS; idx++)
					{
						progress = idx * progress_inc;
						
						new_point = GetCurvePoint (progress, curve_idx : curve_idx);
						
						if (idx > 0)
						{
							line_length += (new_point - last_point).magnitude;

							while (letter_idx < letters.Length && line_length > letters_offset)
							{
								// calculate relative progress between the last two points which would represent the next letters offset distance
								progress = curve_idx + ((idx - 1) * progress_inc) + (((letters_offset - last_line_length) / (line_length - last_line_length)) * progress_inc);

								letter_progressions [letter_idx] = progress;
								
								letter_idx++;

								// Work out offset value for next letter
								if (letter_idx < letters.Length && !letters [letter_idx].StubInstance && letters [letter_idx].VisibleCharacter)
								{
									letter = letters [letter_idx];
									
									if (letter.m_progression_variables.LineValue > line_number)
									{
										line_number = letter.m_progression_variables.LineValue;
										
										// Set a new base offset value to that of the first letter of this new line
										base_letters_offset = letter.BaseVerticesCenter.x / anim_manager.AnimationInterface.MovementScale - (letter.Width / anim_manager.AnimationInterface.MovementScale) / 2;
										
										curve_idx = 0;
										idx = -1;
										line_length = 0;
									}
									
									// Setup letter offset value
									letters_offset = ((letter.BaseVerticesCenter.x / anim_manager.AnimationInterface.MovementScale) - base_letters_offset);

									// Handle alignment-specific offset values
									offsetBasedOnTextAlignment ();
								}
							}
							
							if (letter_idx == letters.Length)
							{
								reachedEndOfLetters = true;
								break;
							}
						}
						
						last_point = new_point;
						last_line_length = line_length;
					}
				}

				// Handle any letters which didn't have room to fit on the line
				for(int idx=letter_idx; idx < letters.Length; idx++)
				{
					letter = letters[letter_idx];

					if (letter.m_progression_variables.LineValue == line_number)
						// Force remaining letters to the end of the line
						letter_progressions [idx] = m_pointData.m_numActiveCurvePoints - 1.001f;
					else if(letter.VisibleCharacter && !letter.StubInstance)
					{
						line_number = letter.m_progression_variables.LineValue;

						line_length = 0;

						// Set a new base offset value to that of the first letter of this new line
						base_letters_offset = letter.BaseVerticesCenter.x / anim_manager.AnimationInterface.MovementScale - (letter.Width / anim_manager.AnimationInterface.MovementScale) / 2;

						// Setup letter offset value 
						letters_offset = ((letter.BaseVerticesCenter.x / anim_manager.AnimationInterface.MovementScale) - base_letters_offset);

						// Handle alignment-specific offset values
						offsetBasedOnTextAlignment ();

						break;
					}

					letter_idx++;
				}

				if (letter_idx == letters.Length)
				{
					reachedEndOfLetters = true;
					break;
				}
			}

//			string progressionsString = "";
//			for (int i = 0; i < letter_progressions.Length; i++)
//				progressionsString += letter_progressions [i] + "\n";
//			Debug.Log("Curve Letter Progressions:\n" + progressionsString);

			return letter_progressions;
		}

		// Get an approximation of the belzier curve length
		float GetCurveLength(int curve_idx)
		{
			int num_precision_intervals = NUM_CURVE_SAMPLE_SUBSECTIONS;
			Vector3? last_point = null;
			Vector3 current_point;
			float curve_length = 0;
			
			for(int idx=0; idx < num_precision_intervals; idx++)
			{
				current_point = GetCurvePoint((float) idx / (num_precision_intervals-1), curve_idx : curve_idx);
				
				if(last_point != null)
				{
					curve_length += ((Vector3)(current_point - last_point)).magnitude;
				}
				
				last_point = current_point;
			}

			return curve_length;
		}
		
		public tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = new tfxJSONObject();
//			
//			tfxJSONArray anchors_data = new tfxJSONArray();
//			tfxJSONObject anchor_point_data;
//			
//			foreach(BezierCurvePoint anchor_point in m_anchor_points)
//			{
//				anchor_point_data = new tfxJSONObject();
//				anchor_point_data["m_anchor_point"] = anchor_point.m_anchor_point.ExportData();
//				anchor_point_data["m_handle_point"] = anchor_point.m_handle_point.ExportData();
//				
//				anchors_data.Add(anchor_point_data);
//			}
//			
//			json_data["ANCHORS_DATA"] = anchors_data;
//			
			return new tfxJSONValue(json_data);
		}
		
		public void ImportData(tfxJSONObject json_data)
		{
//			m_anchor_points = new List<BezierCurvePoint>();
//			
//			BezierCurvePoint curve_point;
//			tfxJSONObject anchor_json;
//			
//			foreach(tfxJSONValue anchor_data in json_data["ANCHORS_DATA"].Array)
//			{
//				anchor_json = anchor_data.Obj;
//				curve_point = new BezierCurvePoint();
//				curve_point.m_anchor_point = anchor_json["m_anchor_point"].Obj.JSONtoVector3();
//				curve_point.m_handle_point = anchor_json["m_handle_point"].Obj.JSONtoVector3();
//				m_anchor_points.Add(curve_point);
//			}
		}
		
	#if UNITY_EDITOR

		// Handle inspector displaying Bezier Curve position setup options
		public void OnInspector(System.Action<string> recordUndoObjectCallback = null)
		{
			EditorGUI.indentLevel++;

			bool guiChanged = GUI.changed;

			m_baselineOffset = EditorGUILayout.FloatField ("Baseline Y Offset", m_baselineOffset);

			if (!guiChanged && GUI.changed)
			{
				if (recordUndoObjectCallback != null)
					recordUndoObjectCallback ("Curve Baseline Offset changed");

				guiChanged = true;
			}

			m_editor_visible = EditorGUILayout.Foldout(m_editor_visible, new GUIContent("Curve Points" + (m_editor_visible ? "  [Scene View Debug]" : "")));

			// Force the curve editing options to be visible the first time the CURVE option is set
			if (m_pointData.m_numActiveCurvePoints == 0)
				m_editor_visible = true;

			if(m_editor_visible)
			{
				EditorGUI.indentLevel++;

				Vector3 newValue;

				for(int idx=0; idx < m_pointData.m_numActiveCurvePoints; idx++)
				{
					EditorGUILayout.BeginHorizontal ();

					EditorGUILayout.LabelField ("#" + idx, GUILayout.MaxWidth(50));

					EditorGUILayout.BeginVertical ();

					newValue = EditorGUILayout.Vector3Field("Anchor", m_pointData.GetAnchorPoint(idx));
					if (!guiChanged && GUI.changed)
					{
						if (recordUndoObjectCallback != null)
							recordUndoObjectCallback ("Anchor Point #" + idx + " changed");

						guiChanged = true;
						m_pointData.SetAnchorPoint (idx, newValue);
					}

					newValue = EditorGUILayout.Vector3Field("Handle", m_pointData.GetHandlePoint(idx));
					if (!guiChanged && GUI.changed)
					{
						if (recordUndoObjectCallback != null)
							recordUndoObjectCallback ("Handle Point #" + idx + " changed");

						guiChanged = true;
						m_pointData.SetHandlePoint (idx, newValue);
					}

					EditorGUILayout.EndVertical ();

					if(idx > 1 && GUILayout.Button("X"))
					{
						m_pointData.m_numActiveCurvePoints--;
						idx--;
						break;
					}

					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.Space ();
				}

				EditorGUILayout.BeginHorizontal ();

				GUILayout.FlexibleSpace ();

				if(GUILayout.Button("Add Point", GUILayout.Width(100)) || m_pointData.m_numActiveCurvePoints == 0)
				{
					AddNewAnchor();
				}

				EditorGUILayout.EndHorizontal ();

				EditorGUI.indentLevel--;
			}

			EditorGUI.indentLevel--;
		}

		public void OnSceneGUI(Vector3 position_offset, Vector3 scale, System.Action<string> recordUndoObjectCallback = null)
		{
			if(m_pointData.m_numActiveCurvePoints < 2)
				return;
			
			bool changed = false;


			DrawCurvePoint(0, position_offset, scale, true, ref changed, recordUndoObjectCallback);
			
			for(int p_idx=1; p_idx < m_pointData.m_numActiveCurvePoints; p_idx++)
			{
				DrawCurvePoint(p_idx, position_offset, scale, p_idx == m_pointData.m_numActiveCurvePoints - 1, ref changed, recordUndoObjectCallback);
				
				Handles.DrawBezier(
									Vector3.Scale(m_pointData.GetAnchorPoint(p_idx-1), scale) + position_offset,
									Vector3.Scale(m_pointData.GetAnchorPoint(p_idx), scale) + position_offset,
									(p_idx == 1 ? Vector3.Scale(m_pointData.GetHandlePoint(p_idx-1), scale) : Vector3.Scale(m_pointData.GetAnchorPoint(p_idx-1) + (m_pointData.GetAnchorPoint(p_idx-1) - m_pointData.GetHandlePoint(p_idx-1)), scale)) + position_offset,
									Vector3.Scale(m_pointData.GetHandlePoint(p_idx), scale) + position_offset,
									Color.red, null, 2);
			}
		}
		

		void DrawCurvePoint(int index, Vector3 position_offset, Vector3 scale, bool start_end_point, ref bool changed, System.Action<string> recordUndoObjectCallback = null)
		{
			Vector3 newValue;

			Vector3 handle_offset =  m_pointData.GetHandlePoint(index) - m_pointData.GetAnchorPoint(index);

			newValue = Vector3.Scale( Handles.PositionHandle(Vector3.Scale(m_pointData.GetAnchorPoint(index),scale) + position_offset, Quaternion.identity) - position_offset, new Vector3(1/scale.x,1/scale.y,1/scale.z));

			if(!changed && GUI.changed)
			{
				changed = true;

				m_pointData.SetAnchorPoint (index, newValue);

				m_pointData.SetHandlePoint( index, newValue + handle_offset);

				if (recordUndoObjectCallback != null)
					recordUndoObjectCallback ("Anchor Point #" + index + " changed");
			}

			newValue = Vector3.Scale(Handles.PositionHandle(Vector3.Scale(m_pointData.GetHandlePoint(index),scale) + position_offset, Quaternion.identity) - position_offset, new Vector3(1/scale.x,1/scale.y,1/scale.z));

			if (!changed && GUI.changed)
			{
				changed = true;

				m_pointData.SetHandlePoint(index, newValue);

				if (recordUndoObjectCallback != null)
					recordUndoObjectCallback ("Handle Point #" + index + " changed");
			}

			Handles.color = Color.white;
			Handles.DrawLine(!start_end_point ? Vector3.Scale(m_pointData.GetAnchorPoint(index), scale) + Vector3.Scale((m_pointData.GetAnchorPoint(index) - m_pointData.GetHandlePoint(index)), scale) + position_offset : Vector3.Scale(m_pointData.GetAnchorPoint(index), scale) + position_offset, 
							Vector3.Scale(m_pointData.GetHandlePoint(index), scale) + position_offset);
		}
	#endif	
	}

}