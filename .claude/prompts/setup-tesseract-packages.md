# Tesseract Studio 공용 패키지 설정 가이드

## 개요
Tesseract Studio에서 만든 Unity 공용 라이브러리 패키지를 프로젝트에 추가하는 방법입니다.
이 패키지들은 GitHub에 호스팅되며, Unity Package Manager(UPM) git URL로 설치합니다.

## 패키지 목록

### 1. com.tesseract.core (v1.1.0)
- **GitHub**: https://github.com/happyone7/com.tesseract.core
- **기능**:
  - `Singleton<T>` - 스레드 안전한 MonoBehaviour 싱글톤 (`HasInstance`로 안전한 null 체크 가능)
  - `GetOrAddComponent<T>()` - GameObject 확장 메서드
  - `GetComponentNoGarbage<T>()` - TryGetComponent 기반 GC-free 컴포넌트 조회
  - `GetRandom<T>()`, `SelectByWeight<T>()` - 컬렉션 확장 메서드 (가중치 랜덤 선택)
  - `GetRandomPointInCircle()`, `GetMirrorPosition()` - 수학 유틸리티
  - `WeightRandom()`, `FormatTime()`, `ToHex()` - 기타 유틸리티
  - `CustomDictionary<TKey, TValue>` - Unity Inspector에서 직렬화 가능한 Dictionary
  - `HideOnTime` - 일정 시간 후 자동 비활성화 컴포넌트
- **네임스페이스**: `Tesseract.Core`
- **어셈블리**: `Tesseract.Core` (autoReferenced: true → Assembly-CSharp에서 자동 참조)

### 2. com.tesseract.objectpool (v1.1.0)
- **GitHub**: https://github.com/happyone7/com.tesseract.objectpool
- **의존성**: `com.tesseract.core >= 1.1.0`
- **기능**:
  - `Pool` - Stack 기반 오브젝트 풀 (자동 확장)
  - `PoolManager` - 싱글톤 풀 매니저 (프리팹 키 + 이름 키 지원)
  - `Poolable` - 풀링 대상 오브젝트 기본 클래스 (`TryPool()`, `TryGetPoolable()`)
  - `ObjectController` - 키 기반 오브젝트 레지스트리
  - `DestroyByTime` / `DestroyByTimeSystem` - 시간 기반 자동 풀 반환 (GC-free)
  - `JobSystemBase` / `ComponentJobSystemBase` - 경량 Non-MonoBehaviour 업데이트 시스템
- **네임스페이스**: `Tesseract.ObjectPool`
- **어셈블리**: `Tesseract.ObjectPool` (autoReferenced: true)

### 3. com.tesseract.audio (v1.1.0)
- **GitHub**: https://github.com/happyone7/com.tesseract.audio
- **의존성**: `com.tesseract.core >= 1.1.0`
- **기능**:
  - `AudioManager` - 싱글톤 오디오 매니저 (BGM/SFX/UI 채널 관리, AudioMixer 연동)
  - `AudioMixerConfig` - ScriptableObject 기반 AudioMixer 설정 (볼륨 파라미터 매핑)
  - `FadingAudioSource` - 코루틴 기반 페이드 인/아웃 오디오 소스
  - `WeightedAudioList` - 가중치 기반 랜덤 사운드 재생
  - `SoundData` / `SoundDataSO` - 사운드 데이터 구조체 및 ScriptableObject
  - `AudioSourcePool` - AudioSource 오브젝트 풀링
  - `ESoundType` - MASTER, BGM, GameSFX, UI_SFX, Extra 채널
  - 통합 API: `SetVolume(ESoundType, float)` / `GetVolume(ESoundType)`
- **네임스페이스**: `Tesseract.Audio`
- **어셈블리**: `Tesseract.Audio` (autoReferenced: true)

### 4. com.tesseract.ui (v1.1.0)
- **GitHub**: https://github.com/happyone7/com.tesseract.ui
- **의존성**: `com.tesseract.core >= 1.1.0`, `Unity.TextMeshPro`
- **기능**:
  - `UIPanel` - Show/Hide/Toggle 패널 (Animator 지원, 콜백)
  - `MovableUI` - 드래그 가능한 UI 패널 (화면 클램핑 옵션)
  - `SelectableSlot<T>` - 제네릭 선택 가능 슬롯 (스프라이트/텍스트/오브젝트 시각 상태)
  - `SelectableGroup<T>` - 단일 선택 그룹 관리자
- **네임스페이스**: `Tesseract.UI`
- **어셈블리**: `Tesseract.UI` (autoReferenced: true)

### 5. com.tesseract.save (v1.1.0)
- **GitHub**: https://github.com/happyone7/com.tesseract.save
- **의존성**: 없음 (독립 패키지)
- **기능**:
  - `SaveManager<T>` - 제네릭 비동기/동기 저장 매니저 (롤링 백업, 원자적 파일 쓰기, 지수 백오프 재시도)
  - `ISaveSerializer` - 플러그인 가능한 직렬화 인터페이스
  - `JsonUtilitySerializer` - Unity JsonUtility 기본 구현체
  - `SafeFileManager` - ConcurrentDictionary 기반 파일별 잠금 관리
- **네임스페이스**: `Tesseract.Save`
- **어셈블리**: `Tesseract.Save` (autoReferenced: true)

### 6. com.tesseract.qa (v1.0.0)
- **GitHub**: https://github.com/happyone7/com.tesseract.qa
- **의존성**: 없음 (독립 패키지)
- **기능**:
  - `QaManager` - 싱글톤 QA 커맨드 매니저 (문자열/키코드/버튼 커맨드)
  - `StringCommand<T1..T4>` - 리플렉션 기반 파라미터 파싱 (int, float, string, bool)
  - `KeyCodeCommand` - 키보드 단축키 커맨드
  - `ButtonCommand` - UI 버튼 커맨드
  - `QaCommandContainer` - 커맨드 그룹핑 추상 컨테이너
- **네임스페이스**: `Tesseract.QA`
- **어셈블리**: `Tesseract.QA` (autoReferenced: true)

### 7. com.tesseract.cache (v1.0.0)
- **GitHub**: https://github.com/happyone7/com.tesseract.cache
- **의존성**: 없음 (독립 패키지)
- **기능**:
  - `ResourceCache<T>` - 제네릭 LRU 캐시 (설정 가능한 최대 크기)
  - `ResourceCacheExtensions` - 문자열 확장 메서드 (`LoadPrefab`, `LoadIcon`, `LoadAudioClip`, `InstantiatePrefab`)
  - 자동 퇴거(eviction) 정책
- **네임스페이스**: `Tesseract.Cache`
- **어셈블리**: `Tesseract.Cache` (autoReferenced: true)

### 8. com.tesseract.tooltip (v1.0.0)
- **GitHub**: https://github.com/happyone7/com.tesseract.tooltip
- **의존성**: `com.tesseract.core >= 1.1.0`, `Unity.TextMeshPro`
- **기능**:
  - `TooltipView` - TMP 텍스트, 자동 크기 조절, CanvasGroup 페이드
  - `TooltipManager` - 마우스 추적, 고정 위치, 슬롯 기반 포지셔닝, 화면 경계 클램핑
  - `TooltipTrigger` - 포인터 호버 시 자동 툴팁 표시 컴포넌트
- **네임스페이스**: `Tesseract.Tooltip`
- **어셈블리**: `Tesseract.Tooltip` (autoReferenced: true)

### 9. com.tesseract.toast (v1.0.0)
- **GitHub**: https://github.com/happyone7/com.tesseract.toast
- **의존성**: `com.tesseract.core >= 1.1.0`, `Unity.TextMeshPro`
- **기능**:
  - `ToastView` - AnimationCurve 기반 페이드, 자동 숨김, 설정 가능한 딜레이/지속시간
  - `ToastManager` - Show/Hide API, `OnToastShown` 이벤트 (사운드 연동용)
  - 코루틴 기반 (외부 의존성 없음), `Time.unscaledDeltaTime` 사용 (일시정지 안전)
- **네임스페이스**: `Tesseract.Toast`
- **어셈블리**: `Tesseract.Toast` (autoReferenced: true)

## 설치 방법

### 방법 1: manifest.json 직접 편집 (권장)
`Packages/manifest.json`의 `dependencies`에 추가:

```json
{
  "dependencies": {
    "com.tesseract.core": "https://github.com/happyone7/com.tesseract.core.git#v1.1.0",
    "com.tesseract.objectpool": "https://github.com/happyone7/com.tesseract.objectpool.git#v1.1.0",
    "com.tesseract.audio": "https://github.com/happyone7/com.tesseract.audio.git#v1.1.0",
    "com.tesseract.ui": "https://github.com/happyone7/com.tesseract.ui.git#v1.1.0",
    "com.tesseract.save": "https://github.com/happyone7/com.tesseract.save.git#v1.1.0",
    "com.tesseract.qa": "https://github.com/happyone7/com.tesseract.qa.git#v1.0.0",
    "com.tesseract.cache": "https://github.com/happyone7/com.tesseract.cache.git#v1.0.0",
    "com.tesseract.tooltip": "https://github.com/happyone7/com.tesseract.tooltip.git#v1.0.0",
    "com.tesseract.toast": "https://github.com/happyone7/com.tesseract.toast.git#v1.0.0",
    ...기존 패키지들...
  }
}
```

**의존성 주의사항**:
- `com.tesseract.core` 필수: objectpool, audio, ui, tooltip, toast
- 독립 패키지 (단독 사용 가능): save, qa, cache
- 필요한 패키지만 선택적으로 설치 가능

### 방법 2: Unity Editor에서 추가
1. Window > Package Manager 열기
2. '+' 버튼 > "Add package from git URL..."
3. 먼저: `https://github.com/happyone7/com.tesseract.core.git#v1.1.0` (다른 패키지의 의존성)
4. 필요한 패키지 추가:
   - `https://github.com/happyone7/com.tesseract.objectpool.git#v1.1.0`
   - `https://github.com/happyone7/com.tesseract.audio.git#v1.1.0`
   - `https://github.com/happyone7/com.tesseract.ui.git#v1.1.0`
   - `https://github.com/happyone7/com.tesseract.save.git#v1.1.0`
   - `https://github.com/happyone7/com.tesseract.qa.git#v1.0.0`
   - `https://github.com/happyone7/com.tesseract.cache.git#v1.0.0`
   - `https://github.com/happyone7/com.tesseract.tooltip.git#v1.0.0`
   - `https://github.com/happyone7/com.tesseract.toast.git#v1.0.0`

## 사용 예시

### Singleton 사용
```csharp
using Tesseract.Core;

public class GameManager : Singleton<GameManager>
{
    protected override void Awake()
    {
        base.Awake(); // 반드시 호출
        // 초기화 로직
    }
}

// 접근
GameManager.Instance.DoSomething();

// 안전한 null 체크 (Instance 호출 없이, 자동 생성 방지)
if (Singleton<GameManager>.HasInstance)
{
    GameManager.Instance.DoSomething();
}
```

### Object Pool 사용
```csharp
using Tesseract.ObjectPool;

// 1. 프리팹에 Poolable 컴포넌트 부착
// 2. 시간 기반 자동 반환이 필요하면 DestroyByTime도 부착

// 풀에서 가져오기 (풀이 없으면 자동 생성)
GameObject bullet = Poolable.TryGetPoolable(bulletPrefab, transform);

// 풀에 반환 (Poolable이 없으면 Destroy로 폴백)
Poolable.TryPool(bullet);

// 이름 기반 풀 사용
GameObject effect = Poolable.TryGetPoolable(effectPrefab, null, "VFX_Explosion");
```

### 컬렉션 확장 메서드
```csharp
using Tesseract.Core;

// 랜덤 요소
var randomItem = myList.GetRandom();

// 가중치 랜덤 선택
var selected = items.SelectByWeight(totalWeight, item => item.weight);

// GC-free 컴포넌트 조회
var rb = gameObject.GetComponentNoGarbage<Rigidbody>();
var comp = gameObject.GetOrAddComponent<MyComponent>();
```

### CustomDictionary (Inspector 직렬화)
```csharp
using Tesseract.Core;

public class ItemDatabase : MonoBehaviour
{
    [SerializeField]
    private CustomDictionary<string, int> itemPrices = new CustomDictionary<string, int>();
}
```

### AudioManager 사용
```csharp
using Tesseract.Audio;

// 1. AudioMixerConfig ScriptableObject 생성 (Create > Tesseract > Audio > AudioMixerConfig)
// 2. AudioMixer 파라미터 이름 매핑 설정
// 3. AudioManager를 씬에 배치하고 config 할당

// BGM 재생
AudioManager.Instance.PlayBGM(bgmClip);

// SFX 재생
AudioManager.Instance.PlaySfx(sfxClip);

// 볼륨 설정 (0~1 범위, 내부적으로 로그 dB 변환)
AudioManager.Instance.SetVolume(ESoundType.BGM, 0.8f);
float vol = AudioManager.Instance.GetVolume(ESoundType.BGM);

// 가중치 랜덤 사운드
WeightedAudioList hitSounds; // Inspector에서 설정
AudioClip clip = hitSounds.GetRandomClip();
```

### UIPanel 사용
```csharp
using Tesseract.UI;

public class SettingsPanel : UIPanel
{
    public void OpenSettings()
    {
        Show(); // Animator가 있으면 애니메이션 재생
    }

    public void CloseSettings()
    {
        Hide();
    }
}

// 콜백 등록
panel.OnShowStarted += () => Debug.Log("패널 열림");
panel.OnHideCompleted += () => Debug.Log("패널 닫힘");
```

### SelectableGroup 사용
```csharp
using Tesseract.UI;

// SelectableGroup<T>로 탭/슬롯 단일 선택 관리
SelectableGroup<ItemData> itemGroup;

// 슬롯 추가 (자동으로 단일 선택 관리)
itemGroup.AddSlot(slot);

// 선택 변경 콜백
itemGroup.OnSelectionChanged += (selectedSlot) => {
    Debug.Log($"선택됨: {selectedSlot.data.name}");
};
```

### SaveManager 사용
```csharp
using Tesseract.Save;

// 1. 저장 데이터 클래스 정의
[System.Serializable]
public class PlayerSaveData
{
    public int level = 1;
    public float playTime = 0f;
    public List<string> unlockedItems = new List<string>();
}

// 2. SaveManager 초기화
var saveManager = new SaveManager<PlayerSaveData>(
    "player_save",              // 파일명
    new JsonUtilitySerializer() // 직렬화 방식
);

// 3. 비동기 저장/로드
await saveManager.SaveAsync(playerData);
PlayerSaveData loaded = await saveManager.LoadAsync();

// 4. 동기 저장/로드
saveManager.Save(playerData);
PlayerSaveData loaded2 = saveManager.Load();

// 롤링 백업이 자동으로 관리됨 (기본 3개)
```

## 버전 관리

패키지를 최신 버전으로 업데이트하려면 manifest.json의 태그를 변경:
```json
"com.tesseract.core": "https://github.com/happyone7/com.tesseract.core.git#v1.2.0"
```

최신 main 브랜치를 사용하려면 (개발용):
```json
"com.tesseract.core": "https://github.com/happyone7/com.tesseract.core.git"
```

## 패키지 수정 및 배포 방법

패키지 코드를 수정하고 새 버전을 배포하려면:

```bash
# 1. 패키지 레포 클론
git clone git@github.com:happyone7/com.tesseract.core.git
cd com.tesseract.core

# 2. 코드 수정 후 커밋
git add -A && git commit -m "fix: 수정 내용"

# 3. package.json 버전 업데이트
# "version": "1.2.0"

# 4. 태그 생성 및 push
git tag v1.2.0
git push origin main --tags

# 5. 사용하는 프로젝트의 manifest.json에서 태그 업데이트
# #v1.1.0 → #v1.2.0
```

## Unity 호환성
- **최소 버전**: Unity 6000.0 (Unity 6)
- **테스트 완료**: Unity 6000.1
