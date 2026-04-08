# 🔒 Prison Life — Unity 모바일 게임 프로토타입

> **Prison Life**를 레퍼런스로 제작한 3D 탑다운 모바일 게임 프로토타입입니다.  
> 채광 → 수갑 제조 → 죄수 납품 → 수익 → 해금의 풀 프로덕션 루프를 5일 일정으로 구현했습니다.

---

## 📌 프로젝트 개요

| 항목 | 내용 |
|---|---|
| 장르 | 3D 탑다운 모바일 방치형 시뮬레이션 |
| 엔진 | Unity (URP) |
| 개발 기간 | 5일 (프로토타입) |
| 플랫폼 | PC (Windows) |
> 모바일(Android/iOS) 환경을 기준으로 설계되었으나,  
> 빌드 환경 문제로 PC(Windows) 빌드로 전환하여 제출합니다.  
> Floating Joystick 입력 / 카메라 기준 방향 변환 등 모바일 UX 설계는 코드 레벨에서 유지되어 있습니다.
| 개발 인원 | 1인 |

---

## 🎮 핵심 게임플레이 루프

```
광물 채광
    ↓
광물존 투입 → 수갑 기계 자동 생산
    ↓
수갑존 픽업 → 카운터 납품
    ↓
죄수 수갑 구매 → 머니존 지불
    ↓
플레이어 수익 획득 → 해금존 구매
    ↓
드릴 / 광부 / 캐셔 / 감옥 순차 해금
    ↓
게임 클리어
```

---

## 🛠 구현 기능

### 플레이어 시스템
- **Floating Joystick** 기반 모바일 터치 입력 (Joystick Pack — fenerax studios)
- **카메라 기준 방향 변환** 이동 — 카메라 각도에 무관하게 정확한 방향 조작
- **채광 모드 3종** — 곡괭이(딜레이 채광) / 드릴(즉시 채광) / 드릴 불도저(범위 즉시 채광)
- **Unity Primitive 도구 비주얼** — 채광 시에만 활성화, 곡괭이 스윙·드릴 진동 연출 코드 구현
- **퍼즈 시스템** — 게임 시작 및 유휴 감지 시 조작 가이드 UI 표시

### 오브젝트 풀링
- 제네릭 `ObjectPool<T>` 클래스 단일 구현으로 광물 / 수갑 / FloatingText 재사용
- `OreSpawner` — 채광된 광물 일정 시간 비활성화 후 원위치 재활성화

### 생산 파이프라인
- `OreZone` — 진입 후 `consumeInterval`마다 광물 1개씩 소모 (딜레이 지불 구조)
- `HandcuffMachine` — 광물 1개당 `productionTimePerOre` 후 수갑 1개 생산
- `HandcuffZone` — 생산된 수갑 3D 스택으로 적재, 플레이어/캐셔 픽업 분리 처리
- `Counter` — 카운터 수갑 재고 중간 버퍼 역할, `consumeInterval`마다 죄수에게 1개씩 소모

### 죄수 AI 시스템
- **NavMesh Agent** 기반 이동 — 스폰 포인트 → 대기 슬롯 → 카운터 → 감옥
- `PrisonerQueue` — 일정 시간마다 자동 스폰, 최대 대기 수 초과 시 중단
- **World Space 말풍선 UI** — 앞 죄수만 활성화, 충족률에 따라 색상 변화 (빨강→주황→초록→골드)
- `PrisonerData` ScriptableObject — 수요 범위 / 보상 금액 데이터 분리 관리
- 랜덤 수갑 요구 수량 (`demandMin` ~ `demandMax`)

### 자동화 NPC
- **Miner (광부)** — Transform 직접 이동, 웨이포인트 순환 순찰, OreObject 접촉 시 자동 채광 → OreZone 즉시 전달
- **Cashier (캐셔)** — NavMesh Agent 기반 HandcuffZone → Counter 왕복 자동 운반, 수갑 없을 시 대기

### 해금 시스템
- `UnlockZone` 베이스 클래스 상속 구조 — DrillZone / DrillBulldozerZone / MinerZone / CashierZone / PrisonZone
- **딜레이 지불** — 진입 중 `payInterval`마다 금액 차감, 이탈 시 중단
- **Plane + World Space Canvas UI** — 바닥에 눕힌 Fill Image 게이지, 아이콘/텍스트 오버레이
- `UnlockManager` 싱글톤 — 전체 해금 조건 감지 및 순차 활성화
- `WaitForSecondsRealtime` 전면 적용 — `TimeScale = 0` 퍼즈 상태에서도 정상 동작

### 경제 시스템
- `PlayerWallet` — 플레이어 보유 금액 단일 관리
- `MoneyZone` — 죄수 지불 금액을 3D 오브젝트로 적재, 픽업 시 플레이어 등 뒤 스택 비주얼 생성
- `PlayerMoneyStack` — 등 뒤 돈 오브젝트 스택, 지불 시 연동 제거

### 감옥 시스템
- `PrisonManager` — 죄수 수용 카운트 관리, 최대 수용 시 퍼즈 + 카메라 연출 + 감옥존 해금
- `PrisonCell` — 해금 전 비활성, 해금 시 `SetActive(true)`로 침대/공간 등장
- `PrisonPrisoner` — Transform 직접 이동, 대기 → 셀 배정 → 침대 이동 → 회전 보간으로 눕기 연출

### 카메라 시스템
- `CameraFollow` — `SmoothDamp` 기반 플레이어 추적, `LateUpdate` 처리
- `CameraController` — POI 이동 연출 전담, position + rotation 동시 `SmoothStep` 보간
- 연출 중 `CameraFollow` 자동 비활성 → 복귀 후 재활성

### UI / UX
- `FloatingText` — World Space TMP, 위로 떠오르며 페이드 아웃, ObjectPool 재사용
- `UIManager` — 조작 가이드 페이드 인/아웃, `Time.unscaledDeltaTime` 처리
- `UnlockNoticeUI` — 해금 알림 팝업 페이드 연출
- `GameClearUI` — TitleImage / GameIconImage 바운스 등장 / ContinueBtn 순차 활성화
- `DemandBubbleUI` — Fill Image 수직 채우기, 충족률 구간별 색상 전환

### 튜토리얼 시스템
- `TutorialManager` — 6단계 순차 진행, 조건 충족 시 자동 전환
- `TutorialArrow` — 목적지까지 거리 기준으로 **방향 지시 모드** ↔ **목적지 위 바운스 모드** 자동 전환
  - 원거리: 플레이어 앞에 고정, 목적지 방향 가리킴
  - 근거리: 목적지 위 바운스, 카메라 빌보드
- 1단계 가장 가까운 활성 광물 실시간 자동 탐색

### 사운드 시스템
- `SoundManager` — AudioSource 풀 기반 동시 재생 지원 (라운드 로빈)
- 8종 효과음 분류 — 곡괭이 / Zone 상호작용 / 수갑 생산 / 돈 픽업 / 해금 / 드릴 / 광물 채광 / 게임 클리어
- `PlayWithRandomPitch` — 반복 채광 효과음 피치 랜덤 변형으로 단조로움 방지

---

## 📁 스크립트 구조

```
Scripts/
├── Input/
│   └── JoystickInputHandler.cs     // Joystick Pack 연동, 최초 입력 감지
├── Player/
│   ├── PlayerMovement.cs           // CharacterController 이동, 카메라 기준 방향 변환
│   ├── PlayerMining.cs             // 채광 모드 3종, 도구 비주얼 연동
│   ├── PlayerCarry.cs              // 수갑 운반, 스택 비주얼
│   ├── PlayerWallet.cs             // 보유 금액 관리, 최초 돈 획득 트리거
│   ├── PlayerMoneyStack.cs         // 등 뒤 돈 스택 비주얼
│   └── PlayerToolVisual.cs         // 도구 Primitive 생성, 스윙/진동 연출
├── Mining/
│   ├── OreObject.cs                // 채광 이벤트, Player/NPC 태그 분기
│   ├── OreSpawner.cs               // 광물 리스폰 관리
│   └── DrillMining.cs              // 불도저 범위 채광 OverlapSphere
├── Pool/
│   └── ObjectPool.cs               // 제네릭 오브젝트 풀
├── Production/
│   ├── HandcuffMachine.cs          // 광물 → 수갑 생산 타이머
│   └── HandcuffObject.cs           // 수갑 오브젝트
├── Zone/
│   ├── OreZone.cs                  // 광물 딜레이 소모, 광부 전달 수신
│   ├── HandcuffZone.cs             // 수갑 3D 스택, 플레이어/캐셔 픽업 분리
│   ├── Counter.cs                  // 카운터 재고 버퍼, 죄수 소모 루틴
│   ├── CounterZone.cs              // 플레이어 납품 트리거
│   ├── MoneyZone.cs                // 돈 3D 적재, 픽업 처리
│   └── PrisonerZone.cs             // (초기 구조)
├── NPC/
│   ├── Prisoner.cs                 // 죄수 상태머신, NavMesh 이동
│   ├── PrisonerQueue.cs            // 대기열 관리, 자동 스폰
│   ├── PrisonerSpawner.cs          // 죄수 ObjectPool 생성
│   ├── Miner.cs                    // 광부 Transform 순찰, 자동 채광
│   ├── MinerManager.cs             // 광부 3명 활성화 관리
│   └── Cashier.cs                  // 캐셔 NavMesh 왕복 운반
├── Prison/
│   ├── PrisonManager.cs            // 수용 카운트, 퍼즈 연출, 클리어 시퀀스
│   ├── PrisonCell.cs               // 침대 슬롯, 해금 시 활성화
│   └── PrisonPrisoner.cs           // Transform 이동, 눕기 회전 보간
├── Unlock/
│   ├── UnlockZone.cs               // 해금존 베이스, 딜레이 지불 루틴
│   ├── UnlockZoneUI.cs             // Plane + Canvas UI, Fill 게이지
│   ├── UnlockManager.cs            // 전체 해금 조건 관리
│   ├── DrillZone.cs                // 드릴 해금, 카메라 연출 트리거
│   ├── DrillBulldozerZone.cs       // 불도저 해금
│   ├── MinerZone.cs                // 광부 해금
│   ├── CashierZone.cs              // 캐셔 해금
│   └── PrisonZone.cs               // 감옥 해금, PrisonManager 연동
├── Camera/
│   ├── CameraFollow.cs             // SmoothDamp 플레이어 추적
│   └── CameraController.cs         // POI 연출, position+rotation 보간
├── Tutorial/
│   ├── TutorialManager.cs          // 6단계 순차 관리, 조건 감지
│   └── TutorialArrow.cs            // 거리 기준 방향/목적지 모드 전환
└── UI/
    ├── UIManager.cs                // 조작 가이드 페이드
    ├── FloatingText.cs             // 월드 팝업 텍스트
    ├── FloatingTextPool.cs         // FloatingText ObjectPool
    ├── DemandBubbleUI.cs           // 죄수 수갑 요구 게이지 UI
    ├── UnlockNoticeUI.cs           // 해금 알림 팝업
    ├── SoundManager.cs             // AudioSource 풀, 8종 SFX 관리
    └── GameClearUI.cs              // 클리어 UI, 아이콘 바운스 연출
```

---

## 🔧 기술 스택 및 설계 포인트

### 아키텍처 패턴

| 패턴 | 적용 위치 |
|---|---|
| 싱글톤 | `UnlockManager`, `PrisonManager`, `SoundManager`, `TutorialManager` 등 |
| 오브젝트 풀링 | `ObjectPool<T>` 제네릭으로 광물/수갑/FloatingText 공용 재사용 |
| 베이스 클래스 상속 | `UnlockZone` → 5종 해금존 공통 로직 분리 |
| 이벤트 기반 통신 | `Action` 델리게이트로 `OreObject.OnMined`, `JoystickInputHandler.OnFirstInput` 등 |
| ScriptableObject | `PrisonerData` — 죄수 수요/보상 데이터 런타임 분리 |

### TimeScale 대응
- `WaitForSecondsRealtime` + `Time.unscaledDeltaTime` 전면 적용
- 감옥 가득 참 퍼즈(`TimeScale = 0`) 중에도 카메라 연출 / UI 페이드 정상 동작

### NavMesh vs Transform 이동 선택
- **NavMesh** — 죄수(Prisoner), 캐셔(Cashier) : 복잡한 경로가 필요한 NPC
- **Transform 직접 이동** — 광부(Miner), 감옥 죄수(PrisonPrisoner) : 단순 경로, NavMesh 장애물 충돌 회피 문제 해결

---

## 🎯 개발 일정

| Day | 주요 구현 |
|---|---|
| Day 1 | 플레이어 이동 / 채광 / 오브젝트 풀링 / 카메라 / UI 기반 |
| Day 2 | 생산 파이프라인 / 죄수 AI / 수갑→카운터→머니 전체 흐름 |
| Day 3 | 경제 시스템 / 해금존 / 드릴 모드 / UnlockManager |
| Day 4 | 광부 / 캐셔 자동화 NPC |
| Day 5 | 감옥 시스템 / 게임 클리어 / 사운드 / 튜토리얼 / 도구 비주얼 / 폴리싱 |

---

## 📦 외부 에셋

| 에셋 | 용도 |
|---|---|
| Joystick Pack (fenerax studios) | 모바일 Floating Joystick 입력 |

---

## 📸 플레이 영상

> *플레이영상 https://youtu.be/MW_HVXQMaSM*

---

## 💡 개발 회고

### 잘 된 점
- **제네릭 ObjectPool** 하나로 여러 오브젝트 타입을 재사용하여 코드 중복 최소화
- **UnlockZone 베이스 클래스** 설계로 5종 해금존을 최소 코드로 확장
- **TimeScale 대응 철저** — 퍼즈 상태에서도 모든 UI/연출이 정상 동작

### 개선 포인트
- 광물 탐색 `FindObjectsByType` → OreSpawner 캐싱 리스트로 최적화 필요
- 모든 사운드가 동일한 볼륨으로 출력되어 공간감과 몰입도가 저하 -> 오브젝트별 Audio Source 적용 필요
- 플레이어 및 NPC 애니메이션 미구현 — 애니메이션 에셋 수급 후 Animator 파라미터 연동 예정
