"""
Soulspire UI Asset Generator - Sprint 1
Dark Fantasy Pixel Art UI Assets
TA Team Lead (Art Director)

Generates all Sprint 1 priority UI image assets using PIL/Pillow.
"""

from PIL import Image, ImageDraw
import os

# === Color Palette (from ArtDirection 4.1) ===
C = {
    # UI Palette
    'ui_bg':        (0x12, 0x10, 0x1A),
    'ui_panel':     (0x1A, 0x18, 0x28),
    'ui_panel_lit': (0x24, 0x22, 0x36),
    'ui_frame':     (0x5A, 0x50, 0x70),
    'ui_frame_act': (0xB0, 0xA0, 0x80),
    'ui_frame_acc': (0xFF, 0xD8, 0x4D),
    'txt_main':     (0xE0, 0xDC, 0xD0),
    'txt_sub':      (0xA0, 0x98, 0x90),
    'txt_disabled': (0x60, 0x58, 0x50),
    # Magic/Glow
    'emerald':      (0x40, 0xD4, 0x70),
    'gold':         (0xFF, 0xD8, 0x4D),
    'ruby':         (0xD4, 0x40, 0x40),
    'sapphire':     (0x40, 0x80, 0xD4),
    'magic_white':  (0xE8, 0xE4, 0xF0),
    # Environment
    'dark_stone':   (0x2A, 0x2A, 0x3A),
    'mid_stone':    (0x3A, 0x3A, 0x50),
    'light_stone':  (0x4A, 0x48, 0x60),
    'highlight':    (0x6A, 0x60, 0x80),
}
TRANSPARENT = (0, 0, 0, 0)

def hex_to_rgba(hex_tuple, alpha=255):
    return hex_tuple + (alpha,)

def darker(color, factor=0.7):
    return tuple(int(c * factor) for c in color[:3])

def lighter(color, factor=1.3):
    return tuple(min(255, int(c * factor)) for c in color[:3])

def rgba(color, alpha=255):
    return color[:3] + (alpha,)


# === Output directories ===
BASE = '/mnt/c/UnityProjects/test1/Assets/Art/UI'
FRAMES_DIR = os.path.join(BASE, 'Frames')
BUTTONS_DIR = os.path.join(BASE, 'Buttons')
ICONS_DIR = os.path.join(BASE, 'Icons')

for d in [FRAMES_DIR, BUTTONS_DIR, ICONS_DIR]:
    os.makedirs(d, exist_ok=True)


# ============================================================
# 1. Panel Frame (9-slice) - 64x64
# ============================================================
def create_panel_frame():
    """Stone + gold trim panel frame for 9-slice slicing."""
    w, h = 64, 64
    img = Image.new('RGBA', (w, h), TRANSPARENT)
    draw = ImageDraw.Draw(img)

    # Panel background fill
    draw.rectangle([3, 3, w-4, h-4], fill=rgba(C['ui_panel']))

    # Outer border (dark stone)
    for i in range(2):
        draw.rectangle([i, i, w-1-i, h-1-i], outline=rgba(darker(C['ui_frame'], 0.6)))

    # Main stone border
    draw.rectangle([2, 2, w-3, h-3], outline=rgba(C['ui_frame']))

    # Inner highlight (top-left light source)
    draw.line([(3, 3), (w-4, 3)], fill=rgba(C['highlight']))  # top inner edge
    draw.line([(3, 3), (3, h-4)], fill=rgba(C['highlight']))  # left inner edge

    # Inner shadow (bottom-right)
    draw.line([(4, h-4), (w-4, h-4)], fill=rgba(darker(C['ui_frame'], 0.5)))
    draw.line([(w-4, 4), (w-4, h-4)], fill=rgba(darker(C['ui_frame'], 0.5)))

    # Corner rune decorations (4 corners) - small gold dots
    corners = [(4, 4), (w-6, 4), (4, h-6), (w-6, h-6)]
    for cx, cy in corners:
        # Small rune cross pattern
        draw.point((cx, cy), fill=rgba(C['ui_frame_acc']))
        draw.point((cx+1, cy), fill=rgba(C['ui_frame_act']))
        draw.point((cx, cy+1), fill=rgba(C['ui_frame_act']))

    # Gold accent line on border (subtle)
    # Top center accent
    mid = w // 2
    draw.line([(mid-6, 2), (mid+5, 2)], fill=rgba(C['ui_frame_act']))
    # Bottom center accent
    draw.line([(mid-6, h-3), (mid+5, h-3)], fill=rgba(C['ui_frame_act']))

    img.save(os.path.join(FRAMES_DIR, 'panel_frame.png'))
    print("  Created: panel_frame.png (64x64)")


# ============================================================
# 2. Button Frames - Basic (Stone) - 4 states, 48x24
# ============================================================
def draw_button(draw, w, h, border_color, fill_color, highlight, shadow, accent=None):
    """Draw a stone-style button."""
    # Outer border
    draw.rectangle([0, 0, w-1, h-1], outline=rgba(darker(border_color, 0.5)))
    # Main border
    draw.rectangle([1, 1, w-2, h-2], outline=rgba(border_color))
    # Fill
    draw.rectangle([2, 2, w-3, h-3], fill=rgba(fill_color))
    # Top highlight
    draw.line([(2, 2), (w-3, 2)], fill=rgba(highlight))
    draw.line([(2, 2), (2, h-3)], fill=rgba(highlight))
    # Bottom shadow
    draw.line([(3, h-3), (w-3, h-3)], fill=rgba(shadow))
    draw.line([(w-3, 3), (w-3, h-3)], fill=rgba(shadow))
    # Optional accent dots at corners
    if accent:
        draw.point((3, 3), fill=rgba(accent))
        draw.point((w-4, 3), fill=rgba(accent))
        draw.point((3, h-4), fill=rgba(accent))
        draw.point((w-4, h-4), fill=rgba(accent))

def create_basic_buttons():
    """Stone button in 4 states: Idle, Hover, Pressed, Disabled."""
    w, h = 48, 24

    states = {
        'idle': {
            'border': C['ui_frame'],
            'fill': C['dark_stone'],
            'highlight': C['light_stone'],
            'shadow': darker(C['dark_stone'], 0.6),
        },
        'hover': {
            'border': C['ui_frame_act'],
            'fill': C['mid_stone'],
            'highlight': C['highlight'],
            'shadow': C['dark_stone'],
        },
        'pressed': {
            'border': C['ui_frame'],
            'fill': darker(C['dark_stone'], 0.8),
            'highlight': darker(C['dark_stone'], 0.6),  # inverted - shadow on top
            'shadow': C['light_stone'],  # light on bottom (pressed effect)
        },
        'disabled': {
            'border': darker(C['ui_frame'], 0.5),
            'fill': darker(C['dark_stone'], 0.6),
            'highlight': darker(C['dark_stone'], 0.8),
            'shadow': darker(C['dark_stone'], 0.4),
        },
    }

    for state_name, colors in states.items():
        img = Image.new('RGBA', (w, h), TRANSPARENT)
        draw = ImageDraw.Draw(img)
        draw_button(draw, w, h, colors['border'], colors['fill'],
                    colors['highlight'], colors['shadow'])
        img.save(os.path.join(BUTTONS_DIR, f'btn_basic_{state_name}.png'))
        print(f"  Created: btn_basic_{state_name}.png (48x24)")


# ============================================================
# 3. Button Frames - Accent (Gold) - 4 states, 48x24
# ============================================================
def create_accent_buttons():
    """Gold accent button in 4 states."""
    w, h = 48, 24

    gold_dark = darker(C['gold'], 0.5)
    gold_mid = darker(C['gold'], 0.7)

    states = {
        'idle': {
            'border': C['ui_frame_acc'],
            'fill': C['dark_stone'],
            'highlight': C['gold'],
            'shadow': gold_dark,
            'accent': C['gold'],
        },
        'hover': {
            'border': C['gold'],
            'fill': C['mid_stone'],
            'highlight': lighter(C['gold']),
            'shadow': gold_mid,
            'accent': lighter(C['gold']),
        },
        'pressed': {
            'border': gold_mid,
            'fill': darker(C['dark_stone'], 0.8),
            'highlight': gold_dark,
            'shadow': C['gold'],
            'accent': gold_mid,
        },
        'disabled': {
            'border': darker(C['ui_frame'], 0.6),
            'fill': darker(C['dark_stone'], 0.6),
            'highlight': darker(C['dark_stone'], 0.8),
            'shadow': darker(C['dark_stone'], 0.4),
            'accent': None,
        },
    }

    for state_name, colors in states.items():
        img = Image.new('RGBA', (w, h), TRANSPARENT)
        draw = ImageDraw.Draw(img)
        draw_button(draw, w, h, colors['border'], colors['fill'],
                    colors['highlight'], colors['shadow'], colors['accent'])
        img.save(os.path.join(BUTTONS_DIR, f'btn_accent_{state_name}.png'))
        print(f"  Created: btn_accent_{state_name}.png (48x24)")


# ============================================================
# 4. HP Bar Frame - 200x16
# ============================================================
def create_hp_bar_frame():
    """Stone frame for HP bar."""
    w, h = 200, 16
    img = Image.new('RGBA', (w, h), TRANSPARENT)
    draw = ImageDraw.Draw(img)

    # Outer dark border
    draw.rectangle([0, 0, w-1, h-1], outline=rgba(darker(C['ui_frame'], 0.5)))
    # Stone border
    draw.rectangle([1, 1, w-2, h-2], outline=rgba(C['ui_frame']))
    # Inner background (dark)
    draw.rectangle([2, 2, w-3, h-3], fill=rgba(C['ui_bg']))
    # Top inner highlight
    draw.line([(2, 2), (w-3, 2)], fill=rgba(darker(C['ui_frame'], 0.7)))
    # Bottom inner shadow
    draw.line([(2, h-3), (w-3, h-3)], fill=rgba(darker(C['ui_bg'], 0.7)))

    # Small rune accents at ends
    draw.point((3, 3), fill=rgba(C['ui_frame_act']))
    draw.point((w-4, 3), fill=rgba(C['ui_frame_act']))
    draw.point((3, h-4), fill=rgba(C['ui_frame_act']))
    draw.point((w-4, h-4), fill=rgba(C['ui_frame_act']))

    img.save(os.path.join(FRAMES_DIR, 'hp_bar_frame.png'))
    print("  Created: hp_bar_frame.png (200x16)")


# ============================================================
# 5. HP Bar Fill - 196x12 (green->yellow->red gradient)
# ============================================================
def create_hp_bar_fill():
    """HP fill with green->yellow->red gradient."""
    w, h = 196, 12
    img = Image.new('RGBA', (w, h), TRANSPARENT)

    green = C['emerald']
    yellow = (0xE0, 0xD0, 0x40)
    red = C['ruby']

    for x in range(w):
        t = x / (w - 1)
        if t < 0.5:
            # green -> yellow
            t2 = t * 2
            r = int(green[0] + (yellow[0] - green[0]) * t2)
            g = int(green[1] + (yellow[1] - green[1]) * t2)
            b = int(green[2] + (yellow[2] - green[2]) * t2)
        else:
            # yellow -> red
            t2 = (t - 0.5) * 2
            r = int(yellow[0] + (red[0] - yellow[0]) * t2)
            g = int(yellow[1] + (red[1] - yellow[1]) * t2)
            b = int(yellow[2] + (red[2] - yellow[2]) * t2)

        for y in range(h):
            # Add slight vertical shading (brighter at top)
            brightness = 1.0 + 0.15 * (1 - y / (h - 1)) - 0.1
            pr = min(255, int(r * brightness))
            pg = min(255, int(g * brightness))
            pb = min(255, int(b * brightness))
            img.putpixel((x, y), (pr, pg, pb, 255))

    # Top highlight line
    draw = ImageDraw.Draw(img)
    for x in range(w):
        px = img.getpixel((x, 0))
        bright = tuple(min(255, int(c * 1.3)) for c in px[:3]) + (180,)
        img.putpixel((x, 0), bright)

    img.save(os.path.join(FRAMES_DIR, 'hp_bar_fill.png'))
    print("  Created: hp_bar_fill.png (196x12)")


# ============================================================
# 6. Bit (Soul) Icon - 24x24 green gem
# ============================================================
def create_bit_icon():
    """Green gem icon for Soul/Bit resource."""
    w, h = 24, 24
    img = Image.new('RGBA', (w, h), TRANSPARENT)
    draw = ImageDraw.Draw(img)

    # Diamond/gem shape (hexagonal gem)
    cx, cy = 11, 11

    # Gem body colors
    gem_dark = darker(C['emerald'], 0.4)
    gem_mid = darker(C['emerald'], 0.7)
    gem_base = C['emerald']
    gem_light = lighter(C['emerald'], 1.3)
    gem_highlight = (0xA0, 0xFF, 0xC0)

    # Outline (1px black)
    outline_points = [
        (cx, cy-7),      # top
        (cx+6, cy-2),    # top-right
        (cx+6, cy+3),    # right
        (cx, cy+8),      # bottom
        (cx-6, cy+3),    # left
        (cx-6, cy-2),    # top-left
    ]
    draw.polygon(outline_points, outline=(0, 0, 0, 255), fill=rgba(gem_mid))

    # Inner facets - upper left (bright, light source)
    upper_left = [
        (cx, cy-6),
        (cx-5, cy-1),
        (cx, cy),
    ]
    draw.polygon(upper_left, fill=rgba(gem_base))

    # Upper right (slightly darker)
    upper_right = [
        (cx, cy-6),
        (cx+5, cy-1),
        (cx, cy),
    ]
    draw.polygon(upper_right, fill=rgba(gem_mid))

    # Lower left
    lower_left = [
        (cx-5, cy-1),
        (cx-5, cy+2),
        (cx, cy+7),
        (cx, cy),
    ]
    draw.polygon(lower_left, fill=rgba(gem_mid))

    # Lower right (darkest)
    lower_right = [
        (cx+5, cy-1),
        (cx+5, cy+2),
        (cx, cy+7),
        (cx, cy),
    ]
    draw.polygon(lower_right, fill=rgba(gem_dark))

    # Highlight sparkle (top-left facet)
    draw.point((cx-2, cy-3), fill=rgba(gem_highlight))
    draw.point((cx-1, cy-4), fill=rgba(gem_highlight))
    draw.point((cx-3, cy-2), fill=rgba(gem_light))

    # Center line (facet division)
    draw.line([(cx, cy-6), (cx, cy+7)], fill=(0, 0, 0, 80))
    draw.line([(cx-5, cy-1), (cx+5, cy-1)], fill=(0, 0, 0, 60))

    # Glow effect - soft green pixels around
    glow_color = rgba(C['emerald'], 40)
    for dx in [-1, 0, 1]:
        for dy in [-1, 0, 1]:
            for px, py in outline_points:
                gx, gy = px + dx, py + dy
                if 0 <= gx < w and 0 <= gy < h:
                    existing = img.getpixel((gx, gy))
                    if existing[3] == 0:  # only on transparent pixels
                        img.putpixel((gx, gy), glow_color)

    img.save(os.path.join(ICONS_DIR, 'icon_bit.png'))
    print("  Created: icon_bit.png (24x24)")


# ============================================================
# 7. Core (Essence) Icon - 24x24 gold octagon
# ============================================================
def create_core_icon():
    """Gold octagon icon for Essence/Core resource."""
    w, h = 24, 24
    img = Image.new('RGBA', (w, h), TRANSPARENT)
    draw = ImageDraw.Draw(img)

    cx, cy = 11, 11

    gold_dark = darker(C['gold'], 0.4)
    gold_mid = darker(C['gold'], 0.7)
    gold_base = C['gold']
    gold_light = lighter(C['gold'], 1.2)
    gold_highlight = (0xFF, 0xFF, 0xA0)

    # Octagon shape
    r = 8  # radius
    import math
    octagon = []
    for i in range(8):
        angle = math.pi * 2 * i / 8 - math.pi / 8  # rotated for flat top
        px = cx + int(r * math.cos(angle))
        py = cy + int(r * math.sin(angle))
        octagon.append((px, py))

    # Black outline
    draw.polygon(octagon, outline=(0, 0, 0, 255), fill=rgba(gold_mid))

    # Upper facets (brighter - light from top-left)
    # Simple shading: top half brighter
    for y_off in range(-r, 0):
        for x_off in range(-r, r+1):
            px, py = cx + x_off, cy + y_off
            if 0 <= px < w and 0 <= py < h:
                existing = img.getpixel((px, py))
                if existing[3] > 0 and existing != (0, 0, 0, 255):
                    if x_off < 0:  # top-left = brightest
                        img.putpixel((px, py), rgba(gold_base))
                    else:
                        img.putpixel((px, py), rgba(gold_mid))

    # Lower half darker
    for y_off in range(1, r+1):
        for x_off in range(-r, r+1):
            px, py = cx + x_off, cy + y_off
            if 0 <= px < w and 0 <= py < h:
                existing = img.getpixel((px, py))
                if existing[3] > 0 and existing != (0, 0, 0, 255):
                    if x_off > 0:  # bottom-right = darkest
                        img.putpixel((px, py), rgba(gold_dark))
                    else:
                        img.putpixel((px, py), rgba(darker(gold_mid, 0.8)))

    # Inner rune - simple cross pattern
    draw.line([(cx, cy-4), (cx, cy+4)], fill=rgba(gold_dark, 120))
    draw.line([(cx-4, cy), (cx+4, cy)], fill=rgba(gold_dark, 120))

    # Highlight sparkle
    draw.point((cx-3, cy-3), fill=rgba(gold_highlight))
    draw.point((cx-2, cy-4), fill=rgba(gold_highlight))
    draw.point((cx-4, cy-2), fill=rgba(gold_light))

    # Center gem dot
    draw.point((cx, cy), fill=rgba(gold_light))
    draw.point((cx-1, cy), fill=rgba(gold_base))
    draw.point((cx, cy-1), fill=rgba(gold_base))

    # Glow effect
    glow_color = rgba(C['gold'], 35)
    for pt in octagon:
        for dx in [-1, 0, 1]:
            for dy in [-1, 0, 1]:
                gx, gy = pt[0] + dx, pt[1] + dy
                if 0 <= gx < w and 0 <= gy < h:
                    existing = img.getpixel((gx, gy))
                    if existing[3] == 0:
                        img.putpixel((gx, gy), glow_color)

    img.save(os.path.join(ICONS_DIR, 'icon_core.png'))
    print("  Created: icon_core.png (24x24)")


# ============================================================
# 8. Tower Inventory Slot - 48x48
# ============================================================
def create_tower_slot():
    """Stone frame slot for tower inventory bar."""
    w, h = 48, 48
    img = Image.new('RGBA', (w, h), TRANSPARENT)
    draw = ImageDraw.Draw(img)

    # Outer dark border
    draw.rectangle([0, 0, w-1, h-1], outline=rgba(darker(C['ui_frame'], 0.5)))
    # Stone border (2px thick effect)
    draw.rectangle([1, 1, w-2, h-2], outline=rgba(C['ui_frame']))
    draw.rectangle([2, 2, w-3, h-3], outline=rgba(darker(C['ui_frame'], 0.8)))
    # Inner background
    draw.rectangle([3, 3, w-4, h-4], fill=rgba(C['ui_bg']))

    # Beveled edges (top-left light, bottom-right dark)
    draw.line([(3, 3), (w-4, 3)], fill=rgba(C['highlight'], 80))
    draw.line([(3, 3), (3, h-4)], fill=rgba(C['highlight'], 80))
    draw.line([(4, h-4), (w-4, h-4)], fill=rgba(darker(C['ui_bg'], 0.5)))
    draw.line([(w-4, 4), (w-4, h-4)], fill=rgba(darker(C['ui_bg'], 0.5)))

    # Corner accents (small rune dots)
    accent = rgba(C['ui_frame_act'], 150)
    for cx_pos, cy_pos in [(4, 4), (w-5, 4), (4, h-5), (w-5, h-5)]:
        draw.point((cx_pos, cy_pos), fill=accent)

    # Subtle inner border pattern (stone texture hint)
    for i in range(6, w-6, 4):
        draw.point((i, 3), fill=rgba(C['mid_stone'], 60))
        draw.point((i, h-4), fill=rgba(darker(C['dark_stone'], 0.5), 60))

    img.save(os.path.join(FRAMES_DIR, 'tower_slot.png'))
    print("  Created: tower_slot.png (48x48)")


# ============================================================
# Additional: Tooltip frame (9-slice, 48x48 base)
# ============================================================
def create_tooltip_frame():
    """Small stone panel for tooltips, 9-slice compatible."""
    w, h = 48, 48
    img = Image.new('RGBA', (w, h), TRANSPARENT)
    draw = ImageDraw.Draw(img)

    # Semi-transparent background
    draw.rectangle([2, 2, w-3, h-3], fill=rgba(C['ui_panel'], 230))

    # Outer border
    draw.rectangle([0, 0, w-1, h-1], outline=rgba(darker(C['ui_frame'], 0.6)))
    draw.rectangle([1, 1, w-2, h-2], outline=rgba(C['ui_frame']))

    # Highlight edges
    draw.line([(2, 2), (w-3, 2)], fill=rgba(C['highlight'], 100))
    draw.line([(2, 2), (2, h-3)], fill=rgba(C['highlight'], 100))

    # Shadow edges
    draw.line([(3, h-3), (w-3, h-3)], fill=rgba(darker(C['ui_frame'], 0.4)))
    draw.line([(w-3, 3), (w-3, h-3)], fill=rgba(darker(C['ui_frame'], 0.4)))

    img.save(os.path.join(FRAMES_DIR, 'tooltip_frame.png'))
    print("  Created: tooltip_frame.png (48x48)")


# ============================================================
# Additional: Dropdown frame (9-slice, 64x32)
# ============================================================
def create_dropdown_frame():
    """Dropdown panel frame."""
    w, h = 64, 32
    img = Image.new('RGBA', (w, h), TRANSPARENT)
    draw = ImageDraw.Draw(img)

    # Background
    draw.rectangle([2, 2, w-3, h-3], fill=rgba(C['ui_panel']))

    # Borders
    draw.rectangle([0, 0, w-1, h-1], outline=rgba(darker(C['ui_frame'], 0.6)))
    draw.rectangle([1, 1, w-2, h-2], outline=rgba(C['ui_frame']))

    # Highlight
    draw.line([(2, 2), (w-3, 2)], fill=rgba(C['highlight'], 80))
    draw.line([(2, 2), (2, h-3)], fill=rgba(C['highlight'], 80))

    img.save(os.path.join(FRAMES_DIR, 'dropdown_frame.png'))
    print("  Created: dropdown_frame.png (64x32)")


# ============================================================
# Run all generators
# ============================================================
if __name__ == '__main__':
    print("=== Soulspire UI Asset Generator ===")
    print()

    print("[1/8] Panel Frame (9-slice)...")
    create_panel_frame()

    print("[2/8] Basic Buttons (4 states)...")
    create_basic_buttons()

    print("[3/8] Accent Buttons (4 states)...")
    create_accent_buttons()

    print("[4/8] HP Bar Frame...")
    create_hp_bar_frame()

    print("[5/8] HP Bar Fill...")
    create_hp_bar_fill()

    print("[6/8] Bit (Soul) Icon...")
    create_bit_icon()

    print("[7/8] Core (Essence) Icon...")
    create_core_icon()

    print("[8/8] Tower Inventory Slot...")
    create_tower_slot()

    print()
    print("=== Bonus Assets ===")
    create_tooltip_frame()
    create_dropdown_frame()

    print()
    print("=== All UI assets generated successfully! ===")
    print(f"Output directories:")
    print(f"  Frames:  {FRAMES_DIR}")
    print(f"  Buttons: {BUTTONS_DIR}")
    print(f"  Icons:   {ICONS_DIR}")
