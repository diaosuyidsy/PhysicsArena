#if TMP
using UnityEngine;
using UnityEditor;
using System.Collections;
using TMPro.EditorUtilities;

namespace TextFx
{
	[CustomEditor(typeof(TextFxTextMeshPro)), CanEditMultipleObjects]
	public class TextFxTextMeshPro_Inspector : TMP_EditorPanel {

		TextFxTextMeshPro tmpEffect;
		TextFxTextMeshPro TMPEffect
		{
			get {
				if (tmpEffect == null)
				{
					tmpEffect = (TextFxTextMeshPro)target;
				}
				return tmpEffect;
			}
		}

		TextFxAnimationManager animationManager;
		TextFxAnimationManager AnimationManager
		{
			get {
				if (animationManager == null)
				{
					animationManager = TMPEffect.AnimationManager;
				}
				return animationManager;
			}
		}

		new void OnEnable()
		{
			EditorApplication.update += UpdateManager;

			base.OnEnable ();
		}

		new void OnDisable()
		{
			EditorApplication.update -= UpdateManager;

			base.OnDisable ();
		}

		void UpdateManager()
		{
			TextFxBaseInspector.UpdateManager (AnimationManager);
		}

		public override void OnInspectorGUI ()
		{
			// Draw TextFx inspector section
			TextFxBaseInspector.DrawTextFxInspectorSection(this, AnimationManager, ()=> {
				RefreshTextCurveData();
			});

			// Draw default TMP inspector content
			base.OnInspectorGUI();
		}

		new void OnSceneGUI()
		{
			base.OnSceneGUI ();

			if (TMPEffect.RenderToCurve && TMPEffect.BezierCurve.EditorVisible)
			{
				TMPEffect.OnSceneGUIBezier (AnimationManager.Transform.position, AnimationManager.Scale * AnimationManager.AnimationInterface.MovementScale);

				if (GUI.changed)
				{
					RefreshTextCurveData ();
				}
			}
		}

		void RefreshTextCurveData()
		{
			AnimationManager.CheckCurveData ();

			// Update mesh values to latest using new curve offset values
			TMPEffect.ForceUpdateCachedVertData();

			TMPEffect.UpdateTextFxMesh();
		}
	}
}
#endif