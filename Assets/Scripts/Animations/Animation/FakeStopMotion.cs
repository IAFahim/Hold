using System;
using UnityEngine;

namespace Animations.Animation
{
    public class FakeStopMotion : MonoBehaviour
    {
        // public Animator animator;
        // public int fps = 8;
        //
        // private float _time;
        // private bool _grabVelocity;
        //
        // private void OnValidate()
        // {
        //     if (!animator) animator = GetComponent<Animator>();
        // }
        //
        // private void Update()
        // {
        //     if (_grabVelocity)
        //     {
        //         velocity = animator.velocity / animator.speed;
        //         _grabVelocity = false;
        //     }
        //     
        //     _time += Time.deltaTime;
        //     var updateTime = 1f / fps;
        //     animator.speed = 0f;
        //     
        //     if (_time > updateTime)
        //     {
        //         _time -= updateTime;
        //         animator.speed = updateTime / Time.deltaTime;
        //         _grabVelocity = true;
        //     }
        // }
    }
}