export function formatarTelefoneBrasileiro(valor: string) {
  const digitos = valor.replace(/\D/g, '').slice(0, 11)

  if (!digitos) return ''
  if (digitos.length <= 2) return `(${digitos}`

  const ddd = digitos.slice(0, 2)
  const numero = digitos.slice(2)
  if (numero.length <= 5) return `(${ddd}) ${numero}`

  return `(${ddd}) ${numero.slice(0, 5)}-${numero.slice(5)}`
}
