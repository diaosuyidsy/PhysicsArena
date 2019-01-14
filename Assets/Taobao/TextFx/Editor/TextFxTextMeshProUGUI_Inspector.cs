#if TMP
using UnityEngine;
using UnityEditor;
using System.Collections;
using TMPro.EditorUtilities;

namespace TextFx
{
	[CustomEditor(typeof(TextFxTextMeshProUGUI)), CanEditMultipleObjects]
	public class TextFxTextMeshProUGUI_Inspector : TMP_UiEditorPanel {

		TextFxTextMeshProUGUI tmpEffect;
		TextFxAnimationManager animationManager;

		new void OnEnable()
		{
			tmpEffect = (TextFxTextMeshProUGUI) target;
			animationManager = tmpEffect.AnimationManager;

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
			TextFxBaseInspector.UpdateManager (animationManager);
		}

		public override void OnInspectorGUI ()
		{
			// Draw TextFx inspector section
			TextFxBaseInspector.DrawTextFxInspectorSection(this, animationManager, ()=> {
				RefreshTextCurveData();
			});

			// Draw default inspector content
			base.OnInspectorGUI();
		}

		new void OnSceneGUI()
		{
			base.OnSceneGUI ();

			if (tmpEffect.RenderToCurve && tmpEffect.BezierCurve.EditorVisible)
			{
				tmpEffect.OnSceneGUIBezier (animationManager.Transform.position, animationManager.Scale * animationManager.AnimationInterface.MovementScale);

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
			tmpEffect.ForceUpdateCachedVertData();


			tmpEffect.UpdateTextFxMesh();
		}
	}
}
#endif