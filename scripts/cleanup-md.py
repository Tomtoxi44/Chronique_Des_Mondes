"""
cleanup-md.py
Nettoie le markdown préprocessé pour réduire le nombre de pages :
1. Supprime les lignes vides consécutives (max 1 entre paragraphes)
2. Tronque les blocs de code dépassant 35 lignes (garde début + fin + note)
3. Supprime les sauts de page explicites
"""

import re, sys

INPUT  = r"D:\Projet_Perso\Chronique_Des_Mondes\docs\dossier-bloc2-processed.md"
OUTPUT = r"D:\Projet_Perso\Chronique_Des_Mondes\docs\dossier-bloc2-clean.md"
MAX_CODE_LINES = 35

with open(INPUT, encoding='utf-8') as f:
    content = f.read()

# ── 1. Normalise les fins de ligne ────────────────────────────────────────
content = content.replace('\r\n', '\n')

# ── 2. Max 2 lignes vides consécutives → 1 ───────────────────────────────
content = re.sub(r'\n{3,}', '\n\n', content)

# ── 3. Tronquer blocs de code longs ──────────────────────────────────────
def truncate_code_block(match):
    fence_open  = match.group(1)   # ```lang
    code        = match.group(2)
    fence_close = match.group(3)   # ```
    
    lines = code.split('\n')
    # Enlever la dernière ligne vide si présente
    while lines and lines[-1].strip() == '':
        lines.pop()
    
    if len(lines) <= MAX_CODE_LINES:
        return match.group(0)
    
    keep_start = 25
    keep_end   = 8
    truncated = (
        lines[:keep_start]
        + [f'', f'    // ... ({len(lines) - keep_start - keep_end} lignes supprimées — voir dossier-bloc2.md) ...', f'']
        + lines[-keep_end:]
    )
    return f"{fence_open}\n{chr(10).join(truncated)}\n{fence_close}"

content = re.sub(
    r'(```[^\n]*)\n([\s\S]*?)(```)',
    truncate_code_block,
    content
)

# ── 4. Supprimer sauts de page explicites ────────────────────────────────
content = content.replace('\x0c', '')  # form feed
content = re.sub(r'\\pagebreak', '', content)

with open(OUTPUT, encoding='utf-8') as f:
    original_lines = len(f.readlines())

with open(OUTPUT, 'w', encoding='utf-8') as f:
    f.write(content)

new_lines = len(content.split('\n'))
print(f"Lignes avant : {original_lines}")
print(f"Lignes après : {new_lines}")
print(f"Réduction    : {original_lines - new_lines} lignes ({100*(original_lines - new_lines)//max(original_lines,1)}%)")
print(f"Sortie       : {OUTPUT}")
