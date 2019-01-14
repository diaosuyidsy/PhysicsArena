using UnityEngine;
using System.Collections.Generic;
using Boomlagoon.TextFx.JSON;

namespace TextFx
{
	public enum LOOP_TYPE
	{
		LOOP,
		LOOP_REVERSE
	}

	[System.Serializable]
	public class ActionLoopCycle
	{
		public int m_start_action_idx = 0;
		public int m_end_action_idx = 0;
		public int m_number_of_loops = 0;
		public LOOP_TYPE m_loop_type = LOOP_TYPE.LOOP;
		public bool m_delay_first_only = false;
		public bool m_finish_at_end = true;
		public int m_active_loop_index = -1;

		bool m_first_pass = true;
		public bool FirstPass { get { return m_first_pass; } set { m_first_pass = value; } }
		
		public ActionLoopCycle(){}
		
		public ActionLoopCycle(int start, int end)
		{
			m_start_action_idx = start;
			m_end_action_idx = end;
		}
		
		public ActionLoopCycle(int start, int end, int num_loops, LOOP_TYPE loop_type)
		{
			m_start_action_idx = start;
			m_end_action_idx = end;
			m_number_of_loops = num_loops;
			m_loop_type = loop_type;
		}
		
		public ActionLoopCycle Clone(int loop_index = -1)
		{

			ActionLoopCycle action_loop = new ActionLoopCycle(m_start_action_idx,m_end_action_idx);
			
			action_loop.m_active_loop_index = loop_index >= 0 ? loop_index : m_active_loop_index;
			action_loop.m_number_of_loops = m_number_of_loops;
			action_loop.m_loop_type = m_loop_type;
			action_loop.m_delay_first_only = m_delay_first_only;
			action_loop.m_finish_at_end = m_finish_at_end;
			
			return action_loop;
		}
		
		public int SpanWidth
		{
			get
			{
				return m_end_action_idx - m_start_action_idx;
			}
		}
		
		public tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			json_data["m_finish_at_end"] = m_finish_at_end;
			json_data["m_delay_first_only"] = m_delay_first_only;
			json_data["m_end_action_idx"] = m_end_action_idx;
			json_data["m_loop_type"] = (int) m_loop_type;
			json_data["m_number_of_loops"] = m_number_of_loops;
			json_data["m_start_action_idx"] = m_start_action_idx;
			
			return new tfxJSONValue(json_data);
		}
		
		public void ImportData(tfxJSONObject json_data)
		{
			if(json_data.ContainsKey("m_finish_at_end"))
				m_finish_at_end = json_data["m_finish_at_end"].Boolean;
			else
				m_finish_at_end = false;
			m_delay_first_only = json_data["m_delay_first_only"].Boolean;
			m_end_action_idx = (int) json_data["m_end_action_idx"].Number;
			m_loop_type = (LOOP_TYPE) (int) json_data["m_loop_type"].Number;
			m_number_of_loops = (int) json_data["m_number_of_loops"].Number;
			m_start_action_idx = (int) json_data["m_start_action_idx"].Number;
		}
	}

	[System.Serializable]
	public class LetterAnimation
	{
		const char DELIMITER_CHAR = '|';
		
		[SerializeField]
		List<LetterAction> m_letter_actions = new List<LetterAction>();
		[SerializeField]
		List<ActionLoopCycle> m_loop_cycles = new List<ActionLoopCycle>();
		
		public LETTERS_TO_ANIMATE m_letters_to_animate_option = LETTERS_TO_ANIMATE.ALL_LETTERS;
		public List<int> m_letters_to_animate;
		public int m_letters_to_animate_custom_idx = 1;
		[SerializeField]
		int m_num_white_space_chars_to_include = 0;
		[SerializeField]
		public ActionColorProgression m_defaultTextColourProgression = new ActionColorProgression(new VertexColour(Color.white));
		
		public int NumActions { get { return m_letter_actions.Count; } }
		public int NumLoops { get { return m_loop_cycles.Count; } }
		public List<LetterAction> LetterActions { get { return m_letter_actions; } }
		public List<ActionLoopCycle> ActionLoopCycles { get { return m_loop_cycles; } }
		
		LETTER_ANIMATION_STATE m_animation_state = LETTER_ANIMATION_STATE.PLAYING;
		public LETTER_ANIMATION_STATE CurrentAnimationState { get { return m_animation_state; } set { m_animation_state = value; } }

		public void AddAction(LetterAction letter_action)
		{
			AddAction (letter_action, -1);
		}

		public void AddAction(LetterAction letter_action, int index)
		{
			if(m_letter_actions == null)
				m_letter_actions = new List<LetterAction>();

			if(index < 0)
				m_letter_actions.Add(letter_action);
			else
				m_letter_actions.Insert(index, letter_action);
		}
		
		public LetterAction AddAction()
		{
			if(m_letter_actions == null)
				m_letter_actions = new List<LetterAction>();

			LetterAction newAction = new LetterAction();
            m_letter_actions.Add(newAction);

			return newAction;
        }
		
		public void InsertAction(int index, LetterAction action)
		{
			if(m_letter_actions == null)
				m_letter_actions = new List<LetterAction>();

			if(index >= 0 && index <= m_letter_actions.Count)
				m_letter_actions.Insert(index, action);

			UpdateLoopCyclesAfterIndex (index, 1);
		}
		
		public void RemoveAction(int index, bool deleteAffectedLoops = true)
		{
			RemoveActions (index, 1, deleteAffectedLoops);
		}
		
		public void RemoveActions(int index, int count, bool deleteAffectedLoops = true)
		{
			if(m_letter_actions != null && index >= 0 && index + count <= m_letter_actions.Count)
				m_letter_actions.RemoveRange(index, count);

			// Ammend the loop list info to reflect the removed Actions
			ActionLoopCycle loop_cycle;
			for(int loop_idx = 0; loop_idx < m_loop_cycles.Count; loop_idx++)
			{
				loop_cycle = m_loop_cycles[loop_idx];

				if(loop_cycle.m_start_action_idx >= index + count)
				{
					// This loop cycle starts beyond where the actions where removed from, so can still exist.
					// Needs to be moved back a bit though
					loop_cycle.m_start_action_idx -= count;
					loop_cycle.m_end_action_idx -= count;
				}
				else if(deleteAffectedLoops && loop_cycle.m_end_action_idx >= index)
				{
					// A loop which used the removed actions
					// Remove this loop
					m_loop_cycles.RemoveAt(loop_idx);
					loop_idx--;
					continue;
				}
			}
		}
		
		public LetterAction GetAction(int index)
		{
			if(m_letter_actions != null && index >= 0 && index < m_letter_actions.Count)
				return m_letter_actions[index];
			else
				return null;
		}
		
		public void AddLoop()
		{
			if(m_loop_cycles == null)
				m_loop_cycles = new List<ActionLoopCycle>();
			
			m_loop_cycles.Add(new ActionLoopCycle());
		}

		public void InsertLoop(int index, ActionLoopCycle loop)
		{
			InsertLoop (index, loop, false);
		}

		public void InsertLoop(int index, ActionLoopCycle loop, bool force_insert)
		{
			if(m_loop_cycles == null)
				m_loop_cycles = new List<ActionLoopCycle>();

			if (!force_insert)
			{
				// Check if loop already exists
				foreach (ActionLoopCycle loop_cycle in m_loop_cycles)
					if (loop_cycle.m_start_action_idx == loop.m_start_action_idx && loop_cycle.m_end_action_idx == loop.m_end_action_idx)
						return;
			}

			m_loop_cycles.Insert (index, loop);
		}
		
		public void RemoveLoop(int index)
		{
			if(m_loop_cycles != null && index >= 0 && index < m_loop_cycles.Count)
				m_loop_cycles.RemoveAt(index);
		}
		
		public void RemoveLoops(int index, int count)
		{
			if(m_loop_cycles != null && index >= 0 && index + count < m_loop_cycles.Count)
				m_loop_cycles.RemoveRange(index, count);
		}
		
		public ActionLoopCycle GetLoop(int index)
		{
			if(m_loop_cycles != null && index >= 0 && index < m_loop_cycles.Count)
				return m_loop_cycles[index];
			else
				return null;
		}
		
		
		public void AddLoop(int start_idx, int end_idx, bool change_type)
		{
			bool valid_loop_addition = true;
			int insert_at_idx = 0;
			
			if(end_idx >= start_idx && start_idx >= 0 && start_idx < m_letter_actions.Count && end_idx >= 0 && end_idx < m_letter_actions.Count)
			{
				int new_loop_width = end_idx - start_idx;
				int count = 1;
				foreach(ActionLoopCycle loop in m_loop_cycles)
				{
					if((start_idx < loop.m_start_action_idx && (end_idx >loop.m_start_action_idx && end_idx < loop.m_end_action_idx))
						|| (end_idx > loop.m_end_action_idx && (start_idx > loop.m_start_action_idx && start_idx < loop.m_end_action_idx)))
					{
						// invalid loop
						valid_loop_addition = false;
						Debug.LogWarning("Invalid Loop Added: Loops can not intersect other loops.");
						break;
					}
					else if(start_idx == loop.m_start_action_idx && end_idx == loop.m_end_action_idx)
					{
						// Entry already exists, so either add to it, or change its type
						valid_loop_addition = false;
						if(change_type)
						{
							loop.m_loop_type = loop.m_loop_type == LOOP_TYPE.LOOP ? LOOP_TYPE.LOOP_REVERSE : LOOP_TYPE.LOOP;
						}
						else
						{
							loop.m_number_of_loops ++;
						}
						break;
					}
					else
					{
						if(new_loop_width >= loop.SpanWidth)
						{
							insert_at_idx = count;
						}
					}
							
					count++;
				}
			}
			else
			{
				valid_loop_addition = false;
				Debug.LogWarning("Invalid Loop Added: Check that start/end index are in bounds.");
			}
			
			
			if(valid_loop_addition)
			{
				m_loop_cycles.Insert(insert_at_idx, new ActionLoopCycle(start_idx, end_idx));
			}
		}

		void CalculateLettersToAnimate(LetterSetup[] letters)
		{
			int num_letters = letters.Length;
			
			// Populate list of letters to animate by index, and set Letter indexes accordingly
			if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.ALL_LETTERS)
			{
				int progression_idx = 0;
				int progression_idx_inc_white_space = 0;
				LetterSetup last_letter = null;
				
				m_num_white_space_chars_to_include = 0;
				
				m_letters_to_animate = new List<int>();
				for(int letter_idx=0; letter_idx < num_letters; letter_idx++)
				{
					if(letters[letter_idx].VisibleCharacter && !letters[letter_idx].StubInstance)
					{
						m_letters_to_animate.Add(letter_idx);
						
						letters[letter_idx].m_progression_variables.SetLetterValue(progression_idx, progression_idx_inc_white_space);
						
						progression_idx++;
						progression_idx_inc_white_space++;
					}
					else
					{
						letters[letter_idx].m_progression_variables.SetLetterValue(-1);
						
						if(last_letter != null && last_letter.VisibleCharacter)
						{
							progression_idx_inc_white_space++;
							
							m_num_white_space_chars_to_include++;
						}
					}
					
					last_letter = letters[letter_idx];
				}
			}
			else if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.FIRST_LETTER || m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_LETTER)
			{
				m_letters_to_animate = new List<int>();
				m_letters_to_animate.Add(m_letters_to_animate_option == LETTERS_TO_ANIMATE.FIRST_LETTER ? 0 : letters.Length -1);
				
				letters[m_letters_to_animate_option == LETTERS_TO_ANIMATE.FIRST_LETTER ? 0 : letters.Length - 1].m_progression_variables.SetLetterValue(0);
			}
			else if(m_letters_to_animate_option != LETTERS_TO_ANIMATE.CUSTOM)
			{
				m_letters_to_animate = new List<int>();
				
				int line_idx = m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_LETTER_LINES ? 0 : -1;
				int word_idx = m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_LETTER_WORDS ? 0 : -1;
				int target_idx = 0;
				
				if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_WORD)
					target_idx = letters[letters.Length-1].m_progression_variables.WordValue;
				else if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_LINE)
					target_idx = letters[letters.Length-1].m_progression_variables.LineValue;
				else if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.NTH_WORD || m_letters_to_animate_option == LETTERS_TO_ANIMATE.NTH_LINE)
					target_idx = m_letters_to_animate_custom_idx - 1;
				
				
				int letter_idx = 0;
				int progression_idx = 0;
				int index_of_last_visible_letter = -1;
				
				foreach(LetterSetup letter in letters)
				{
					if(letter.VisibleCharacter)
					{
						if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.FIRST_LINE || m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_LINE || m_letters_to_animate_option == LETTERS_TO_ANIMATE.NTH_LINE)
						{
							if(letter.m_progression_variables.LineValue == target_idx)
							{
								letter.m_progression_variables.SetLetterValue(progression_idx);
								m_letters_to_animate.Add(letter_idx);
								progression_idx ++;
							}
						}
						else if(letter.m_progression_variables.LineValue > line_idx)
						{
							if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.FIRST_LETTER_LINES)
							{
								letter.m_progression_variables.SetLetterValue(progression_idx);
								m_letters_to_animate.Add(letter_idx);
								progression_idx ++;
								
							}
							else if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_LETTER_LINES && index_of_last_visible_letter >= 0)
							{
								letters[index_of_last_visible_letter].m_progression_variables.SetLetterValue(progression_idx);
								m_letters_to_animate.Add(index_of_last_visible_letter);
								progression_idx ++;
							}
							line_idx = letter.m_progression_variables.LineValue;
						}
						
						if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.FIRST_WORD || m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_WORD || m_letters_to_animate_option == LETTERS_TO_ANIMATE.NTH_WORD)
						{
							if(letter.m_progression_variables.WordValue == target_idx)
							{
								letter.m_progression_variables.SetLetterValue(progression_idx);
								m_letters_to_animate.Add(letter_idx);
								progression_idx ++;
							}
						}
						else if(letter.m_progression_variables.WordValue > word_idx)
						{
							if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.FIRST_LETTER_WORDS)
							{
								letter.m_progression_variables.SetLetterValue(progression_idx);
								m_letters_to_animate.Add(letter_idx);
								progression_idx ++;
							}
							else if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_LETTER_WORDS && index_of_last_visible_letter >= 0)
							{
								letters[index_of_last_visible_letter].m_progression_variables.SetLetterValue(progression_idx);
								m_letters_to_animate.Add(index_of_last_visible_letter);
								progression_idx ++;
							}
							word_idx = letter.m_progression_variables.WordValue;
						}
					}
					
					if(letter.VisibleCharacter)
						index_of_last_visible_letter = letter_idx;
					
					letter_idx++;
				}
				
				if(m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_LETTER_WORDS || m_letters_to_animate_option == LETTERS_TO_ANIMATE.LAST_LETTER_LINES)
				{
					if(m_letters_to_animate.Count == 0 || m_letters_to_animate[m_letters_to_animate.Count - 1] != index_of_last_visible_letter)
					{
						letters[index_of_last_visible_letter].m_progression_variables.SetLetterValue(progression_idx);
						m_letters_to_animate.Add(index_of_last_visible_letter);
					}
				}
				
			}
			else
			{
				int progression_idx = 0;
				int progression_idx_inc_white_space = 0;
				LetterSetup last_letter = null;

				m_num_white_space_chars_to_include = 0;

				int numWhiteSpacesSinceLastCustomLetter = 0;

				for(int letter_idx=0; letter_idx < num_letters; letter_idx++)
				{
					if(letters[letter_idx].VisibleCharacter && !letters[letter_idx].StubInstance)
					{
						if (m_letters_to_animate.Contains (letter_idx))
						{
							letters [letter_idx].m_progression_variables.SetLetterValue (progression_idx, progression_idx_inc_white_space);

							progression_idx++;
							progression_idx_inc_white_space++;

							numWhiteSpacesSinceLastCustomLetter = 0;
						}
					}
					else
					{
						letters[letter_idx].m_progression_variables.SetLetterValue(-1);

						if(last_letter != null && last_letter.VisibleCharacter && progression_idx > 0)
						{
							progression_idx_inc_white_space++;

							m_num_white_space_chars_to_include++;
							numWhiteSpacesSinceLastCustomLetter++;
						}
					}

					last_letter = letters[letter_idx];
				}

				m_num_white_space_chars_to_include -= numWhiteSpacesSinceLastCustomLetter;
			}
		}

		public void RefreshDefaultTextColour(LetterSetup[] letters)
		{
			CalculateLettersToAnimate(letters);
			
			// Set values for default text colour progression
			VertexColour[] defaultextColours = new VertexColour[m_letters_to_animate.Count];
			
			for(int idx = 0; idx < m_letters_to_animate.Count; idx++)
			{
				int letterIdx = m_letters_to_animate [idx];

				if (letterIdx >= letters.Length)
					break;

				defaultextColours [idx] = letters[letterIdx].BaseColour;
			}
			
			m_defaultTextColourProgression.SetValues(defaultextColours);
			
			// Mark as the default colour progression
			m_defaultTextColourProgression.SetReferenceData(-1, ANIMATION_DATA_TYPE.COLOUR, true);
		}


		public void PrepareData(TextFxAnimationManager anim_manager, LetterSetup[] letters, ANIMATION_DATA_TYPE what_to_update, int num_words, int num_lines, AnimatePerOptions animate_per)
		{
			if(letters == null || letters.Length == 0)
			{
				return;
			}

			if(m_letters_to_animate == null || what_to_update == ANIMATION_DATA_TYPE.ALL || what_to_update == ANIMATION_DATA_TYPE.ANIMATE_ON)
			{
				CalculateLettersToAnimate(letters);
			}

			if(what_to_update == ANIMATION_DATA_TYPE.ALL || what_to_update == ANIMATION_DATA_TYPE.COLOUR || m_defaultTextColourProgression == null)
			{
				// Set values for default text colour progression
				VertexColour[] defaultextColours = new VertexColour[m_letters_to_animate.Count];

				for(int idx = 0; idx < m_letters_to_animate.Count; idx++)
				{
					int letterIdx = m_letters_to_animate [idx];

					if (letterIdx >= letters.Length)
						break;
					
					defaultextColours [idx] = letters[letterIdx].BaseColour;
				}

				m_defaultTextColourProgression.SetValues(defaultextColours);
				
				// Mark as the default colour progression
				m_defaultTextColourProgression.SetReferenceData(-1, ANIMATION_DATA_TYPE.COLOUR, true);
			}

			// Prepare progression data in all actions
			LetterAction letter_action;
			LetterAction prev_action = null;
			bool prev_action_end_state = true;
			for(int action_idx = 0; action_idx < m_letter_actions.Count; action_idx ++)
			{
				letter_action = m_letter_actions[action_idx];

				letter_action.PrepareData(anim_manager, ref letters, this, action_idx, what_to_update, m_letters_to_animate.Count, m_num_white_space_chars_to_include, num_words, num_lines, prev_action, animate_per, m_defaultTextColourProgression, prev_action_end_state);
				
				if(letter_action.m_action_type == ACTION_TYPE.ANIM_SEQUENCE)
				{
					// Set default previous action settings
					prev_action_end_state = true;
					prev_action = letter_action;
				}
				
				// Check for reverse loops, and how the animation should progress from there
				foreach(ActionLoopCycle loop_cycle in m_loop_cycles)
				{
					if(loop_cycle.m_end_action_idx == action_idx && loop_cycle.m_loop_type == LOOP_TYPE.LOOP_REVERSE && !loop_cycle.m_finish_at_end)
					{
						prev_action = m_letter_actions[loop_cycle.m_start_action_idx];
						prev_action_end_state = false;
					}
				}
			}
		}

		public tfxJSONValue ExportDataAsPresetSection(bool saveSampleTextData = true)
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			if(m_loop_cycles.Count > 0)
			{
				tfxJSONArray loops_data = new tfxJSONArray();
				
				foreach(ActionLoopCycle action_loop in m_loop_cycles)
				{
					loops_data.Add(action_loop.ExportData());
				}
				
				json_data["LOOPS_DATA"] = loops_data;
			}
			
			tfxJSONArray actions_data = new tfxJSONArray();
			foreach(LetterAction action in m_letter_actions)
			{
				actions_data.Add(action.ExportData());
			}
			json_data["ACTIONS_DATA"] = actions_data;

			if(saveSampleTextData)
				json_data["SAMPLE_NUM_LETTERS_ANIMATED"] = m_letters_to_animate.Count;

			return new tfxJSONValue(json_data);
		}

		public void ImportPresetSectionData(tfxJSONObject json_data, LetterSetup[] letters, string assetNameSuffix = "")
		{
			m_letter_actions = new List<LetterAction>();
			m_loop_cycles = new List<ActionLoopCycle>();

			ImportPresetSectionData (json_data, letters, 0, 0, assetNameSuffix);
		}

		public void ImportPresetSectionData(tfxJSONObject json_data, LetterSetup[] letters, int action_insert_index, int loop_insert_index, string assetNameSuffix = "")
		{
			int num_actions = 0, num_loops = 0;
			ImportPresetSectionData (json_data, letters, action_insert_index, loop_insert_index, ref num_actions, ref num_loops, assetNameSuffix);
		}

		public void ImportPresetSectionData(tfxJSONObject json_data, LetterSetup[] letters, int action_insert_index, int loop_insert_index, ref int num_actions_added, ref int num_loops_added, string assetNameSuffix = "")
		{
			if(m_letter_actions == null)
				m_letter_actions = new List<LetterAction>();

			float timing_scale = -1;

			if(m_letters_to_animate == null || m_letters_to_animate.Count == 0)
			{
				CalculateLettersToAnimate(letters);
			}

			if(json_data.ContainsKey("SAMPLE_NUM_LETTERS_ANIMATED") && m_letters_to_animate != null && m_letters_to_animate.Count > 0)
			{
				timing_scale = m_letters_to_animate.Count / ((float) json_data["SAMPLE_NUM_LETTERS_ANIMATED"].Number);
			}


			LetterAction letter_action;
			num_actions_added = 0;
			foreach(tfxJSONValue action_data in json_data["ACTIONS_DATA"].Array)
			{
				letter_action = new LetterAction();
				letter_action.ImportData(action_data.Obj, assetNameSuffix, timing_scale: timing_scale);

				if(num_actions_added == 0 && action_insert_index > 0)
				{
					// Inserting new actions into the middle of the animation. Set first action to continue from last
					letter_action.m_offset_from_last = true;
				}

				InsertAction(action_insert_index + num_actions_added, letter_action);

				num_actions_added++;
			}


			if (m_loop_cycles == null)
				m_loop_cycles = new List<ActionLoopCycle>();


			num_loops_added = 0;

			if(json_data.ContainsKey("LOOPS_DATA"))
			{
				ActionLoopCycle loop_cycle;
				
				foreach(tfxJSONValue loop_data in json_data["LOOPS_DATA"].Array)
				{
					loop_cycle = new ActionLoopCycle();
					loop_cycle.ImportData(loop_data.Obj);
					loop_cycle.m_start_action_idx += action_insert_index;
					loop_cycle.m_end_action_idx += action_insert_index;

					// Check for invalid loops
					if(loop_cycle.m_start_action_idx < m_letter_actions.Count && loop_cycle.m_end_action_idx < m_letter_actions.Count)
					{
						m_loop_cycles.Insert(loop_insert_index + num_loops_added, loop_cycle);
						
						num_loops_added++;
					}
				}
			}
		}

		public tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			json_data["m_letters_to_animate"] = m_letters_to_animate.ExportData();
			json_data["m_letters_to_animate_custom_idx"] = m_letters_to_animate_custom_idx;
			json_data["m_letters_to_animate_option"] = (int) m_letters_to_animate_option;
			
			if(m_loop_cycles.Count > 0)
			{
				tfxJSONArray loops_data = new tfxJSONArray();
				
				foreach(ActionLoopCycle action_loop in m_loop_cycles)
				{
					loops_data.Add(action_loop.ExportData());
				}
				
				json_data["LOOPS_DATA"] = loops_data;
			}
			
			tfxJSONArray actions_data = new tfxJSONArray();
			foreach(LetterAction action in m_letter_actions)
			{
				actions_data.Add(action.ExportData());
			}
			json_data["ACTIONS_DATA"] = actions_data;
			
			return new tfxJSONValue(json_data);
		}

		public void ImportData(tfxJSONObject json_data, string assetNameSuffix = "")
		{
			
			m_letters_to_animate = json_data["m_letters_to_animate"].Array.JSONtoListInt();
			m_letters_to_animate_custom_idx = (int) json_data["m_letters_to_animate_custom_idx"].Number;
			m_letters_to_animate_option = (LETTERS_TO_ANIMATE) (int) json_data["m_letters_to_animate_option"].Number;
			
			m_letter_actions = new List<LetterAction>();
			LetterAction letter_action;
			foreach(tfxJSONValue action_data in json_data["ACTIONS_DATA"].Array)
			{
				letter_action = new LetterAction();
				letter_action.ImportData(action_data.Obj, assetNameSuffix);
				m_letter_actions.Add(letter_action);
			}

			m_loop_cycles = new List<ActionLoopCycle>();
			if(json_data.ContainsKey("LOOPS_DATA"))
			{
				ActionLoopCycle loop_cycle;
				
				foreach(tfxJSONValue loop_data in json_data["LOOPS_DATA"].Array)
				{
					loop_cycle = new ActionLoopCycle();
					loop_cycle.ImportData(loop_data.Obj);
					
					// Check for invalid loops
					if(loop_cycle.m_start_action_idx < m_letter_actions.Count && loop_cycle.m_end_action_idx < m_letter_actions.Count)
					{
						m_loop_cycles.Add(loop_cycle);
					}
					
				}
			}
		}

		public void UpdateLoopCyclesAfterIndex(int index_of_change, int offset_amount)
		{
			// Check for any existing loop setups which reference actions that have now been pushed further back in the animation
			foreach(ActionLoopCycle loop_cycle in m_loop_cycles)
			{
				if(loop_cycle.m_start_action_idx >= index_of_change)
				{
					loop_cycle.m_start_action_idx += offset_amount;
					loop_cycle.m_end_action_idx += offset_amount;
				}
			}
		}
	}
}