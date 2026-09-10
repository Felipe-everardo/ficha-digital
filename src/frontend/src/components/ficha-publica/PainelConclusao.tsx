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
      <p className="eyebrow">Ficha concluída</p>
      <h2>Obrigado. Suas informações foram recebidas.</h2>
      <p>
        O questionário e o aceite foram registrados. O profissional responsável
        poderá consultar a confirmação no sistema.
      </p>
      <dl className="completion-details">
        <div>
          <dt>Status</dt>
          <dd>{termoAceito.statusFicha}</dd>
        </div>
        <div>
          <dt>Concluída em</dt>
          <dd>{formatarDataHora(termoAceito.aceitoEmUtc)}</dd>
        </div>
        <div>
          <dt>Código da evidência</dt>
          <dd className="evidence-code">{termoAceito.evidenciaHash}</dd>
        </div>
      </dl>
      <p className="completion-guidance">
        Você já pode fechar esta página. Não é necessário enviar uma captura de
        tela pelo aplicativo de mensagens.
      </p>
    </div>
  )
}
