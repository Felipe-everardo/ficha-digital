import { type FormEvent, useEffect, useRef, useState } from 'react'
import './App.css'
import { StudioBrand } from './components/StudioBrand'
import { AreaProfissionalPage } from './pages/AreaProfissionalPage'
import { ClientesPage } from './pages/ClientesPage'
import { FichaPublicaPage } from './pages/FichaPublicaPage'
import { FichaDetalhePage } from './pages/FichaDetalhePage'
import { FichasPage } from './pages/FichasPage'
import {
  obterAntiforgeryToken,
  obterSessaoProfissional,
} from './services/autenticacao'
import {
  ApiRequestError,
  ApiValidationError,
  criarCliente,
  emitirConviteFicha,
  getApiStatus,
  type ApiStatus,
  type ClienteCriado,
  type ConviteFichaCriado,
  type CriarClienteInput,
} from './services/api'

type ErrosFormulario = Partial<Record<keyof CriarClienteInput, string>>

const camposPorNomeDaApi: Record<string, keyof CriarClienteInput> = {
  nomecompleto: 'nomeCompleto',
  nomesocial: 'nomeSocial',
  pronomes: 'pronomes',
  datanascimento: 'dataNascimento',
  celular: 'celular',
  email: 'email',
}

const formularioInicial: CriarClienteInput = {
  nomeCompleto: '',
  nomeSocial: '',
  pronomes: '',
  dataNascimento: '',
  celular: '',
  email: '',
}

function formatarDataParaInput(data: Date) {
  const ano = data.getFullYear()
  const mes = String(data.getMonth() + 1).padStart(2, '0')
  const dia = String(data.getDate()).padStart(2, '0')

  return `${ano}-${mes}-${dia}`
}

function CadastroClientePage() {
  const dataMaximaNascimento = formatarDataParaInput(new Date())
  const [apiStatus, setApiStatus] = useState<ApiStatus | null>(null)
  const [statusError, setStatusError] = useState<string | null>(null)
  const [formulario, setFormulario] = useState<CriarClienteInput>(formularioInicial)
  const [clienteCriado, setClienteCriado] = useState<ClienteCriado | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<ErrosFormulario>({})
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [conviteGerado, setConviteGerado] =
    useState<ConviteFichaCriado | null>(null)
  const [gerandoConvite, setGerandoConvite] = useState(false)
  const [erroConvite, setErroConvite] = useState<string | null>(null)
  const [mensagemCopia, setMensagemCopia] = useState<string | null>(null)
  const conviteInputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    getApiStatus()
      .then(setApiStatus)
      .catch(() => setStatusError('Não foi possível acessar a API.'))
  }, [])

  const connectionState = statusError
    ? 'error'
    : apiStatus
      ? 'success'
      : 'loading'

  function atualizarCampo(campo: keyof CriarClienteInput, valor: string) {
    setFormulario((formularioAtual) => ({
      ...formularioAtual,
      [campo]: valor,
    }))

    setFieldErrors((errosAtuais) => ({
      ...errosAtuais,
      [campo]: undefined,
    }))
    setSubmitError(null)
  }

  function mapearErrosDaApi(
    errors: Record<string, string[]>,
  ): ErrosFormulario {
    const errosMapeados: ErrosFormulario = {}

    for (const [nomeCampo, mensagens] of Object.entries(errors)) {
      const campo = camposPorNomeDaApi[nomeCampo.toLowerCase()]

      if (campo && mensagens.length > 0) {
        errosMapeados[campo] = mensagens[0]
      }
    }

    return errosMapeados
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitError(null)
    setFieldErrors({})
    setIsSubmitting(true)

    try {
      const antiforgeryToken = await obterAntiforgeryToken()
      const response = await criarCliente(formulario, antiforgeryToken)
      setClienteCriado(response)
      setConviteGerado(null)
      setErroConvite(null)
      setMensagemCopia(null)
      setFormulario(formularioInicial)
    } catch (error) {
      if (error instanceof ApiValidationError) {
        setFieldErrors(mapearErrosDaApi(error.errors))
        setSubmitError('Confira os campos destacados e tente novamente.')
      } else {
        setSubmitError(
          'Não foi possível enviar seus dados. Tente novamente em alguns instantes.',
        )
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleGerarConvite() {
    if (!clienteCriado) {
      return
    }

    setGerandoConvite(true)
    setErroConvite(null)
    setMensagemCopia(null)

    try {
      const antiforgeryToken = await obterAntiforgeryToken()
      const convite = await emitirConviteFicha(
        clienteCriado.id,
        antiforgeryToken,
      )

      setConviteGerado({
        ...convite,
        linkPreenchimento: new URL(
          convite.linkPreenchimento,
          window.location.origin,
        ).toString(),
      })
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 401) {
        window.location.replace('/profissional/entrar')
        return
      }

      setErroConvite(
        error instanceof ApiRequestError
          ? error.message
          : 'Não foi possível gerar o convite.',
      )
    } finally {
      setGerandoConvite(false)
    }
  }

  async function handleCopiarConvite() {
    if (!conviteGerado) {
      return
    }

    try {
      await navigator.clipboard.writeText(conviteGerado.linkPreenchimento)
      setMensagemCopia('Link copiado. Agora você pode enviá-lo ao cliente.')
    } catch {
      conviteInputRef.current?.focus()
      conviteInputRef.current?.select()
      setMensagemCopia(
        'O link foi selecionado para você copiar manualmente.',
      )
    }
  }

  return (
    <main className="page-shell">
      <section className="form-card" aria-labelledby="page-title">
        <a className="page-back-link" href="/profissional">
          ← Voltar para a área profissional
        </a>

        <header className="page-header">
          <StudioBrand compacta />

          <div>
            <p className="eyebrow">Área profissional</p>
            <h1 id="page-title">Cadastrar cliente</h1>
            <p className="intro">
              Informe os dados iniciais usados para identificar o cliente e
              manter contato sobre o atendimento.
            </p>
          </div>
        </header>

        <div
          className={`status-card status-card--${connectionState}`}
          aria-live="polite"
        >
          <span className="status-dot" aria-hidden="true" />
          <p className="status-message">
            {statusError ??
              (apiStatus
                ? `Sistema disponível — versão ${apiStatus.version}`
                : 'Verificando disponibilidade do sistema...')}
          </p>
        </div>

        {clienteCriado ? (
          <div className="success-panel" role="status">
            <p className="eyebrow">Cliente cadastrado</p>
            <h2>{clienteCriado.nomeParaExibicao} foi cadastrado.</h2>
            <p>
              Os dados iniciais foram salvos. Você já pode gerar o link de
              preenchimento para enviar ao cliente.
            </p>
            <div className="success-actions">
              <button
                type="button"
                disabled={gerandoConvite || conviteGerado !== null}
                onClick={handleGerarConvite}
              >
                {gerandoConvite
                  ? 'Gerando convite...'
                  : conviteGerado
                    ? 'Convite gerado'
                    : 'Gerar convite agora'}
              </button>
              <button
                className="secondary-button"
                type="button"
                onClick={() => {
                  setClienteCriado(null)
                  setConviteGerado(null)
                  setErroConvite(null)
                  setMensagemCopia(null)
                }}
              >
                Cadastrar outro cliente
              </button>
            </div>

            {erroConvite && (
              <p className="form-error" role="alert">
                {erroConvite}
              </p>
            )}

            {conviteGerado && (
              <section
                className="registration-invitation"
                aria-labelledby="registration-invitation-title"
              >
                <p className="eyebrow">Convite pronto</p>
                <h3 id="registration-invitation-title">
                  Envie este link para {clienteCriado.nomeParaExibicao}
                </h3>
                <p>
                  Válido até{' '}
                  <strong>
                    {new Intl.DateTimeFormat('pt-BR', {
                      dateStyle: 'short',
                      timeStyle: 'short',
                    }).format(new Date(conviteGerado.expiraEmUtc))}
                  </strong>
                  .
                </p>
                <div className="registration-invitation-link">
                  <input
                    ref={conviteInputRef}
                    type="text"
                    readOnly
                    aria-label="Link de preenchimento"
                    value={conviteGerado.linkPreenchimento}
                    onFocus={(event) => event.target.select()}
                  />
                  <button type="button" onClick={handleCopiarConvite}>
                    Copiar link
                  </button>
                </div>
                {mensagemCopia && (
                  <p className="registration-copy-message" role="status">
                    {mensagemCopia}
                  </p>
                )}
              </section>
            )}
          </div>
        ) : (
          <form className="client-form" onSubmit={handleSubmit}>
            <div className="form-heading">
              <span>Etapa 1</span>
              <div>
                <h2>Dados pessoais</h2>
                <p>Os campos marcados com * são obrigatórios.</p>
              </div>
            </div>

            <div className="field-grid">
              <label className="field field--full">
                <span>Nome completo *</span>
                <input
                  type="text"
                  name="nomeCompleto"
                  autoComplete="name"
                  maxLength={150}
                  required
                  aria-invalid={Boolean(fieldErrors.nomeCompleto)}
                  aria-describedby={
                    fieldErrors.nomeCompleto
                      ? 'nomeCompleto-error'
                      : undefined
                  }
                  value={formulario.nomeCompleto}
                  onChange={(event) =>
                    atualizarCampo('nomeCompleto', event.target.value)
                  }
                />
                {fieldErrors.nomeCompleto && (
                  <small className="field-error" id="nomeCompleto-error">
                    {fieldErrors.nomeCompleto}
                  </small>
                )}
              </label>

              <label className="field">
                <span>Nome social (opcional)</span>
                <input
                  type="text"
                  name="nomeSocial"
                  autoComplete="nickname"
                  maxLength={150}
                  aria-invalid={Boolean(fieldErrors.nomeSocial)}
                  aria-describedby={
                    fieldErrors.nomeSocial ? 'nomeSocial-error' : undefined
                  }
                  value={formulario.nomeSocial}
                  onChange={(event) =>
                    atualizarCampo('nomeSocial', event.target.value)
                  }
                />
                {fieldErrors.nomeSocial && (
                  <small className="field-error" id="nomeSocial-error">
                    {fieldErrors.nomeSocial}
                  </small>
                )}
              </label>

              <label className="field">
                <span>Pronomes (opcional)</span>
                <input
                  type="text"
                  name="pronomes"
                  maxLength={50}
                  placeholder="Ex.: ela/dela"
                  aria-invalid={Boolean(fieldErrors.pronomes)}
                  aria-describedby={
                    fieldErrors.pronomes ? 'pronomes-error' : undefined
                  }
                  value={formulario.pronomes}
                  onChange={(event) =>
                    atualizarCampo('pronomes', event.target.value)
                  }
                />
                {fieldErrors.pronomes && (
                  <small className="field-error" id="pronomes-error">
                    {fieldErrors.pronomes}
                  </small>
                )}
              </label>

              <label className="field">
                <span>Data de nascimento *</span>
                <input
                  type="date"
                  name="dataNascimento"
                  autoComplete="bday"
                  max={dataMaximaNascimento}
                  required
                  aria-invalid={Boolean(fieldErrors.dataNascimento)}
                  aria-describedby={
                    fieldErrors.dataNascimento
                      ? 'dataNascimento-error'
                      : undefined
                  }
                  value={formulario.dataNascimento}
                  onChange={(event) =>
                    atualizarCampo('dataNascimento', event.target.value)
                  }
                />
                {fieldErrors.dataNascimento && (
                  <small className="field-error" id="dataNascimento-error">
                    {fieldErrors.dataNascimento}
                  </small>
                )}
              </label>

              <label className="field">
                <span>Celular *</span>
                <input
                  type="tel"
                  name="celular"
                  autoComplete="tel"
                  maxLength={25}
                  placeholder="(21) 99999-9999"
                  required
                  aria-invalid={Boolean(fieldErrors.celular)}
                  aria-describedby={
                    fieldErrors.celular ? 'celular-error' : undefined
                  }
                  value={formulario.celular}
                  onChange={(event) =>
                    atualizarCampo('celular', event.target.value)
                  }
                />
                {fieldErrors.celular && (
                  <small className="field-error" id="celular-error">
                    {fieldErrors.celular}
                  </small>
                )}
              </label>

              <label className="field field--full">
                <span>E-mail (opcional)</span>
                <input
                  type="email"
                  name="email"
                  autoComplete="email"
                  maxLength={254}
                  placeholder="nome@exemplo.com"
                  aria-invalid={Boolean(fieldErrors.email)}
                  aria-describedby={
                    fieldErrors.email ? 'email-error' : undefined
                  }
                  value={formulario.email}
                  onChange={(event) =>
                    atualizarCampo('email', event.target.value)
                  }
                />
                {fieldErrors.email && (
                  <small className="field-error" id="email-error">
                    {fieldErrors.email}
                  </small>
                )}
              </label>
            </div>

            {submitError && (
              <p className="form-error" role="alert">
                {submitError}
              </p>
            )}

            <div className="form-actions">
              <button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Enviando...' : 'Salvar e continuar'}
              </button>
            </div>
          </form>
        )}
      </section>
    </main>
  )
}

function CadastroClienteProtegidoPage() {
  const [estadoSessao, setEstadoSessao] = useState<
    'verificando' | 'autenticado' | 'erro'
  >('verificando')

  useEffect(() => {
    const abortController = new AbortController()

    obterSessaoProfissional(abortController.signal)
      .then((sessao) => {
        if (!sessao) {
          window.location.replace('/profissional/entrar')
          return
        }

        setEstadoSessao('autenticado')
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setEstadoSessao('erro')
      })

    return () => abortController.abort()
  }, [])

  if (estadoSessao === 'verificando') {
    return (
      <main className="page-shell">
        <div aria-live="polite">
          <StudioBrand contexto="Área profissional" />
          <p className="status-message">
            Verificando sua sessão profissional...
          </p>
        </div>
      </main>
    )
  }

  if (estadoSessao === 'erro') {
    return (
      <main className="page-shell">
        <section className="form-card" role="alert">
          <StudioBrand contexto="Área profissional" />
          <p className="eyebrow">Área profissional</p>
          <h1>Não foi possível verificar sua sessão.</h1>
          <button type="button" onClick={() => window.location.reload()}>
            Tentar novamente
          </button>
        </section>
      </main>
    )
  }

  return <CadastroClientePage />
}

function App() {
  const detalheFichaMatch = window.location.pathname.match(
    /^\/profissional\/fichas\/([0-9a-fA-F-]{36})\/?$/,
  )
  const paginaListaFichas =
    window.location.pathname === '/profissional/fichas'
  const paginaListaClientes =
    window.location.pathname === '/profissional/clientes'
  const paginaCadastroCliente =
    window.location.pathname === '/profissional/clientes/novo'
  const paginaProfissional = window.location.pathname.startsWith(
    '/profissional',
  )
  const paginaPublica = window.location.pathname.startsWith(
    '/fichas/preencher',
  )

  if (detalheFichaMatch) {
    return <FichaDetalhePage fichaId={detalheFichaMatch[1]} />
  }

  if (paginaListaFichas) {
    return <FichasPage />
  }

  if (paginaListaClientes) {
    return <ClientesPage />
  }

  if (paginaCadastroCliente) {
    return <CadastroClienteProtegidoPage />
  }

  if (paginaProfissional) {
    return <AreaProfissionalPage />
  }

  return paginaPublica ? <FichaPublicaPage /> : <AreaProfissionalPage />
}

export default App
