# 프로젝트 주요 객체 명세 및 동작 분석 보고서

이 문서는 프로젝트 내에 등장하는 주요 객체들(플레이어 차량, 경찰 차량, 키 아이템, 탈출구, 매니저)의 내부 변수 구성, 데이터 참조 및 작동 방식, 책임 범위와 실행 순서를 상세히 설명합니다.

---

## 1. 객체별 상세 분석 (변수, 참조, 책임 범위)

### A. 플레이어 차량 (Player Vehicle)

플레이어 차량은 차량의 물리 제어, 입력 처리, 체력 관리, 아이템 수집 상태 기록, 그리고 상태별 사운드 재생을 담당하며 여러 컴포넌트가 협력하는 구조입니다.

#### ① CarController (차량 제어 및 물리)
* **책임 범위**: 키보드 입력을 받아 차량의 모터 토크, 조향각, 브레이크를 제어하고, 바퀴 물리(WheelCollider)와 시각적 모델(Transform)을 동기화합니다. 또한, 리스폰 처리 및 적 충돌 무시(GhostMode) 상태를 제어합니다.
* **주요 변수 및 형태**:
  | 변수명 | 타입 (Type) | 참조 및 설명 |
  | :--- | :--- | :--- |
  | `horizontalInput` | `float` (private) | Horizontal 입력 축 값 (-1.0f ~ 1.0f). 좌우 방향 조향 제어에 사용. |
  | `verticalInput` | `float` (private) | Vertical 입력 축 값 (-1.0f ~ 1.0f). 전진 및 후진 가속에 사용. |
  | `currentSteerAngle` | `float` (private) | 현재 조향각. `horizontalInput * maxSteerAngle` 값으로 계산됨. |
  | `currentbreakForce` | `float` (private) | 현재 브레이크 감속 힘. Space 키 입력 여부에 따라 `breakForce` 또는 `0`이 됨. |
  | `isBreaking` | `bool` (private) | Space 키 입력 상태. |
  | `spawnPoints` | `Transform[]` (public) | 차량 리스폰 시 활용할 맵 상의 위치 지점들. |
  | `rb` | `Rigidbody` (private) | 차량의 물리 제어 및 속도 값을 확인하기 위한 컴포넌트. |
  | `IsGhostMode` | `bool` (public get, private set) | 리스폰 직후 무적 및 충돌 무시 상태 여부를 나타내는 프로퍼티. |
  | `ghostTime` | `float` (private, Serialized) | 리스폰 후 고스트 모드 유지 시간 (기본값: 3초). |
  | `enemyCars` | `GameObject[]` (private, Serialized) | 물리적 충돌을 무시할 적(경찰) 차량의 프리팹/오브젝트 배열. |
  | `motorForce` | `float` (private, Serialized) | 차량 가속 힘 세기. |
  | `breakForce` | `float` (private, Serialized) | 차량 제동 힘 세기. |
  | `maxSteerAngle` | `float` (private, Serialized) | 차량 최대 조향 회전 각도. |
  | `frontLeftWheelCollider` <br> `frontRightWheelCollider` <br> `rearLeftWheelCollider` <br> `rearRightWheelCollider` | `WheelCollider` (private, Serialized) | 실제 물리 시뮬레이션을 수행하는 전륜/후륜 WheelCollider 컴포넌트들. |
  | `frontLeftWheelTransform` <br> `frontRightWheelTransform` <br> `rearLeftWheelTransform` <br> `rearRightWheelTransform` | `Transform` (private, Serialized) | 시각적 연출을 위한 3D 바퀴 메쉬 트랜스폼. |
  | `engineSound` <br> `brakeSound` | `AudioSource` (private, Serialized) | 차량 엔진음 및 브레이크 작동 마찰음 재생 컴포넌트. |
  | `minPitch` / `maxPitch` | `float` (private, Serialized) | 속도에 따라 변동될 엔진 사운드의 피치(Pitch) 범위. |
* **작동 및 참조**:
  - `Input.GetAxis` 및 `Input.GetKey`를 통해 사용자 키보드 입력을 매 프레임 감지합니다.
  - `FixedUpdate`에서 물리 바퀴에 토크(`motorTorque`) 및 조향각(`steerAngle`)을 가하고, `UpdateWheels`를 호출해 `WheelCollider.GetWorldPose` 값을 바퀴 메쉬의 `Transform`에 대입하여 실제 굴러가는 바퀴를 시각화합니다.
  - `UpdateEngineSound()`와 `UpdateBrakeSound()`를 통해 현재 차량의 `Rigidbody.linearVelocity.magnitude`에 맞추어 사운드 피치를 동기화하거나 재생/정지합니다.
  - `R` 키 입력 시 `spawnPoints` 중에서 차량의 현재 위치와 최단거리에 있는 포인트를 계산해 이동시키고, `GhostMode` 코루틴을 통해 3초간 `Physics.IgnoreCollision`을 설정하여 적 콜라이더들과 물리 연산을 무시합니다.

#### ② PlayerHP (플레이어 체력)
* **책임 범위**: 플레이어 차량의 체력 값을 보존 및 갱신하고, 피격 시 데미지 감쇄 및 일시적 무적 처리, 체력 소진 시 게임오버 이벤트를 트리거합니다.
* **주요 변수 및 형태**:
  | 변수명 | 타입 (Type) | 참조 및 설명 |
  | :--- | :--- | :--- |
  | `maxHP` | `int` (private, Serialized) | 플레이어 차량의 최대 체력 (기본값: 100). |
  | `isInvincible` | `bool` (private) | 적 차량과 충돌 후 일시적인 피격 무적 시간 여부. |
  | `invincibleTime` | `float` (private, Serialized) | 피격 시 제공되는 무적 시간 (기본값: 1.0초). |
  | `currentHP` | `int` (private) | 플레이어 차량의 현재 체력. |
* **작동 및 참조**:
  - 적 차량(`EnemyChaser`)과의 충돌 시 `TakeDamage(int damage)`가 호출됩니다.
  - 피격 시 `isInvincible`이 참인 경우 데미지 계산을 무시하고, 거짓이면 `currentHP`를 깎은 뒤 `UIManager.Instance.UpdateHP`를 호출해 UI 화면을 갱신합니다.
  - `currentHP`가 0 이하가 되면 `GameManager.Instance.GameOver()`를 호출해 게임을 끝내고, 0보다 크면 `Invincible` 코루틴을 돌려 일시적 무적 처리를 합니다.

#### ③ ItemCollector (아이템 수집 수집기)
* **책임 범위**: 플레이어 차량에 부착되어 수집한 키 아이템의 개수를 카운트하며 UI에 수집 현황을 텍스트로 표기하고, 목표 개수 달성 시 탈출구(Goal)를 활성화시킵니다.
* **주요 변수 및 형태**:
  | 변수명 | 타입 (Type) | 참조 및 설명 |
  | :--- | :--- | :--- |
  | `goal` | `GameObject` (public) | 탈출구(탈출 지점) 오브젝트. 아이템을 다 모으기 전까진 비활성화(`SetActive(false)`) 상태를 유지. |
  | `totalItemCount` | `int` (public) | 스테이지 내에 존재하는 전체 키 아이템의 개수. |
  | `currentItemCount` | `int` (public) | 플레이어가 획득한 현재 키 아이템 개수. |
  | `itemText` | `TextMeshProUGUI` (public) | 화면에 아이템 획득 진행도를 표시하는 UI 텍스트. |
* **작동 및 참조**:
  - 아이템 오브젝트(`Item`)가 플레이어의 트리거 범위에 충돌하면 `Item` 컴포넌트가 플레이어 트랜스폼에서 `ItemCollector`를 참조하여 `CollectItem()`을 호출합니다.
  - `currentItemCount`를 증가시키고 텍스트를 업데이트하며, 만약 `currentItemCount >= totalItemCount`가 되면 `goal.SetActive(true)`를 통해 탈출구를 활성화시킵니다.

#### ④ PlayerAttack (플레이어 공격 - 2D 테스트용)
* **책임 범위**: 2D 테스트 공간에서 마우스 클릭 시 대상 방향으로 투사체를 날립니다. (현재 3D 차량 시뮬레이션 메인 시스템과는 별개의 2D 스크립트 세트에 해당함)
* **주요 변수 및 형태**:
  | 변수명 | 타입 (Type) | 참조 및 설명 |
  | :--- | :--- | :--- |
  | `projectilePrefab` | `GameObject` (public) | 발사할 투사체 프리팹. |
  | `firePoint` | `Transform` (public) | 투사체가 생성되어 발사되는 시작점 위치. |
  | `enemyTarget` | `Transform` (public) | 투사체가 날아갈 목표(적)의 트랜스폼. |

---

### B. 경찰 차량 / 적 차량 (Police / Enemy Vehicle)

#### ① EnemyChaser (적 추적 AI)
* **책임 범위**: 플레이어 차량을 추적하기 위해 경로를 탐색하고, 플레이어 차량과 물리적 충돌 시 플레이어에게 타격을 가한 뒤 잠시 정지 및 대기합니다.
* **주요 변수 및 형태**:
  | 변수명 | 타입 (Type) | 참조 및 설명 |
  | :--- | :--- | :--- |
  | `player` | `Transform` (public) | 추적 대상인 플레이어 차량의 위치 트랜스폼. |
  | `agent` | `NavMeshAgent` (private) | 인공지능 경로 탐색 및 이동을 위한 내비게이션 에이전트. |
  | `playerCar` | `CarController` (private) | 플레이어가 현재 고스트 모드(리스폰 상태)인지 판단하기 위한 참조 변수. |
  | `isPaused` | `bool` (private) | 충돌 후 추적을 일시 정지하는 딜레이 상태 플래그. |
* **작동 및 참조**:
  - `Update`에서 플레이어 차량 컴포넌트인 `CarController.IsGhostMode`가 참이거나 본인이 일시 정지(`isPaused`) 상태라면 `agent.isStopped = true`로 두고 제자리에 멈춥니다.
  - 두 조건에 해당하지 않고 `NavMesh` 위에 정상 배치되어 있다면 `agent.SetDestination(player.position)`을 호출해 플레이어를 끝까지 쫓아갑니다.
  - 플레이어의 Collider와 물리적 충돌(`OnCollisionEnter`)이 감지되면 플레이어 내부의 `PlayerHP` 컴포넌트를 가져와 `TakeDamage(10)`를 호출해 10만큼 데미지를 입히며, `PauseChase` 코루틴을 통해 3초간 정지 및 대기합니다.

#### ② EnemyHealth (적 체력 - 2D 테스트용)
* **책임 범위**: 적 체력을 유지하고, 투사체 타격을 받았을 때 피격 처리 후 사망 시 스스로를 파괴합니다. (2D용)
* **주요 변수 및 형태**:
  | 변수명 | 타입 (Type) | 참조 및 설명 |
  | :--- | :--- | :--- |
  | `hp` | `int` (public) | 적의 체력 값 (기본값: 100). |

---

### C. 키 아이템 (Key Item)

#### ① Item
* **책임 범위**: 플레이어 차량과의 트리거 충돌을 감지해 수집 이벤트를 알려주고 자신을 씬에서 제거합니다.
* **주요 변수 및 형태**:
  - 별도 저장하는 클래스 변수 없음 (동작 메소드 중심)
* **작동 및 참조**:
  - 플레이어 차량이 아이템의 트리거 존 내로 진입하면 `OnTriggerEnter(Collider other)`가 트리거됩니다.
  - 접촉한 오브젝트(`other`)의 부모에게서 `ItemCollector`를 탐색하고, 수집가 컴포넌트가 발견되면 `collector.CollectItem()`을 호출한 뒤 `Destroy(gameObject)`로 자신을 파괴합니다.

---

### D. 탈출구 (Exit / Goal)

#### ① Goal
* **책임 범위**: 플레이어가 모든 키 아이템을 수집한 뒤 탈출 지점 트리거에 들어왔는지 판정하여 스테이지 클리어 이벤트를 활성화합니다.
* **주요 변수 및 형태**:
  | 변수명 | 타입 (Type) | 참조 및 설명 |
  | :--- | :--- | :--- |
  | `clearPanel` | `GameObject` (public) | 스테이지 성공/클리어 UI 패널 오브젝트. |
* **작동 및 참조**:
  - 플레이어 차량이 탈출구 트리거 영역에 접촉하면 `OnTriggerEnter(Collider other)`가 실행됩니다.
  - 들어온 대상의 `ItemCollector`를 참조하여 모든 아이템이 수집(`IsAllCollected() == true`)되었는지 확인합니다.
  - 모든 조건 충족 시 `clearPanel.SetActive(true)`로 성공 화면을 노출하고, `other`의 부모에서 `CarController`를 찾아 차량의 엔진 및 브레이크 사운드를 완전히 끈 뒤(`StopCarSound()`), `Time.timeScale = 0`으로 변경하여 전체 게임 물리 및 진행을 일시 정지시킵니다.

---

### E. 매니저 (Managers)

#### ① GameManager
* **책임 범위**: 게임 전체 라이프사이클을 조율하며, 게임 오버 패널 활성화, 씬 재시작(Restart) 및 종료(Quit)를 수행합니다.
* **주요 변수 및 형태**:
  | 변수명 | 타입 (Type) | 참조 및 설명 |
  | :--- | :--- | :--- |
  | `Instance` | `GameManager` (public, static) | 어디서든 쉽게 게임 매니저에 접근할 수 있도록 하는 싱글톤 객체. |
  | `gameOverPanel` | `GameObject` (private, Serialized) | 게임 오버(플레이어 사망) 시 화면에 띄울 패널 UI. |
* **작동 및 참조**:
  - `Awake` 시점에 정적 싱글톤인 `Instance`를 선언해 메모리에 상주합니다.
  - 플레이어 체력이 소진될 때 `PlayerHP.GameOver()` 단계에서 `GameManager.Instance.GameOver()`를 호출해 게임을 정지(`Time.timeScale = 0`)시키고 패배 화면을 보여줍니다.
  - UI 상에서 재시작 버튼을 누르면 `Restart()`가 호출되어 시간 속도(`Time.timeScale = 1`)를 되돌린 뒤 현재 활성화된 씬을 다시 로드합니다.

#### ② UIManager
* **책임 범위**: 체력 바(Slider) 및 체력 텍스트를 사용자가 보기 쉽게 화면 상에 갱신하여 표시합니다.
* **주요 변수 및 형태**:
  | 변수명 | 타입 (Type) | 참조 및 설명 |
  | :--- | :--- | :--- |
  | `Instance` | `UIManager` (public, static) | 어디서든 체력 수치를 전달해 UI를 갱신할 수 있게 하는 싱글톤 객체. |
  | `hpSlider` | `Slider` (private, Serialized) | 체력을 백분율/막대바로 시각화할 UI 슬라이더 컴포넌트. |
  | `hpText` | `TMP_Text` (private, Serialized) | 현재 남은 체력을 숫자로 표기할 텍스트 컴포넌트. |
* **작동 및 참조**:
  - 플레이어가 데미지를 받을 때마다 `PlayerHP`에서 `UIManager.Instance.UpdateHP(currentHP, maxHP)`를 호출합니다.
  - 호출된 수치를 받아 슬라이더의 최댓값(`maxValue`), 현재값(`value`)을 각각 대입하고 텍스트에 "HP : 수치" 포맷으로 반영합니다.

---

## 2. 객체들의 동작/상호작용 실행 순서 (Execution Sequence)

게임 내 프레임 및 라이프사이클에 따른 객체들의 연쇄적 작동 순서는 다음과 같이 정의됩니다.

```mermaid
sequenceDiagram
    autonumber
    actor Player as 사용자 (입력)
    participant CC as 플레이어 (CarController)
    participant IC as 플레이어 (ItemCollector)
    participant EC as 경찰 차량 (EnemyChaser)
    participant Item as 키 아이템 (Item)
    participant Goal as 탈출구 (Goal)
    participant PHP as 플레이어 체력 (PlayerHP)
    participant UM as UI 매니저 (UIManager)
    participant GM as 게임 매니저 (GameManager)

    Note over CC, GM: A. 초기화 단계 (Awake & Start)
    GM->>GM: Awake() 싱글톤 인스턴스 등록
    UM->>UM: Awake() 싱글톤 인스턴스 등록
    PHP->>UM: Start() 최초 체력 UI 업데이트 호출 (UpdateHP)
    IC->>Goal: Start() 탈출구 비활성화 (SetActive(false))

    Note over Player, CC: B. 프레임 업데이트 루프 (FixedUpdate & Update)
    Player->>CC: W/S/A/D 키보드 입력
    CC->>CC: GetInput() 입력값 감지 (vertical/horizontal/isBreaking)
    CC->>CC: HandleMotor() & HandleSteering() 바퀴 토크 및 각도 적용
    CC->>CC: UpdateWheels() 바퀴 메쉬 회전/위치 물리 값에 동기화
    CC->>CC: UpdateEngineSound() 속도에 따른 엔진 피치 변조 재생

    Note over EC, CC: C. 적 AI 실시간 추적 루프
    EC->>CC: player.position 참조 및 IsGhostMode 판정
    EC->>EC: NavMeshAgent를 이용해 플레이어 차량 방향으로 경로 탐색 및 이동

    Note over CC, Item: D. 아이템 획득 및 탈출구 개방 단계
    CC->>Item: 트리거 영역 접촉 (OnTriggerEnter)
    Item->>IC: collector.CollectItem() 호출
    IC->>IC: currentItemCount 증가 & UI 갱신
    Item->>Item: Destroy(gameObject) 씬에서 삭제
    Note over IC, Goal: 모든 아이템 획득 시
    IC->>Goal: goal.SetActive(true) 탈출구 오브젝트 활성화

    Note over CC, Goal: E. 최종 탈출 및 클리어 단계
    CC->>Goal: 탈출구 트리거 영역 진입 (OnTriggerEnter)
    Goal->>IC: IsAllCollected() 검사
    Goal->>CC: stopCarSound() 호출 (엔진 및 제동 사운드 오프)
    Goal->>Goal: clearPanel.SetActive(true) 클리어 패널 오픈
    Goal->>GM: Time.timeScale = 0 게임 일시정지

    Note over EC, PHP: F. 경찰 충돌 및 게임오버 단계
    EC->>CC: 물리적 차체 충돌 감지 (OnCollisionEnter)
    EC->>PHP: hp.TakeDamage(10) 호출
    PHP->>UM: UpdateHP() 잔여 체력 UI 반영
    alt 체력이 0 이하인 경우 (사망)
        PHP->>GM: GameOver() 호출
        GM->>GM: gameOverPanel.SetActive(true) & Time.timeScale = 0 정지
    alt 체력이 남은 경우 (피격 후 무적 돌입)
        PHP->>PHP: Invincible() 코루틴 실행 (1초간 무적 설정)
        EC->>EC: PauseChase() 코루틴 실행 (3초간 AI 일시정지)
    end
```

### 상세 단계별 보완 설명
1. **시작 및 초기화 (A)**: `GameManager`와 `UIManager`가 메모리에 고유 싱글톤으로 가장 먼저 할당됩니다. `PlayerHP`는 준비된 최대 체력 정보(100)를 `UIManager`에 쏴주어 HP 바를 가득 채우고, `ItemCollector`는 아직 수집하지 않은 상태이므로 `Goal` 오브젝트를 숨겨놓습니다.
2. **입력 및 이동 (B)**: 플레이어가 가속 키를 누르면 `CarController`가 `WheelCollider`에 토크를 주어 `Rigidbody` 속도를 발생시키고, 이 속도값에 비례하여 엔진음 소리 피치를 조절합니다.
3. **추적 (C)**: 경찰 차량은 플레이어 차량의 `Transform` 위치를 매 프레임 참조하여 목적지로 주입받아 경로 추적을 진행합니다.
4. **아이템 수집 (D)**: 맵 상에 놓인 아이템을 플레이어가 통과하면 `Item` 측에서 플레이어 컴포넌트를 확인해 `CollectItem()`을 트리거하고 본인은 소멸합니다. 수집에 완료되면 `Goal`이 지도 상에 활성화됩니다.
5. **충돌 처리 (F)**: 만약 추격하던 경찰차가 플레이어 차와 충돌하면, 플레이어의 HP를 10 깎고 UI를 갱신합니다. 충돌 직후 서로가 또 연속해서 데미지를 주고 받거나 탈출하지 못하는 현상을 막기 위해, 플레이어는 **1초간 데미지 무적(Invincible)**에 들어가며 적 경찰차는 **3초간 이동 AI가 일시정지(isPaused)** 상태로 고정됩니다.
6. **클리어 또는 패배 (E, F)**: 체력이 완전히 다 닳으면 `GameManager`를 통해 패배 패널이 뜨며 멈춥니다. 반대로 체력이 버텨주는 한도 내에서 아이템을 모두 모으고 탈출구에 도착하면 `Goal`에서 클리어 패널을 띄우고 시간이 정지(timeScale = 0)하며 소리가 전부 꺼집니다.
