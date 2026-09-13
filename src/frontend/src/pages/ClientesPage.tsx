import { type FormEvent, useEffect, useRef, useState } from 'react'
import {
  ApiRequestError,
  emitirConviteFicha,
  listarClientes,
  type ClientesPaginados,
  type FiltrosClientes,
  type TipoProcedimento,
} from '../services/api'
import { obterAntiforgeryToken } from '../services/autenticacao'
import { CalendarInput } from '../components/CalendarInput'
import { formatarTelefoneBrasileiro } from '../utils/telefone'
import './ClientesPage.css'

const TAMANHO_PAGINA = 10

type FiltrosFormulario = {
  busca: string
  telefone: string
  instagram: string
  tipoProcedimento: string
  ultimaFichaDe: string
  ultimaFichaAte: string
}

const FILTROS_VAZIOS: FiltrosFormulario = {
  busca: '',
  telefone: '',
  instagram: '',
  tipoProcedimento: '',
  ultimaFichaDe: '',
  ultimaFichaAte: '',
}

type ConviteEmPreparacao = {
  clienteId: string
  clienteNome: string
}

type ConviteGerado = {
  clienteNome: string
  profissionalResponsavelNome: string
  procedimento: Exclude<TipoProcedimento, 'NaoInformado'>
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

function formatarProcedimento(tipo: TipoProcedimento) {
  if (tipo === 'Tatuagem') return 'Tatuagem'
  if (tipo === 'Piercing') return 'Piercing'
  return 'Não informado'
}

function criarFiltros(formulario: FiltrosFormulario): FiltrosClientes {
  return {
    busca: formulario.busca.trim() || undefined,
    telefone: formulario.telefone.trim() || undefined,
    instagram: formulario.instagram.trim() || undefined,
    tipoProcedimento:
      (formulario.tipoProcedimento as TipoProcedimento) || undefined,
    ultimaFichaDe: formulario.ultimaFichaDe || undefined,
    ultimaFichaAte: formulario.ultimaFichaAte || undefined,
  }
}

export function ClientesPage() {
  const [pagina, setPagina] = useState(1)
  const [resultado, setResultado] = useState<ClientesPaginados | null>(null)
  const [carregando, setCarregando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const procedimentosPermitidos: Array<'Tatuagem' | 'Piercing'> = [
    'Tatuagem',
    'Piercing',
  ]
  const [filtrosFormulario, setFiltrosFormulario] =
    useState<FiltrosFormulario>(FILTROS_VAZIOS)
  const [filtrosAplicados, setFiltrosAplicados] =
    useState<FiltrosClientes | null>(null)
  const [conviteEmPreparacao, setConviteEmPreparacao] =
    useState<ConviteEmPreparacao | null>(null)
  const [tipoProcedimento, setTipoProcedimento] = useState<
    '' | Exclude<TipoProcedimento, 'NaoInformado'>
  >('')
  const [profissionalResponsavelNome, setProfissionalResponsavelNome] =
    useState('')
  const [clienteEmitindoId, setClienteEmitindoId] = useState<string | null>(null)
  const [erroEmissao, setErroEmissao] = useState<string | null>(null)
  const [conviteGerado, setConviteGerado] = useState<ConviteGerado | null>(null)
  const [mensagemCopia, setMensagemCopia] = useState<string | null>(null)
  const convitePanelRef = useRef<HTMLElement>(null)
  const conviteInputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (filtrosAplicados === null) {
      setResultado(null)
      setErro(null)
      setCarregando(false)
      return
    }

    const abortController = new AbortController()

    setCarregando(true)
    setErro(null)

    listarClientes(
      pagina,
      TAMANHO_PAGINA,
      filtrosAplicados,
      abortController.signal,
    )
      .then(setResultado)
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return
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
        if (!abortController.signal.aborted) setCarregando(false)
      })

    return () => abortController.abort()
  }, [pagina, filtrosAplicados])

  useEffect(() => {
    if (conviteGerado || conviteEmPreparacao) {
      convitePanelRef.current?.scrollIntoView({
        behavior: 'smooth',
        block: 'nearest',
      })
    }
  }, [conviteGerado, conviteEmPreparacao])

  function atualizarFiltro(campo: keyof FiltrosFormulario, valor: string) {
    setFiltrosFormulario((atual) => ({ ...atual, [campo]: valor }))
  }

  function aplicarFiltros(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setPagina(1)
    setFiltrosAplicados(criarFiltros(filtrosFormulario))
  }

  function limparFiltros() {
    setFiltrosFormulario(FILTROS_VAZIOS)
    setPagina(1)
    setFiltrosAplicados(null)
  }

  function prepararConvite(clienteId: string, clienteNome: string) {
    setConviteGerado(null)
    setErroEmissao(null)
    setTipoProcedimento('')
    setProfissionalResponsavelNome('')
    setConviteEmPreparacao({ clienteId, clienteNome })
  }

  async function handleEmitirConvite() {
    if (
      !conviteEmPreparacao ||
      !tipoProcedimento ||
      !profissionalResponsavelNome.trim()
    ) {
      setErroEmissao('Informe o profissional responsável e o procedimento.')
      return
    }

    setClienteEmitindoId(conviteEmPreparacao.clienteId)
    setErroEmissao(null)
    setMensagemCopia(null)

    try {
      const antiforgeryToken = await obterAntiforgeryToken()
      const convite = await emitirConviteFicha(
        conviteEmPreparacao.clienteId,
        profissionalResponsavelNome.trim(),
        tipoProcedimento,
        antiforgeryToken,
      )

      setConviteGerado({
        clienteNome: conviteEmPreparacao.clienteNome,
        profissionalResponsavelNome: profissionalResponsavelNome.trim(),
        procedimento: tipoProcedimento,
        link: new URL(
          convite.linkPreenchimento,
          window.location.origin,
        ).toString(),
        expiraEmUtc: convite.expiraEmUtc,
      })
      setConviteEmPreparacao(null)
      setFiltrosAplicados((atuais) =>
        atuais === null ? atuais : { ...atuais },
      )
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
    if (!conviteGerado) return

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
  const possuiFiltroPreenchido = Object.values(
    criarFiltros(filtrosFormulario),
  ).some((valor) => valor !== undefined)

  return (
    <main className="clients-shell">
      <header className="clients-header">
        <div>
          <a
            className="clients-back-link desktop-section-navigation"
            href="/profissional"
          >
            ← Voltar ao painel
          </a>
          <p className="eyebrow">Área profissional</p>
          <h1>Clientes</h1>
          <p>
            Encontre rapidamente um cliente, veja sua última ficha e
            acesse o histórico completo.
          </p>
        </div>

        <a className="clients-primary-link" href="/profissional/clientes/novo">
          Cadastrar cliente
        </a>
      </header>

      <form className="clients-filters" onSubmit={aplicarFiltros}>
        <div className="filters-heading">
          <div>
            <p className="eyebrow">Busca principal</p>
            <h2>Encontrar clientes</h2>
          </div>
          <button type="button" className="filters-clear" onClick={limparFiltros}>
            Limpar filtros
          </button>
        </div>

        <div className="filters-grid">
          <label className="filter-field filter-field--wide">
            <span>Nome do cliente</span>
            <input
              type="search"
              maxLength={150}
              placeholder="Ex.: Ana"
              autoComplete="off"
              value={filtrosFormulario.busca}
              onChange={(event) => atualizarFiltro('busca', event.target.value)}
            />
          </label>
          <label className="filter-field">
            <span>Telefone</span>
            <input
              type="tel"
              maxLength={15}
              inputMode="numeric"
              placeholder="Ex.: (21) 99999-9999"
              autoComplete="off"
              value={filtrosFormulario.telefone}
              onChange={(event) =>
                atualizarFiltro(
                  'telefone',
                  formatarTelefoneBrasileiro(event.target.value),
                )
              }
            />
          </label>
          <label className="filter-field">
            <span>Instagram</span>
            <input
              type="search"
              maxLength={100}
              placeholder="Ex.: @usuario"
              autoComplete="off"
              value={filtrosFormulario.instagram}
              onChange={(event) =>
                atualizarFiltro('instagram', event.target.value)
              }
            />
          </label>
          <label className="filter-field">
            <span>Último procedimento</span>
            <select
              value={filtrosFormulario.tipoProcedimento}
              onChange={(event) =>
                atualizarFiltro('tipoProcedimento', event.target.value)
              }
            >
              <option value="">Todos</option>
              <option value="Tatuagem">Tatuagem</option>
              <option value="Piercing">Piercing</option>
            </select>
          </label>
          <label className="filter-field">
            <span>Última ficha de</span>
            <CalendarInput
              max={filtrosFormulario.ultimaFichaAte || undefined}
              value={filtrosFormulario.ultimaFichaDe}
              onValueChange={(valor) => atualizarFiltro('ultimaFichaDe', valor)}
            />
          </label>
          <label className="filter-field">
            <span>Última ficha até</span>
            <CalendarInput
              min={filtrosFormulario.ultimaFichaDe || undefined}
              value={filtrosFormulario.ultimaFichaAte}
              onValueChange={(valor) => atualizarFiltro('ultimaFichaAte', valor)}
            />
          </label>
        </div>

        <button type="submit" className="filters-submit" disabled={carregando}>
          {carregando
            ? 'Atualizando...'
            : possuiFiltroPreenchido
              ? 'Buscar clientes'
              : 'Mostrar todos os clientes'}
        </button>
      </form>

      <section className="clients-content" aria-live="polite">
        {conviteEmPreparacao && (
          <section className="invitation-result" ref={convitePanelRef}>
            <div className="invitation-result-heading">
              <div>
                <p className="eyebrow">Novo procedimento</p>
                <h2>Convite para {conviteEmPreparacao.clienteNome}</h2>
              </div>
              <button
                className="invitation-close-button"
                type="button"
                onClick={() => setConviteEmPreparacao(null)}
              >
                Cancelar
              </button>
            </div>
            <label className="invitation-procedure-field">
              <span>Profissional responsável *</span>
              <input
                type="text"
                maxLength={150}
                required
                placeholder="Nome completo"
                value={profissionalResponsavelNome}
                onChange={(event) => {
                  setProfissionalResponsavelNome(event.target.value)
                  setErroEmissao(null)
                }}
              />
            </label>
            <label className="invitation-procedure-field">
              <span>Qual procedimento será realizado?</span>
              <select
                required
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
            <button
              type="button"
              disabled={
                !tipoProcedimento ||
                !profissionalResponsavelNome.trim() ||
                clienteEmitindoId !== null
              }
              onClick={handleEmitirConvite}
            >
              {clienteEmitindoId ? 'Gerando...' : 'Gerar convite'}
            </button>
          </section>
        )}

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
                  {conviteGerado.procedimento} para {conviteGerado.clienteNome}
                </h2>
              </div>
              <button
                className="invitation-close-button"
                type="button"
                onClick={() => {
                  setConviteGerado(null)
                  setMensagemCopia(null)
                }}
              >
                Fechar
              </button>
            </div>
            <p>
              Profissional responsável:{' '}
              <strong>{conviteGerado.profissionalResponsavelNome}</strong>.
              {' '}Este link expira em{' '}
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

        {!carregando && !erro && resultado === null && (
          <div className="clients-state clients-state--initial">
            <p className="eyebrow">Lista sob demanda</p>
            <h2>Como deseja encontrar o cliente?</h2>
            <p>
              Digite um nome, telefone ou Instagram, ou combine os filtros
              acima. Para consultar a base inteira, use “Mostrar todos os
              clientes”.
            </p>
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
            <p className="eyebrow">Nenhum resultado</p>
            <h2>Nenhum cliente foi encontrado.</h2>
            <button type="button" onClick={limparFiltros}>
              Limpar filtros
            </button>
          </div>
        )}

        {!erro && resultado && resultado.itens.length > 0 && (
          <>
            <div className="clients-summary">
              <strong>{resultado.totalItens}</strong>{' '}
              {resultado.totalItens === 1
                ? 'cliente encontrado'
                : 'clientes encontrados'}
              {carregando && <span>Atualizando...</span>}
            </div>

            <div className="clients-table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Cliente</th>
                    <th>Contato</th>
                    <th>Último procedimento</th>
                    <th>Profissional</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {resultado.itens.map((cliente) => (
                    <tr key={cliente.id}>
                      <td data-label="Cliente">
                        <div>
                          <strong>{cliente.nomeParaExibicao}</strong>
                          {cliente.nomeCompleto &&
                            cliente.nomeParaExibicao !== cliente.nomeCompleto && (
                              <span>{cliente.nomeCompleto}</span>
                            )}
                        </div>
                      </td>
                      <td data-label="Contato">
                        <div>
                          <strong>{cliente.celular ?? 'Aguardando cliente'}</strong>
                          <span>{cliente.email ?? 'E-mail não informado'}</span>
                        </div>
                      </td>
                      <td data-label="Último procedimento">
                        {cliente.ultimaFicha ? (
                          <div>
                            <strong>
                              {formatarProcedimento(
                                cliente.ultimaFicha.tipoProcedimento,
                              )}
                            </strong>
                            <span>
                              {formatarData(cliente.ultimaFicha.criadaEmUtc)}
                            </span>
                          </div>
                        ) : (
                          'Ficha ainda não gerada'
                        )}
                      </td>
                      <td data-label="Profissional">
                        {cliente.ultimaFicha?.profissionalResponsavelNome ??
                          'Não informado'}
                      </td>
                      <td data-label="Ações">
                        <div className="client-actions">
                          <a href={`/profissional/clientes/${cliente.id}`}>
                            Ver cliente
                          </a>
                          <button
                            className="client-invitation-button"
                            type="button"
                            disabled={clienteEmitindoId !== null}
                            onClick={() =>
                              prepararConvite(cliente.id, cliente.nomeParaExibicao)
                            }
                          >
                            Nova ficha
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
                onClick={() => setPagina((atual) => atual - 1)}
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
                onClick={() => setPagina((atual) => atual + 1)}
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
