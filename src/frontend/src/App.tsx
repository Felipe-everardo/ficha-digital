import { type FormEvent, useEffect, useState } from 'react'
import './App.css'
import { FichaPublicaPage } from './pages/FichaPublicaPage'
import {
  ApiValidationError,
  criarCliente,
  getApiStatus,
  type ApiStatus,
  type ClienteCriado,
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
      const response = await criarCliente(formulario)
      setClienteCriado(response)
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

  return (
    <main className="page-shell">
      <section className="form-card" aria-labelledby="page-title">
        <header className="page-header">
          <div className="brand-mark" aria-hidden="true">
            FD
          </div>

          <div>
            <p className="eyebrow">Estúdio de tatuagem</p>
            <h1 id="page-title">Ficha Digital</h1>
            <p className="intro">
              Comece informando seus dados principais. Eles serão utilizados
              para identificar sua ficha e manter contato sobre o atendimento.
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
            <p className="eyebrow">Cadastro recebido</p>
            <h2>Obrigado, {clienteCriado.nomeParaExibicao}.</h2>
            <p>
              Seus dados principais foram salvos. As próximas partes da ficha
              serão adicionadas nas próximas etapas do projeto.
            </p>
            <button
              className="secondary-button"
              type="button"
              onClick={() => setClienteCriado(null)}
            >
              Cadastrar outra pessoa
            </button>
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

function App() {
  const paginaPublica = window.location.pathname.startsWith(
    '/fichas/preencher',
  )

  return paginaPublica ? <FichaPublicaPage /> : <CadastroClientePage />
}

export default App
