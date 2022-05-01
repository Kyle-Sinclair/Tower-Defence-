using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyAnimationConfig : ScriptableObject {
    [SerializeField]
    AnimationClip move = default, intro = default, outro = default, dying = default;
    [SerializeField]
    AnimationClip appear = default, disappear = default;
    [SerializeField]
    float moveAnimationSpeed = 1f;
    public float MoveAnimationSpeed => moveAnimationSpeed;

    public AnimationClip Move => move;
    public AnimationClip Intro => intro;
    public AnimationClip Outro => outro;
    public AnimationClip Dying => dying;
    public AnimationClip Appear => appear;
    public AnimationClip Disappear => disappear;


}
