#if NGUI
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace TextFx
{
	[CustomEditor(typeof(TextFxNGUI))]
	public class TextFxNGUI_Inspector : UILabelInspector
	{
		TextFxNGUI nguiEffect;
		TextFxAnimationManager animationManager;

		new void OnEnable()
		{
			nguiEffect = (TextFxNGUI) target;
			animationManager = nguiEffect.AnimationManager;

			EditorApplication.update += UpdateManager;

			base.OnEnable ();
		}

		new void OnDisable()
		{
			EditorApplication.update -= UpdateManager;

			base.OnEnable ();
		}

		void UpdateManager()
		{
			TextFxBaseInspector.UpdateManager (animationManager);
		}

		public override void OnInspectorGUI ()
		{
			// Draw TextFx inspector section
			TextFxBaseInspector.DrawTextFxInspectorSection(this, animationManager, ()=> {
				RefreshTextCurveData();
			});

			// Draw default NGUI inspector content
			base.OnInspectorGUI();


		}

		new void OnSceneGUI()
		{
			base.OnSceneGUI ();

			if (nguiEffect.RenderToCurve && nguiEffect.BezierCurve.EditorVisible)
			{
				nguiEffect.OnSceneGUIBezier(animationManager.Transform.position, animationManager.Scale * animationManager.AnimationInterface.MovementScale);

				if (GUI.changed)
				{
					RefreshTextCurveData ();
				}
			}
		}

		void RefreshTextCurveData()
		{
			animationManager.CheckCurveData ();

			// Update mesh values to latest using new curve offset values
			nguiEffect.ForceUpdateCachedVertData();


			nguiEffect.UpdateTextFxMesh();
		}
	}
}
#endif