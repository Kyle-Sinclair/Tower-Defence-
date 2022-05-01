using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : GameBehavior {
    public EnemyFactory originFactory;
    public EnemyFactory OriginFactory
    {

        get => originFactory;
        set {
            Debug.Assert(originFactory == null, "Redefined origin factory");
            originFactory = value;
        }
    }
    GameTile tileFrom, tileTo;
    Vector3 positionFrom, positionTo;
    [SerializeField]
    Transform model = default;
    Direction direction;
    DirectionChange directionChange;
    float directionAngleFrom, directionAngleTo;
    float pathOffset;
    float speed;
    float Health { get; set; }
    public float Scale { get; private set; }
    [SerializeField]
    EnemyAnimationConfig animationConfig = default;
    EnemyAnimator animator;
    Collider targetPointCollider;
    public bool IsValidTarget => animator.CurrentClip == EnemyAnimator.Clip.Move;

    public Collider TargetPointCollider
    {
        set {
            Debug.Assert(targetPointCollider == null, "Redefined collider!");
            targetPointCollider = value;
        }
    }
    float progress, progressFactor;
    public override void Recycle() {
        animator.Stop();
        originFactory.Reclaim(this);
    }

    public void SpawnOn(GameTile tile) {
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        PrepareIntro();
        progress = 0f;
    }

    void Awake() {
            animator.Configure(
                model.GetChild(0).gameObject.AddComponent<Animator>(),
                animationConfig
            );
       
    }
    
    public void Initialize(float scale, float speed, float offset, float health) {
        Scale = scale;
        Health = 100f * scale;

        model.localScale = new Vector3(scale, scale, scale);
        this.speed = speed;
        pathOffset = offset;
        Health = health;
        animator.PlayIntro();
        targetPointCollider.enabled = false;
    }

    void OnDestroy() {
        animator.Destroy();
    }
    public override bool GameUpdate() {
        animator.GameUpdate();
#if UNITY_EDITOR
		if (!animator.IsValid) {
			animator.RestoreAfterHotReload(
				model.GetChild(0).GetComponent<Animator>(),
				animationConfig,
				animationConfig.MoveAnimationSpeed * speed / Scale
			);
		}
#endif
        if (animator.CurrentClip == EnemyAnimator.Clip.Intro) {
            if (!animator.IsDone) {
                return true;
            }
            animator.PlayMove(animationConfig.MoveAnimationSpeed * speed / Scale);
            targetPointCollider.enabled = true;

        }
        else if (animator.CurrentClip >= EnemyAnimator.Clip.Outro) {
            if (animator.IsDone) {
                Recycle();
                return false;
            }
            return true;
        }
        if (Health <= 0f) {
            animator.PlayDying();
            targetPointCollider.enabled = false;

            return true;
        }
        progress += Time.deltaTime * progressFactor;
        while (progress >= 1f) {
          
            if (tileTo == null) {
                Game.EnemyReachedDestination();

                animator.PlayOutro();
                targetPointCollider.enabled = false;

                return true;
            }
            progress = (progress - 1f) / progressFactor;
            PrepareNextState();
            progress *= progressFactor;
        }
        if (directionChange == DirectionChange.None) {

            transform.localPosition =
            Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        }
        else{
            float angle = Mathf.LerpUnclamped(
                directionAngleFrom, directionAngleTo, progress
            );
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }

        return true;
    }
    public void ApplyDamage(float damage) {
        Debug.Assert(damage >= 0f, "Negative damage applied");
        Health -= damage;
    }
    void PrepareNextState() {
        tileFrom = tileTo;
        tileTo = tileTo.NextTileOnPath;
        positionFrom = positionTo;
        if (tileTo == null) {
            PrepareOutro();
            return;
        }
        positionTo = tileFrom.ExitPoint;
        directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
        direction = tileFrom.PathDirection;
        directionAngleFrom = directionAngleTo;
        switch (directionChange) {
            case DirectionChange.None: PrepareForward(); break;
            case DirectionChange.TurnRight: PrepareTurnRight(); break;
            case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
            default: PrepareTurnAround(); break;
        }
    }
    void PrepareForward() {
        transform.localRotation = direction.GetRotation();
        directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
        progressFactor = speed;

    }
    void PrepareTurnRight() {
        directionAngleTo = directionAngleFrom + 90f;
        model.localPosition = new Vector3(pathOffset -0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));

    }
    void PrepareTurnLeft() {
        directionAngleTo = directionAngleFrom - 90f;
        model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));

    }
    void PrepareTurnAround() {
        directionAngleTo = directionAngleFrom + (pathOffset < 0f ? 180f : -180f);
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localPosition = positionFrom;
        progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));


    }
    void PrepareIntro() {
        positionFrom = tileFrom.transform.localPosition;
        transform.localPosition = positionFrom;

        positionTo = tileFrom.ExitPoint;
        direction = tileFrom.PathDirection;
        directionChange = DirectionChange.None;
        directionAngleFrom = directionAngleTo = direction.GetAngle();
        transform.localRotation = direction.GetRotation();
        model.localPosition = new Vector3(pathOffset, 0f);
        progressFactor = 2f * speed;

    }
    void PrepareOutro() {
        positionTo = tileFrom.transform.localPosition;
        directionChange = DirectionChange.None;
        directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f * speed;
    }
}
