with open(r'D:\Projet_Perso\Chronique_Des_Mondes\docs\dossier-bloc2-processed.md', encoding='utf-8') as f:
    lines = f.readlines()

code_blocks = sum(1 for l in lines if l.strip().startswith('```'))
empty_lines = sum(1 for l in lines if l.strip() == '')
total = len(lines)
imgs = sum(1 for l in lines if l.strip().startswith('!['))
print(f'Lignes totales  : {total}')
print(f'Lignes vides    : {empty_lines}')
print(f'Delimiterus code: {code_blocks} ({code_blocks//2} blocs)')
print(f'Images          : {imgs}')
