import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { obterAntiforgeryToken } from '../services/autenticacao'
import { CalendarInput } from '../components/CalendarInput'
import { MonthInput } from '../components/MonthInput'
import { formatarMesAno, obterMesAno } from '../utils/periodo'
import {
  ApiRequestError,
  ApiValidationError,
  atualizarDespesa,
  criarDespesa,
  obterFinanceiro,
  type CategoriaDespesa,
  type Despesa,
  type FinanceiroResultado,
  type SalvarDespesaInput,
} from '../services/api'
import './FinanceiroPage.css'

type Periodo = 'todos' | 'dia' | 'mes' | 'ano'

const tamanhoPagina = 10
const agoraLocal = new Date()
const hoje = [
  agoraLocal.getFullYear(),
  (agoraLocal.getMonth() + 1).toString().padStart(2, '0'),
  agoraLocal.getDate().toString().padStart(2, '0'),
].join('-')
const mesAtual = formatarMesAno(
  agoraLocal.getFullYear(),
  agoraLocal.getMonth() + 1,
)
const anoAtual = hoje.slice(0, 4)

const formularioInicial: SalvarDespesaInput = {
  data: hoje,
  categoria: 'Materiais',
  descricao: '',
  valor: 0,
}

const categorias: Array<{ valor: CategoriaDespesa; rotulo: string }> = [
  { valor: 'Materiais', rotulo: 'Materiais' },
  { valor: 'Aluguel', rotulo: 'Aluguel' },
  { valor: 'Contas', rotulo: 'Contas' },
  { valor: 'Manutencao', rotulo: 'Manutenção' },
  { valor: 'Marketing', rotulo: 'Marketing' },
  { valor: 'ImpostosETaxas', rotulo: 'Impostos e taxas' },
  { valor: 'PagamentoProfissional', rotulo: 'Pagamento de profissional' },
  { valor: 'Outro', rotulo: 'Outro' },
]

const moeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

const dataBrasileira = new Intl.DateTimeFormat('pt-BR', {
  timeZone: 'UTC',
})

function limitesDoPeriodo(periodo: Periodo, valor: string) {
  if (periodo === 'dia' && valor) {
    return { dataDe: valor, dataAte: valor }
  }

  if (periodo === 'mes') {
    const mesAno = obterMesAno(valor, agoraLocal.getFullYear())
    if (!mesAno) return {}

    const ano = Number(mesAno.ano)
    const mes = Number(mesAno.mes)
    const ultimoDia = new Date(Date.UTC(ano, mes, 0)).getUTCDate()
    const valorIso = `${mesAno.ano}-${mesAno.mes}`
    return {
      dataDe: `${valorIso}-01`,
      dataAte: `${valorIso}-${ultimoDia.toString().padStart(2, '0')}`,
    }
  }

  if (periodo === 'ano' && /^\d{4}$/.test(valor)) {
    return { dataDe: `${valor}-01-01`, dataAte: `${valor}-12-31` }
  }

  return {}
}

function rotuloCategoria(categoria: CategoriaDespesa) {
  return categorias.find((item) => item.valor === categoria)?.rotulo ?? categoria
}

export function FinanceiroPage() {
  const [resultado, setResultado] = useState<FinanceiroResultado | null>(null)
  const [pagina, setPagina] = useState(1)
  const [revisao, setRevisao] = useState(0)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [periodo, setPeriodo] = useState<Periodo>('mes')
  const [valorPeriodo, setValorPeriodo] = useState(mesAtual)
  const [periodoAplicado, setPeriodoAplicado] = useState<{
    periodo: Periodo
    valor: string
  }>({ periodo: 'mes', valor: mesAtual })
  const [formulario, setFormulario] = useState<SalvarDespesaInput>(
    formularioInicial,
  )
  const [despesaEmEdicao, setDespesaEmEdicao] = useState<string | null>(null)
  const [salvando, setSalvando] = useState(false)
  const [erroFormulario, setErroFormulario] = useState<string | null>(null)
  const [sucesso, setSucesso] = useState<string | null>(null)

  const filtros = useMemo(
    () => limitesDoPeriodo(periodoAplicado.periodo, periodoAplicado.valor),
    [periodoAplicado],
  )

  useEffect(() => {
    const abortController = new AbortController()
    setCarregando(true)
    setErro(null)

    obterFinanceiro(pagina, tamanhoPagina, filtros, abortController.signal)
      .then((dados) => setResultado(dados))
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
            : 'Não foi possível carregar o financeiro.',
        )
      })
      .finally(() => setCarregando(false))

    return () => abortController.abort()
  }, [filtros, pagina, revisao])

  function mudarPeriodo(novoPeriodo: Periodo) {
    setPeriodo(novoPeriodo)
    setValorPeriodo(
      novoPeriodo === 'dia'
        ? hoje
        : novoPeriodo === 'mes'
          ? mesAtual
          : novoPeriodo === 'ano'
            ? anoAtual
            : '',
    )
  }

  function aplicarPeriodo(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setPagina(1)
    setPeriodoAplicado({ periodo, valor: valorPeriodo })
  }

  async function salvar(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSalvando(true)
    setErroFormulario(null)
    setSucesso(null)

    try {
      const token = await obterAntiforgeryToken()
      if (despesaEmEdicao) {
        await atualizarDespesa(despesaEmEdicao, formulario, token)
        setSucesso('Despesa atualizada com sucesso.')
      } else {
        await criarDespesa(formulario, token)
        setSucesso('Despesa registrada com sucesso.')
      }

      setFormulario(formularioInicial)
      setDespesaEmEdicao(null)
      setPagina(1)
      setRevisao((valor) => valor + 1)
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 401) {
        window.location.replace('/profissional/entrar')
        return
      }

      if (error instanceof ApiValidationError) {
        setErroFormulario(
          Object.values(error.errors).flat()[0] ??
            'Confira os dados informados.',
        )
      } else {
        setErroFormulario(
          error instanceof ApiRequestError
            ? error.message
            : 'Não foi possível salvar a despesa.',
        )
      }
    } finally {
      setSalvando(false)
    }
  }

  function editar(despesa: Despesa) {
    setDespesaEmEdicao(despesa.id)
    setFormulario({
      data: despesa.data,
      categoria: despesa.categoria,
      descricao: despesa.descricao,
      valor: despesa.valor,
    })
    setErroFormulario(null)
    setSucesso(null)
    document.querySelector('#despesa-formulario')?.scrollIntoView({
      behavior: 'smooth',
      block: 'start',
    })
  }

  const resumo = resultado?.resumo

  return (
    <main className="finance-shell">
      <a
        className="records-back-link desktop-section-navigation"
        href="/profissional"
      >
        ← Voltar para a área profissional
      </a>

      <header className="finance-header">
        <div>
          <p className="eyebrow">Gestão do estúdio</p>
          <h1>Financeiro</h1>
          <p>
            Acompanhe os valores recebidos, as despesas e o saldo real do
            período.
          </p>
        </div>
        <a
          className="records-primary-link desktop-section-navigation"
          href="/profissional/fichas"
        >
          Ver atendimentos
        </a>
      </header>

      <form className="finance-period" onSubmit={aplicarPeriodo}>
        <div>
          <p className="eyebrow">Período</p>
          <h2>Resumo financeiro</h2>
        </div>
        <label>
          <span>Visualizar por</span>
          <select
            value={periodo}
            onChange={(event) => mudarPeriodo(event.target.value as Periodo)}
          >
            <option value="todos">Todo o período</option>
            <option value="dia">Dia</option>
            <option value="mes">Mês</option>
            <option value="ano">Ano</option>
          </select>
        </label>
        {periodo !== 'todos' && (
          <label>
            <span>{periodo === 'dia' ? 'Data' : periodo === 'mes' ? 'Mês' : 'Ano'}</span>
            {periodo === 'dia' ? (
              <CalendarInput
                type="date"
                max={hoje}
                required
                value={valorPeriodo}
                onChange={(event) => setValorPeriodo(event.target.value)}
              />
            ) : periodo === 'mes' ? (
              <MonthInput
                required
                value={valorPeriodo}
                onValueChange={setValorPeriodo}
              />
            ) : (
              <input
                type="number"
                min="2000"
                max={anoAtual}
                placeholder="Ex.: 2026"
                required
                value={valorPeriodo}
                onChange={(event) => setValorPeriodo(event.target.value)}
              />
            )}
          </label>
        )}
        <button type="submit">Aplicar</button>
      </form>

      <section className="finance-summary" aria-label="Resumo do período">
        <article>
          <span>Recebido</span>
          <strong>{moeda.format(resumo?.totalRecebido ?? 0)}</strong>
          <small>{resumo?.atendimentosPagos ?? 0} pagamentos confirmados</small>
        </article>
        <article>
          <span>Saídas</span>
          <strong>{moeda.format(resumo?.totalSaidas ?? 0)}</strong>
          <small>{resultado?.totalDespesas ?? 0} despesas no período</small>
        </article>
        <article className={(resumo?.saldo ?? 0) < 0 ? 'finance-summary--negative' : 'finance-summary--balance'}>
          <span>Saldo</span>
          <strong>{moeda.format(resumo?.saldo ?? 0)}</strong>
          <small>Recebido menos saídas</small>
        </article>
      </section>

      <div className="finance-content">
        <section
          className="finance-form-card"
          id="despesa-formulario"
          aria-labelledby="finance-expense-title"
        >
          <p className="eyebrow">Saída</p>
          <h2 id="finance-expense-title">
            {despesaEmEdicao ? 'Editar despesa' : 'Registrar despesa'}
          </h2>
          <form onSubmit={salvar}>
            <label>
              <span>Data *</span>
              <CalendarInput
                type="date"
                max={hoje}
                required
                value={formulario.data}
                onChange={(event) => setFormulario({ ...formulario, data: event.target.value })}
              />
            </label>
            <label>
              <span>Categoria *</span>
              <select
                required
                value={formulario.categoria}
                onChange={(event) => setFormulario({
                  ...formulario,
                  categoria: event.target.value as CategoriaDespesa,
                })}
              >
                {categorias.map((categoria) => (
                  <option key={categoria.valor} value={categoria.valor}>
                    {categoria.rotulo}
                  </option>
                ))}
              </select>
            </label>
            <label className="finance-form-card__wide">
              <span>Descrição *</span>
              <input
                type="text"
                maxLength={200}
                required
                placeholder="Ex.: luvas e materiais descartáveis"
                value={formulario.descricao}
                onChange={(event) => setFormulario({ ...formulario, descricao: event.target.value })}
              />
            </label>
            <label>
              <span>Valor *</span>
              <input
                type="number"
                min="0.01"
                max="999999.99"
                step="0.01"
                placeholder="Ex.: 150,00"
                required
                value={formulario.valor || ''}
                onChange={(event) => setFormulario({
                  ...formulario,
                  valor: Number(event.target.value),
                })}
              />
            </label>

            {erroFormulario && <p className="form-error finance-form-message" role="alert">{erroFormulario}</p>}
            {sucesso && <p className="finance-success finance-form-message" role="status">{sucesso}</p>}

            <div className="finance-form-actions">
              <button type="submit" disabled={salvando}>
                {salvando ? 'Salvando...' : despesaEmEdicao ? 'Salvar alteração' : 'Registrar despesa'}
              </button>
              {despesaEmEdicao && (
                <button
                  className="secondary-button"
                  type="button"
                  onClick={() => {
                    setDespesaEmEdicao(null)
                    setFormulario(formularioInicial)
                    setErroFormulario(null)
                  }}
                >
                  Cancelar edição
                </button>
              )}
            </div>
          </form>
        </section>

        <section className="finance-expenses" aria-labelledby="finance-list-title">
          <div className="finance-section-heading">
            <div>
              <p className="eyebrow">Histórico de saídas</p>
              <h2 id="finance-list-title">Despesas do período</h2>
            </div>
            <span>{resultado?.totalDespesas ?? 0} registros</span>
          </div>

          {erro && <p className="form-error" role="alert">{erro}</p>}
          {carregando ? (
            <p className="finance-empty" aria-live="polite">Carregando financeiro...</p>
          ) : resultado && resultado.despesas.length > 0 ? (
            <div className="finance-table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Data</th>
                    <th>Despesa</th>
                    <th>Valor</th>
                    <th>Registrado por</th>
                    <th>Ação</th>
                  </tr>
                </thead>
                <tbody>
                  {resultado.despesas.map((despesa) => (
                    <tr key={despesa.id}>
                      <td data-label="Data">{dataBrasileira.format(new Date(`${despesa.data}T00:00:00Z`))}</td>
                      <td data-label="Despesa">
                        <strong>{despesa.descricao}</strong>
                        <small>{rotuloCategoria(despesa.categoria)}</small>
                      </td>
                      <td data-label="Valor"><strong>{moeda.format(despesa.valor)}</strong></td>
                      <td data-label="Registrado por">{despesa.profissionalNome}</td>
                      <td data-label="Ação">
                        <button className="finance-edit-button" type="button" onClick={() => editar(despesa)}>
                          Editar
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="finance-empty">Nenhuma despesa registrada neste período.</p>
          )}

          {(resultado?.totalPaginas ?? 0) > 1 && (
            <nav className="finance-pagination" aria-label="Paginação das despesas">
              <button type="button" disabled={pagina <= 1} onClick={() => setPagina((valor) => valor - 1)}>
                Anterior
              </button>
              <span>Página {pagina} de {resultado?.totalPaginas}</span>
              <button type="button" disabled={pagina >= (resultado?.totalPaginas ?? 1)} onClick={() => setPagina((valor) => valor + 1)}>
                Próxima
              </button>
            </nav>
          )}
        </section>
      </div>
    </main>
  )
}
