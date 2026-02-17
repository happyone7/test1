using UnityEngine;

namespace Soulspire.UI
{
    /// <summary>
    /// 다크 판타지 컬러 팔레트 (ArtDirection_v0.1.md 4절 기준)
    /// </summary>
    public static class UIColors
    {
        // ── 배경/환경 ──────────────────────────────────────────
        public static readonly Color DeepBg      = new Color32(0x0A, 0x0A, 0x12, 0xFF); // #0A0A12 최심부 배경
        public static readonly Color MainBg      = new Color32(0x14, 0x14, 0x20, 0xFF); // #141420 기본 배경
        public static readonly Color BrightBg    = new Color32(0x1E, 0x1E, 0x30, 0xFF); // #1E1E30 밝은 배경
        public static readonly Color Stone       = new Color32(0x2A, 0x2A, 0x3A, 0xFF); // #2A2A3A 돌/유적
        public static readonly Color BrightStone = new Color32(0x3A, 0x3A, 0x50, 0xFF); // #3A3A50 밝은 돌

        // ── 마법/발광 ──────────────────────────────────────────
        public static readonly Color MagicWhite    = new Color32(0xE8, 0xE4, 0xF0, 0xFF); // #E8E4F0
        public static readonly Color Gold          = new Color32(0xFF, 0xD8, 0x4D, 0xFF); // #FFD84D
        public static readonly Color RubyRed       = new Color32(0xD4, 0x40, 0x40, 0xFF); // #D44040
        public static readonly Color EmeraldGreen  = new Color32(0x40, 0xD4, 0x70, 0xFF); // #40D470
        public static readonly Color SapphireBlue  = new Color32(0x40, 0x80, 0xD4, 0xFF); // #4080D4
        public static readonly Color MagicPurple   = new Color32(0x90, 0x60, 0xD0, 0xFF); // #9060D0
        public static readonly Color FlameOrange   = new Color32(0xE0, 0x80, 0x30, 0xFF); // #E08030
        public static readonly Color LightningYellow = new Color32(0xE0, 0xD0, 0x40, 0xFF); // #E0D040

        // ── 타워 시그니처 (주색상) ─────────────────────────────
        public static readonly Color TowerArrow     = new Color32(0xC0, 0xA8, 0x70, 0xFF); // #C0A870 황동
        public static readonly Color TowerCannon    = new Color32(0xE0, 0x80, 0x30, 0xFF); // #E08030 화염
        public static readonly Color TowerIce       = new Color32(0x40, 0x80, 0xD4, 0xFF); // #4080D4 사파이어
        public static readonly Color TowerLightning = new Color32(0xE0, 0xD0, 0x40, 0xFF); // #E0D040 번개
        public static readonly Color TowerLaser     = new Color32(0x90, 0x60, 0xD0, 0xFF); // #9060D0 마법 퍼플
        public static readonly Color TowerVoid      = new Color32(0x30, 0x20, 0x50, 0xFF); // #302050 짙은 보라

        // ── 타워 시그니처 (보조색상) ───────────────────────────
        public static readonly Color TowerArrowSub     = new Color32(0xE8, 0xE4, 0xF0, 0xFF); // #E8E4F0
        public static readonly Color TowerCannonSub    = new Color32(0xD4, 0x40, 0x40, 0xFF); // #D44040
        public static readonly Color TowerIceSub       = new Color32(0x80, 0xC0, 0xE0, 0xFF); // #80C0E0
        public static readonly Color TowerLightningSub = new Color32(0xFF, 0xFF, 0xFF, 0xFF); // #FFFFFF
        public static readonly Color TowerLaserSub     = new Color32(0xD0, 0x80, 0xFF, 0xFF); // #D080FF
        public static readonly Color TowerVoidSub      = new Color32(0x60, 0x40, 0xA0, 0xFF); // #6040A0

        // ── UI ─────────────────────────────────────────────────
        public static readonly Color UIBg          = new Color32(0x12, 0x10, 0x1A, 0xFF); // #12101A UI 배경
        public static readonly Color Panel         = new Color32(0x1A, 0x18, 0x28, 0xFF); // #1A1828 UI 패널
        public static readonly Color BrightPanel   = new Color32(0x24, 0x22, 0x36, 0xFF); // #242236 UI 밝은 패널
        public static readonly Color Border        = new Color32(0x5A, 0x50, 0x70, 0xFF); // #5A5070 UI 프레임 기본
        public static readonly Color BorderActive  = new Color32(0xB0, 0xA0, 0x80, 0xFF); // #B0A080 UI 프레임 활성
        public static readonly Color BorderAccent  = new Color32(0xFF, 0xD8, 0x4D, 0xFF); // #FFD84D UI 프레임 강조
        public static readonly Color TextMain      = new Color32(0xE0, 0xDC, 0xD0, 0xFF); // #E0DCD0 텍스트 메인
        public static readonly Color TextSub       = new Color32(0xA0, 0x98, 0x90, 0xFF); // #A09890 텍스트 서브
        public static readonly Color TextDisabled  = new Color32(0x60, 0x58, 0x50, 0xFF); // #605850 텍스트 비활성

        // ── 레거시 호환 별칭 ───────────────────────────────────
        // 기존 코드에서 참조하던 이름을 유지하여 컴파일 에러 방지
        public static readonly Color Yellow       = Gold;            // #FFD84D
        public static readonly Color Orange       = FlameOrange;     // #E08030
        public static readonly Color Red          = RubyRed;         // #D44040
        public static readonly Color NeonGreen    = EmeraldGreen;    // #40D470
        public static readonly Color NeonBlue     = SapphireBlue;    // #4080D4
        public static readonly Color NeonPurple   = MagicPurple;     // #9060D0
        public static readonly Color BtnGreenBg   = new Color32(0x15, 0x30, 0x20, 0xFF); // #153020
        public static readonly Color BtnYellowBg  = new Color32(0x30, 0x28, 0x10, 0xFF); // #302810
    }
}
