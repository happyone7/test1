

That background task was from the Explore agent's internal work and has already been incorporated into the analysis I provided. No additional action needed.

총괄PD님, 위 5개 액션에 대한 승인을 기다리고 있습니다. 어떤 범위까지 진행할까요?
ource 복수 관리 필요
- **게임플레이 코드에 PlaySfx/PlayBgm 호출 연동**: SoundManager와 SoundKeys는 준비되어 있으나, Tower/Projectile/Node/WaveSpawner/GameManager 등 실제 게임 코드에서 아직 호출하고 있는지 확인 및 연동 필요
- **씬에 SoundManager 오브젝트 배치 확인**: Singleton이므로 씬에 인스턴스가 있거나 자동 생성 로직이 있어야 함. GameScene에 배치 여부 검증 필요
- **설정 UI(SettingsPopup)와 볼륨 슬라이더 연결**: `SetBgmVolume`/`SetSfxVolume` API는 있으나 SettingsPopup UI에서 실제 바인딩되어 있는지 확인 필요
- **네임스페이스 정리**: `Nodebreaker.Audio` → 게임명이 Soulspire로 변경됨. 리네이밍 시점 결정 필요 (프로토타입 후반 또는 다음 스프린트)
