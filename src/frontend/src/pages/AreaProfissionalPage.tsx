import { type FormEvent, useEffect, useState } from 'react'
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
        <div className="professional-loading" aria-live="polite">
          <span className="loading-indicator" aria-hidden="true" />
          Verificando sessão protegida...
        </div>
      </main>
    )
  }

  if (estado.tipo === 'erro') {
    return (
      <main className="professional-shell">
        <section className="professional-auth-card" role="alert">
          <div className="brand-mark" aria-hidden="true">
            FD
          </div>
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
          <div className="brand-mark" aria-hidden="true">
            FD
          </div>
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
                required
                value={email}
                onChange={(event) => {
                  setEmail(event.target.value)
                  setErroFormulario(null)
                }}
              />
            </label>

            <label>
              <span>Senha</span>
              <input
                type="password"
                autoComplete="current-password"
                maxLength={128}
                required
                value={senha}
                onChange={(event) => {
                  setSenha(event.target.value)
                  setErroFormulario(null)
                }}
              />
            </label>

            {erroFormulario && (
              <p className="form-error" role="alert">
                {erroFormulario}
              </p>
            )}

            <button type="submit" disabled={autenticando}>
              {autenticando ? 'Entrando...' : 'Entrar com segurança'}
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
    <main className="professional-dashboard-shell">
      <header className="professional-topbar">
        <div className="professional-brand">
          <div className="brand-mark" aria-hidden="true">
            FD
          </div>
          <div>
            <p>Ficha Digital</p>
            <span>Área profissional</span>
          </div>
        </div>

        <div className="professional-account">
          <div>
            <strong>{estado.sessao.nomeCompleto}</strong>
            <span>{estado.sessao.email}</span>
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
        <p className="eyebrow">Sessão protegida ativa</p>
        <h1 id="professional-dashboard-title">
          Olá, {estado.sessao.nomeCompleto.split(' ')[0]}.
        </h1>
        <p className="professional-copy">
          Consulte clientes, gere novos convites e acompanhe o andamento das
          fichas digitais do estúdio.
        </p>

        {erroFormulario && (
          <p className="form-error" role="alert">
            {erroFormulario}
          </p>
        )}

        <div className="professional-placeholder-grid">
          <a
            className="professional-placeholder-card professional-placeholder-card--action"
            href="/profissional/clientes"
          >
            <span>01</span>
            <h2>Clientes</h2>
            <p>Consulte os cadastros e acesse os próximos passos.</p>
            <strong>Ver clientes →</strong>
          </a>
          <a
            className="professional-placeholder-card professional-placeholder-card--action"
            href="/profissional/fichas"
          >
            <span>02</span>
            <h2>Fichas</h2>
            <p>Acompanhamento dos estados e confirmações do cliente.</p>
            <strong>Ver fichas →</strong>
          </a>
          <article className="professional-placeholder-card">
            <span>03</span>
            <h2>Convites</h2>
            <p>Geração e compartilhamento dos links temporários.</p>
          </article>
        </div>
      </section>
    </main>
  )
}
