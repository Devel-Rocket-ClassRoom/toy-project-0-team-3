# CLAUDE.md

이 파일은 이 저장소에서 코드 작업 시 Claude Code(claude.ai/code)에 지침을 제공합니다.

## 프로젝트 개요

Unity 6000.3.11f1 기반의 던전 크롤러 액션 RPG입니다. C# 스크립트는 `Assets/Scripts/`에 위치합니다. Universal Render Pipeline, 카메라용 Cinemachine, 입력 처리를 위한 Unity Input System, 경로 탐색을 위한 NavMesh, CSV 파일로 게임 데이터를 로드하는 CsvHelper를 사용합니다.

## 명령어

**코드 포맷팅:**
```
dotnet csharpier Assets/Scripts
```

**빌드 및 테스트 실행:** Unity 에디터를 통해 수행 — CLI 빌드 스크립트 없음.  
**테스트 파일:** `Assets/Scripts/Csv/DataTableManagerTest.cs`, `Assets/Scripts/UI/Dungeon/Inventory/ItemSlotListTest.cs`

## 아키텍처

### 모듈 구조 (`Assets/Scripts/`)

| 폴더 | 역할 |
|------|------|
| `CombatSystem/` | 공유 데미지/체력 베이스 클래스 및 상태 효과 |
| `Player/` | 플레이어 입력, 이동, 공격 콤보, 스킬, 인벤토리 |
| `Monster/` | 적 AI 상태 머신 및 보스 구현 |
| `Map/` | 절차적 생성, 스폰, 상자/포탈 로직 |
| `UI/` | 상점, 인벤토리 슬롯, 체력/마나 표시, 타이틀 화면 |
| `Csv/` | CSV 데이터 테이블을 로드하는 싱글톤 `DataTableManager` |
| `SceneManager/` | 씬 로딩 헬퍼 및 메뉴 UI |

### 핵심 상속 구조

```
LivingEntity (CombatSystem)
├── PlayerStatus (Player)
└── BaseMonster (Monster)
```

`LivingEntity`는 `Health`, `OnDamage()`, `OnDead` 이벤트, 무적 로직을 소유합니다. 데미지를 받는 모든 객체는 `IDamagable`을 구현합니다.

### 플레이어 시스템

- **PlayerInput** — Unity Input System 액션을 읽고, 형제 컴포넌트가 소비하는 불리언/축을 노출
- **PlayerMovement** — 입력 기반으로 `Rigidbody`에 속도 적용; `PlayerRotation`을 통해 회전 처리
- **PlayerAttack** — 3히트 콤보; 애니메이터 구동 및 스윙마다 `HitBox` 콜라이더 생성
- **PlayerSkill** — Q/W/E/R에 매핑된 4개 스킬; 각 슬롯은 마나 비용과 쿨다운을 가짐; `PlayerStatus`에서 마나 읽기
- **PlayerInventory** / **PlayerInteractive** — 인벤토리 관리 및 상자/포탈 상호작용 (키: `Tab` / `F`)

### 몬스터 AI 상태 머신

모든 몬스터는 `BaseMonster`를 상속하며 다음을 구현합니다:  
**대기(Idle) → 경계(Alert) → 추적(Trace) → 공격(Attack) → 복귀(Return) → 사망(Dead)**

- `SimpleMonster` — 구형 범위 감지
- `SensoryMonster` — 시야각 감지
- `AnubisMonster`, `DragonBoss`, `Mushroom`, `RockMonster` — 공격 행동을 재정의하는 구체적인 보스/적

경로 탐색은 Unity `NavMeshAgent`를 사용합니다.

### 맵 생성

`RandomMapGenerator`가 타일 그리드를 생성하고, `MonsterRandomSpawner`와 `RandomItemGenerator`가 맵 생성 후 적과 전리품을 배치합니다. `ExitPortal`은 `SceneLoader`를 통해 씬 전환을 트리거합니다.

### 데이터 테이블

`DataTableManager`(싱글톤)는 CsvHelper를 사용하여 `Assets/Resources/DataTables/`에서 CSV 파일을 읽고 타입이 지정된 테이블(예: `ItemTable`)을 노출합니다. `DataTableManager.Instance.GetTable<T>()`를 통해 접근합니다.

### 상태 효과

화상(Burn), 감전(Electric), 동상(Frostbite)은 `StatusEffectData`에서 추적되며 `LivingEntity`의 코루틴을 통해 적용됩니다. 각 효과는 틱 데미지와 지속 시간을 가집니다.

## 주요 제약 사항

- `.meta`, `.unity`, `.csproj`, `.slnx` 파일은 수동으로 편집하면 안 됩니다 — Unity가 관리합니다. `.claude/settings.json`이 도구 수준에서 이 파일들에 대한 읽기/쓰기를 차단합니다.
- `Library/`, `Temp/`, `Logs/`, `obj/` 디렉토리는 생성된 파일입니다; `.claude/settings.json`이 해당 디렉토리에 대한 도구 접근을 차단합니다.
- 코드에는 영어와 한국어 주석이 혼재할 수 있습니다.
