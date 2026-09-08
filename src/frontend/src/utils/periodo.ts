export function formatarEntradaMesAno(valor: string) {
  const valorIso = valor.match(/^(\d{4})-(\d{1,2})$/)
  if (valorIso) {
    return `${valorIso[2].padStart(2, '0')}/${valorIso[1]}`
  }

  const texto = valor.replace(/[^\d/]/g, '')
  const separador = texto.indexOf('/')
  if (separador >= 0) {
    const mes = texto.slice(0, separador).replace(/\D/g, '').slice(0, 2)
    const ano = texto.slice(separador + 1).replace(/\D/g, '').slice(0, 4)
    return `${mes}/${ano}`
  }

  const digitos = texto.replace(/\D/g, '').slice(0, 6)
  return digitos.length <= 2
    ? digitos
    : `${digitos.slice(0, 2)}/${digitos.slice(2)}`
}

export function obterMesAno(valor: string, anoPadrao: number) {
  const resultado = valor.trim().match(/^(0?[1-9]|1[0-2])(?:\/(\d{4}))?$/)
  if (!resultado) return null

  return {
    mes: resultado[1].padStart(2, '0'),
    ano: resultado[2] ?? String(anoPadrao),
  }
}

export function formatarMesAno(ano: number, mes: number) {
  return `${String(mes).padStart(2, '0')}/${ano}`
}
