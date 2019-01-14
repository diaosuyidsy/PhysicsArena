#if PLAYMAKER
using UnityEngine;
using TextFx;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Animation)]
	[Tooltip("Plays a TextFx animation component.")]
	public class TextFxPlayAnimation : FsmStateAction
	{
		[RequiredField, Title("TextFx Object")]
		[ObjectType(typeof(GameObject))]
		public FsmObject effect;

        public FsmFloat delay = 0f;
		public AnimationTime timeUsed = AnimationTime.GAME_TIME;
		public ON_FINISH_ACTION onFinishAction = ON_FINISH_ACTION.NONE;

		public override void Reset()
		{
            effect = null;
			delay = 0;
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

            // Set the onFinishAction
            textEffect.AnimationManager.m_on_finish_action = onFinishAction;

            // Set which time-system to use
            textEffect.AnimationManager.m_time_type = timeUsed;

            // Play text animation
            textEffect.AnimationManager.PlayAnimation(delay.Value);
		}
	}
}
#endif
