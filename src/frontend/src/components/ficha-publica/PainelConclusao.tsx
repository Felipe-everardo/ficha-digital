import type { TermoConsentimentoAceito } from '../../services/api'

type PainelConclusaoProps = {
  termoAceito: TermoConsentimentoAceito
}

function formatarDataHora(dataIso: string) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'long',
    timeStyle: 'short',
  }).format(new Date(dataIso))
}

export function PainelConclusao({ termoAceito }: PainelConclusaoProps) {
  return (
    <div className="completion-panel" role="status">
      <span className="completion-symbol" aria-hidden="true">
        ✓
      </span>
      <p className="eyebrow">Consentimento registrado</p>
      <h2>Procedimento autorizado com segurança.</h2>
      <p>
        Seu aceite, seu nome e sua assinatura foram registrados e protegidos
        por uma verificação de integridade. Não há nenhuma etapa adicional para
        você preencher antes do atendimento.
      </p>
      <dl className="completion-details">
        <div>
          <dt>Status</dt>
          <dd>Autorizada para o procedimento</dd>
        </div>
        <div>
          <dt>Autorizada em</dt>
          <dd>{formatarDataHora(termoAceito.aceitoEmUtc)}</dd>
        </div>
      </dl>
      <p className="completion-guidance">
        Você já pode fechar esta página. Não é necessário enviar uma captura de
        tela pelo aplicativo de mensagens.
      </p>
    </div>
  )
}
