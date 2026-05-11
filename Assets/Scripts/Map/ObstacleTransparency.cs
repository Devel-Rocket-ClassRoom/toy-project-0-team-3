using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

// 카메라와 플레이어 사이에 위치한 장애물을 감지하여 반투명하게 처리하는 컴포넌트.
// SphereCast를 사용하여 장애물을 감지하고, 장애물 머티리얼을 복사하여 알파값을 보간(Lerp)하는 방식으로 페이드 효과를 구현한다.
// 장애물이 더 이상 시야를 막지 않으면 원본 머티리얼로 복원하고 추적 목록에서 제거한다.

public class ObstacleTransparecncy : MonoBehaviour
{
    //플레이어의 Transform. 카메라 ~ 플레이어 방향 계산에 사용된다.
    [Header("References")]
    public Transform player;
    public Camera mainCamera;

    [Header("Settings")]
    //장애물로 인식할 레이어 마스크. 이 레이어에 속한 오브젝트만 투명화 대상이 된다.
    public LayerMask obstacleLayer;
    public Material fadeMaterial;
    public float fadedAlpha = 0.25f;
    public float fadeSpeed = 5f;

    //장애물이 시야를 막을 때 적용할 목표 알파값 (0 ~ 1). 낮을수록 더 투명하다.    public float fadedAlpha = 0.25f;
    //알파값을 목표치로 보간하는 속도. 값이 클수록 빠르게 전환된다.

    // 현재 페이드 처리 중인 Renderer와 그 상태를 매핑하는 딕셔너리.
    // 장애물이 시야를 막기 시작하면 등록되고, 완전히 불투명하게 복원되면 제거된다.
    private Dictionary<Renderer, RendererState> _trackedRenderers = new();

    // 각 Renderer의 페이드 상태를 추적하기 위한 내부 데이터 클래스.
    private class RendererState
    {
        //오브젝트의 원본 머티리얼 배열. 페이드 복원 시 사용된다.ummary>
        public Material[] originalMaterials;

        //투명화 처리를 위해 원본에서 복사한 머티리얼 배열. 이 배열의 알파값을 조작한다.
        public Material[] fadeMaterials;

        //현재 프레임에서 보간할 목표 알파값 (fadedAlpha 또는 1f).
        public float targetAlpha;

        //이번 프레임에 SphereCast에 의해 시야를 막고 있는지 여부.
        public bool isBlocking;
    }

    // 매 프레임 장애물 감지와 페이드 갱신을 순서대로 실행한다.
    private void Update()
    {
        DetectBlockingObject();
        UpdateFade();
    }

    // 카메라에서 플레이어 방향으로 SphereCast를 쏘아 시야를 막는 장애물을 감지한다.
    // 감지된 Renderer는 <see cref="_trackedRenderers"/>에 등록되며, 목표 알파값이 설정된다.
    // 이번 프레임에 감지되지 않은 Renderer는 목표 알파값을 1(불투명)로 되돌린다.
    void DetectBlockingObject()
    {
        if (player == null)
            return;

        foreach (var state in _trackedRenderers.Values)
        {
            state.isBlocking = false;
        }
        Vector3 camPos = mainCamera.transform.position;
        Vector3 dir = player.position - camPos;
        float dist = dir.magnitude;

        RaycastHit[] hits = Physics.SphereCastAll(
            camPos,
            0.3f,
            dir.normalized,
            dist,
            obstacleLayer
        );

        foreach (var hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null)
                continue;
            // Fadeble => 오타지만 유니티 내부에서도 Fadeble
            if (!hit.collider.CompareTag("Fadeble"))
            {
                continue;
            }

            if (!_trackedRenderers.ContainsKey(rend))
            {
                RegisterRenderer(rend);
            }

            _trackedRenderers[rend].isBlocking = true;
            _trackedRenderers[rend].targetAlpha = fadedAlpha;
        }

        foreach (var state in _trackedRenderers.Values)
        {
            if (!state.isBlocking)
            {
                state.targetAlpha = 1f;
            }
        }
    }

    // 새로 감지된 Renderer를 추적 목록에 등록한다.
    // 원본 머티리얼을 저장하고, 투명화를 위한 머티리얼 복사본을 생성하여 Renderer에 적용한다.
    void RegisterRenderer(Renderer rend)
    {
        var state = new RendererState();
        state.originalMaterials = rend.sharedMaterials;
        state.fadeMaterials = new Material[rend.sharedMaterials.Length];

        for (int i = 0; i < rend.sharedMaterials.Length; i++)
        {
            state.fadeMaterials[i] = new Material(fadeMaterial);
            Color c = state.fadeMaterials[i].color;
            state.fadeMaterials[i].color = new Color(c.r, c.g, c.b, 1f);
        }

        state.targetAlpha = fadedAlpha;
        state.isBlocking = true;
        _trackedRenderers[rend] = state;

        rend.materials = state.fadeMaterials;
    }

    // 추적 중인 모든 Renderer의 알파값을 목표치로 보간하여 페이드 효과를 갱신한다.
    // 완전히 불투명하게 복원된 Renderer는 원본 머티리얼로 교체하고, 복사본을 파괴한 뒤 추적 목록에서 제거한다.
    // null이 된 Renderer도 함께 정리한다.
    void UpdateFade()
    {
        List<Renderer> toRemove = new();

        foreach (var (rend, state) in _trackedRenderers)
        {
            if (rend == null)
            {
                toRemove.Add(rend);
                continue;
            }

            bool fullyOpaque = true;
            foreach (var mat in state.fadeMaterials)
            {
                Color c = mat.color;
                float newAlpha = Mathf.Lerp(c.a, state.targetAlpha, Time.deltaTime * fadeSpeed);
                mat.color = new Color(c.r, c.g, c.b, newAlpha);
                if (Mathf.Abs(newAlpha - 1f) > 0.01f)
                    fullyOpaque = false;
            }

            if (fullyOpaque && !state.isBlocking)
            {
                rend.materials = state.originalMaterials;
                foreach (var mat in state.fadeMaterials)
                {
                    Destroy(mat);
                }
                toRemove.Add(rend);
            }
        }
        foreach (var rend in toRemove)
        {
            _trackedRenderers.Remove(rend);
        }
    }
}
