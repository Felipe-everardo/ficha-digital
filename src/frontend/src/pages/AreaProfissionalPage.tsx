import { type FormEvent, useEffect, useState } from 'react'
import logoManuscrito from '../assets/manuscrito-estudio-logo.png'
import { StudioBrand } from '../components/StudioBrand'
import { ProfessionalMobileLayout } from '../components/ProfessionalMobileNav'
import { ApiRequestError } from '../services/api'
import {
  entrarProfissional,
  obterSessaoProfissional,
  sairProfissional,
  type SessaoProfissional,
} from '../services/autenticacao'
import './AreaProfissionalPage.css'

type EstadoAreaProfissional =
  | { tipo: 'carregando' }
  | { tipo: 'anonimo' }
  | { tipo: 'autenticado'; sessao: SessaoProfissional }
  | { tipo: 'erro'; mensagem: string }

export function AreaProfissionalPage() {
  const [estado, setEstado] = useState<EstadoAreaProfissional>({
    tipo: 'carregando',
  })
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [senhaVisivel, setSenhaVisivel] = useState(false)
  const [autenticando, setAutenticando] = useState(false)
  const [encerrandoSessao, setEncerrandoSessao] = useState(false)
  const [erroFormulario, setErroFormulario] = useState<string | null>(null)

  useEffect(() => {
    const abortController = new AbortController()

    obterSessaoProfissional(abortController.signal)
      .then((sessao) => {
        if (sessao) {
          setEstado({ tipo: 'autenticado', sessao })
          window.history.replaceState(
            window.history.state,
            '',
            '/profissional',
          )
        } else {
          setEstado({ tipo: 'anonimo' })
          window.history.replaceState(
            window.history.state,
            '',
            '/profissional/entrar',
          )
        }
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setEstado({
          tipo: 'erro',
          mensagem:
            error instanceof ApiRequestError
              ? error.message
              : 'Não foi possível acessar a área profissional.',
        })
      })

    return () => abortController.abort()
  }, [])

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setAutenticando(true)
    setErroFormulario(null)

    try {
      const sessao = await entrarProfissional(email.trim(), senha)

      setEstado({ tipo: 'autenticado', sessao })
      setEmail('')
      setSenha('')
      window.history.replaceState(
        window.history.state,
        '',
        '/profissional',
      )
    } catch (error) {
      setSenha('')
      setErroFormulario(
        error instanceof ApiRequestError
          ? error.message
          : 'Não foi possível entrar. Tente novamente.',
      )
    } finally {
      setSenhaVisivel(false)
      setAutenticando(false)
    }
  }

  async function handleLogout() {
    setEncerrandoSessao(true)

    try {
      await sairProfissional()
      setEstado({ tipo: 'anonimo' })
      window.history.replaceState(
        window.history.state,
        '',
        '/profissional/entrar',
      )
    } catch (error) {
      setErroFormulario(
        error instanceof ApiRequestError
          ? error.message
          : 'Não foi possível encerrar a sessão.',
      )
    } finally {
      setEncerrandoSessao(false)
    }
  }

  if (estado.tipo === 'carregando') {
    return (
      <main className="professional-shell">
        <div className="professional-loading-panel" aria-live="polite">
          <StudioBrand contexto="Área profissional" />
          <div className="professional-loading">
            <span className="loading-indicator" aria-hidden="true" />
            Verificando sessão protegida...
          </div>
        </div>
      </main>
    )
  }

  if (estado.tipo === 'erro') {
    return (
      <main className="professional-shell">
        <section className="professional-auth-card" role="alert">
          <StudioBrand contexto="Área profissional" />
          <p className="eyebrow">Área profissional</p>
          <h1>Não foi possível carregar</h1>
          <p className="professional-copy">{estado.mensagem}</p>
          <button type="button" onClick={() => window.location.reload()}>
            Tentar novamente
          </button>
        </section>
      </main>
    )
  }

  if (estado.tipo === 'anonimo') {
    return (
      <main className="professional-shell">
        <section
          className="professional-auth-card"
          aria-labelledby="professional-login-title"
        >
          <StudioBrand contexto="Área profissional" />
          <p className="eyebrow">Acesso reservado</p>
          <h1 id="professional-login-title">Área profissional</h1>
          <p className="professional-copy">
            Entre com sua conta para acompanhar fichas e convites do estúdio.
          </p>

          <form className="professional-login-form" onSubmit={handleLogin}>
            <label>
              <span>E-mail</span>
              <input
                type="email"
                autoComplete="username"
                maxLength={254}
                placeholder="Ex.: profissional@email.com"
                required
                value={email}
                onChange={(event) => {
                  setEmail(event.target.value)
                  setErroFormulario(null)
                }}
              />
            </label>

            <div className="professional-password-field">
              <label htmlFor="senha-profissional">Senha</label>
              <div className="professional-password-input">
                <input
                  id="senha-profissional"
                  type={senhaVisivel ? 'text' : 'password'}
                  autoComplete="current-password"
                  maxLength={128}
                  required
                  value={senha}
                  onChange={(event) => {
                    setSenha(event.target.value)
                    setErroFormulario(null)
                  }}
                />
                <button
                  className="professional-password-toggle"
                  type="button"
                  aria-label={senhaVisivel ? 'Ocultar senha' : 'Mostrar senha'}
                  title={senhaVisivel ? 'Ocultar senha' : 'Mostrar senha'}
                  aria-controls="senha-profissional"
                  aria-pressed={senhaVisivel}
                  onClick={() => setSenhaVisivel((visivel) => !visivel)}
                >
                  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    {senhaVisivel ? (
                      <>
                        <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" />
                        <circle cx="12" cy="12" r="3" />
                      </>
                    ) : (
                      <>
                        <path d="M3 9c2 4 5 6 9 6s7-2 9-6M5 12l-2 3m6-1-1 4m7-4 1 4m3-6 2 3" />
                      </>
                    )}
                  </svg>
                </button>
              </div>
            </div>

            {erroFormulario && (
              <p className="form-error" role="alert">
                {erroFormulario}
              </p>
            )}

            <button type="submit" disabled={autenticando}>
              {autenticando ? 'Entrando...' : 'Entrar'}
            </button>
          </form>

          <p className="professional-security-note">
            Não existe cadastro público. As contas são gerenciadas pelo
            estúdio.
          </p>
        </section>
      </main>
    )
  }

  return (
    <ProfessionalMobileLayout activeSection="inicio">
      <main className="professional-dashboard-shell">
      <header className="professional-topbar">
        <StudioBrand
          className="professional-brand"
          contexto="Área profissional"
        />

        <div className="professional-account">
          <div>
            <strong>{estado.sessao.nomeCompleto}</strong>
            <span>{estado.sessao.email}</span>
            <span>Conta do estúdio</span>
          </div>
          <button
            className="secondary-button professional-logout"
            type="button"
            disabled={encerrandoSessao}
            onClick={handleLogout}
          >
            {encerrandoSessao ? 'Saindo...' : 'Sair'}
          </button>
        </div>
      </header>

      <section
        className="professional-dashboard"
        aria-labelledby="professional-dashboard-title"
      >
        <h1 id="professional-dashboard-title">
          Bem-vindo ao Manuscrito Studio.
        </h1>
        <p className="professional-copy">
          Gerencie clientes, convites e fichas digitais do estúdio em um só
          lugar.
        </p>

        {erroFormulario && (
          <p className="form-error" role="alert">
            {erroFormulario}
          </p>
        )}

        <img
          className="professional-home-logo"
          src={logoManuscrito}
          alt="Logo Manuscrito Estudio"
        />

        <div className="professional-placeholder-grid">
          <a
            className="professional-placeholder-card professional-placeholder-card--action"
            href="/profissional/clientes"
          >
            <span>01</span>
            <h2>Clientes</h2>
            <p>Busque clientes, veja o atendimento atual e gere nova ficha.</p>
            <strong>Ver clientes →</strong>
          </a>
          <a
            className="professional-placeholder-card professional-placeholder-card--action"
            href="/profissional/fichas"
          >
            <span>02</span>
            <h2>Histórico</h2>
            <p>Consulte o arquivo completo de fichas anteriores.</p>
            <strong>Ver histórico →</strong>
          </a>
        </div>
      </section>
      </main>
    </ProfessionalMobileLayout>
  )
}
