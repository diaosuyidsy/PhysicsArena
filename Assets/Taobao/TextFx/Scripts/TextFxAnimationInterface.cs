using UnityEngine;

namespace TextFx
{
	public interface TextFxAnimationInterface
	{
		TextFxAnimationManager AnimationManager { get; }

		int LayerOverride { get; }			// A layer index to be applied to any externally used elements (particle systems for instance)
		float MovementScale { get; }		// A scaling factor to use on all positional movements to normalise with rest of assets
		string AssetNameSuffix { get; }     // A suffix used to find specific asset alternative versions.
		TEXTFX_IMPLEMENTATION TextFxImplementation { get; }	// Returns the type of textFx implementation it is, so that some implementation-specific actions can be taken
		TextAlignment TextAlignment { get; }	// Returns the text-alignment setting for each UI implementation
		bool FlippedMeshVerts { get; }			// Is the ordering of the letter mesh verts flipped to how TextFx is expecting them?

		System.Action OnMeshUpdateCall { get; set; }

		GameObject GameObject { get; }

		UnityEngine.Object ObjectInstance { get; }

		bool CurvePositioningEnabled { get; }

		bool RenderToCurve { get; set; }

		TextFxBezierCurve BezierCurve { get; set; }			// Used to position the text on if specified

#if UNITY_EDITOR
		void DrawBezierInspector();
		void OnSceneGUIBezier(Vector3 position_offset, Vector3 scale);
#endif

		// Call to redraw the mesh with the provided mesh vertex positions and colours
		void UpdateTextFxMesh();

		// A guaranteed method for updating the text for gui renderer
		void SetText(string text);

		// A uniform method for updating the text colour using the UI renderering system base text colour
		void SetColour(Color colour);
		
		// Returns the number of verts used for the current rendered text
		int NumMeshVerts { get; }

		// Access to a specified vert from the current state of the rendered text
		Vector3 GetMeshVert(int index);

		// Access to a specified vert colour from the current state of the rendered text
		Color GetMeshColour(int index);
	}
}