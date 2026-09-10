import type { ConviteFichaAberto } from '../../services/api'

type ResumoProcedimentoProps = {
  convite: ConviteFichaAberto
}

export function ResumoProcedimento({ convite }: ResumoProcedimentoProps) {
  const procedimento = convite.tipoProcedimento === 'Piercing'
    ? 'Piercing'
    : convite.tipoProcedimento === 'Tatuagem'
      ? 'Tatuagem'
      : 'Não informado'

  return (
    <aside className="appointment-summary" aria-label="Seu atendimento">
      <div>
        <span>Procedimento</span>
        <strong>{procedimento}</strong>
      </div>
      <div>
        <span>Profissional responsável</span>
        <strong>{convite.profissionalResponsavelNome}</strong>
      </div>
    </aside>
  )
}
