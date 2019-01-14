#if PLAYMAKER
using UnityEngine;
using TextFx;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Animation)]
	[Tooltip("Sets the text of a TextFx animation.")]
	public class TextFxSetText : FsmStateAction
	{
        [RequiredField, Title("TextFx Object")]
        [ObjectType(typeof(GameObject))]
        public FsmObject effect;

        public FsmString text;

		public override void Reset()
		{
			effect = null;
		}

		public override void OnEnter()
		{
			DoPlayEffect();
			Finish();
		}


		void DoPlayEffect()
		{
            if (effect.Value == null)
            {
                LogWarning("Missing a TextFx component reference!");
                return;
            }

            TextFxAnimationInterface textEffect = (effect.Value as GameObject).GetComponent(typeof(TextFxAnimationInterface)) as TextFxAnimationInterface;

            if (textEffect == null)
            {
                LogWarning("No TextFx component found!");
                return;
            }

            // Set the animations text
            textEffect.SetText(text.Value);
		}


	}
}
#endif
