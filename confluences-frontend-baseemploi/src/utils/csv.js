// Lecture des CSV exportés d'Excel : séparateur ";" (Excel en français) ou ",", encodage UTF-8
// ou Windows-1252 (« CSV (séparateur : point-virgule) » d'Excel Windows), ligne de titre
// éventuelle au-dessus des en-têtes.

// "Date de naissance " → "date de naissance" ; "E-mail AS" → "e mail as"
export const normalizeHeader = (s) => String(s ?? '')
  .normalize('NFD').replace(/[̀-ͯ]/g, '')
  .toLowerCase()
  .replace(/[^a-z0-9&]+/g, ' ')
  .trim()

export function detectDelimiter(text) {
  const firstLines = text.split(/\r?\n/).slice(0, 10).join('\n')
  const counts = [';', ',', '\t'].map(d => [d, firstLines.split(d).length])
  return counts.sort((a, b) => b[1] - a[1])[0][0]
}

export function parseCsv(text, delimiter) {
  const rows = []
  let row = []
  let field = ''
  let inQuotes = false
  for (let i = 0; i < text.length; i++) {
    const c = text[i]
    if (inQuotes) {
      if (c === '"') {
        if (text[i + 1] === '"') { field += '"'; i++ } else inQuotes = false
      } else field += c
    } else if (c === '"') inQuotes = true
    else if (c === delimiter) { row.push(field); field = '' }
    else if (c === '\n' || c === '\r') {
      if (c === '\r' && text[i + 1] === '\n') i++
      row.push(field); field = ''
      rows.push(row); row = []
    } else field += c
  }
  if (field.length || row.length) { row.push(field); rows.push(row) }
  return rows
    .map(r => r.map(cell => cell.trim()))
    .filter(r => r.some(cell => cell !== ''))
}

// Décode le fichier : UTF-8 s'il est valide, sinon Windows-1252.
export function decodeCsv(buffer) {
  try {
    const text = new TextDecoder('utf-8', { fatal: true }).decode(buffer)
    return { text: text.charCodeAt(0) === 0xfeff ? text.slice(1) : text, encoding: 'UTF-8' }
  } catch {
    return { text: new TextDecoder('windows-1252').decode(buffer), encoding: 'Windows-1252' }
  }
}

export async function readCsvFile(file) {
  const { text, encoding } = decodeCsv(await file.arrayBuffer())
  const delimiter = detectDelimiter(text)
  return { rows: parseCsv(text, delimiter), encoding, delimiter }
}
