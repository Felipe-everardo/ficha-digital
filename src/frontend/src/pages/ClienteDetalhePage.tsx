import { useEffect, useState } from 'react'
import {
  ApiRequestError,
  obterDetalheCliente,
  type ClienteDetalhe,
  type TipoProcedimento,
} from '../services/api'
import './ClienteDetalhePage.css'

type ClienteDetalhePageProps = {
  clienteId: string
}

function formatarDataHora(dataUtc: string) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(dataUtc))
}

function formatarDataNascimento(data: string | null) {
  if (!data) return 'Aguardando preenchimento'
  const [ano, mes, dia] = data.split('-').map(Number)
  return new Intl.DateTimeFormat('pt-BR').format(new Date(ano, mes - 1, dia))
}

function formatarProcedimento(tipo: TipoProcedimento) {
  if (tipo === 'Tatuagem') return 'Tatuagem'
  if (tipo === 'Piercing') return 'Piercing'
  return 'Não informado'
}

function formatarStatus(status: string) {
  const nomes: Record<string, string> = {
    Rascunho: 'Rascunho',
    ConviteEnviado: 'Convite enviado',
    EmPreenchimento: 'Em preenchimento',
    Concluida: 'Concluída',
    Expirada: 'Expirada',
    Cancelada: 'Cancelada',
  }
  return nomes[status] ?? status
}

export function ClienteDetalhePage({ clienteId }: ClienteDetalhePageProps) {
  const [cliente, setCliente] = useState<ClienteDetalhe | null>(null)
  const [erro, setErro] = useState<string | null>(null)

  useEffect(() => {
    const abortController = new AbortController()

    obterDetalheCliente(clienteId, abortController.signal)
      .then(setCliente)
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return
        if (error instanceof ApiRequestError && error.status === 401) {
          window.location.replace('/profissional/entrar')
          return
        }
        setErro(
          error instanceof ApiRequestError
            ? error.message
            : 'Não foi possível carregar o cliente.',
        )
      })

    return () => abortController.abort()
  }, [clienteId])

  if (erro) {
    return (
      <main className="client-detail-shell">
        <section className="client-detail-state client-detail-state--error">
          <p>{erro}</p>
          <a href="/profissional/clientes">Voltar aos clientes</a>
        </section>
      </main>
    )
  }

  if (!cliente) {
    return (
      <main className="client-detail-shell">
        <section className="client-detail-state">
          <span className="client-detail-loading" aria-hidden="true" />
          Carregando histórico do cliente...
        </section>
      </main>
    )
  }

  return (
    <main className="client-detail-shell">
      <header className="client-detail-header">
        <a href="/profissional/clientes">← Voltar aos clientes</a>
        <p className="eyebrow">Cadastro do cliente</p>
        <h1>{cliente.nomeParaExibicao}</h1>
        <p>
          Referência: <strong>{cliente.nomeReferencia}</strong> · cadastrado em{' '}
          {formatarDataHora(cliente.criadoEmUtc)}
        </p>
      </header>

      <section className="client-detail-section">
        <div className="client-detail-section-heading">
          <p className="eyebrow">Dados pessoais</p>
          <h2>Informações atuais</h2>
        </div>
        <dl className="client-detail-grid">
          <div><dt>Nome completo</dt><dd>{cliente.nomeCompleto ?? 'Aguardando preenchimento'}</dd></div>
          <div><dt>Nome social</dt><dd>{cliente.nomeSocial ?? 'Não informado'}</dd></div>
          <div><dt>Pronomes</dt><dd>{cliente.pronomes ?? 'Não informado'}</dd></div>
          <div><dt>Estado civil</dt><dd>{cliente.estadoCivil ?? 'Não informado'}</dd></div>
          <div><dt>Nascimento</dt><dd>{formatarDataNascimento(cliente.dataNascimento)}</dd></div>
          <div><dt>CPF</dt><dd>{cliente.cpf ?? 'Não informado'}</dd></div>
          <div><dt>Celular</dt><dd>{cliente.celular ?? 'Aguardando preenchimento'}</dd></div>
          <div><dt>Telefone adicional</dt><dd>{cliente.telefoneAdicional ?? 'Não informado'}</dd></div>
          <div><dt>E-mail</dt><dd>{cliente.email ?? 'Não informado'}</dd></div>
          <div><dt>Instagram</dt><dd>{cliente.instagram ?? 'Não informado'}</dd></div>
          <div>
            <dt>Contato de emergência</dt>
            <dd>
              {cliente.contatoEmergenciaNome ?? 'Não informado'}
              {cliente.contatoEmergenciaCelular
                ? ` · ${cliente.contatoEmergenciaCelular}`
                : ''}
            </dd>
          </div>
          <div>
            <dt>Endereço</dt>
            <dd>
              {cliente.logradouro && cliente.numero
                ? `${cliente.logradouro}, ${cliente.numero}${cliente.complemento ? ` — ${cliente.complemento}` : ''} — ${cliente.bairro}, ${cliente.cidade}/${cliente.estado} — CEP ${cliente.cep}`
                : 'Não informado'}
            </dd>
          </div>
        </dl>
      </section>

      <section className="client-detail-section">
        <div className="client-detail-section-heading">
          <p className="eyebrow">Histórico</p>
          <h2>Fichas e procedimentos</h2>
          <p>
            {cliente.fichas.length === 1
              ? '1 ficha registrada'
              : `${cliente.fichas.length} fichas registradas`}
          </p>
        </div>

        {cliente.fichas.length === 0 ? (
          <div className="client-detail-empty">
            Este cliente ainda não possui fichas.
          </div>
        ) : (
          <div className="client-history-table">
            <table>
              <thead>
                <tr>
                  <th>Procedimento</th>
                  <th>Profissional</th>
                  <th>Status</th>
                  <th>Criada em</th>
                  <th>Ação</th>
                </tr>
              </thead>
              <tbody>
                {cliente.fichas.map((ficha) => (
                  <tr key={ficha.id}>
                    <td data-label="Procedimento">
                      <strong>{formatarProcedimento(ficha.tipoProcedimento)}</strong>
                    </td>
                    <td data-label="Profissional">{ficha.profissionalResponsavelNome}</td>
                    <td data-label="Status">{formatarStatus(ficha.status)}</td>
                    <td data-label="Criada em">{formatarDataHora(ficha.criadaEmUtc)}</td>
                    <td data-label="Ação">
                      <a
                        className="client-history-link"
                        href={`/profissional/fichas/${ficha.id}`}
                      >
                        Abrir ficha
                      </a>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </main>
  )
}
