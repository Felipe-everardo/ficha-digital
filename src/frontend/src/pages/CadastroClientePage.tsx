import { type FormEvent, useEffect, useRef, useState } from 'react'
import {
  obterAntiforgeryToken,
  obterSessaoProfissional,
} from '../services/autenticacao'
import {
  ApiRequestError,
  ApiValidationError,
  criarCliente,
  emitirConviteFicha,
  type ClienteCriado,
  type ConviteFichaCriado,
  type CriarClienteInput,
  type TipoProcedimento,
} from '../services/api'

type ErrosFormulario = Partial<Record<keyof CriarClienteInput, string>>

const camposPorNomeDaApi: Record<string, keyof CriarClienteInput> = {
  nomereferencia: 'nomeReferencia',
}

const formularioInicial: CriarClienteInput = {
  nomeReferencia: '',
}

function FormularioCadastroCliente() {
  const [formulario, setFormulario] = useState<CriarClienteInput>(formularioInicial)
  const [clienteCriado, setClienteCriado] = useState<ClienteCriado | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<ErrosFormulario>({})
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [conviteGerado, setConviteGerado] =
    useState<ConviteFichaCriado | null>(null)
  const [gerandoConvite, setGerandoConvite] = useState(false)
  const [tipoProcedimento, setTipoProcedimento] = useState<
    '' | Exclude<TipoProcedimento, 'NaoInformado'>
  >('')
  const [profissionalResponsavelNome, setProfissionalResponsavelNome] =
    useState('')
  const [erroConvite, setErroConvite] = useState<string | null>(null)
  const [mensagemCopia, setMensagemCopia] = useState<string | null>(null)
  const conviteInputRef = useRef<HTMLInputElement>(null)
  const procedimentosPermitidos: Array<'Tatuagem' | 'Piercing'> = [
    'Tatuagem',
    'Piercing',
  ]

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
      setTipoProcedimento('')
      setProfissionalResponsavelNome('')
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
    if (
      !clienteCriado ||
      !tipoProcedimento ||
      !profissionalResponsavelNome.trim()
    ) {
      setErroConvite('Informe o profissional responsável e o procedimento.')
      return
    }

    setGerandoConvite(true)
    setErroConvite(null)
    setMensagemCopia(null)

    try {
      const antiforgeryToken = await obterAntiforgeryToken()
      const convite = await emitirConviteFicha(
        clienteCriado.id,
        profissionalResponsavelNome.trim(),
        tipoProcedimento,
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
        <header className="page-header">
          <div>
            <p className="eyebrow">Área profissional</p>
            <h1 id="page-title">Cadastrar cliente</h1>
            <p className="intro">
              Informe apenas um nome para identificar o cliente. Os demais
              dados serão preenchidos por ele no link do convite.
            </p>
          </div>
        </header>

        {clienteCriado ? (
          <div className="success-panel" role="status">
            <p className="eyebrow">Cliente cadastrado</p>
            <h2>{clienteCriado.nomeParaExibicao} foi cadastrado.</h2>
            <p>
              O nome de referência foi salvo. Agora gere o link para que o
              cliente complete seus dados e a ficha.
            </p>
            <label className="success-procedure-field">
              <span>Profissional responsável *</span>
              <input
                type="text"
                maxLength={150}
                required
                disabled={conviteGerado !== null}
                placeholder="Nome completo"
                value={profissionalResponsavelNome}
                onChange={(event) => {
                  setProfissionalResponsavelNome(event.target.value)
                  setErroConvite(null)
                }}
              />
            </label>
            <label className="success-procedure-field">
              <span>Procedimento *</span>
              <select
                required
                disabled={conviteGerado !== null}
                value={tipoProcedimento}
                onChange={(event) =>
                  setTipoProcedimento(
                    event.target.value as
                      | ''
                      | Exclude<TipoProcedimento, 'NaoInformado'>,
                  )
                }
              >
                <option value="">Selecione</option>
                {procedimentosPermitidos.map((procedimento) => (
                  <option key={procedimento} value={procedimento}>
                    {procedimento}
                  </option>
                ))}
              </select>
            </label>
            <div className="success-actions">
              <button
                type="button"
                disabled={
                  !tipoProcedimento ||
                  !profissionalResponsavelNome.trim() ||
                  gerandoConvite ||
                  conviteGerado !== null
                }
                onClick={handleGerarConvite}
              >
                {gerandoConvite
                  ? 'Gerando convite...'
                  : conviteGerado
                    ? 'Convite gerado'
                    : 'Gerar convite agora'}
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
                  Responsável: <strong>{profissionalResponsavelNome}</strong>.
                  {' '}Válido até{' '}
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
              <span>Cadastro inicial</span>
              <div>
                <h2>Identificação do cliente</h2>
                <p>O profissional precisa preencher somente este campo.</p>
              </div>
            </div>

            <div className="field-grid">
              <label className="field field--full">
                <span>Nome para identificar o cliente *</span>
                <input
                  type="text"
                  name="nomeReferencia"
                  maxLength={150}
                  placeholder="Ex.: Ana ou Cliente da Lia"
                  required
                  aria-invalid={Boolean(fieldErrors.nomeReferencia)}
                  aria-describedby={
                    fieldErrors.nomeReferencia
                      ? 'nomeReferencia-error'
                      : undefined
                  }
                  value={formulario.nomeReferencia}
                  onChange={(event) =>
                    atualizarCampo('nomeReferencia', event.target.value)
                  }
                />
                <small>
                  Pode ser o primeiro nome ou a forma como você reconhece essa
                  pessoa. O cliente informará o nome completo depois.
                </small>
                {fieldErrors.nomeReferencia && (
                  <small className="field-error" id="nomeReferencia-error">
                    {fieldErrors.nomeReferencia}
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
                {isSubmitting ? 'Salvando...' : 'Cadastrar cliente'}
              </button>
            </div>
          </form>
        )}
      </section>
    </main>
  )
}

export function CadastroClientePage() {
  const [estadoSessao, setEstadoSessao] = useState<
    | { tipo: 'verificando' }
    | { tipo: 'autenticado' }
    | { tipo: 'erro' }
  >({ tipo: 'verificando' })

  useEffect(() => {
    const abortController = new AbortController()

    obterSessaoProfissional(abortController.signal)
      .then((sessao) => {
        if (!sessao) {
          window.location.replace('/profissional/entrar')
          return
        }

        setEstadoSessao({ tipo: 'autenticado' })
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        setEstadoSessao({ tipo: 'erro' })
      })

    return () => abortController.abort()
  }, [])

  if (estadoSessao.tipo === 'verificando') {
    return (
      <main className="page-shell">
        <p className="status-message" aria-live="polite">
          Verificando sua sessão profissional...
        </p>
      </main>
    )
  }

  if (estadoSessao.tipo === 'erro') {
    return (
      <main className="page-shell">
        <section className="form-card" role="alert">
          <p className="eyebrow">Área profissional</p>
          <h1>Não foi possível verificar sua sessão.</h1>
          <button type="button" onClick={() => window.location.reload()}>
            Tentar novamente
          </button>
        </section>
      </main>
    )
  }

  return <FormularioCadastroCliente />
}
