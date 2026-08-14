import { useEffect, useRef, useState } from 'react'
import {
  ApiRequestError,
  emitirConviteFicha,
  listarClientes,
  type ClientesPaginados,
} from '../services/api'
import { obterAntiforgeryToken } from '../services/autenticacao'
import './ClientesPage.css'

const TAMANHO_PAGINA = 10

type ConviteGerado = {
  clienteNome: string
  link: string
  expiraEmUtc: string
}

function formatarData(dataUtc: string) {
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(new Date(dataUtc))
}

export function ClientesPage() {
  const [pagina, setPagina] = useState(1)
  const [resultado, setResultado] = useState<ClientesPaginados | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [clienteEmitindoId, setClienteEmitindoId] = useState<string | null>(null)
  const [erroEmissao, setErroEmissao] = useState<string | null>(null)
  const [conviteGerado, setConviteGerado] = useState<ConviteGerado | null>(null)
  const [mensagemCopia, setMensagemCopia] = useState<string | null>(null)
  const convitePanelRef = useRef<HTMLElement>(null)
  const conviteInputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    const abortController = new AbortController()

    setCarregando(true)
    setErro(null)

    listarClientes(pagina, TAMANHO_PAGINA, abortController.signal)
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
            : 'Não foi possível carregar os clientes.',
        )
      })
      .finally(() => {
        if (!abortController.signal.aborted) {
          setCarregando(false)
        }
      })

    return () => abortController.abort()
  }, [pagina])

  useEffect(() => {
    if (conviteGerado) {
      convitePanelRef.current?.scrollIntoView({
        behavior: 'smooth',
        block: 'nearest',
      })
    }
  }, [conviteGerado])

  async function handleEmitirConvite(
    clienteId: string,
    clienteNome: string,
  ) {
    setClienteEmitindoId(clienteId)
    setErroEmissao(null)
    setMensagemCopia(null)
    setConviteGerado(null)

    try {
      const antiforgeryToken = await obterAntiforgeryToken()
      const convite = await emitirConviteFicha(
        clienteId,
        antiforgeryToken,
      )

      setConviteGerado({
        clienteNome,
        link: new URL(
          convite.linkPreenchimento,
          window.location.origin,
        ).toString(),
        expiraEmUtc: convite.expiraEmUtc,
      })
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 401) {
        window.location.replace('/profissional/entrar')
        return
      }

      setErroEmissao(
        error instanceof ApiRequestError
          ? error.message
          : 'Não foi possível gerar o convite.',
      )
    } finally {
      setClienteEmitindoId(null)
    }
  }

  async function handleCopiarConvite() {
    if (!conviteGerado) {
      return
    }

    try {
      await navigator.clipboard.writeText(conviteGerado.link)
      setMensagemCopia('Link copiado. Agora você pode enviá-lo ao cliente.')
    } catch {
      conviteInputRef.current?.focus()
      conviteInputRef.current?.select()
      setMensagemCopia(
        'O navegador não permitiu a cópia automática. O link foi selecionado para você copiar manualmente.',
      )
    }
  }

  const totalPaginasExibido = Math.max(resultado?.totalPaginas ?? 1, 1)

  return (
    <main className="clients-shell">
      <header className="clients-header">
        <div>
          <a className="clients-back-link" href="/profissional">
            ← Voltar ao painel
          </a>
          <p className="eyebrow">Área profissional</p>
          <h1>Clientes</h1>
          <p>
            Consulte os dados básicos cadastrados. Informações de saúde não
            são exibidas nesta lista.
          </p>
        </div>

        <a className="clients-primary-link" href="/profissional/clientes/novo">
          Cadastrar cliente
        </a>
      </header>

      <section className="clients-content" aria-live="polite">
        {conviteGerado && (
          <section
            className="invitation-result"
            aria-labelledby="invitation-result-title"
            ref={convitePanelRef}
          >
            <div className="invitation-result-heading">
              <div>
                <p className="eyebrow">Convite gerado</p>
                <h2 id="invitation-result-title">
                  Link para {conviteGerado.clienteNome}
                </h2>
              </div>
              <button
                className="invitation-close-button"
                type="button"
                aria-label="Fechar resultado do convite"
                onClick={() => {
                  setConviteGerado(null)
                  setMensagemCopia(null)
                }}
              >
                Fechar
              </button>
            </div>

            <p>
              Este link expira em{' '}
              <strong>
                {new Intl.DateTimeFormat('pt-BR', {
                  dateStyle: 'short',
                  timeStyle: 'short',
                }).format(new Date(conviteGerado.expiraEmUtc))}
              </strong>
              . Envie-o somente para a pessoa indicada.
            </p>

            <div className="invitation-link-row">
              <label>
                <span>Link de preenchimento</span>
                <input
                  ref={conviteInputRef}
                  type="text"
                  readOnly
                  value={conviteGerado.link}
                  onFocus={(event) => event.target.select()}
                />
              </label>
              <button type="button" onClick={handleCopiarConvite}>
                Copiar link
              </button>
            </div>

            {mensagemCopia && (
              <p className="invitation-copy-message" role="status">
                {mensagemCopia}
              </p>
            )}
          </section>
        )}

        {erroEmissao && (
          <div className="clients-action-error" role="alert">
            {erroEmissao}
          </div>
        )}

        {carregando && !resultado && (
          <div className="clients-state">
            <span className="clients-loading-indicator" aria-hidden="true" />
            Carregando clientes...
          </div>
        )}

        {erro && (
          <div className="clients-state clients-state--error" role="alert">
            <p>{erro}</p>
            <button type="button" onClick={() => window.location.reload()}>
              Tentar novamente
            </button>
          </div>
        )}

        {!erro && resultado && resultado.itens.length === 0 && (
          <div className="clients-state clients-state--empty">
            <p className="eyebrow">Nenhum cadastro</p>
            <h2>A lista de clientes está vazia.</h2>
            <p>Cadastre o primeiro cliente para começar.</p>
            <a
              className="clients-primary-link"
              href="/profissional/clientes/novo"
            >
              Cadastrar primeiro cliente
            </a>
          </div>
        )}

        {!erro && resultado && resultado.itens.length > 0 && (
          <>
            <div className="clients-summary">
              <strong>{resultado.totalItens}</strong>{' '}
              {resultado.totalItens === 1
                ? 'cliente cadastrado'
                : 'clientes cadastrados'}
              {carregando && <span>Atualizando...</span>}
            </div>

            <div className="clients-table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Cliente</th>
                    <th>Pronomes</th>
                    <th>Contato</th>
                    <th>Cadastrado em</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {resultado.itens.map((cliente) => (
                    <tr key={cliente.id}>
                      <td data-label="Cliente">
                        <div>
                          <strong>{cliente.nomeParaExibicao}</strong>
                          {cliente.nomeParaExibicao !== cliente.nomeCompleto && (
                            <span>{cliente.nomeCompleto}</span>
                          )}
                        </div>
                      </td>
                      <td data-label="Pronomes">
                        <div>{cliente.pronomes ?? 'Não informado'}</div>
                      </td>
                      <td data-label="Contato">
                        <div>
                          <strong>{cliente.celular}</strong>
                          <span>{cliente.email ?? 'E-mail não informado'}</span>
                        </div>
                      </td>
                      <td data-label="Cadastrado em">
                        <div>{formatarData(cliente.criadoEmUtc)}</div>
                      </td>
                      <td data-label="Ações">
                        <div>
                          <button
                            className="client-invitation-button"
                            type="button"
                            disabled={clienteEmitindoId !== null}
                            onClick={() =>
                              handleEmitirConvite(
                                cliente.id,
                                cliente.nomeParaExibicao,
                              )
                            }
                          >
                            {clienteEmitindoId === cliente.id
                              ? 'Gerando...'
                              : 'Gerar novo convite'}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <nav className="clients-pagination" aria-label="Paginação">
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
                disabled={
                  pagina >= resultado.totalPaginas || carregando
                }
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
