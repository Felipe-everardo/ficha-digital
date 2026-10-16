// Mantém nomes legíveis e remove caracteres inválidos em nomes de arquivo do Windows.
export function nomeArquivoFicha(nomeCliente: string, criadaEmUtc: string): string {
  const nome = nomeCliente.normalize('NFC')
    .replace(/[<>:"/\\|?*\p{Cc}]/gu, ' ')
    .replace(/\s+/gu, ' ')
    .trim()
    .replace(/[. ]+$/g, '')
    .slice(0, 100)
    .trim() || 'Cliente'
  const data = criadaEmUtc.slice(0, 10)
  return `Ficha - ${nome} - ${data}.pdf`
}
