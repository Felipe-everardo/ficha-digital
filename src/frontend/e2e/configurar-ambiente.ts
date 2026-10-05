import { iniciarFrontendE2E, verificarPortaLivre } from './servidores'

export default async function configurarAmbienteE2E() {
  // Recusa uma API já aberta, que pode usar o banco de desenvolvimento.
  await verificarPortaLivre(5057)
  return iniciarFrontendE2E()
}
