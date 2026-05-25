using System.Linq;
using UnityEngine;

namespace Player
{
    public sealed class AnimationService
    {
        private const int BaseLayerIndex = 0;
        private const float AnimationEndThreshold = 1.0f;

        private readonly Animator _animator;
        private readonly int _stateParameterHash;

        public AnimationService(Animator animator)
        {
            _animator = animator;
            _stateParameterHash = Animator.StringToHash("state");
        }

        public void SetState(States state)
        {
            _animator.SetInteger(_stateParameterHash, (int)state);
        }

        public void SetFloat(string parameterName, float value)
        {
            _animator.SetFloat(parameterName, value);
        }

        public bool IsAnimationFinished(string stateName)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(BaseLayerIndex);

            return stateInfo.IsName(stateName) && stateInfo.normalizedTime >= AnimationEndThreshold;
        }

        public float GetAnimationLength(string stateName)
        {
            AnimationClip clip = _animator.runtimeAnimatorController.animationClips
                .FirstOrDefault(clip => clip.name == stateName);

            return clip?.length ?? 0f;
        }
    }
}