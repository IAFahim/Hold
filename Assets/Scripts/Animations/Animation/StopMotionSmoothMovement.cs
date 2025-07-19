using System;
using UnityEngine;

namespace Animations.Animation
{
    public class StopMotionSmoothMovement : MonoBehaviour
    {
        public FakeStopMotion FSM;

        private void OnValidate()
        {
            if (!FSM) FSM = GetComponent<FakeStopMotion>();
        }

        private void OnAnimatorMove()
        {
            transform.position += FSM.velocity * Time.deltaTime;
        }
    }
}