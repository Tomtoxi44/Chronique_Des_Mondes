/**
 * mermaid-preprocess.js
 * Extrait les blocs ```mermaid``` d'un fichier Markdown,
 * les rend en PNG via mmdc, et remplace les blocs par des images.
 *
 * Usage: node mermaid-preprocess.js <input.md> <output.md> <img-dir>
 */

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const [, , inputFile, outputMd, imgDir] = process.argv;

if (!inputFile || !outputMd || !imgDir) {
  console.error('Usage: node mermaid-preprocess.js <input.md> <output.md> <img-dir>');
  process.exit(1);
}

if (!fs.existsSync(imgDir)) fs.mkdirSync(imgDir, { recursive: true });

let content = fs.readFileSync(inputFile, 'utf8').replace(/\r\n/g, '\n');
const mermaidRegex = /```mermaid\n([\s\S]*?)```/g;

let match;
let idx = 0;
const replacements = [];

while ((match = mermaidRegex.exec(content)) !== null) {
  const diagramCode = match[1];
  const imgName = `diagram_${idx}.png`;
  const imgPath = path.join(imgDir, imgName);
  const tmpMmd = path.join(imgDir, `tmp_${idx}.mmd`);

  fs.writeFileSync(tmpMmd, diagramCode, 'utf8');

  try {
    execSync(`mmdc -i "${tmpMmd}" -o "${imgPath}" -b white --width 650`, { stdio: 'pipe' });
    console.log(`  [OK]  diagram_${idx}.png`);
    replacements.push({ original: match[0], imgPath: imgPath.replace(/\\/g, '/'), ok: true });
  } catch (e) {
    console.error(`  [ERR] diagram_${idx}: ${e.stderr ? e.stderr.toString().slice(0, 200) : e.message}`);
    replacements.push({ original: match[0], code: diagramCode, ok: false, idx });
  }

  if (fs.existsSync(tmpMmd)) fs.unlinkSync(tmpMmd);
  idx++;
}

let result = content;
for (const r of replacements) {
  if (r.ok) {
    result = result.replace(r.original, `![Diagramme](${r.imgPath})`);
  } else {
    // Fallback: bloc de code plain (pandoc le gardera tel quel)
    result = result.replace(r.original, `> *(Diagramme ${r.idx} — voir version HTML)*\n\n\`\`\`\n${r.code}\`\`\``);
  }
}

fs.writeFileSync(outputMd, result, 'utf8');
console.log(`\nFichier intermédiaire : ${outputMd}`);
console.log(`Diagrammes traités    : ${idx} (${replacements.filter(r => r.ok).length} OK)`);
