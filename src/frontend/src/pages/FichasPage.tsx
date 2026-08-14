import { useEffect, useState } from 'react'
import {
  ApiRequestError,
  listarFichas,
  type FichaResumo,
  type FichasPaginadas,
} from '../services/api'
import './FichasPage.css'

const TAMANHO_PAGINA = 10

type StatusVisual = {
  texto: string
  classe: string
}

function obterStatusVisual(ficha: FichaResumo): StatusVisual {
  if (ficha.status === 'ConviteEnviado' && ficha.conviteExpirado) {
    return { texto: 'Convite expirado', classe: 'expired' }
  }

  const statusPorCodigo: Record<string, StatusVisual> = {
    Rascunho: { texto: 'Rascunho', classe: 'draft' },
    ConviteEnviado: { texto: 'Convite enviado', classe: 'sent' },
    EmPreenchimento: { texto: 'Em preenchimento', classe: 'progress' },
    Concluida: { texto: 'Concluída', classe: 'completed' },
    Expirada: { texto: 'Expirada', classe: 'expired' },
    Cancelada: { texto: 'Cancelada', classe: 'cancelled' },
  }

  return (
    statusPorCodigo[ficha.status] ?? {
      texto: ficha.status,
      classe: 'draft',
    }
  )
}

function formatarDataHora(dataUtc: string) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(dataUtc))
}

export function FichasPage() {
  const [pagina, setPagina] = useState(1)
  const [resultado, setResultado] = useState<FichasPaginadas | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)

  useEffect(() => {
    const abortController = new AbortController()

    setCarregando(true)
    setErro(null)

    listarFichas(pagina, TAMANHO_PAGINA, abortController.signal)
      .then(setResultado)
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        if (error instanceof ApiRequestError && error.status === 401) {
          window.location.replace('/profissional/entrar')
          return
        }

        setErro(
          error instanceof ApiRequestError
            ? error.message
            : 'Não foi possível carregar as fichas.',
        )
      })
      .finally(() => {
        if (!abortController.signal.aborted) {
          setCarregando(false)
        }
      })

    return () => abortController.abort()
  }, [pagina])

  const totalPaginasExibido = Math.max(resultado?.totalPaginas ?? 1, 1)

  return (
    <main className="records-shell">
      <header className="records-header">
        <div>
          <a className="records-back-link" href="/profissional">
            ← Voltar ao painel
          </a>
          <p className="eyebrow">Área profissional</p>
          <h1>Fichas</h1>
          <p>
            Acompanhe o andamento dos convites sem expor respostas de saúde
            nesta visão geral.
          </p>
        </div>

        <a className="records-primary-link" href="/profissional/clientes">
          Gerar convite
        </a>
      </header>

      <section className="records-content" aria-live="polite">
        {carregando && !resultado && (
          <div className="records-state">
            <span className="records-loading-indicator" aria-hidden="true" />
            Carregando fichas...
          </div>
        )}

        {erro && (
          <div className="records-state records-state--error" role="alert">
            <p>{erro}</p>
            <button type="button" onClick={() => window.location.reload()}>
              Tentar novamente
            </button>
          </div>
        )}

        {!erro && resultado && resultado.itens.length === 0 && (
          <div className="records-state records-state--empty">
            <p className="eyebrow">Nenhuma ficha</p>
            <h2>Ainda não existem convites emitidos.</h2>
            <p>Escolha um cliente para gerar a primeira ficha.</p>
            <a className="records-primary-link" href="/profissional/clientes">
              Ver clientes
            </a>
          </div>
        )}

        {!erro && resultado && resultado.itens.length > 0 && (
          <>
            <div className="records-summary">
              <strong>{resultado.totalItens}</strong>{' '}
              {resultado.totalItens === 1
                ? 'ficha registrada'
                : 'fichas registradas'}
              {carregando && <span>Atualizando...</span>}
            </div>

            <div className="records-table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Cliente</th>
                    <th>Estado</th>
                    <th>Ficha criada em</th>
                    <th>Validade do convite</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {resultado.itens.map((ficha) => {
                    const status = obterStatusVisual(ficha)

                    return (
                      <tr key={ficha.id}>
                        <td data-label="Cliente">
                          <strong>{ficha.clienteNome}</strong>
                        </td>
                        <td data-label="Estado">
                          <span
                            className={`record-status record-status--${status.classe}`}
                          >
                            {status.texto}
                          </span>
                        </td>
                        <td data-label="Ficha criada em">
                          {formatarDataHora(ficha.criadaEmUtc)}
                        </td>
                        <td data-label="Validade do convite">
                          {ficha.conviteExpiraEmUtc
                            ? formatarDataHora(ficha.conviteExpiraEmUtc)
                            : 'Convite não emitido'}
                        </td>
                        <td data-label="Ações">
                          <a
                            className="record-detail-link"
                            href={`/profissional/fichas/${ficha.id}`}
                          >
                            Ver detalhes
                          </a>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>

            <nav className="records-pagination" aria-label="Paginação">
              <button
                className="secondary-button"
                type="button"
                disabled={pagina === 1 || carregando}
                onClick={() => setPagina((paginaAtual) => paginaAtual - 1)}
              >
                Anterior
              </button>
              <span>
                Página {resultado.pagina} de {totalPaginasExibido}
              </span>
              <button
                className="secondary-button"
                type="button"
                disabled={pagina >= resultado.totalPaginas || carregando}
                onClick={() => setPagina((paginaAtual) => paginaAtual + 1)}
              >
                Próxima
              </button>
            </nav>
          </>
        )}
      </section>
    </main>
  )
}
