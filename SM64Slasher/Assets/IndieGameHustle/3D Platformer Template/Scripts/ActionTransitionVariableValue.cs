namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Variables;

    [AddComponentMenu("")]
	public class ActionTransitionVariableValue : IAction
	{
        [VariableFilter(Variable.DataType.Number)]
        public VariableProperty variable = new VariableProperty(Variable.VarType.GlobalVariable);

        [Space]
        public float duration = 1f;
        public NumberProperty target = new NumberProperty(5.0f);

        private bool forceStop = false;

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
        {
            this.forceStop = false;
            float initialValue = (float)this.variable.Get(target);
            float targetValue = this.target.GetValue(target);

            float startTime = Time.time;
            WaitUntil waitUntil = new WaitUntil(() =>
            {
                float t = (Time.time - startTime) / this.duration;
                float value = Mathf.Lerp(initialValue, targetValue, t);
                this.variable.Set(value, target);

                return t >= 1f || forceStop;
            });

            yield return waitUntil;
            yield return 0;
        }

        public override void Stop()
        {
            base.Stop();
            this.forceStop = true;
        }

        #if UNITY_EDITOR
        public static new string NAME = "Variable/Transition Variable Value";
		#endif
	}
}
