import { type FormEvent, useEffect, useState } from 'react'
import './App.css'
import {
  criarCliente,
  getApiStatus,
  type ApiStatus,
  type ClienteCriado,
  type CriarClienteInput,
} from './services/api'

const formularioInicial: CriarClienteInput = {
  nomeCompleto: '',
  dataNascimento: '',
  celular: '',
}

function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus | null>(null)
  const [statusError, setStatusError] = useState<string | null>(null)
  const [formulario, setFormulario] = useState<CriarClienteInput>(formularioInicial)
  const [clienteCriado, setClienteCriado] = useState<ClienteCriado | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)
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
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitError(null)
    setIsSubmitting(true)

    try {
      const response = await criarCliente(formulario)
      setClienteCriado(response)
      setFormulario(formularioInicial)
    } catch {
      setSubmitError(
        'Não foi possível enviar seus dados. Confira as informações e tente novamente.',
      )
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
                  value={formulario.nomeCompleto}
                  onChange={(event) =>
                    atualizarCampo('nomeCompleto', event.target.value)
                  }
                />
              </label>

              <label className="field">
                <span>Data de nascimento *</span>
                <input
                  type="date"
                  name="dataNascimento"
                  autoComplete="bday"
                  required
                  value={formulario.dataNascimento}
                  onChange={(event) =>
                    atualizarCampo('dataNascimento', event.target.value)
                  }
                />
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
                  value={formulario.celular}
                  onChange={(event) =>
                    atualizarCampo('celular', event.target.value)
                  }
                />
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

export default App
