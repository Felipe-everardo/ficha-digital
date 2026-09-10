import { ProfessionalMobileLayout } from '../components/ProfessionalMobileNav'
import { AreaProfissionalPage } from '../pages/AreaProfissionalPage'
import { CadastroClientePage } from '../pages/CadastroClientePage'
import { ClienteDetalhePage } from '../pages/ClienteDetalhePage'
import { ClientesPage } from '../pages/ClientesPage'
import { FichaDetalhePage } from '../pages/FichaDetalhePage'
import { FichaPublicaPage } from '../pages/FichaPublicaPage'
import { FichasPage } from '../pages/FichasPage'

function normalizarCaminho(caminho: string) {
  if (caminho.length > 1 && caminho.endsWith('/')) {
    return caminho.slice(0, -1)
  }

  return caminho
}

export function AppRoutes() {
  const caminho = normalizarCaminho(window.location.pathname)
  const detalheCliente = caminho.match(
    /^\/profissional\/clientes\/([0-9a-fA-F-]{36})$/,
  )
  const detalheFicha = caminho.match(
    /^\/profissional\/fichas\/([0-9a-fA-F-]{36})$/,
  )

  if (detalheFicha) {
    return (
      <ProfessionalMobileLayout activeSection="fichas">
        <FichaDetalhePage fichaId={detalheFicha[1]} />
      </ProfessionalMobileLayout>
    )
  }

  if (detalheCliente) {
    return (
      <ProfessionalMobileLayout activeSection="clientes">
        <ClienteDetalhePage clienteId={detalheCliente[1]} />
      </ProfessionalMobileLayout>
    )
  }

  if (caminho === '/profissional/fichas') {
    return (
      <ProfessionalMobileLayout activeSection="fichas">
        <FichasPage />
      </ProfessionalMobileLayout>
    )
  }

  if (caminho === '/profissional/clientes') {
    return (
      <ProfessionalMobileLayout activeSection="clientes">
        <ClientesPage />
      </ProfessionalMobileLayout>
    )
  }

  if (caminho === '/profissional/clientes/novo') {
    return (
      <ProfessionalMobileLayout activeSection="clientes">
        <CadastroClientePage />
      </ProfessionalMobileLayout>
    )
  }

  if (caminho.startsWith('/fichas/preencher')) {
    return <FichaPublicaPage />
  }

  return <AreaProfissionalPage />
}
