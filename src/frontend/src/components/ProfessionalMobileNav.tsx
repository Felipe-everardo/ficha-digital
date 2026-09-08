import type { ReactNode } from 'react'
import './ProfessionalMobileNav.css'

export type ProfessionalSection =
  | 'clientes'
  | 'fichas'
  | 'financeiro'
  | 'inicio'

type ProfessionalMobileLayoutProps = {
  activeSection?: ProfessionalSection
  children: ReactNode
}

const itens = [
  { id: 'inicio', label: 'Início', href: '/profissional' },
  { id: 'clientes', label: 'Clientes', href: '/profissional/clientes' },
  { id: 'fichas', label: 'Fichas', href: '/profissional/fichas' },
  { id: 'financeiro', label: 'Financeiro', href: '/profissional/financeiro' },
] as const

function NavigationIcon({ section }: { section: ProfessionalSection }) {
  if (section === 'clientes') {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M16 20v-1.5a4.5 4.5 0 0 0-4.5-4.5h-4A4.5 4.5 0 0 0 3 18.5V20M9.5 10a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7ZM17 11a3 3 0 0 0 0-6M18 14.5a4 4 0 0 1 3 3.87V20" />
      </svg>
    )
  }

  if (section === 'fichas') {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M7 3h8l4 4v14H7V3Zm8 0v5h4M10 12h6M10 16h6" />
      </svg>
    )
  }

  if (section === 'inicio') {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="m3 11 9-8 9 8M5.5 9.5V21h13V9.5M9.5 21v-6h5v6" />
      </svg>
    )
  }

  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M4 7.5h16v11H4v-11Zm0 3.5h16M15.5 15h2M7 5h10" />
    </svg>
  )
}

export function ProfessionalMobileLayout({
  activeSection,
  children,
}: ProfessionalMobileLayoutProps) {
  return (
    <div className="professional-mobile-layout">
      {children}
      <nav
        className="professional-mobile-nav"
        aria-label="Navegação da área profissional"
      >
        {itens.map((item) => {
          const ativo = activeSection === item.id

          return (
            <a
              key={item.id}
              className={ativo ? 'professional-mobile-nav__active' : undefined}
              href={item.href}
              aria-current={ativo ? 'page' : undefined}
            >
              <NavigationIcon section={item.id} />
              <span>{item.label}</span>
            </a>
          )
        })}
      </nav>
    </div>
  )
}
