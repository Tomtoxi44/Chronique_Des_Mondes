"""
build-reference-docx.py
Crée un reference.docx pour pandoc depuis zéro (pas de base corrompue).
Marges réduites, police compacte, interligne simple.
"""

from docx import Document
from docx.shared import Cm, Pt, RGBColor
from docx.enum.text import WD_LINE_SPACING
import os

OUT = r"D:\Projet_Perso\Chronique_Des_Mondes\docs\reference.docx"

doc = Document()

# ── 1. Marges A4 ──────────────────────────────────────────────────────────
for section in doc.sections:
    section.page_width    = Cm(21.0)
    section.page_height   = Cm(29.7)
    section.top_margin    = Cm(1.8)
    section.bottom_margin = Cm(1.8)
    section.left_margin   = Cm(1.8)
    section.right_margin  = Cm(1.8)

# ── 2. Style Normal ────────────────────────────────────────────────────────
normal = doc.styles['Normal']
normal.font.name = 'Calibri'
normal.font.size = Pt(10)
normal.paragraph_format.space_before       = Pt(0)
normal.paragraph_format.space_after        = Pt(4)
normal.paragraph_format.line_spacing_rule  = WD_LINE_SPACING.SINGLE

# ── 3. Titres ──────────────────────────────────────────────────────────────
heading_defs = [
    ('Heading 1', Pt(16), True,  Pt(10), Pt(4),  RGBColor(0x1E, 0x1B, 0x4B)),
    ('Heading 2', Pt(13), True,  Pt(8),  Pt(2),  RGBColor(0x31, 0x2E, 0x81)),
    ('Heading 3', Pt(11), True,  Pt(6),  Pt(2),  RGBColor(0x4F, 0x46, 0xE5)),
    ('Heading 4', Pt(10), True,  Pt(4),  Pt(2),  RGBColor(0x6B, 0x7A, 0x99)),
]

for name, size, bold, before, after, color in heading_defs:
    try:
        s = doc.styles[name]
    except KeyError:
        s = doc.styles.add_style(name, 1)  # WD_STYLE_TYPE.PARAGRAPH = 1
    s.font.name  = 'Calibri'
    s.font.size  = size
    s.font.bold  = bold
    s.font.color.rgb = color
    s.paragraph_format.space_before      = before
    s.paragraph_format.space_after       = after
    s.paragraph_format.line_spacing_rule = WD_LINE_SPACING.SINGLE
    s.paragraph_format.keep_with_next    = True

doc.save(OUT)

# ── 4. Prendre les styles Source Code de la v2 générée et les copier ─────
from docx import Document as Doc2
from docx.oxml import OxmlElement
import copy

v2_path = r"D:\Projet_Perso\Chronique_Des_Mondes\dossier-bloc2-v2.docx"
try:
    v2 = Doc2(v2_path)
    target_doc = Document(OUT)
    
    for style_name in ['Source Code', 'VerbatimStringTok', 'Verbatim Char']:
        try:
            src_style = v2.styles[style_name]
            # Add style to our reference
            new_style = target_doc.styles.add_style(style_name, src_style.type)
            new_style.font.name = 'Courier New'
            new_style.font.size = Pt(8)
            if hasattr(new_style, 'paragraph_format'):
                new_style.paragraph_format.space_before = Pt(2)
                new_style.paragraph_format.space_after  = Pt(2)
                new_style.paragraph_format.line_spacing_rule = WD_LINE_SPACING.SINGLE
        except Exception as e:
            print(f"  Style {style_name}: {e}")
    
    target_doc.save(OUT)
    print(f"Styles code ajoutés au reference.docx")
except Exception as e:
    print(f"Copie styles ignorée: {e}")

print(f"reference.docx final : {OUT}")
print(f"Taille : {os.path.getsize(OUT):,} octets")