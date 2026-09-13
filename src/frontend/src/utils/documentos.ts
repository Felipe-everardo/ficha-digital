function somenteDigitos(valor: string, tamanhoMaximo: number) {
  return valor.replace(/\D/g, '').slice(0, tamanhoMaximo)
}

export function formatarCpf(valor: string) {
  return somenteDigitos(valor, 11)
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d{1,2})$/, '$1-$2')
}

export function formatarCep(valor: string) {
  return somenteDigitos(valor, 8).replace(/(\d{5})(\d)/, '$1-$2')
}
