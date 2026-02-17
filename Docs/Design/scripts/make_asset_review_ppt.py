#!/usr/bin/env python3
import datetime
import os
import zipfile
from xml.sax.saxutils import escape

OUT_PATH = os.path.join("Docs", "Design", "Soulspire_Asset_Review_v0.2.pptx")

SLIDE_W = 12192000
SLIDE_H = 6858000


def textbox(shape_id, name, x, y, cx, cy, text, size=1800, bold=False, color="FFFFFF", fill=None, line="5B6B8A", align="l"):
    fill_xml = (
        f"<a:solidFill><a:srgbClr val=\"{fill}\"/></a:solidFill>"
        if fill
        else "<a:noFill/>"
    )
    line_xml = (
        f"<a:ln w=\"12700\"><a:solidFill><a:srgbClr val=\"{line}\"/></a:solidFill></a:ln>"
        if line
        else "<a:ln><a:noFill/></a:ln>"
    )
    b = "1" if bold else "0"
    return f"""
<p:sp>
  <p:nvSpPr>
    <p:cNvPr id="{shape_id}" name="{escape(name)}"/>
    <p:cNvSpPr/>
    <p:nvPr/>
  </p:nvSpPr>
  <p:spPr>
    <a:xfrm><a:off x="{x}" y="{y}"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm>
    <a:prstGeom prst="rect"><a:avLst/></a:prstGeom>
    {fill_xml}
    {line_xml}
  </p:spPr>
  <p:txBody>
    <a:bodyPr wrap="square" lIns="91440" tIns="45720" rIns="91440" bIns="45720" anchor="t"/>
    <a:lstStyle/>
    <a:p>
      <a:pPr algn="{align}"/>
      <a:r><a:rPr lang="ko-KR" sz="{size}" b="{b}"><a:solidFill><a:srgbClr val="{color}"/></a:solidFill></a:rPr><a:t>{escape(text)}</a:t></a:r>
      <a:endParaRPr lang="ko-KR"/>
    </a:p>
  </p:txBody>
</p:sp>
"""


def slide_xml(shapes_xml):
    return f"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<p:sld xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
       xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
       xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main">
  <p:cSld>
    <p:spTree>
      <p:nvGrpSpPr>
        <p:cNvPr id="1" name=""/>
        <p:cNvGrpSpPr/>
        <p:nvPr/>
      </p:nvGrpSpPr>
      <p:grpSpPr>
        <a:xfrm>
          <a:off x="0" y="0"/>
          <a:ext cx="0" cy="0"/>
          <a:chOff x="0" y="0"/>
          <a:chExt cx="0" cy="0"/>
        </a:xfrm>
      </p:grpSpPr>
      {shapes_xml}
    </p:spTree>
  </p:cSld>
  <p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr>
</p:sld>
"""


def build_slides():
    slides = []

    # Slide 1
    s = []
    sid = 2
    s.append(textbox(sid, "bg", 0, 0, SLIDE_W, SLIDE_H, "", fill="0B0F1A", line=None))
    sid += 1
    s.append(textbox(sid, "title", 685800, 685800, 10800000, 900000, "Soulspire TD Asset Review", size=4400, bold=True, color="D8E4FF", line=None))
    sid += 1
    s.append(textbox(sid, "sub", 685800, 1600200, 10800000, 900000, "AssetManifest 기반 그래픽/사운드 검토용\n버전: v0.2 / 날짜: 2026-02-14", size=1800, color="AFC3E8", line=None))
    sid += 1
    s.append(textbox(sid, "guide", 685800, 2514600, 10800000, 2743200, "이 PPT에서 확인할 내용\n1) 스타일 방향 고정\n2) 타워/적 실루엣 방향\n3) 스테이지 테마 톤\n4) UI 가독성 기준\n5) SFX/BGM 톤 기준", size=2200, color="FFFFFF", fill="121A2A", line="2BFF88"))
    slides.append(slide_xml("".join(s)))

    # Slide 2: style + palette
    s = []
    sid = 2
    s.append(textbox(sid, "bg", 0, 0, SLIDE_W, SLIDE_H, "", fill="0B0F1A", line=None))
    sid += 1
    s.append(textbox(sid, "title", 457200, 228600, 11200000, 600000, "스타일 방향 & 컬러 검토", size=3200, bold=True, color="D8E4FF", line=None))
    sid += 1
    s.append(textbox(sid, "left", 457200, 914400, 4572000, 2057400, "키워드\n- 2D 탑다운\n- 네온 회로\n- 빠른 피드백\n- 연출 우선\n- 복잡한 디테일\n\n체크\n- 전투 중 식별 속도\n- 배경 대비 오브젝트 명도", size=1700, color="D8E4FF", fill="121A2A", line="5B6B8A"))
    sid += 1
    # palette chips
    chips = [
        ("0B0F1A", "Neutral Dark"),
        ("121A2A", "Panel"),
        ("2BFF88", "Stage Early"),
        ("37B6FF", "Blue Accent"),
        ("FF9A3D", "Orange Accent"),
        ("FF4D5A", "Alert"),
        ("FFD84D", "Rare"),
    ]
    x0 = 5334000
    y0 = 914400
    for i, (hexv, label) in enumerate(chips):
        y = y0 + i * 518160
        txt_color = "111111" if hexv in ("FFD84D", "2BFF88", "37B6FF", "FF9A3D") else "FFFFFF"
        s.append(textbox(sid, f"chip{i}", x0, y, 1828800, 365760, f"{label} #{hexv}", size=1200, color=txt_color, fill=hexv, line="FFFFFF"))
        sid += 1
    s.append(textbox(sid, "ref", 7620000, 914400, 4114800, 3657600, "레퍼런스 이미지 붙여넣기 영역\n(스타일락 후보 4~6장)", size=1800, color="AFC3E8", fill="121A2A", line="37B6FF", align="ctr"))
    slides.append(slide_xml("".join(s)))

    # Slide 3: Towers
    s = []
    sid = 2
    s.append(textbox(sid, "bg", 0, 0, SLIDE_W, SLIDE_H, "", fill="0B0F1A", line=None))
    sid += 1
    s.append(textbox(sid, "title", 457200, 228600, 11200000, 600000, "타워 6종 이미지 검토 보드", size=3200, bold=True, color="D8E4FF", line=None))
    sid += 1
    towers = [
        "T01 Arrow",
        "T02 Cannon",
        "T03 Ice",
        "T04 Lightning",
        "T05 Laser",
        "T06 Void",
    ]
    box_w = 3505200
    box_h = 2148840
    start_x = 457200
    start_y = 914400
    gap_x = 304800
    gap_y = 228600
    for i, t in enumerate(towers):
        row = i // 3
        col = i % 3
        x = start_x + col * (box_w + gap_x)
        y = start_y + row * (box_h + gap_y)
        s.append(textbox(sid, f"tower{i}", x, y, box_w, box_h, f"{t}\n이미지 삽입\n체크: 복잡한 디테일 / 역할 인지", size=1600, color="D8E4FF", fill="121A2A", line="2BFF88", align="ctr"))
        sid += 1
    slides.append(slide_xml("".join(s)))

    # Slide 4: Nodes
    s = []
    sid = 2
    s.append(textbox(sid, "bg", 0, 0, SLIDE_W, SLIDE_H, "", fill="0B0F1A", line=None))
    sid += 1
    s.append(textbox(sid, "title", 457200, 228600, 11200000, 600000, "Node 9종 이미지 검토 보드", size=3200, bold=True, color="D8E4FF", line=None))
    sid += 1
    nodes = ["N01 Bit", "N02 Quick", "N03 Heavy", "N04 Shield", "N05 Swarm", "N06 Regen", "N07 Phase", "N08 Split", "N09 Boss"]
    box_w = 3657600
    box_h = 1508760
    start_x = 304800
    start_y = 914400
    gap_x = 152400
    gap_y = 152400
    for i, n in enumerate(nodes):
        row = i // 3
        col = i % 3
        x = start_x + col * (box_w + gap_x)
        y = start_y + row * (box_h + gap_y)
        border = "FF4D5A" if n == "N09 Boss" else "37B6FF"
        s.append(textbox(sid, f"node{i}", x, y, box_w, box_h, f"{n}\n이미지 삽입", size=1500, color="D8E4FF", fill="121A2A", line=border, align="ctr"))
        sid += 1
    slides.append(slide_xml("".join(s)))

    # Slide 5: Stages
    s = []
    sid = 2
    s.append(textbox(sid, "bg", 0, 0, SLIDE_W, SLIDE_H, "", fill="0B0F1A", line=None))
    sid += 1
    s.append(textbox(sid, "title", 457200, 152400, 11200000, 500000, "스테이지 테마 10종 톤 검토", size=3000, bold=True, color="D8E4FF", line=None))
    sid += 1
    stages = [
        "1 Data Stream", "2 Memory Block", "3 Cache Layer", "4 Pipeline", "5 Processor Core",
        "6 Bus Network", "7 Kernel Space", "8 Overflow Zone", "9 Root Access", "10 Kernel Panic",
    ]
    stage_colors = ["2BFF88", "37B6FF", "B86CFF", "FF9A3D", "FF4D5A", "8BE5FF", "7C4DFF", "9F1E1E", "FFD84D", "FFFFFF"]
    col_w = 5715000
    card_h = 548640
    for i, st in enumerate(stages):
        col = 0 if i < 5 else 1
        row = i if i < 5 else i - 5
        x = 457200 + col * (col_w + 304800)
        y = 762000 + row * 594360
        s.append(textbox(sid, f"st{i}", x, y, col_w, card_h, f"{st}  |  이미지 삽입 / 톤 확인", size=1300, color="D8E4FF", fill="121A2A", line="5B6B8A"))
        sid += 1
        s.append(textbox(sid, f"stchip{i}", x + col_w - 914400, y + 76200, 762000, 381000, "", size=1200, fill=stage_colors[i], line="FFFFFF"))
        sid += 1
    slides.append(slide_xml("".join(s)))

    # Slide 6: UI + VFX
    s = []
    sid = 2
    s.append(textbox(sid, "bg", 0, 0, SLIDE_W, SLIDE_H, "", fill="0B0F1A", line=None))
    sid += 1
    s.append(textbox(sid, "title", 457200, 228600, 11200000, 600000, "UI / VFX 검토 보드", size=3200, bold=True, color="D8E4FF", line=None))
    sid += 1
    sections = [
        ("HUD (Wave / Bit / HP)", 457200, 914400, 5486400, 1371600),
        ("Hub Skill Tree", 457200, 2438400, 5486400, 1981200),
        ("Run Complete / Stage Clear", 457200, 4572000, 5486400, 1828800),
        ("VFX Micro/Small", 6248400, 914400, 5486400, 1371600),
        ("VFX Medium/Large", 6248400, 2438400, 5486400, 1981200),
        ("Boss/Legend FX", 6248400, 4572000, 5486400, 1828800),
    ]
    for title, x, y, w, h in sections:
        s.append(textbox(sid, title, x, y, w, h, f"{title}\n이미지 or GIF 프레임 붙여넣기", size=1500, color="D8E4FF", fill="121A2A", line="37B6FF", align="ctr"))
        sid += 1
    slides.append(slide_xml("".join(s)))

    # Slide 7: Audio
    s = []
    sid = 2
    s.append(textbox(sid, "bg", 0, 0, SLIDE_W, SLIDE_H, "", fill="0B0F1A", line=None))
    sid += 1
    s.append(textbox(sid, "title", 457200, 228600, 11200000, 600000, "사운드 검토 시트", size=3200, bold=True, color="D8E4FF", line=None))
    sid += 1
    s.append(textbox(sid, "sfx", 457200, 914400, 5638800, 2438400, "SFX 체크\n- Tower Fire/Hit (타워별)\n- Node Death (일반/보스)\n- Wave Clear / Stage Clear\n- Upgrade / UI Click / Alert\n\n각 이벤트 3~5변형", size=1600, color="D8E4FF", fill="121A2A", line="2BFF88"))
    sid += 1
    s.append(textbox(sid, "bgm", 6400800, 914400, 5334000, 2438400, "BGM 체크\n- Hub 1트랙\n- Normal / Tension / Boss (3트랙)\n- Lo-Fi 톤 유지\n- 루프 클릭 유무\n- 전투 중 UI 가청성", size=1600, color="D8E4FF", fill="121A2A", line="FF9A3D"))
    sid += 1
    s.append(textbox(sid, "links", 457200, 3505200, 11277600, 2895600, "샘플 링크/파일 경로 붙여넣기\n- SFX Pack A:\n- SFX Pack B:\n- BGM Draft 01:\n- BGM Draft 02:\n\n청취 후 판정: Keep / Revise / Reject", size=1500, color="AFC3E8", fill="121A2A", line="5B6B8A"))
    slides.append(slide_xml("".join(s)))

    # Slide 8: Confirmed decisions
    s = []
    sid = 2
    s.append(textbox(sid, "bg", 0, 0, SLIDE_W, SLIDE_H, "", fill="0B0F1A", line=None))
    sid += 1
    s.append(textbox(sid, "title", 457200, 228600, 11200000, 600000, "의사결정 확정본", size=3200, bold=True, color="D8E4FF", line=None))
    sid += 1
    q = (
        "1) 기준 해상도/PPU: 1920x1080 / PPU 100\n"
        "2) 아트 방향: 연출 우선\n"
        "3) 타워/Node: 복잡한 디테일\n"
        "4) 스테이지 톤: 초반 차분 -> 후반 강렬\n"
        "5) UI: 깔끔/정보 우선\n"
        "6) 사운드 톤: Lo-Fi\n"
        "7) BGM: Normal / Tension / Boss (3트랙)\n"
        "8) 제작 비율: AI 80% / 수작업 20%"
    )
    s.append(textbox(sid, "qs", 457200, 914400, 11277600, 3505200, q, size=1700, color="FFFFFF", fill="121A2A", line="FF4D5A"))
    sid += 1
    s.append(textbox(sid, "decision", 457200, 4572000, 11277600, 1828800, "다음 액션\n- 이미지 시안 붙여넣기\n- Keep/Revise/Reject 표시\n- 타일맵 셀 크기 대비 스프라이트 비율 확정", size=1600, color="D8E4FF", fill="121A2A", line="5B6B8A"))
    slides.append(slide_xml("".join(s)))

    return slides


def content_types(slide_count):
    slide_overrides = "\n".join(
        f'<Override PartName="/ppt/slides/slide{i}.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slide+xml"/>'
        for i in range(1, slide_count + 1)
    )
    return f"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/ppt/presentation.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"/>
  <Override PartName="/ppt/slideMasters/slideMaster1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml"/>
  <Override PartName="/ppt/slideLayouts/slideLayout1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml"/>
  <Override PartName="/ppt/theme/theme1.xml" ContentType="application/vnd.openxmlformats-officedocument.theme+xml"/>
  {slide_overrides}
  <Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>
  <Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>
</Types>
"""


def root_rels():
    return """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="ppt/presentation.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties" Target="docProps/app.xml"/>
</Relationships>
"""


def app_xml(slide_count):
    return f"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties"
            xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes">
  <Application>Codex</Application>
  <Slides>{slide_count}</Slides>
  <Notes>0</Notes>
  <HiddenSlides>0</HiddenSlides>
  <PresentationFormat>On-screen Show (16:9)</PresentationFormat>
  <Company></Company>
  <AppVersion>16.0000</AppVersion>
</Properties>
"""


def core_xml():
    now = datetime.datetime.now(datetime.timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")
    return f"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties"
                   xmlns:dc="http://purl.org/dc/elements/1.1/"
                   xmlns:dcterms="http://purl.org/dc/terms/"
                   xmlns:dcmitype="http://purl.org/dc/dcmitype/"
                   xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <dc:title>Soulspire TD Asset Review</dc:title>
  <dc:creator>Codex</dc:creator>
  <cp:lastModifiedBy>Codex</cp:lastModifiedBy>
  <dcterms:created xsi:type="dcterms:W3CDTF">{now}</dcterms:created>
  <dcterms:modified xsi:type="dcterms:W3CDTF">{now}</dcterms:modified>
</cp:coreProperties>
"""


def presentation_xml(slide_count):
    sld_ids = []
    rel_idx = 2
    for i in range(1, slide_count + 1):
        sld_ids.append(f'<p:sldId id="{255 + i}" r:id="rId{rel_idx}"/>')
        rel_idx += 1
    sld_ids_xml = "\n    ".join(sld_ids)
    return f"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<p:presentation xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main">
  <p:sldMasterIdLst>
    <p:sldMasterId id="2147483648" r:id="rId1"/>
  </p:sldMasterIdLst>
  <p:sldIdLst>
    {sld_ids_xml}
  </p:sldIdLst>
  <p:sldSz cx="{SLIDE_W}" cy="{SLIDE_H}" type="screen16x9"/>
  <p:notesSz cx="6858000" cy="9144000"/>
  <p:defaultTextStyle/>
</p:presentation>
"""


def presentation_rels(slide_count):
    rels = [
        '<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideMaster" Target="slideMasters/slideMaster1.xml"/>'
    ]
    for i in range(1, slide_count + 1):
        rels.append(f'<Relationship Id="rId{i+1}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide{i}.xml"/>')
    return """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  %s
</Relationships>
""" % "\n  ".join(rels)


def slide_rels():
    return """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideLayout" Target="../slideLayouts/slideLayout1.xml"/>
</Relationships>
"""


def slide_layout_xml():
    return """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<p:sldLayout xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
             xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
             xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"
             type="blank" preserve="1">
  <p:cSld name="Blank">
    <p:spTree>
      <p:nvGrpSpPr>
        <p:cNvPr id="1" name=""/>
        <p:cNvGrpSpPr/>
        <p:nvPr/>
      </p:nvGrpSpPr>
      <p:grpSpPr>
        <a:xfrm>
          <a:off x="0" y="0"/>
          <a:ext cx="0" cy="0"/>
          <a:chOff x="0" y="0"/>
          <a:chExt cx="0" cy="0"/>
        </a:xfrm>
      </p:grpSpPr>
    </p:spTree>
  </p:cSld>
  <p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr>
</p:sldLayout>
"""


def slide_layout_rels():
    return """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideMaster" Target="../slideMasters/slideMaster1.xml"/>
</Relationships>
"""


def slide_master_xml():
    return """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<p:sldMaster xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
             xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
             xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main">
  <p:cSld>
    <p:spTree>
      <p:nvGrpSpPr>
        <p:cNvPr id="1" name=""/>
        <p:cNvGrpSpPr/>
        <p:nvPr/>
      </p:nvGrpSpPr>
      <p:grpSpPr>
        <a:xfrm>
          <a:off x="0" y="0"/>
          <a:ext cx="0" cy="0"/>
          <a:chOff x="0" y="0"/>
          <a:chExt cx="0" cy="0"/>
        </a:xfrm>
      </p:grpSpPr>
    </p:spTree>
  </p:cSld>
  <p:clrMap bg1="lt1" tx1="dk1" bg2="lt2" tx2="dk2"
            accent1="accent1" accent2="accent2" accent3="accent3"
            accent4="accent4" accent5="accent5" accent6="accent6"
            hlink="hlink" folHlink="folHlink"/>
  <p:sldLayoutIdLst>
    <p:sldLayoutId id="2147483649" r:id="rId1"/>
  </p:sldLayoutIdLst>
  <p:txStyles>
    <p:titleStyle><a:lvl1pPr algn="l"><a:defRPr sz="4400" b="1"/></a:lvl1pPr></p:titleStyle>
    <p:bodyStyle><a:lvl1pPr algn="l"><a:defRPr sz="1800"/></a:lvl1pPr></p:bodyStyle>
    <p:otherStyle><a:defPPr><a:defRPr sz="1600"/></a:defPPr></p:otherStyle>
  </p:txStyles>
</p:sldMaster>
"""


def slide_master_rels():
    return """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideLayout" Target="../slideLayouts/slideLayout1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme" Target="../theme/theme1.xml"/>
</Relationships>
"""


def theme_xml():
    return """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<a:theme xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" name="Office Theme">
  <a:themeElements>
    <a:clrScheme name="Office">
      <a:dk1><a:srgbClr val="000000"/></a:dk1>
      <a:lt1><a:srgbClr val="FFFFFF"/></a:lt1>
      <a:dk2><a:srgbClr val="1F497D"/></a:dk2>
      <a:lt2><a:srgbClr val="EEECE1"/></a:lt2>
      <a:accent1><a:srgbClr val="4F81BD"/></a:accent1>
      <a:accent2><a:srgbClr val="C0504D"/></a:accent2>
      <a:accent3><a:srgbClr val="9BBB59"/></a:accent3>
      <a:accent4><a:srgbClr val="8064A2"/></a:accent4>
      <a:accent5><a:srgbClr val="4BACC6"/></a:accent5>
      <a:accent6><a:srgbClr val="F79646"/></a:accent6>
      <a:hlink><a:srgbClr val="0000FF"/></a:hlink>
      <a:folHlink><a:srgbClr val="800080"/></a:folHlink>
    </a:clrScheme>
    <a:fontScheme name="Office">
      <a:majorFont>
        <a:latin typeface="Calibri"/>
        <a:ea typeface=""/>
        <a:cs typeface=""/>
      </a:majorFont>
      <a:minorFont>
        <a:latin typeface="Calibri"/>
        <a:ea typeface=""/>
        <a:cs typeface=""/>
      </a:minorFont>
    </a:fontScheme>
    <a:fmtScheme name="Office">
      <a:fillStyleLst>
        <a:solidFill><a:schemeClr val="phClr"/></a:solidFill>
        <a:gradFill rotWithShape="1">
          <a:gsLst>
            <a:gs pos="0"><a:schemeClr val="phClr"><a:tint val="50000"/><a:satMod val="300000"/></a:schemeClr></a:gs>
            <a:gs pos="35000"><a:schemeClr val="phClr"><a:tint val="37000"/><a:satMod val="300000"/></a:schemeClr></a:gs>
            <a:gs pos="100000"><a:schemeClr val="phClr"><a:tint val="15000"/><a:satMod val="350000"/></a:schemeClr></a:gs>
          </a:gsLst>
          <a:lin ang="16200000" scaled="1"/>
        </a:gradFill>
      </a:fillStyleLst>
      <a:lnStyleLst>
        <a:ln w="9525" cap="flat" cmpd="sng" algn="ctr"><a:solidFill><a:schemeClr val="phClr"/></a:solidFill></a:ln>
        <a:ln w="25400" cap="flat" cmpd="sng" algn="ctr"><a:solidFill><a:schemeClr val="phClr"/></a:solidFill></a:ln>
        <a:ln w="38100" cap="flat" cmpd="sng" algn="ctr"><a:solidFill><a:schemeClr val="phClr"/></a:solidFill></a:ln>
      </a:lnStyleLst>
      <a:effectStyleLst>
        <a:effectStyle><a:effectLst/></a:effectStyle>
        <a:effectStyle><a:effectLst/></a:effectStyle>
        <a:effectStyle><a:effectLst/></a:effectStyle>
      </a:effectStyleLst>
      <a:bgFillStyleLst>
        <a:solidFill><a:schemeClr val="phClr"/></a:solidFill>
        <a:solidFill><a:schemeClr val="phClr"><a:tint val="95000"/><a:satMod val="170000"/></a:schemeClr></a:solidFill>
        <a:gradFill rotWithShape="1">
          <a:gsLst>
            <a:gs pos="0"><a:schemeClr val="phClr"><a:shade val="63000"/><a:satMod val="120000"/></a:schemeClr></a:gs>
            <a:gs pos="100000"><a:schemeClr val="phClr"><a:shade val="37000"/><a:satMod val="120000"/></a:schemeClr></a:gs>
          </a:gsLst>
          <a:lin ang="16200000" scaled="1"/>
        </a:gradFill>
      </a:bgFillStyleLst>
    </a:fmtScheme>
  </a:themeElements>
  <a:objectDefaults/>
  <a:extraClrSchemeLst/>
</a:theme>
"""


def main():
    slides = build_slides()
    slide_count = len(slides)
    os.makedirs(os.path.dirname(OUT_PATH), exist_ok=True)

    with zipfile.ZipFile(OUT_PATH, "w", compression=zipfile.ZIP_DEFLATED) as zf:
        zf.writestr("[Content_Types].xml", content_types(slide_count))
        zf.writestr("_rels/.rels", root_rels())

        zf.writestr("docProps/app.xml", app_xml(slide_count))
        zf.writestr("docProps/core.xml", core_xml())

        zf.writestr("ppt/presentation.xml", presentation_xml(slide_count))
        zf.writestr("ppt/_rels/presentation.xml.rels", presentation_rels(slide_count))

        zf.writestr("ppt/slideMasters/slideMaster1.xml", slide_master_xml())
        zf.writestr("ppt/slideMasters/_rels/slideMaster1.xml.rels", slide_master_rels())
        zf.writestr("ppt/slideLayouts/slideLayout1.xml", slide_layout_xml())
        zf.writestr("ppt/slideLayouts/_rels/slideLayout1.xml.rels", slide_layout_rels())
        zf.writestr("ppt/theme/theme1.xml", theme_xml())

        for i, s in enumerate(slides, start=1):
            zf.writestr(f"ppt/slides/slide{i}.xml", s)
            zf.writestr(f"ppt/slides/_rels/slide{i}.xml.rels", slide_rels())

    print(f"Generated: {OUT_PATH}")
    print(f"Slides: {slide_count}")


if __name__ == "__main__":
    main()
